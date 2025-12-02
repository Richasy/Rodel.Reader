// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Internal;

/// <summary>
/// 用户配置提供器实现.
/// </summary>
internal sealed class ProfileProvider : IProfileProvider
{
    private readonly IZLibDispatcher _dispatcher;
    private readonly IHtmlParser _parser;
    private readonly Func<string> _getMirror;
    private readonly Func<bool> _isAuthenticated;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="ProfileProvider"/> 类的新实例.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="parser">HTML 解析器.</param>
    /// <param name="getMirror">获取镜像地址的委托.</param>
    /// <param name="isAuthenticated">获取是否已认证的委托.</param>
    /// <param name="logger">日志器.</param>
    public ProfileProvider(
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
    public async Task<DownloadLimits> GetDownloadLimitsAsync(CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());

        var mirror = _getMirror();
        var url = $"{mirror}/users/downloads";

        _logger.LogDebug("Getting download limits");

        var html = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);
        return _parser.ParseDownloadLimits(html);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<DownloadHistoryItem>> GetDownloadHistoryAsync(
        int page = 1,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        Guard.IsAuthenticated(_isAuthenticated());

        var mirror = _getMirror();
        var url = UrlBuilder.BuildDownloadHistoryUrl(mirror, page, fromDate, toDate);

        _logger.LogDebug("Getting download history, page: {Page}", page);

        var html = await _dispatcher.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var items = _parser.ParseDownloadHistory(html, mirror);

        return new PagedResult<DownloadHistoryItem>
        {
            Items = items,
            CurrentPage = page,
            TotalPages = 1, // 下载历史页面没有总页数信息
            PageSize = items.Count,
        };
    }
}
