// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.Legado.Helpers;
using Richasy.RodelReader.Sources.Legado.Internal;

namespace Richasy.RodelReader.Sources.Legado;

/// <summary>
/// Legado 客户端实现.
/// </summary>
public sealed class LegadoClient : ILegadoClient
{
    private readonly LegadoDispatcher _dispatcher;
    private readonly ILogger<LegadoClient> _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置.</param>
    public LegadoClient(LegadoClientOptions options)
        : this(options, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置.</param>
    /// <param name="logger">日志记录器.</param>
    public LegadoClient(LegadoClientOptions options, ILogger<LegadoClient>? logger)
        : this(options, null, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置.</param>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志记录器.</param>
    public LegadoClient(LegadoClientOptions options, HttpClient? httpClient, ILogger<LegadoClient>? logger)
    {
        Options = Guard.NotNull(options);
        _logger = logger ?? NullLogger<LegadoClient>.Instance;

        _logger.LogInformation("Initializing LegadoClient for {BaseUrl} (ServerType: {ServerType})",
            options.BaseUrl, options.ServerType);

        _dispatcher = new LegadoDispatcher(options, httpClient, _logger);

        _logger.LogInformation("LegadoClient initialized successfully");
    }

    /// <inheritdoc/>
    public LegadoClientOptions Options { get; }

    #region 书架管理

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> GetBookshelfAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        _logger.LogDebug("Getting bookshelf");

        var response = await _dispatcher.GetAsync(
            ApiEndpoints.GetBookshelf,
            LegadoJsonContext.Default.ApiResponseListBook,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var books = UnwrapResponse(response);
        _logger.LogInformation("Retrieved {Count} books from bookshelf", books?.Count ?? 0);

        return books ?? [];
    }

    /// <inheritdoc/>
    public async Task SaveBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNull(book);

        _logger.LogDebug("Saving book: {BookName} by {Author}", book.Name, book.Author);

        await _dispatcher.PostAsync(
            ApiEndpoints.SaveBook,
            book,
            LegadoJsonContext.Default.Book,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Book saved: {BookName}", book.Name);
    }

    /// <inheritdoc/>
    public async Task DeleteBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNull(book);

        _logger.LogDebug("Deleting book: {BookName}", book.Name);

        await _dispatcher.PostAsync(
            ApiEndpoints.DeleteBook,
            book,
            LegadoJsonContext.Default.Book,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Book deleted: {BookName}", book.Name);
    }

    #endregion

    #region 章节操作

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Chapter>> GetChapterListAsync(string bookUrl, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNullOrEmpty(bookUrl);

        _logger.LogDebug("Getting chapter list for book: {BookUrl}", bookUrl);

        var response = await _dispatcher.GetAsync(
            ApiEndpoints.GetChapterList,
            LegadoJsonContext.Default.ApiResponseListChapter,
            new Dictionary<string, string> { { "url", bookUrl } },
            cancellationToken).ConfigureAwait(false);

        var chapters = UnwrapResponse(response);
        _logger.LogInformation("Retrieved {Count} chapters for book", chapters?.Count ?? 0);

        return chapters?.OrderBy(c => c.Index).ToList() ?? [];
    }

    /// <inheritdoc/>
    public async Task<ChapterContent> GetChapterContentAsync(string bookUrl, int chapterIndex, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNullOrEmpty(bookUrl);
        Guard.NonNegative(chapterIndex);

        _logger.LogDebug("Getting content for chapter {Index} of book: {BookUrl}", chapterIndex, bookUrl);

        var response = await _dispatcher.GetAsync(
            ApiEndpoints.GetBookContent,
            LegadoJsonContext.Default.ApiResponseString,
            new Dictionary<string, string>
            {
                { "url", bookUrl },
                { "index", chapterIndex.ToString() },
            },
            cancellationToken).ConfigureAwait(false);

        var rawContent = UnwrapResponse(response);
        var htmlContent = ContentFormatter.ConvertToHtml(rawContent);

        _logger.LogDebug("Retrieved content for chapter {Index}, length: {Length}", chapterIndex, htmlContent.Length);

        return new ChapterContent
        {
            BookUrl = bookUrl,
            ChapterIndex = chapterIndex,
            Content = htmlContent,
        };
    }

    #endregion

    #region 进度同步

    /// <inheritdoc/>
    public async Task SaveProgressAsync(BookProgress progress, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNull(progress);

        _logger.LogDebug("Saving progress for book: {BookName}, chapter: {ChapterIndex}",
            progress.Name, progress.DurChapterIndex);

        await _dispatcher.PostAsync(
            ApiEndpoints.SaveBookProgress,
            progress,
            LegadoJsonContext.Default.BookProgress,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Progress saved for book: {BookName}", progress.Name);
    }

    #endregion

    #region 书源管理

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BookSource>> GetBookSourcesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        _logger.LogDebug("Getting all book sources");

        var response = await _dispatcher.GetAsync(
            ApiEndpoints.GetBookSources,
            LegadoJsonContext.Default.ApiResponseListBookSource,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var sources = UnwrapResponse(response);
        _logger.LogInformation("Retrieved {Count} book sources", sources?.Count ?? 0);

        return sources ?? [];
    }

    /// <inheritdoc/>
    public async Task<BookSource?> GetBookSourceAsync(string sourceUrl, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNullOrEmpty(sourceUrl);

        _logger.LogDebug("Getting book source: {SourceUrl}", sourceUrl);

        var response = await _dispatcher.GetAsync(
            ApiEndpoints.GetBookSource,
            LegadoJsonContext.Default.ApiResponseBookSource,
            new Dictionary<string, string> { { "url", sourceUrl } },
            cancellationToken).ConfigureAwait(false);

        var source = UnwrapResponse(response);
        _logger.LogDebug("Book source retrieved: {SourceName}", source?.BookSourceName ?? "null");

        return source;
    }

    /// <inheritdoc/>
    public async Task SaveBookSourceAsync(BookSource source, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNull(source);

        _logger.LogDebug("Saving book source: {SourceName}", source.BookSourceName);

        await _dispatcher.PostAsync(
            ApiEndpoints.SaveBookSource,
            source,
            LegadoJsonContext.Default.BookSource,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Book source saved: {SourceName}", source.BookSourceName);
    }

    /// <inheritdoc/>
    public async Task SaveBookSourcesAsync(IEnumerable<BookSource> sources, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNull(sources);

        var sourceList = sources.ToList();
        _logger.LogDebug("Saving {Count} book sources", sourceList.Count);

        await _dispatcher.PostAsync(
            ApiEndpoints.SaveBookSources,
            sourceList,
            LegadoJsonContext.Default.ListBookSource,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Saved {Count} book sources", sourceList.Count);
    }

    /// <inheritdoc/>
    public async Task DeleteBookSourcesAsync(IEnumerable<BookSource> sources, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNull(sources);

        var sourceList = sources.ToList();
        _logger.LogDebug("Deleting {Count} book sources", sourceList.Count);

        await _dispatcher.PostAsync(
            ApiEndpoints.DeleteBookSources,
            sourceList,
            LegadoJsonContext.Default.ListBookSource,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted {Count} book sources", sourceList.Count);
    }

    #endregion

    #region 封面

    /// <inheritdoc/>
    public async Task<Stream> GetCoverAsync(string coverPath, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Guard.NotNullOrEmpty(coverPath);

        _logger.LogDebug("Getting cover: {CoverPath}", coverPath);

        var stream = await _dispatcher.GetStreamAsync(
            ApiEndpoints.Cover,
            new Dictionary<string, string> { { "path", coverPath } },
            cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Cover retrieved successfully");
        return stream;
    }

    /// <inheritdoc/>
    public Uri GetCoverUri(string coverPath)
    {
        Guard.NotNullOrEmpty(coverPath);

        var url = ApiEndpoints.BuildUrl(
            Options.BaseUrl,
            ApiEndpoints.Cover,
            Options.ServerType,
            Options.AccessToken,
            new Dictionary<string, string> { { "path", coverPath } });
        return new Uri(url);
    }

    #endregion

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _dispatcher.Dispose();
        _disposed = true;
        _logger.LogDebug("LegadoClient disposed");
    }

    private static T? UnwrapResponse<T>(ApiResponse<T> response)
    {
        if (!response.IsSuccess)
        {
            throw new Exceptions.LegadoApiException(
                response.ErrorMsg ?? "Unknown error",
                response.ErrorMsg ?? "API request failed",
                isApiError: true);
        }

        return response.Data;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
