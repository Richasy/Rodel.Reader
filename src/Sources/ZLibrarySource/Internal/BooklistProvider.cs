// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Internal;

/// <summary>
/// 书单提供器实现.
/// </summary>
internal sealed class BooklistProvider : IBooklistProvider
{
    private readonly IZLibDispatcher _dispatcher;
    private readonly IHtmlParser _parser;
    private readonly Func<string> _getMirror;
    private readonly Func<bool> _isAuthenticated;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="BooklistProvider"/> 类的新实例.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="parser">HTML 解析器.</param>
    /// <param name="getMirror">获取镜像地址的委托.</param>
    /// <param name="isAuthenticated">获取是否已认证的委托.</param>
    /// <param name="logger">日志器.</param>
    public BooklistProvider(
        IZLibDispatcher dispatcher,
        IHtmlParser parser,
        Func<string> getMirror,
        Func<bool> isAuthenticated,
        ILogger logger)
    {
        _dispatcher = dispatcher;
        _parser = parser;
        _getMirror = getMirror;
        _isAuthenticated = isAuthenticated;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<Booklist>> SearchPublicAsync(
        string query,
        int page = 1,
        int pageSize = 10,
        SortOrder order = SortOrder.Popular,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());

        var mirror = _getMirror();
        var url = UrlBuilder.BuildBooklistSearchUrl(mirror, query, page, order, isPrivate: false);

        _logger.LogDebug("Searching public booklists: {Query}, page: {Page}", query, page);

        var html = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var (booklists, totalPages) = _parser.ParseBooklistResults(html, mirror);

        return new PagedResult<Booklist>
        {
            Items = booklists,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<Booklist>> SearchPrivateAsync(
        string query,
        int page = 1,
        int pageSize = 10,
        SortOrder order = SortOrder.Popular,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());

        var mirror = _getMirror();
        var url = UrlBuilder.BuildBooklistSearchUrl(mirror, query, page, order, isPrivate: true);

        _logger.LogDebug("Searching private booklists: {Query}, page: {Page}", query, page);

        var html = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var (booklists, totalPages) = _parser.ParseBooklistResults(html, mirror);

        return new PagedResult<Booklist>
        {
            Items = booklists,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<BookItem>> GetBooksInListAsync(
        string booklistId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());
        Guard.NotNullOrWhiteSpace(booklistId);

        var mirror = _getMirror();
        var url = $"{mirror}/papi/booklist/{booklistId}/get-books/{page}";

        _logger.LogDebug("Getting books in booklist: {BooklistId}, page: {Page}", booklistId, page);

        var json = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);

        var response = JsonSerializer.Deserialize(json, ZLibraryJsonContext.Default.BooklistApiResponse);
        if (response?.Books == null)
        {
            return new PagedResult<BookItem>
            {
                Items = [],
                CurrentPage = page,
                TotalPages = 1,
                PageSize = 0,
            };
        }

        var books = new List<BookItem>();
        foreach (var wrapper in response.Books)
        {
            if (wrapper.Book == null)
            {
                continue;
            }

            var book = wrapper.Book;
            var authors = book.Author?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToList();

            books.Add(new BookItem
            {
                Id = book.Id ?? "unknown",
                Name = book.Title ?? "Unknown",
                Isbn = book.Identifier,
                Url = book.Href != null ? $"{mirror}{book.Href}" : null,
                CoverUrl = book.Cover,
                Authors = authors,
                Publisher = book.Publisher,
                Year = book.Year,
                Language = book.Language,
                Extension = book.Extension,
                FileSize = book.FilesizeString,
                Rating = book.QualityScore,
            });
        }

        return new PagedResult<BookItem>
        {
            Items = books,
            CurrentPage = page,
            TotalPages = response.Pagination?.TotalPages ?? 1,
            PageSize = books.Count,
        };
    }
}
