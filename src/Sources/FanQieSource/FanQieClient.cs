// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.FanQie.Abstractions;

namespace Richasy.RodelReader.Sources.FanQie;

/// <summary>
/// 番茄小说客户端.
/// </summary>
public sealed class FanQieClient : IFanQieClient
{
    private readonly Internal.FanQieDispatcher _dispatcher;
    private readonly ILogger<FanQieClient>? _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置（可选）.</param>
    /// <param name="httpClient">HTTP 客户端（可选）.</param>
    /// <param name="logger">日志记录器（可选）.</param>
    public FanQieClient(
        FanQieClientOptions? options = null,
        HttpClient? httpClient = null,
        ILogger<FanQieClient>? logger = null)
    {
        options ??= new FanQieClientOptions();
        _logger = logger;
        _dispatcher = new Internal.FanQieDispatcher(options, httpClient, logger);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<BookItem>> SearchBooksAsync(
        string query,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(query);
        Helpers.Guard.NonNegative(offset);

        _logger?.LogDebug("Searching books with query: {Query}, offset: {Offset}", query, offset);
        return await _dispatcher.SearchBooksAsync(query, offset, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<BookDetail?> GetBookDetailAsync(
        string bookId,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(bookId);

        _logger?.LogDebug("Getting book detail for: {BookId}", bookId);
        return await _dispatcher.GetBookDetailAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BookVolume>> GetBookTocAsync(
        string bookId,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(bookId);

        _logger?.LogDebug("Getting book TOC for: {BookId}", bookId);
        return await _dispatcher.GetBookTocAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ChapterContent?> GetChapterContentAsync(
        string bookId,
        string bookTitle,
        ChapterItem chapter,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(bookId);
        Helpers.Guard.NotNullOrEmpty(bookTitle);
        Helpers.Guard.NotNull(chapter);

        // 单章节使用范围请求，范围为 "order-order"
        var chapterRange = $"{chapter.Order}-{chapter.Order}";
        var chapterInfoMap = new Dictionary<string, ChapterItem> { { chapter.ItemId, chapter } };

        var results = await _dispatcher.GetBatchContentByRangeAsync(
            chapterRange,
            bookId,
            bookTitle,
            chapterInfoMap,
            cancellationToken).ConfigureAwait(false);

        return results.Count > 0 ? results[0] : null;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChapterContent>> GetChapterContentsAsync(
        string bookId,
        string bookTitle,
        IEnumerable<ChapterItem> chapters,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(bookId);
        Helpers.Guard.NotNullOrEmpty(bookTitle);
        Helpers.Guard.NotNull(chapters);

        var chapterList = chapters.ToList();
        if (chapterList.Count == 0)
        {
            return [];
        }

        var chapterInfoMap = chapterList.ToDictionary(c => c.ItemId, c => c);

        // 使用范围请求
#pragma warning disable CS0618 // Type or member is obsolete
        return await _dispatcher.GetBatchContentAsync(
            chapterList.Select(c => c.ItemId),
            bookId,
            bookTitle,
            chapterInfoMap,
            cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChapterContent>> GetChapterContentsByRangeAsync(
        string bookId,
        string bookTitle,
        string chapterRange,
        Dictionary<string, ChapterItem> chapterInfoMap,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(bookId);
        Helpers.Guard.NotNullOrEmpty(bookTitle);
        Helpers.Guard.NotNullOrEmpty(chapterRange);
        Helpers.Guard.NotNull(chapterInfoMap);

        _logger?.LogDebug("Getting chapter contents by range: {Range}", chapterRange);

        return await _dispatcher.GetBatchContentByRangeAsync(
            chapterRange,
            bookId,
            bookTitle,
            chapterInfoMap,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<(BookDetail Detail, IReadOnlyList<ChapterContent> Chapters)> DownloadBookAsync(
        string bookId,
        IProgress<(int Current, int Total)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(bookId);

        _logger?.LogInformation("Starting download for book: {BookId}", bookId);

        // 1. 获取书籍详情
        var detail = await GetBookDetailAsync(bookId, cancellationToken).ConfigureAwait(false)
            ?? throw new Exceptions.FanQieApiException(404, $"Book not found: {bookId}");

        // 2. 获取目录
        var volumes = await GetBookTocAsync(bookId, cancellationToken).ConfigureAwait(false);

        // 3. 获取所有章节（番茄小说是免费平台，所有章节都可下载）
        var allChapters = volumes
            .SelectMany(v => v.Chapters)
            .ToList();

        if (allChapters.Count == 0)
        {
            _logger?.LogWarning("No chapters found for book: {BookId}", bookId);
            return (detail, []);
        }

        _logger?.LogInformation("Found {Count} chapters to download.", allChapters.Count);

        // 4. 使用范围请求批量下载
        var allContents = new List<ChapterContent>();
        var chapterInfoMap = allChapters.ToDictionary(c => c.ItemId, c => c);
        var total = allChapters.Count;
        var batchSize = 25;

        for (var i = 0; i < total; i += batchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startOrder = allChapters[i].Order;
            var endIndex = Math.Min(i + batchSize - 1, total - 1);
            var endOrder = allChapters[endIndex].Order;
            var chapterRange = $"{startOrder}-{endOrder}";

            var contents = await _dispatcher.GetBatchContentByRangeAsync(
                chapterRange,
                bookId,
                detail.Title,
                chapterInfoMap,
                cancellationToken).ConfigureAwait(false);

            allContents.AddRange(contents);
            progress?.Report((Math.Min(i + batchSize, total), total));

            _logger?.LogDebug("Downloaded {Current}/{Total} chapters.", Math.Min(i + batchSize, total), total);
        }

        // 5. 按章节顺序排序
        var orderedContents = allContents.OrderBy(c => c.Order).ToList();

        _logger?.LogInformation("Download completed. Total chapters: {Count}", orderedContents.Count);

        return (detail, orderedContents);
    }

    /// <inheritdoc/>
    public async Task<byte[]> DownloadImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(imageUrl);

        _logger?.LogDebug("Downloading image: {Url}", imageUrl);
        return await _dispatcher.DownloadImageAsync(imageUrl, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, byte[]>> DownloadImagesAsync(
        IEnumerable<string> imageUrls,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNull(imageUrls);

        var urlList = imageUrls.ToList();
        if (urlList.Count == 0)
        {
            return new Dictionary<string, byte[]>();
        }

        _logger?.LogDebug("Downloading {Count} images", urlList.Count);
        return await _dispatcher.DownloadImagesAsync(urlList, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, int>?> GetCommentCountAsync(
        string bookId,
        string chapterId,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(bookId);
        Helpers.Guard.NotNullOrEmpty(chapterId);

        _logger?.LogDebug("Getting comment count for book: {BookId}, chapter: {ChapterId}", bookId, chapterId);
        return await _dispatcher.GetCommentCountAsync(bookId, chapterId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<CommentListResult?> GetCommentsAsync(
        string bookId,
        string chapterId,
        int paragraphIndex,
        string? offset = null,
        CancellationToken cancellationToken = default)
    {
        Helpers.Guard.NotNullOrEmpty(bookId);
        Helpers.Guard.NotNullOrEmpty(chapterId);
        Helpers.Guard.NonNegative(paragraphIndex);

        _logger?.LogDebug(
            "Getting comments for book: {BookId}, chapter: {ChapterId}, paragraph: {ParagraphIndex}",
            bookId, chapterId, paragraphIndex);
        return await _dispatcher.GetCommentsAsync(bookId, chapterId, paragraphIndex, offset, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _dispatcher.Dispose();
        _disposed = true;
    }
}
