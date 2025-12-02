// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Internal;

/// <summary>
/// 搜索提供器实现.
/// </summary>
internal sealed class SearchProvider : ISearchProvider
{
    private readonly IZLibDispatcher _dispatcher;
    private readonly IHtmlParser _parser;
    private readonly Func<string> _getMirror;
    private readonly Func<bool> _isAuthenticated;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="SearchProvider"/> 类的新实例.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="parser">HTML 解析器.</param>
    /// <param name="getMirror">获取镜像地址的委托.</param>
    /// <param name="isAuthenticated">获取是否已认证的委托.</param>
    /// <param name="logger">日志器.</param>
    public SearchProvider(
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
    public async Task<PagedResult<BookItem>> SearchAsync(
        string query,
        int page = 1,
        BookSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());

        if (string.IsNullOrWhiteSpace(query))
        {
            throw new EmptyQueryException();
        }

        var mirror = _getMirror();
        var url = UrlBuilder.BuildSearchUrl(mirror, query, page, options);

        _logger.LogDebug("Searching books: {Query}, page: {Page}", query, page);

        var html = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var (books, totalPages) = _parser.ParseSearchResults(html, mirror);

        return new PagedResult<BookItem>
        {
            Items = books,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = options?.PageSize ?? 10,
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<BookItem>> FullTextSearchAsync(
        string query,
        int page = 1,
        FullTextSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());

        if (string.IsNullOrWhiteSpace(query))
        {
            throw new EmptyQueryException();
        }

        options ??= new FullTextSearchOptions { MatchWords = true };

        if (!options.MatchPhrase && !options.MatchWords)
        {
            throw new ArgumentException("You must specify either MatchPhrase or MatchWords.");
        }

        if (options.MatchPhrase)
        {
            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2)
            {
                throw new ArgumentException("At least 2 words must be provided for phrase search. Use MatchWords for single word search.");
            }
        }

        var mirror = _getMirror();
        var url = UrlBuilder.BuildFullTextSearchUrl(mirror, query, page, options);

        _logger.LogDebug("Full text searching books: {Query}, page: {Page}", query, page);

        var html = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var (books, totalPages) = _parser.ParseSearchResults(html, mirror);

        return new PagedResult<BookItem>
        {
            Items = books,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = options.PageSize,
        };
    }
}
