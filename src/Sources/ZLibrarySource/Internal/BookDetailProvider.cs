// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Internal;

/// <summary>
/// 书籍详情提供器实现.
/// </summary>
internal sealed class BookDetailProvider : IBookDetailProvider
{
    private readonly IZLibDispatcher _dispatcher;
    private readonly IHtmlParser _parser;
    private readonly Func<string> _getMirror;
    private readonly Func<bool> _isAuthenticated;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="BookDetailProvider"/> 类的新实例.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="parser">HTML 解析器.</param>
    /// <param name="getMirror">获取镜像地址的委托.</param>
    /// <param name="isAuthenticated">获取是否已认证的委托.</param>
    /// <param name="logger">日志器.</param>
    public BookDetailProvider(
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
    public async Task<BookDetail> GetByIdAsync(string bookId, CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());
        Guard.NotNullOrWhiteSpace(bookId);

        var mirror = _getMirror();
        var url = $"{mirror}/book/{bookId}";

        _logger.LogDebug("Getting book detail by ID: {BookId}", bookId);

        return await GetByUrlAsync(url, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<BookDetail> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());
        Guard.NotNullOrWhiteSpace(url);

        _logger.LogDebug("Getting book detail by URL: {Url}", url);

        var mirror = _getMirror();
        var html = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);

        try
        {
            return _parser.ParseBookDetail(html, url, mirror);
        }
        catch (ParseException)
        {
            throw new BookNotFoundException(url);
        }
    }
}
