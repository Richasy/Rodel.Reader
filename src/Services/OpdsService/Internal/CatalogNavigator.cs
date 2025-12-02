// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Internal;

/// <summary>
/// 目录导航器实现.
/// </summary>
internal sealed class CatalogNavigator : ICatalogNavigator
{
    private readonly IOpdsDispatcher _dispatcher;
    private readonly IOpdsParser _parser;
    private readonly OpdsClientOptions _options;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="CatalogNavigator"/> 类的新实例.
    /// </summary>
    /// <param name="dispatcher">HTTP 分发器.</param>
    /// <param name="parser">解析器.</param>
    /// <param name="options">客户端配置.</param>
    /// <param name="logger">日志器.</param>
    public CatalogNavigator(
        IOpdsDispatcher dispatcher,
        IOpdsParser parser,
        OpdsClientOptions options,
        ILogger logger)
    {
        _dispatcher = Guard.NotNull(dispatcher);
        _parser = Guard.NotNull(parser);
        _options = Guard.NotNull(options);
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public async Task<OpdsFeed> GetFeedAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(uri);

        _logger.LogDebug("Getting feed from {Uri}", uri);

        var stream = await _dispatcher.GetStreamAsync(uri, cancellationToken).ConfigureAwait(false);
        var feed = await _parser.ParseFeedAsync(stream, uri, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Retrieved feed '{Title}' with {EntryCount} entries", feed.Title, feed.Entries.Count);

        return feed;
    }

    /// <inheritdoc/>
    public Task<OpdsFeed> GetRootAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting root feed from {RootUri}", _options.RootUri);
        return GetFeedAsync(_options.RootUri, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<OpdsFeed?> GetNextPageAsync(OpdsFeed currentFeed, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(currentFeed);

        var nextLink = currentFeed.GetNextLink();
        if (nextLink == null)
        {
            _logger.LogDebug("No next page link found in feed '{Title}'", currentFeed.Title);
            return null;
        }

        _logger.LogDebug("Navigating to next page: {Uri}", nextLink.Href);
        return await GetFeedAsync(nextLink.Href, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<OpdsFeed?> GetPreviousPageAsync(OpdsFeed currentFeed, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(currentFeed);

        var prevLink = currentFeed.GetPreviousLink();
        if (prevLink == null)
        {
            _logger.LogDebug("No previous page link found in feed '{Title}'", currentFeed.Title);
            return null;
        }

        _logger.LogDebug("Navigating to previous page: {Uri}", prevLink.Href);
        return await GetFeedAsync(prevLink.Href, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<OpdsFeed?> NavigateToEntryAsync(OpdsEntry entry, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(entry);

        if (!entry.IsNavigationEntry)
        {
            _logger.LogDebug("Entry '{Title}' is not a navigation entry", entry.Title);
            return null;
        }

        var navLink = entry.GetNavigationLink();
        if (navLink == null)
        {
            _logger.LogWarning("Navigation entry '{Title}' has no navigation link", entry.Title);
            return null;
        }

        _logger.LogDebug("Navigating to entry '{Title}': {Uri}", entry.Title, navLink.Href);
        return await GetFeedAsync(navLink.Href, cancellationToken).ConfigureAwait(false);
    }
}
