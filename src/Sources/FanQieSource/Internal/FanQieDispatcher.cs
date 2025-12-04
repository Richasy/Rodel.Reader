// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Internal;

/// <summary>
/// HTTP 请求调度器.
/// </summary>
internal sealed class FanQieDispatcher : IDisposable
{
    private const string DeviceTokenHeader = "X-FanQie-Device";

    private readonly HttpClient _httpClient;
    private readonly FanQieClientOptions _options;
    private readonly ILogger? _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly SemaphoreSlim _deviceLock = new(1, 1);
    private bool _disposed;

    // 非章节 API 的设备令牌（客户端生命周期内缓存）
    private string? _builtInDeviceToken;
    private string? _selfHostDeviceToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieDispatcher"/> class.
    /// </summary>
    /// <param name="options">客户端配置.</param>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志记录器.</param>
    public FanQieDispatcher(FanQieClientOptions options, HttpClient? httpClient = null, ILogger? logger = null)
    {
        _options = options;
        _logger = logger;
        _semaphore = new SemaphoreSlim(options.MaxConcurrentRequests, options.MaxConcurrentRequests);

        _httpClient = httpClient ?? new HttpClient { Timeout = options.Timeout };
        ConfigureHttpClient();
    }

    /// <summary>
    /// 搜索书籍.
    /// </summary>
    /// <param name="query">搜索关键词.</param>
    /// <param name="offset">偏移量.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>搜索结果.</returns>
    public async Task<SearchResult<BookItem>> SearchBooksAsync(
        string query,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = ApiEndpoints.GetSearchUrl(query, offset, _options.Aid);
            var response = await GetAsync<Models.Internal.SearchApiResponse>(url, cancellationToken).ConfigureAwait(false);

            if (response.Code != 0)
            {
                throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Search failed.");
            }

            var items = response.Data?.RetData?.Select(MapToBookItem).ToList() ?? [];

            return new SearchResult<BookItem>
            {
                Items = items,
                HasMore = response.Data?.HasMore ?? false,
                NextOffset = response.Data?.Offset ?? (offset + items.Count),
                SearchId = response.Data?.SearchId,
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Official search API failed, trying third-party API...");
            return await SearchBooksWithThirdPartyApiAsync(query, offset, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 获取书籍详情.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>书籍详情.</returns>
    public async Task<BookDetail?> GetBookDetailAsync(
        string bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = ApiEndpoints.GetBookDetailUrl(bookId, _options.Aid);
            var response = await GetAsync<Models.Internal.BookDetailApiResponse>(url, cancellationToken).ConfigureAwait(false);

            if (response.Code != 0)
            {
                throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get book detail failed.");
            }

            if (response.Data is null || response.Data.Count == 0)
            {
                return null;
            }

            return MapToBookDetail(response.Data[0]);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Official book detail API failed, trying third-party API...");
            return await GetBookDetailWithThirdPartyApiAsync(bookId, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 获取书籍目录.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>卷列表.</returns>
    public async Task<IReadOnlyList<BookVolume>> GetBookTocAsync(
        string bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = ApiEndpoints.GetBookTocUrl(bookId);
            var response = await GetAsync<Models.Internal.BookTocApiResponse>(url, cancellationToken).ConfigureAwait(false);

            if (response.Code != 0)
            {
                throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get book TOC failed.");
            }

            return ParseToc(response.Data);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Official book TOC API failed, trying third-party API...");
            return await GetBookTocWithThirdPartyApiAsync(bookId, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 批量获取章节内容（使用范围表示法）.
    /// </summary>
    /// <param name="chapterRange">章节范围，格式为 "start-end"，如 "1-10".</param>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="bookTitle">书籍标题.</param>
    /// <param name="chapterInfoMap">章节信息映射表（itemId -> ChapterItem）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>章节内容列表.</returns>
    public async Task<IReadOnlyList<ChapterContent>> GetBatchContentByRangeAsync(
        string chapterRange,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken = default)
    {
        // 章节内容获取优先级：SelfHost API（如有）→ 内置 API

        // 如果有自部署 API，先尝试使用
        if (!string.IsNullOrEmpty(_options.SelfHostApiBaseUrl))
        {
            try
            {
                return await FetchBatchContentByRangeFromApiAsync(
                    _options.SelfHostApiBaseUrl,
                    chapterRange,
                    bookId,
                    bookTitle,
                    chapterInfoMap,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger?.LogWarning(ex, "SelfHost API failed for batch content (range: {Range}), falling back to built-in API...", chapterRange);
            }
        }

        // 使用内置 API
        return await FetchBatchContentByRangeFromApiAsync(
            FanQieClientOptions.BuiltInApiBaseUrl,
            chapterRange,
            bookId,
            bookTitle,
            chapterInfoMap,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量获取章节内容.
    /// </summary>
    /// <param name="itemIds">章节 ID 列表.</param>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="bookTitle">书籍标题.</param>
    /// <param name="chapterInfoMap">章节信息映射表（itemId -> ChapterItem）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>章节内容列表.</returns>
    [Obsolete("Use GetBatchContentByRangeAsync instead for better rate limiting.")]
    public async Task<IReadOnlyList<ChapterContent>> GetBatchContentAsync(
        IEnumerable<string> itemIds,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken = default)
    {
        // 将 itemIds 转换为范围请求
        var idList = itemIds.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        // 根据 chapterInfoMap 获取 order，按 order 分组为连续范围
        var orderedChapters = idList
            .Where(id => chapterInfoMap.ContainsKey(id))
            .Select(id => chapterInfoMap[id])
            .OrderBy(c => c.Order)
            .ToList();

        if (orderedChapters.Count == 0)
        {
            return [];
        }

        var ranges = CalculateRanges(orderedChapters.Select(c => c.Order).ToList());
        var results = new List<ChapterContent>();

        foreach (var range in ranges)
        {
            var rangeContent = await GetBatchContentByRangeAsync(
                range,
                bookId,
                bookTitle,
                chapterInfoMap,
                cancellationToken).ConfigureAwait(false);

            results.AddRange(rangeContent);

            // 请求间隔
            if (_options.RequestDelayMs > 0)
            {
                await Task.Delay(_options.RequestDelayMs, cancellationToken).ConfigureAwait(false);
            }
        }

        return results;
    }

    /// <summary>
    /// 计算连续范围.
    /// </summary>
    /// <param name="orders">章节序号列表（已排序）.</param>
    /// <returns>范围列表，如 ["1-5", "8-10"].</returns>
    private static List<string> CalculateRanges(List<int> orders)
    {
        if (orders.Count == 0)
        {
            return [];
        }

        var ranges = new List<string>();
        var start = orders[0];
        var end = orders[0];

        for (var i = 1; i < orders.Count; i++)
        {
            if (orders[i] == end + 1)
            {
                // 连续
                end = orders[i];
            }
            else
            {
                // 断开，保存当前范围
                ranges.Add($"{start}-{end}");
                start = orders[i];
                end = orders[i];
            }
        }

        // 保存最后一个范围
        ranges.Add($"{start}-{end}");

        return ranges;
    }

    /// <summary>
    /// 下载图片.
    /// </summary>
    /// <param name="imageUrl">图片 URL.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>图片二进制数据.</returns>
    public async Task<byte[]> DownloadImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _logger?.LogDebug("Downloading image: {Url}", imageUrl);

            using var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);

            // 设置必要的请求头以避免 403 错误
            request.Headers.Add("Accept", "image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            request.Headers.Add("Referer", "https://fanqienovel.com/");
            request.Headers.Add("Sec-Fetch-Dest", "image");
            request.Headers.Add("Sec-Fetch-Mode", "no-cors");
            request.Headers.Add("Sec-Fetch-Site", "cross-site");

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <summary>
    /// 批量下载图片.
    /// </summary>
    /// <param name="imageUrls">图片 URL 列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>图片 URL 与二进制数据的字典.</returns>
    public async Task<IReadOnlyDictionary<string, byte[]>> DownloadImagesAsync(
        IEnumerable<string> imageUrls,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, byte[]>();
        var urlList = imageUrls.Distinct().ToList();

        foreach (var url in urlList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var data = await DownloadImageAsync(url, cancellationToken).ConfigureAwait(false);
                results[url] = data;

                // 请求间隔
                if (_options.RequestDelayMs > 0 && urlList.IndexOf(url) < urlList.Count - 1)
                {
                    await Task.Delay(_options.RequestDelayMs, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to download image: {Url}", url);
                // 继续下载其他图片，不中断整个流程
            }
        }

        return results;
    }

    /// <summary>
    /// 获取章节段评数量.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>段落索引与评论数量的映射.</returns>
    public async Task<IReadOnlyDictionary<string, int>?> GetCommentCountAsync(
        string bookId,
        string chapterId,
        CancellationToken cancellationToken = default)
    {
        var url = ApiEndpoints.GetCommentCountUrl(bookId, chapterId, _options.Aid);
        _logger?.LogDebug("Getting comment count: {Url}", url);

        var response = await GetAsync<Models.Internal.DataResponse<Models.Internal.CommentCountData>>(
            url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            _logger?.LogWarning("Failed to get comment count: Code={Code}, Message={Message}", response.Code, response.Message);
            return null;
        }

        if (response.Data?.IdeaData is null)
        {
            return new Dictionary<string, int>();
        }

        return response.Data.IdeaData
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IdeaCount);
    }

    /// <summary>
    /// 获取章节段评列表.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    /// <param name="chapterId">章节 ID.</param>
    /// <param name="paragraphIndex">段落索引.</param>
    /// <param name="offset">分页偏移量.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>评论列表结果.</returns>
    public async Task<CommentListResult?> GetCommentsAsync(
        string bookId,
        string chapterId,
        int paragraphIndex,
        string? offset = null,
        CancellationToken cancellationToken = default)
    {
        var url = ApiEndpoints.GetCommentListUrl(bookId, chapterId, paragraphIndex, _options.Aid, offset);
        _logger?.LogDebug("Getting comments: {Url}", url);

        var response = await GetAsync<Models.Internal.DataResponse<Models.Internal.CommentListData>>(
            url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            _logger?.LogWarning("Failed to get comments: Code={Code}, Message={Message}", response.Code, response.Message);
            return null;
        }

        if (response.Data?.Comments is null)
        {
            return new CommentListResult
            {
                Comments = [],
                ParagraphIndex = paragraphIndex,
                HasMore = false,
                NextOffset = null,
                ParagraphContent = response.Data?.ParaSrcContent,
            };
        }

        var comments = response.Data.Comments
            .Select(MapToComment)
            .ToList();

        return new CommentListResult
        {
            Comments = comments,
            ParagraphIndex = paragraphIndex,
            HasMore = response.Data.HasMore,
            NextOffset = response.Data.NextOffset.ToString(),
            ParagraphContent = response.Data.ParaSrcContent,
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // 尝试释放设备令牌（使用 Fire-and-forget 模式，不阻塞 Dispose）
        _ = ReleaseDeviceTokensAsync();

        _semaphore.Dispose();
        _deviceLock.Dispose();
        _httpClient.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// 释放所有已注册的设备令牌.
    /// </summary>
    private async Task ReleaseDeviceTokensAsync()
    {
        try
        {
            var tasks = new List<Task>();

            if (!string.IsNullOrEmpty(_builtInDeviceToken))
            {
                tasks.Add(ReleaseDeviceAsync(FanQieClientOptions.BuiltInApiBaseUrl, _builtInDeviceToken));
            }

            if (!string.IsNullOrEmpty(_selfHostDeviceToken) && !string.IsNullOrEmpty(_options.SelfHostApiBaseUrl))
            {
                tasks.Add(ReleaseDeviceAsync(_options.SelfHostApiBaseUrl, _selfHostDeviceToken));
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to release device tokens during dispose.");
        }
    }

    /// <summary>
    /// 获取第三方 API 基础 URL.
    /// </summary>
    /// <param name="preferSelfHost">是否优先使用自部署 API.</param>
    /// <returns>API 基础 URL.</returns>
    private string GetThirdPartyApiBaseUrl(bool preferSelfHost = true)
    {
        if (preferSelfHost && !string.IsNullOrEmpty(_options.SelfHostApiBaseUrl))
        {
            return _options.SelfHostApiBaseUrl;
        }

        return FanQieClientOptions.BuiltInApiBaseUrl;
    }

    /// <summary>
    /// 注册设备，获取设备令牌.
    /// </summary>
    /// <param name="apiBaseUrl">API 基础 URL.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>设备令牌，如果注册失败则返回 null.</returns>
    private async Task<string?> RegisterDeviceAsync(string apiBaseUrl, CancellationToken cancellationToken)
    {
        try
        {
            var url = ApiEndpoints.GetDeviceRegisterUrl(apiBaseUrl);
            _logger?.LogDebug("Registering device: {Url}", url);

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using var content = new StringContent("{}", Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(new Uri(url), content, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var result = System.Text.Json.JsonSerializer.Deserialize(json, FanQieJsonContext.Default.DeviceRegisterApiResponse);

                if (result?.Code == 0 && !string.IsNullOrEmpty(result.Data?.Token))
                {
                    _logger?.LogDebug("Device registered successfully. Token: {Token}", result.Data.Token);
                    return result.Data.Token;
                }

                _logger?.LogWarning("Device registration failed: Code={Code}, Message={Message}", result?.Code, result?.Message);
                return null;
            }
            finally
            {
                _ = _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to register device at {Url}", apiBaseUrl);
            return null;
        }
    }

    /// <summary>
    /// 释放设备令牌.
    /// </summary>
    /// <param name="apiBaseUrl">API 基础 URL.</param>
    /// <param name="deviceToken">设备令牌.</param>
    private async Task ReleaseDeviceAsync(string apiBaseUrl, string deviceToken)
    {
        try
        {
            var url = ApiEndpoints.GetDeviceReleaseUrl(apiBaseUrl);
            _logger?.LogDebug("Releasing device: {Url}, Token: {Token}", url, deviceToken);

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
                request.Headers.Add(DeviceTokenHeader, deviceToken);
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = System.Text.Json.JsonSerializer.Deserialize(json, FanQieJsonContext.Default.DeviceReleaseResponse);

                if (result?.Code == 0)
                {
                    _logger?.LogDebug("Device released successfully.");
                }
                else
                {
                    _logger?.LogWarning("Device release failed: Code={Code}, Message={Message}", result?.Code, result?.Message);
                }
            }
            finally
            {
                _ = _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to release device at {Url}", apiBaseUrl);
        }
    }

    /// <summary>
    /// 获取或注册非章节 API 使用的设备令牌（客户端生命周期内缓存）.
    /// </summary>
    /// <param name="apiBaseUrl">API 基础 URL.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>设备令牌，可能为 null.</returns>
    private async Task<string?> GetOrRegisterCachedDeviceTokenAsync(string apiBaseUrl, CancellationToken cancellationToken)
    {
        var isBuiltIn = apiBaseUrl == FanQieClientOptions.BuiltInApiBaseUrl;
        var currentToken = isBuiltIn ? _builtInDeviceToken : _selfHostDeviceToken;

        if (!string.IsNullOrEmpty(currentToken))
        {
            return currentToken;
        }

        await _deviceLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // 双重检查
            currentToken = isBuiltIn ? _builtInDeviceToken : _selfHostDeviceToken;
            if (!string.IsNullOrEmpty(currentToken))
            {
                return currentToken;
            }

            var token = await RegisterDeviceAsync(apiBaseUrl, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(token))
            {
                if (isBuiltIn)
                {
                    _builtInDeviceToken = token;
                }
                else
                {
                    _selfHostDeviceToken = token;
                }
            }

            return token;
        }
        finally
        {
            _ = _deviceLock.Release();
        }
    }

    /// <summary>
    /// 从指定 API 使用范围请求获取批量章节内容.
    /// 注意：章节 API 每次调用都需要注册设备并在完成后释放.
    /// </summary>
    private async Task<IReadOnlyList<ChapterContent>> FetchBatchContentByRangeFromApiAsync(
        string apiBaseUrl,
        string chapterRange,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken)
    {
        // 章节 API 每次调用都注册设备
        var deviceToken = await RegisterDeviceAsync(apiBaseUrl, cancellationToken).ConfigureAwait(false);

        try
        {
            var url = ApiEndpoints.GetFallbackBatchContentUrl(apiBaseUrl);
            _logger?.LogDebug("Fetching batch content by range from API: {Url}, Range: {Range}", url, chapterRange);

            var requestBody = new Models.Internal.FallbackBatchContentRequest
            {
                BookId = bookId,
                ChapterRange = chapterRange,
            };

            var response = await PostWithDeviceTokenAsync<Models.Internal.FallbackBatchContentRequest, Models.Internal.FallbackBatchContentApiResponse>(
                url,
                requestBody,
                deviceToken,
                cancellationToken).ConfigureAwait(false);

            if (response.Code != 0)
            {
                throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get batch content failed.");
            }

            var results = new List<ChapterContent>();

            if (response.Data?.Chapters is null || response.Data.Chapters.Count == 0)
            {
                return results;
            }

            // 建立 order -> ChapterItem 的映射
            var orderToChapterMap = chapterInfoMap.Values.ToDictionary(c => c.Order.ToString(), c => c);

            // 按章节顺序处理返回的内容（服务端返回的 key 是章节序号）
            foreach (var kvp in response.Data.Chapters)
            {
                var orderKey = kvp.Key;  // 这是章节序号，如 "1", "2", "3"
                var chapter = kvp.Value;

                if (chapter is null)
                {
                    continue;
                }

                // 通过序号查找对应的章节信息
                orderToChapterMap.TryGetValue(orderKey, out var chapterInfo);
                var itemId = chapterInfo?.ItemId ?? orderKey;

                // 优先使用纯文本内容，如果有 HTML 内容则解析
                string textContent;
                string htmlContent;
                IReadOnlyList<ChapterImage>? images = null;

                if (!string.IsNullOrEmpty(chapter.RawContent))
                {
                    // 解析 HTML 内容
                    (images, textContent, htmlContent) = Helpers.ContentParser.ParseFallbackHtmlContent(chapter.RawContent, itemId);
                }
                else
                {
                    textContent = chapter.TxtContent ?? string.Empty;
                    htmlContent = $"<p>{System.Net.WebUtility.HtmlEncode(textContent).Replace("\n", "</p><p>", StringComparison.Ordinal)}</p>";
                }

                results.Add(new ChapterContent
                {
                    ItemId = itemId,
                    BookId = bookId,
                    BookTitle = bookTitle,
                    Title = chapter.ChapterName ?? chapterInfo?.Title ?? string.Empty,
                    TextContent = textContent,
                    HtmlContent = htmlContent,
                    WordCount = chapter.WordCount > 0 ? chapter.WordCount : Helpers.ContentParser.CountWords(textContent),
                    Order = chapterInfo?.Order ?? (int.TryParse(orderKey, out var o) ? o : 0),
                    VolumeName = chapterInfo?.VolumeName,
                    PublishTime = chapterInfo?.FirstPassTime,
                    Images = images,
                });
            }

            return results;
        }
        finally
        {
            // 章节 API 调用完成后释放设备
            if (!string.IsNullOrEmpty(deviceToken))
            {
                await ReleaseDeviceAsync(apiBaseUrl, deviceToken).ConfigureAwait(false);
            }
        }
    }

    private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _logger?.LogDebug("GET {Url}", url);
            var response = await _httpClient.GetAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var typeInfo = GetTypeInfo<T>();
            return System.Text.Json.JsonSerializer.Deserialize(json, typeInfo)
                ?? throw new Exceptions.FanQieParseException($"Failed to parse response as {typeof(T).Name}.");
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    private static System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> GetTypeInfo<T>()
    {
        if (typeof(T) == typeof(Models.Internal.SearchApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.SearchApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.BookDetailApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.BookDetailApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.BookTocApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.BookTocApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.BatchContentApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.BatchContentApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.CryptKeyApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.CryptKeyApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.FallbackSearchApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.FallbackSearchApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.FallbackBookDetailApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.FallbackBookDetailApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.FallbackBookTocApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.FallbackBookTocApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.FallbackBatchContentApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.FallbackBatchContentApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.DataResponse<Models.Internal.CommentCountData>))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.DataResponseCommentCountData;
        }

        if (typeof(T) == typeof(Models.Internal.DataResponse<Models.Internal.CommentListData>))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.DataResponseCommentListData;
        }

        throw new NotSupportedException($"Type {typeof(T).Name} is not supported.");
    }

    private void ConfigureHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");

        if (_options.CustomHeaders is not null)
        {
            foreach (var header in _options.CustomHeaders)
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private static BookItem MapToBookItem(Models.Internal.SearchBookItem item)
    {
        var creationStatus = BookCreationStatus.Ongoing;
        if (item.CreationStatus == "1")
        {
            creationStatus = BookCreationStatus.Completed;
        }

        return new BookItem
        {
            BookId = item.BookId ?? string.Empty,
            Title = item.Title ?? string.Empty,
            Author = item.Author,
            Abstract = item.Abstract,
            CoverUrl = item.ThumbUrl,
            Category = item.Category,
            Score = item.Score,
            CreationStatus = creationStatus,
        };
    }

    private static BookDetail MapToBookDetail(Models.Internal.BookDetailData data)
    {
        DateTimeOffset? lastUpdate = null;
        if (!string.IsNullOrEmpty(data.LastPublishTime) && long.TryParse(data.LastPublishTime, out var ts1))
        {
            lastUpdate = DateTimeOffset.FromUnixTimeSeconds(ts1);
        }

        DateTimeOffset? createTime = null;
        if (!string.IsNullOrEmpty(data.CreateTime) && DateTimeOffset.TryParse(data.CreateTime, out var parsedTime))
        {
            createTime = parsedTime;
        }

        var wordCount = 0;
        if (!string.IsNullOrEmpty(data.WordNumber) && int.TryParse(data.WordNumber, out var wc))
        {
            wordCount = wc;
        }

        var chapterCount = 0;
        if (!string.IsNullOrEmpty(data.SerialCountString) && int.TryParse(data.SerialCountString, out var sc))
        {
            chapterCount = sc;
        }

        var creationStatus = BookCreationStatus.Ongoing;
        if (!string.IsNullOrEmpty(data.CreationStatus) && int.TryParse(data.CreationStatus, out var cs))
        {
            creationStatus = (BookCreationStatus)cs;
        }

        var gender = BookGender.Unknown;
        if (!string.IsNullOrEmpty(data.Gender) && int.TryParse(data.Gender, out var g))
        {
            gender = (BookGender)g;
        }

        // 解析标签 - tags 可能是空字符串，实际标签从 category_v2 解析
        List<string> tags = [];
        if (!string.IsNullOrEmpty(data.CategoryV2))
        {
            try
            {
                var categories = System.Text.Json.JsonSerializer.Deserialize(data.CategoryV2, FanQieJsonContext.Default.ListCategoryV2Item);
                if (categories is not null)
                {
                    tags = categories.Select(c => c.Name ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).ToList();
                }
            }
            catch
            {
                // 忽略解析错误
            }
        }

        return new BookDetail
        {
            BookId = data.BookId ?? string.Empty,
            Title = data.BookName ?? string.Empty,
            Author = data.Author,
            AuthorId = data.AuthorId,
            Abstract = data.Abstract,
            CoverUrl = data.ThumbUrl,
            Category = data.Category,
            Tags = tags,
            WordCount = wordCount,
            ChapterCount = chapterCount,
            CreationStatus = creationStatus,
            Gender = gender,
            LastUpdateTime = lastUpdate,
            CreateTime = createTime,
            Score = data.Score,
        };
    }

    private static List<BookVolume> ParseToc(Models.Internal.BookTocData? data)
    {
        if (data?.ChapterListWithVolume is null || data.ChapterListWithVolume.Count == 0)
        {
            return [];
        }

        var volumes = new List<BookVolume>();
        var volumeNames = data.VolumeNameList ?? ["正文"];

        for (var vIndex = 0; vIndex < data.ChapterListWithVolume.Count; vIndex++)
        {
            var chapterList = data.ChapterListWithVolume[vIndex];
            var volumeName = vIndex < volumeNames.Count ? volumeNames[vIndex] : $"第{vIndex + 1}卷";

            var chapters = new List<ChapterItem>();
            foreach (var item in chapterList)
            {
                DateTimeOffset? firstPassTime = null;
                if (!string.IsNullOrEmpty(item.FirstPassTime) && long.TryParse(item.FirstPassTime, out var ts))
                {
                    firstPassTime = DateTimeOffset.FromUnixTimeSeconds(ts);
                }

                var order = 0;
                if (!string.IsNullOrEmpty(item.RealChapterOrder) && int.TryParse(item.RealChapterOrder, out var o))
                {
                    order = o;
                }

                chapters.Add(new ChapterItem
                {
                    ItemId = item.ItemId ?? string.Empty,
                    Title = item.Title ?? string.Empty,
                    Order = order,
                    VolumeName = volumeName,
                    // 番茄小说是免费平台，不存在锁定/付费章节
                    IsLocked = false,
                    NeedPay = false,
                    FirstPassTime = firstPassTime,
                });
            }

            volumes.Add(new BookVolume
            {
                Index = vIndex,
                Name = volumeName,
                Chapters = chapters,
            });
        }

        return volumes;
    }

    #region 第三方 API 实现

    /// <summary>
    /// 使用第三方 API 搜索（支持 SelfHost → 内置 API 回退）.
    /// </summary>
    private async Task<SearchResult<BookItem>> SearchBooksWithThirdPartyApiAsync(
        string query,
        int offset,
        CancellationToken cancellationToken)
    {
        // 如果有自部署 API，先尝试使用
        if (!string.IsNullOrEmpty(_options.SelfHostApiBaseUrl))
        {
            try
            {
                return await SearchBooksFromApiAsync(_options.SelfHostApiBaseUrl, query, offset, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger?.LogWarning(ex, "SelfHost API failed for search, falling back to built-in API...");
            }
        }

        // 使用内置 API
        return await SearchBooksFromApiAsync(FanQieClientOptions.BuiltInApiBaseUrl, query, offset, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 从指定 API 搜索书籍.
    /// </summary>
    private async Task<SearchResult<BookItem>> SearchBooksFromApiAsync(
        string apiBaseUrl,
        string query,
        int offset,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetFallbackSearchUrl(apiBaseUrl, query, offset);
        _logger?.LogDebug("Third-party search API: {Url}", url);

        var response = await GetAsync<Models.Internal.FallbackSearchApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Search failed.");
        }

        var items = response.Data?.Books?.Select(MapFallbackToBookItem).ToList() ?? [];

        return new SearchResult<BookItem>
        {
            Items = items,
            HasMore = response.Data?.HasMore ?? false,
            NextOffset = offset + items.Count,
            SearchId = response.Data?.SearchId,
        };
    }

    /// <summary>
    /// 使用第三方 API 获取书籍详情（支持 SelfHost → 内置 API 回退）.
    /// </summary>
    private async Task<BookDetail?> GetBookDetailWithThirdPartyApiAsync(
        string bookId,
        CancellationToken cancellationToken)
    {
        // 如果有自部署 API，先尝试使用
        if (!string.IsNullOrEmpty(_options.SelfHostApiBaseUrl))
        {
            try
            {
                return await GetBookDetailFromApiAsync(_options.SelfHostApiBaseUrl, bookId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger?.LogWarning(ex, "SelfHost API failed for book detail, falling back to built-in API...");
            }
        }

        // 使用内置 API
        return await GetBookDetailFromApiAsync(FanQieClientOptions.BuiltInApiBaseUrl, bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 从指定 API 获取书籍详情.
    /// </summary>
    private async Task<BookDetail?> GetBookDetailFromApiAsync(
        string apiBaseUrl,
        string bookId,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetFallbackBookDetailUrl(apiBaseUrl, bookId);
        _logger?.LogDebug("Third-party book detail API: {Url}", url);

        var response = await GetAsync<Models.Internal.FallbackBookDetailApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get book detail failed.");
        }

        if (response.Data is null)
        {
            return null;
        }

        return MapFallbackToBookDetail(response.Data);
    }

    /// <summary>
    /// 使用第三方 API 获取书籍目录（支持 SelfHost → 内置 API 回退）.
    /// </summary>
    private async Task<IReadOnlyList<BookVolume>> GetBookTocWithThirdPartyApiAsync(
        string bookId,
        CancellationToken cancellationToken)
    {
        // 如果有自部署 API，先尝试使用
        if (!string.IsNullOrEmpty(_options.SelfHostApiBaseUrl))
        {
            try
            {
                return await GetBookTocFromApiAsync(_options.SelfHostApiBaseUrl, bookId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger?.LogWarning(ex, "SelfHost API failed for book TOC, falling back to built-in API...");
            }
        }

        // 使用内置 API
        return await GetBookTocFromApiAsync(FanQieClientOptions.BuiltInApiBaseUrl, bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 从指定 API 获取书籍目录.
    /// </summary>
    private async Task<IReadOnlyList<BookVolume>> GetBookTocFromApiAsync(
        string apiBaseUrl,
        string bookId,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetFallbackBookTocUrl(apiBaseUrl, bookId);
        _logger?.LogDebug("Third-party book TOC API: {Url}", url);

        var response = await GetAsync<Models.Internal.FallbackBookTocApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get book TOC failed.");
        }

        return ParseFallbackToc(response.Data);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _logger?.LogDebug("POST {Url}", url);

            // 使用默认选项序列化，以确保 JsonPropertyName 特性生效（第三方 API 期望 camelCase）
            var json = System.Text.Json.JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri(url), content, cancellationToken).ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                throw new Exceptions.FanQieParseException($"Empty response from {url}. Status: {response.StatusCode}");
            }

            var typeInfo = GetTypeInfo<TResponse>();
            return System.Text.Json.JsonSerializer.Deserialize(responseJson, typeInfo)
                ?? throw new Exceptions.FanQieParseException($"Failed to parse response as {typeof(TResponse).Name}.");
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <summary>
    /// 发送带设备令牌的 POST 请求.
    /// </summary>
    private async Task<TResponse> PostWithDeviceTokenAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        string? deviceToken,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _logger?.LogDebug("POST {Url} (with device token: {HasToken})", url, !string.IsNullOrEmpty(deviceToken));

            // 使用默认选项序列化，以确保 JsonPropertyName 特性生效（第三方 API 期望 camelCase）
            var json = System.Text.Json.JsonSerializer.Serialize(request);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // 添加设备令牌头
            if (!string.IsNullOrEmpty(deviceToken))
            {
                httpRequest.Headers.Add(DeviceTokenHeader, deviceToken);
            }

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                throw new Exceptions.FanQieParseException($"Empty response from {url}. Status: {response.StatusCode}");
            }

            var typeInfo = GetTypeInfo<TResponse>();
            return System.Text.Json.JsonSerializer.Deserialize(responseJson, typeInfo)
                ?? throw new Exceptions.FanQieParseException($"Failed to parse response as {typeof(TResponse).Name}.");
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    private static BookItem MapFallbackToBookItem(Models.Internal.FallbackSearchBookItem item)
    {
        var creationStatus = BookCreationStatus.Ongoing;
        if (item.Status == "1")
        {
            creationStatus = BookCreationStatus.Completed;
        }

        return new BookItem
        {
            BookId = item.BookId ?? string.Empty,
            Title = item.BookName ?? string.Empty,
            Author = item.Author,
            Abstract = item.Description,
            CoverUrl = item.CoverUrl,
            Category = item.Category,
            Score = item.Rating > 0 ? item.Rating.ToString("F1") : null,
            CreationStatus = creationStatus,
        };
    }

    private static BookDetail MapFallbackToBookDetail(Models.Internal.FallbackBookDetailData data)
    {
        var wordCount = 0;
        if (!string.IsNullOrEmpty(data.WordNumber) && int.TryParse(data.WordNumber, out var wc))
        {
            wordCount = wc;
        }

        var creationStatus = data.Status == 1 ? BookCreationStatus.Completed : BookCreationStatus.Ongoing;

        List<string> tags = [];
        if (!string.IsNullOrEmpty(data.Tags))
        {
            tags = data.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        return new BookDetail
        {
            BookId = data.BookId ?? string.Empty,
            Title = data.BookName ?? string.Empty,
            Author = data.Author,
            Abstract = data.Description,
            CoverUrl = data.CoverUrl,
            Tags = tags,
            WordCount = wordCount,
            ChapterCount = data.TotalChapters,
            CreationStatus = creationStatus,
        };
    }

    private static List<BookVolume> ParseFallbackToc(Models.Internal.FallbackBookTocData? data)
    {
        if (data?.ItemDataList is null || data.ItemDataList.Count == 0)
        {
            return [];
        }

        // 后备 API 返回扁平的章节列表，需要按 volume_name 分组
        var volumeGroups = data.ItemDataList
            .GroupBy(c => c.VolumeName ?? "正文")
            .ToList();

        var volumes = new List<BookVolume>();
        var globalOrder = 0;

        for (var vIndex = 0; vIndex < volumeGroups.Count; vIndex++)
        {
            var group = volumeGroups[vIndex];
            var volumeName = group.Key;

            var chapters = new List<ChapterItem>();
            foreach (var item in group)
            {
                globalOrder++;
                DateTimeOffset? firstPassTime = null;
                if (item.FirstPassTime > 0)
                {
                    firstPassTime = DateTimeOffset.FromUnixTimeSeconds(item.FirstPassTime);
                }

                chapters.Add(new ChapterItem
                {
                    ItemId = item.ItemId ?? string.Empty,
                    Title = item.Title ?? string.Empty,
                    Order = globalOrder,
                    VolumeName = volumeName,
                    IsLocked = false,
                    NeedPay = false,
                    FirstPassTime = firstPassTime,
                });
            }

            volumes.Add(new BookVolume
            {
                Index = vIndex,
                Name = volumeName,
                Chapters = chapters,
            });
        }

        return volumes;
    }

    #endregion

    #region Comment Mapping

    private static Comment MapToComment(Models.Internal.CommentItem item)
    {
        Uri? avatar = null;
        if (!string.IsNullOrEmpty(item.UserInfo?.UserAvatar))
        {
            _ = Uri.TryCreate(item.UserInfo.UserAvatar, UriKind.Absolute, out avatar);
        }

        List<Uri>? pictures = null;
        if (item.ImageUrl is not null && item.ImageUrl.Count > 0)
        {
            pictures = item.ImageUrl
                .Where(url => !string.IsNullOrEmpty(url) && url.Length > 5)
                .Select(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null)
                .Where(uri => uri is not null)
                .Cast<Uri>()
                .ToList();
        }

        return new Comment
        {
            Id = item.CommentId ?? string.Empty,
            Content = item.Text ?? string.Empty,
            UserId = item.UserInfo?.UserId,
            UserName = item.UserInfo?.UserName,
            Avatar = avatar,
            IsAuthor = item.UserInfo?.IsAuthor ?? false,
            PublishTime = DateTimeOffset.FromUnixTimeSeconds(item.CreateTimestamp).LocalDateTime,
            LikeCount = item.DiggCount,
            ReplyCount = item.ReplyCount,
            Pictures = pictures,
        };
    }

    #endregion
}
