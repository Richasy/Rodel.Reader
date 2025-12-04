// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Internal;

/// <summary>
/// HTTP 请求调度器.
/// </summary>
internal sealed class FanQieDispatcher : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly FanQieClientOptions _options;
    private readonly ILogger? _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly SemaphoreSlim _configLock = new(1, 1);
    private bool _disposed;

    // 外部 API 配置（延迟加载）
    private string? _externalApiBaseUrl;
    private bool _externalConfigLoaded;

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

        // 如果配置中已有外部 API 地址，直接使用
        if (!string.IsNullOrEmpty(options.ExternalApiBaseUrl))
        {
            _externalApiBaseUrl = options.ExternalApiBaseUrl;
            _externalConfigLoaded = true;
        }
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
            _logger?.LogWarning(ex, "Official search API failed, trying external API...");
            return await SearchBooksWithExternalApiAsync(query, offset, cancellationToken).ConfigureAwait(false);
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
            _logger?.LogWarning(ex, "Official book detail API failed, trying external API...");
            return await GetBookDetailWithExternalApiAsync(bookId, cancellationToken).ConfigureAwait(false);
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
            _logger?.LogWarning(ex, "Official book TOC API failed, trying external API...");
            return await GetBookTocWithExternalApiAsync(bookId, cancellationToken).ConfigureAwait(false);
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
        // 批量章节内容获取仅使用外部 API
        var externalApiUrl = await GetExternalApiBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(externalApiUrl))
        {
            throw new Exceptions.FanQieApiException(500, "External API is not available.");
        }

        return await FetchBatchContentFromExternalApiAsync(
            externalApiUrl,
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

        _semaphore.Dispose();
        _configLock.Dispose();
        _httpClient.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// 获取外部 API 基础 URL（延迟加载）.
    /// </summary>
    private async Task<string?> GetExternalApiBaseUrlAsync(CancellationToken cancellationToken)
    {
        if (_externalConfigLoaded)
        {
            return _externalApiBaseUrl;
        }

        await _configLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // 双重检查
            if (_externalConfigLoaded)
            {
                return _externalApiBaseUrl;
            }

            try
            {
                _logger?.LogDebug("Loading external API config from: {Url}", ApiEndpoints.ExternalConfigUrl);

                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var response = await _httpClient.GetAsync(new Uri(ApiEndpoints.ExternalConfigUrl), cancellationToken).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    var config = System.Text.Json.JsonSerializer.Deserialize(json, FanQieJsonContext.Default.ExternalRemoteConfig);

                    if (config?.Config?.ApiBaseUrl is not null)
                    {
                        _externalApiBaseUrl = config.Config.ApiBaseUrl;
                        _logger?.LogDebug("External API base URL loaded: {Url}", _externalApiBaseUrl);
                    }
                }
                finally
                {
                    _ = _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to load external API config.");
            }

            _externalConfigLoaded = true;
            return _externalApiBaseUrl;
        }
        finally
        {
            _ = _configLock.Release();
        }
    }

    /// <summary>
    /// 从外部 API 批量获取章节内容.
    /// </summary>
    private async Task<IReadOnlyList<ChapterContent>> FetchBatchContentFromExternalApiAsync(
        string apiBaseUrl,
        string chapterRange,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken)
    {
        // 解析范围获取章节列表
        var rangeParts = chapterRange.Split('-');
        if (rangeParts.Length != 2 || !int.TryParse(rangeParts[0], out var startOrder) || !int.TryParse(rangeParts[1], out var endOrder))
        {
            throw new ArgumentException($"Invalid chapter range format: {chapterRange}");
        }

        // 建立 order -> ChapterItem 的映射
        var orderToChapterMap = chapterInfoMap.Values.ToDictionary(c => c.Order, c => c);

        // 收集范围内所有章节的 itemId
        var itemIds = new List<string>();
        for (var order = startOrder; order <= endOrder; order++)
        {
            if (orderToChapterMap.TryGetValue(order, out var chapterInfo))
            {
                itemIds.Add(chapterInfo.ItemId);
            }
        }

        if (itemIds.Count == 0)
        {
            return [];
        }

        // 使用批量 API 获取内容
        var itemIdsStr = string.Join(",", itemIds);
        var url = ApiEndpoints.GetExternalBatchContentUrl(apiBaseUrl, bookId, itemIdsStr);
        _logger?.LogDebug("Fetching batch content from external API: {Url}", url);

        var response = await GetAsync<Models.Internal.ExternalBatchContentApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 200 || response.Data?.Chapters is null)
        {
            _logger?.LogWarning("External batch API returned error: Code={Code}, Message={Message}", response.Code, response.Message);
            return [];
        }

        var results = new List<ChapterContent>();

        foreach (var chapter in response.Data.Chapters)
        {
            if (chapter.Code != 0 || string.IsNullOrEmpty(chapter.Content))
            {
                _logger?.LogWarning("Chapter returned error: Code={Code}", chapter.Code);
                continue;
            }

            var itemId = chapter.NovelData?.ItemId ?? string.Empty;
            var chapterTitle = chapter.Title ?? chapter.NovelData?.Title ?? string.Empty;
            var volumeName = chapter.NovelData?.VolumeName ?? string.Empty;

            // 尝试获取 order
            var order = 0;
            if (!string.IsNullOrEmpty(chapter.NovelData?.RealChapterOrder) &&
                int.TryParse(chapter.NovelData.RealChapterOrder, out var parsedOrder))
            {
                order = parsedOrder;
            }
            else if (!string.IsNullOrEmpty(itemId) && chapterInfoMap.TryGetValue(itemId, out var existingChapter))
            {
                order = existingChapter.Order;
                volumeName = string.IsNullOrEmpty(volumeName) ? existingChapter.VolumeName : volumeName;
            }

            // 尝试获取字数
            var wordCount = 0;
            if (!string.IsNullOrEmpty(chapter.NovelData?.ChapterWordNumber) &&
                int.TryParse(chapter.NovelData.ChapterWordNumber, out var parsedWordCount))
            {
                wordCount = parsedWordCount;
            }

            // 尝试获取发布时间
            DateTimeOffset? publishTime = null;
            if (!string.IsNullOrEmpty(chapter.NovelData?.FirstPassTime) &&
                long.TryParse(chapter.NovelData.FirstPassTime, out var timestamp))
            {
                publishTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            }

            // 外部 API 返回纯文本，需要转换为 HTML
            var content = chapter.Content;
            var (images, textContent, htmlContent) = ParsePlainTextContent(content, itemId);

            if (wordCount == 0)
            {
                wordCount = Helpers.ContentParser.CountWords(textContent);
            }

            results.Add(new ChapterContent
            {
                ItemId = itemId,
                BookId = bookId,
                BookTitle = bookTitle,
                Title = chapterTitle,
                TextContent = textContent,
                HtmlContent = htmlContent,
                WordCount = wordCount,
                Order = order,
                VolumeName = volumeName,
                PublishTime = publishTime,
                Images = images,
            });
        }

        return results;
    }

    /// <summary>
    /// 解析纯文本内容为 HTML.
    /// </summary>
    private static (IReadOnlyList<ChapterImage>? Images, string CleanedText, string HtmlContent) ParsePlainTextContent(string content, string itemId)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return (null, string.Empty, string.Empty);
        }

        // 外部 API 返回的内容可能包含 HTML 实体编码的图片标签，先解码
        var decodedContent = System.Net.WebUtility.HtmlDecode(content);

        // 再次解码，因为可能有双重编码（如 &amp;amp; -> &amp; -> &）
        decodedContent = System.Net.WebUtility.HtmlDecode(decodedContent);

        // 使用 ParseContentWithImages 来提取图片，不移除第一行（外部 API 返回的内容没有标题行）
        return Helpers.ContentParser.ParseContentWithImages(decodedContent, itemId, removeFirstLine: false);
    }

    /// <summary>
    /// 从外部 API 获取单个章节内容.
    /// </summary>
    private async Task<ChapterContent?> FetchSingleChapterContentAsync(
        string apiBaseUrl,
        string itemId,
        string bookId,
        string bookTitle,
        ChapterItem chapterInfo,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetExternalChapterContentUrl(apiBaseUrl, itemId);
        _logger?.LogDebug("Fetching chapter content from external API: {Url}", url);

        var response = await GetAsync<Models.Internal.ExternalChapterContentApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 200 || response.Data is null)
        {
            _logger?.LogWarning("External API returned error: Code={Code}, Message={Message}", response.Code, response.Message);
            return null;
        }

        var content = response.Data.Content ?? string.Empty;

        // 外部 API 返回纯文本，需要解析
        var (images, textContent, htmlContent) = ParsePlainTextContent(content, itemId);

        return new ChapterContent
        {
            ItemId = itemId,
            BookId = bookId,
            BookTitle = bookTitle,
            Title = chapterInfo.Title,
            TextContent = textContent,
            HtmlContent = htmlContent,
            WordCount = Helpers.ContentParser.CountWords(textContent),
            Order = chapterInfo.Order,
            VolumeName = chapterInfo.VolumeName,
            PublishTime = chapterInfo.FirstPassTime,
            Images = images,
        };
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

        if (typeof(T) == typeof(Models.Internal.ExternalRemoteConfig))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalRemoteConfig;
        }

        if (typeof(T) == typeof(Models.Internal.ExternalSearchApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalSearchApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.ExternalBookDetailApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalBookDetailApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.ExternalBookTocApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalBookTocApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.ExternalChapterContentApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalChapterContentApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.ExternalBatchContentApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalBatchContentApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.ExternalFullBookApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalFullBookApiResponse;
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

    #region 外部 API 实现

    /// <summary>
    /// 使用外部 API 搜索.
    /// </summary>
    private async Task<SearchResult<BookItem>> SearchBooksWithExternalApiAsync(
        string query,
        int offset,
        CancellationToken cancellationToken)
    {
        var externalApiUrl = await GetExternalApiBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(externalApiUrl))
        {
            throw new Exceptions.FanQieApiException(500, "External API is not available.");
        }

        var url = ApiEndpoints.GetExternalSearchUrl(externalApiUrl, query, offset);
        _logger?.LogDebug("External search API: {Url}", url);

        var response = await GetAsync<Models.Internal.ExternalSearchApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 200)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Search failed.");
        }

        var items = response.Data?.Books?.Select(MapExternalToBookItem).ToList() ?? [];

        return new SearchResult<BookItem>
        {
            Items = items,
            HasMore = (response.Data?.HasMore ?? 0) == 1,
            NextOffset = offset + items.Count,
            SearchId = response.Data?.SearchId,
        };
    }

    /// <summary>
    /// 使用外部 API 获取书籍详情.
    /// </summary>
    private async Task<BookDetail?> GetBookDetailWithExternalApiAsync(
        string bookId,
        CancellationToken cancellationToken)
    {
        var externalApiUrl = await GetExternalApiBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(externalApiUrl))
        {
            throw new Exceptions.FanQieApiException(500, "External API is not available.");
        }

        var url = ApiEndpoints.GetExternalBookDetailUrl(externalApiUrl, bookId);
        _logger?.LogDebug("External book detail API: {Url}", url);

        var response = await GetAsync<Models.Internal.ExternalBookDetailApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 200)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get book detail failed.");
        }

        if (response.Data?.Data is null)
        {
            return null;
        }

        return MapExternalToBookDetail(response.Data.Data);
    }

    /// <summary>
    /// 使用外部 API 获取书籍目录.
    /// </summary>
    private async Task<IReadOnlyList<BookVolume>> GetBookTocWithExternalApiAsync(
        string bookId,
        CancellationToken cancellationToken)
    {
        var externalApiUrl = await GetExternalApiBaseUrlAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(externalApiUrl))
        {
            throw new Exceptions.FanQieApiException(500, "External API is not available.");
        }

        var url = ApiEndpoints.GetExternalBookTocUrl(externalApiUrl, bookId);
        _logger?.LogDebug("External book TOC API: {Url}", url);

        var response = await GetAsync<Models.Internal.ExternalBookTocApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 200)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get book TOC failed.");
        }

        return ParseExternalToc(response.Data?.Data);
    }

    private static BookItem MapExternalToBookItem(Models.Internal.ExternalSearchBookItem item)
    {
        var creationStatus = BookCreationStatus.Ongoing;
        if (item.CreationStatus == "1")
        {
            creationStatus = BookCreationStatus.Completed;
        }

        return new BookItem
        {
            BookId = item.BookId ?? string.Empty,
            Title = item.BookName ?? string.Empty,
            Author = item.Author,
            Abstract = item.Abstract,
            CoverUrl = item.ThumbUrl,
            Category = item.Category,
            Score = item.Score,
            CreationStatus = creationStatus,
        };
    }

    private static BookDetail MapExternalToBookDetail(Models.Internal.ExternalBookDetailData data)
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
        if (!string.IsNullOrEmpty(data.SerialCount) && int.TryParse(data.SerialCount, out var sc))
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

        // 解析标签
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

    private static List<BookVolume> ParseExternalToc(Models.Internal.ExternalBookTocData? data)
    {
        if (data?.ChapterListWithVolume is null || data.ChapterListWithVolume.Count == 0)
        {
            return [];
        }

        var volumes = new List<BookVolume>();
        var volumeNames = data.VolumeNameList ?? ["正文"];
        var globalOrder = 0;

        for (var vIndex = 0; vIndex < data.ChapterListWithVolume.Count; vIndex++)
        {
            var chapterList = data.ChapterListWithVolume[vIndex];
            var volumeName = vIndex < volumeNames.Count ? volumeNames[vIndex] : $"第{vIndex + 1}卷";

            var chapters = new List<ChapterItem>();
            foreach (var item in chapterList)
            {
                globalOrder++;
                DateTimeOffset? firstPassTime = null;
                var passTimeStr = item.GetFirstPassTime();
                if (!string.IsNullOrEmpty(passTimeStr) && long.TryParse(passTimeStr, out var ts))
                {
                    firstPassTime = DateTimeOffset.FromUnixTimeSeconds(ts);
                }

                chapters.Add(new ChapterItem
                {
                    ItemId = item.GetItemId() ?? string.Empty,
                    Title = item.Title ?? string.Empty,
                    Order = globalOrder,
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
