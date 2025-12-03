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
    private string? _cryptKey;
    private string? _installId;
    private string? _deviceId;
    private bool _disposed;

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
        catch (Exception ex) when (_options.EnableFallback && ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Primary search API failed, trying fallback API...");
            return await SearchBooksWithFallbackAsync(query, offset, cancellationToken).ConfigureAwait(false);
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
        catch (Exception ex) when (_options.EnableFallback && ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Primary book detail API failed, trying fallback API...");
            return await GetBookDetailWithFallbackAsync(bookId, cancellationToken).ConfigureAwait(false);
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
        catch (Exception ex) when (_options.EnableFallback && ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Primary book TOC API failed, trying fallback API...");
            return await GetBookTocWithFallbackAsync(bookId, cancellationToken).ConfigureAwait(false);
        }
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
    public async Task<IReadOnlyList<ChapterContent>> GetBatchContentAsync(
        IEnumerable<string> itemIds,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken = default)
    {
        // 使用外部 API，不需要密钥初始化

        var idList = itemIds.ToList();
        var results = new List<ChapterContent>();

        // 分批处理
        for (var i = 0; i < idList.Count; i += _options.BatchSize)
        {
            var batch = idList.Skip(i).Take(_options.BatchSize).ToList();
            var batchContent = await FetchBatchAsync(batch, bookId, bookTitle, chapterInfoMap, cancellationToken).ConfigureAwait(false);
            results.AddRange(batchContent);

            // 请求间隔
            if (i + _options.BatchSize < idList.Count && _options.RequestDelayMs > 0)
            {
                await Task.Delay(_options.RequestDelayMs, cancellationToken).ConfigureAwait(false);
            }
        }

        return results;
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
        _httpClient.Dispose();
        _disposed = true;
    }

    private async Task<IReadOnlyList<ChapterContent>> FetchBatchAsync(
        List<string> itemIds,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken)
    {
        try
        {
            // 使用外部 API 获取批量内容
            var url = $"{ApiEndpoints.ExternalContent}?tab=批量&item_ids={string.Join(",", itemIds)}&book_id={bookId}";
            _logger?.LogDebug("Fetching batch content from external API: {Url}", url);

            var response = await GetAsync<Models.Internal.ExternalBatchContentApiResponse>(url, cancellationToken).ConfigureAwait(false);

            if (response.Code != 200)
            {
                throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get batch content failed.");
            }

            var results = new List<ChapterContent>();

            if (response.Data?.Chapters is null || response.Data.Chapters.Count == 0)
            {
                return results;
            }

            // 按照请求顺序处理章节
            for (var i = 0; i < itemIds.Count && i < response.Data.Chapters.Count; i++)
            {
                var itemId = itemIds[i];
                var chapter = response.Data.Chapters[i];

                if (chapter.Code != 0 || string.IsNullOrEmpty(chapter.Content))
                {
                    continue;
                }

                // 解析章节信息
                chapterInfoMap.TryGetValue(itemId, out var chapterInfo);

                // 解析内容中的图片并提取纯文本
                var (images, textContent, htmlContent) = Helpers.ContentParser.ParseContentWithImages(chapter.Content);

                results.Add(new ChapterContent
                {
                    ItemId = itemId,
                    BookId = bookId,
                    BookTitle = bookTitle,
                    Title = chapter.Title ?? chapterInfo?.Title ?? string.Empty,
                    TextContent = textContent,
                    HtmlContent = htmlContent,
                    WordCount = Helpers.ContentParser.CountWords(textContent),
                    Order = chapterInfo?.Order ?? 0,
                    VolumeName = chapterInfo?.VolumeName,
                    PublishTime = chapterInfo?.FirstPassTime,
                    Images = images,
                });
            }

            return results;
        }
        catch (Exception ex) when (_options.EnableFallback && ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Primary batch content API failed, trying fallback API...");
            return await FetchBatchWithFallbackAsync(itemIds, bookId, bookTitle, chapterInfoMap, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<ChapterContent?> FetchSingleChapterAsync(
        string itemId,
        string bookId,
        string bookTitle,
        ChapterItem? chapterInfo,
        CancellationToken cancellationToken)
    {
        // 使用外部 API 获取单个章节内容
        var url = $"{ApiEndpoints.ExternalContent}?tab=小说&item_id={itemId}";
        _logger?.LogDebug("Fetching single chapter from external API: {Url}", url);

        var response = await GetAsync<Models.Internal.ExternalContentApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 200)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get chapter content failed.");
        }

        if (response.Data?.Content is null)
        {
            return null;
        }

        // 解析内容中的图片并提取纯文本
        var (images, textContent, htmlContent) = Helpers.ContentParser.ParseContentWithImages(response.Data.Content);

        return new ChapterContent
        {
            ItemId = itemId,
            BookId = bookId,
            BookTitle = bookTitle,
            Title = chapterInfo?.Title ?? string.Empty,
            TextContent = textContent,
            HtmlContent = htmlContent,
            WordCount = Helpers.ContentParser.CountWords(textContent),
            Order = chapterInfo?.Order ?? 0,
            VolumeName = chapterInfo?.VolumeName,
            PublishTime = chapterInfo?.FirstPassTime,
            Images = images,
        };
    }

    private async Task InitializeCryptKeyAsync(CancellationToken cancellationToken)
    {
        _logger?.LogDebug("Initializing crypt key via external device register API...");

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // 调用外部设备注册 API
            var url = $"{ApiEndpoints.ExternalDeviceRegister}?platform=android";
            _logger?.LogDebug("Calling external device register API: {Url}", url);

            var response = await _httpClient.GetAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Device register response: {Response}", responseJson);

            var registerResponse = System.Text.Json.JsonSerializer.Deserialize(responseJson, FanQieJsonContext.Default.DeviceRegisterApiResponse)
                ?? throw new Exceptions.FanQieParseException("Failed to parse device register response.");

            if (registerResponse.Code != 200 || registerResponse.Data is null)
            {
                throw new Exceptions.FanQieApiException(registerResponse.Code, registerResponse.Message ?? "Failed to register device.");
            }

            // 保存设备信息
            _deviceId = registerResponse.Data.DeviceId;
            _installId = registerResponse.Data.InstallId;
            _cryptKey = registerResponse.Data.SecretKey;

            _logger?.LogDebug(
                "Device registered successfully. DeviceId: {DeviceId}, InstallId: {InstallId}",
                _deviceId,
                _installId);
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    private async Task<T> GetWithCookieAsync<T>(string url, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _logger?.LogDebug("GET (with cookie) {Url}", url);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // 优先使用动态获取的 installId，否则使用配置的
            var installId = _installId ?? _options.InstallId;
            if (!string.IsNullOrEmpty(installId))
            {
                request.Headers.Add("Cookie", $"install_id={installId}");
            }

            // 添加 Accept-Encoding 以接收压缩响应
            request.Headers.Add("Accept-Encoding", "gzip");

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // 获取原始字节数据
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Response bytes length: {Length}, first bytes: {FirstBytes}",
                bytes.Length,
                bytes.Length >= 10 ? BitConverter.ToString(bytes, 0, 10) : BitConverter.ToString(bytes));

            // 检查是否是 GZIP 压缩数据并解压
            string json;
            if (bytes.Length >= 2 && bytes[0] == 0x1f && bytes[1] == 0x8b)
            {
                _logger?.LogDebug("Detected GZIP compression, decompressing...");
                using var compressedStream = new MemoryStream(bytes);
                using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                using var resultStream = new MemoryStream();
                await gzipStream.CopyToAsync(resultStream, cancellationToken).ConfigureAwait(false);
                json = Encoding.UTF8.GetString(resultStream.ToArray());
            }
            else
            {
                json = Encoding.UTF8.GetString(bytes);
            }

            _logger?.LogDebug("Response JSON (first 500 chars): {Json}", json.Length > 500 ? json[..500] : json);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new Exceptions.FanQieApiException(-1, "Empty response received from API.");
            }

            var typeInfo = GetTypeInfo<T>();
            return System.Text.Json.JsonSerializer.Deserialize(json, typeInfo)
                ?? throw new Exceptions.FanQieParseException($"Failed to parse response as {typeof(T).Name}.");
        }
        finally
        {
            _ = _semaphore.Release();
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

        if (typeof(T) == typeof(Models.Internal.DeviceRegisterApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.DeviceRegisterApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.ExternalContentApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalContentApiResponse;
        }

        if (typeof(T) == typeof(Models.Internal.ExternalBatchContentApiResponse))
        {
            return (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)(object)FanQieJsonContext.Default.ExternalBatchContentApiResponse;
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

    #region 后备 API 实现

    private async Task<SearchResult<BookItem>> SearchBooksWithFallbackAsync(
        string query,
        int offset,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetFallbackSearchUrl(_options.FallbackApiBaseUrl, query, offset);
        _logger?.LogDebug("Fallback search API: {Url}", url);

        var response = await GetAsync<Models.Internal.FallbackSearchApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Fallback search failed.");
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

    private async Task<BookDetail?> GetBookDetailWithFallbackAsync(
        string bookId,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetFallbackBookDetailUrl(_options.FallbackApiBaseUrl, bookId);
        _logger?.LogDebug("Fallback book detail API: {Url}", url);

        var response = await GetAsync<Models.Internal.FallbackBookDetailApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Fallback get book detail failed.");
        }

        if (response.Data is null)
        {
            return null;
        }

        return MapFallbackToBookDetail(response.Data);
    }

    private async Task<IReadOnlyList<BookVolume>> GetBookTocWithFallbackAsync(
        string bookId,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetFallbackBookTocUrl(_options.FallbackApiBaseUrl, bookId);
        _logger?.LogDebug("Fallback book TOC API: {Url}", url);

        var response = await GetAsync<Models.Internal.FallbackBookTocApiResponse>(url, cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Fallback get book TOC failed.");
        }

        return ParseFallbackToc(response.Data);
    }

    private async Task<IReadOnlyList<ChapterContent>> FetchBatchWithFallbackAsync(
        List<string> itemIds,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetFallbackBatchContentUrl(_options.FallbackApiBaseUrl);
        _logger?.LogDebug("Fallback batch content API: {Url}", url);

        var requestBody = new Models.Internal.FallbackBatchContentRequest
        {
            BookId = bookId,
            ChapterIds = itemIds,
        };

        var response = await PostAsync<Models.Internal.FallbackBatchContentRequest, Models.Internal.FallbackBatchContentApiResponse>(
            url,
            requestBody,
            cancellationToken).ConfigureAwait(false);

        if (response.Code != 0)
        {
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Fallback get batch content failed.");
        }

        var results = new List<ChapterContent>();

        if (response.Data?.Chapters is null || response.Data.Chapters.Count == 0)
        {
            return results;
        }

        foreach (var itemId in itemIds)
        {
            if (!response.Data.Chapters.TryGetValue(itemId, out var chapter) || chapter is null)
            {
                continue;
            }

            chapterInfoMap.TryGetValue(itemId, out var chapterInfo);

            // 优先使用纯文本内容，如果有 HTML 内容则解析
            string textContent;
            string htmlContent;
            IReadOnlyList<ChapterImage>? images = null;

            if (!string.IsNullOrEmpty(chapter.RawContent))
            {
                // 解析 HTML 内容
                (images, textContent, htmlContent) = Helpers.ContentParser.ParseContentWithImages(chapter.RawContent);
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
                Order = chapterInfo?.Order ?? 0,
                VolumeName = chapterInfo?.VolumeName,
                PublishTime = chapterInfo?.FirstPassTime,
                Images = images,
            });
        }

        return results;
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

            // 使用默认选项序列化，以确保 JsonPropertyName 特性生效（后备 API 期望 camelCase）
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
