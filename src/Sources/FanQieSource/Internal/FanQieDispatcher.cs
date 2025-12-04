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
        // 章节内容获取优先级：SelfHost API（如有）→ 内置 API

        var idList = itemIds.ToList();
        var results = new List<ChapterContent>();

        // 分批处理
        for (var i = 0; i < idList.Count; i += _options.BatchSize)
        {
            var batch = idList.Skip(i).Take(_options.BatchSize).ToList();
            var batchContent = await FetchBatchContentAsync(batch, bookId, bookTitle, chapterInfoMap, cancellationToken).ConfigureAwait(false);
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
    /// 获取章节内容（支持 SelfHost → 内置 API 回退）.
    /// </summary>
    private async Task<IReadOnlyList<ChapterContent>> FetchBatchContentAsync(
        List<string> itemIds,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken)
    {
        // 如果有自部署 API，先尝试使用
        if (!string.IsNullOrEmpty(_options.SelfHostApiBaseUrl))
        {
            try
            {
                return await FetchBatchContentFromApiAsync(
                    _options.SelfHostApiBaseUrl,
                    itemIds,
                    bookId,
                    bookTitle,
                    chapterInfoMap,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger?.LogWarning(ex, "SelfHost API failed for batch content, falling back to built-in API...");
            }
        }

        // 使用内置 API
        return await FetchBatchContentFromApiAsync(
            FanQieClientOptions.BuiltInApiBaseUrl,
            itemIds,
            bookId,
            bookTitle,
            chapterInfoMap,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 从指定 API 获取批量章节内容.
    /// </summary>
    private async Task<IReadOnlyList<ChapterContent>> FetchBatchContentFromApiAsync(
        string apiBaseUrl,
        List<string> itemIds,
        string bookId,
        string bookTitle,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.GetFallbackBatchContentUrl(apiBaseUrl);
        _logger?.LogDebug("Fetching batch content from API: {Url}", url);

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
            throw new Exceptions.FanQieApiException(response.Code, response.Message ?? "Get batch content failed.");
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
                Order = chapterInfo?.Order ?? 0,
                VolumeName = chapterInfo?.VolumeName,
                PublishTime = chapterInfo?.FirstPassTime,
                Images = images,
            });
        }

        return results;
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
