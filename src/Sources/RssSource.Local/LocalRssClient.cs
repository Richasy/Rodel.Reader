// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Sources.Rss.Local;

/// <summary>
/// 本地 RSS 客户端.
/// 管理本地存储的 RSS 订阅源，从网络获取文章内容.
/// </summary>
public sealed class LocalRssClient : IRssClient
{
    private readonly IRssStorage _storage;
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocalRssClient> _logger;
    private readonly LocalRssClientOptions _options;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalRssClient"/> class.
    /// </summary>
    /// <param name="storage">RSS 存储服务.</param>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端（可选，用于测试注入）.</param>
    /// <param name="logger">日志记录器.</param>
    public LocalRssClient(
        IRssStorage storage,
        LocalRssClientOptions? options = null,
        HttpClient? httpClient = null,
        ILogger<LocalRssClient>? logger = null)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _options = options ?? new LocalRssClientOptions();
        _logger = logger ?? NullLogger<LocalRssClient>.Instance;
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);

        _httpClient = httpClient ?? CreateDefaultHttpClient();
    }

    /// <inheritdoc/>
    public IRssSourceCapabilities Capabilities => KnownRssSources.Local;

    /// <inheritdoc/>
    public bool IsAuthenticated => true; // 本地源不需要认证

    /// <inheritdoc/>
    public Task<bool> SignInAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("本地 RSS 源不需要登录");
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<bool> SignOutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("本地 RSS 源不需要登出");
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RssFeedGroup> Groups, IReadOnlyList<RssFeed> Feeds)> GetFeedListAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("获取本地订阅源列表");

        var groups = await _storage.GetAllGroupsAsync(cancellationToken).ConfigureAwait(false);
        var feeds = await _storage.GetAllFeedsAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("获取到 {GroupCount} 个分组和 {FeedCount} 个订阅源", groups.Count, feeds.Count);

        return (groups, feeds);
    }

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("获取订阅源详情: {FeedName} ({FeedUrl})", feed.Name, feed.Url);

        try
        {
            var (channel, items) = await FetchFeedContentAsync(feed.Url, cancellationToken).ConfigureAwait(false);

            var articles = items.Select(item => ConvertToArticle(item, feed.Id)).ToList();

            var detail = new RssFeedDetail
            {
                Feed = new RssFeed
                {
                    Id = feed.Id,
                    Name = string.IsNullOrEmpty(channel.Title) ? feed.Name : channel.Title,
                    Url = feed.Url,
                    Website = channel.GetPrimaryLink()?.ToString() ?? feed.Website,
                    Description = channel.Description ?? feed.Description,
                    GroupIds = feed.GroupIds,
                    IconUrl = channel.GetPrimaryImage()?.ToString() ?? feed.IconUrl,
                },
                Articles = articles,
            };

            _logger.LogInformation("成功获取订阅源 {FeedName} 的 {ArticleCount} 篇文章", feed.Name, articles.Count);
            return detail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取订阅源 {FeedName} 失败", feed.Name);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RssFeedDetail>> GetFeedDetailListAsync(
        IEnumerable<RssFeed> feeds,
        CancellationToken cancellationToken = default)
    {
        var feedList = feeds.ToList();
        _logger.LogDebug("批量获取 {FeedCount} 个订阅源的详情", feedList.Count);

        var results = new List<RssFeedDetail>();
        var tasks = new List<Task>();

        foreach (var feed in feedList)
        {
            tasks.Add(FetchFeedWithSemaphoreAsync(feed, results, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        _logger.LogInformation("批量获取完成，成功获取 {SuccessCount}/{TotalCount} 个订阅源", results.Count, feedList.Count);
        return results;
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> AddGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("添加分组: {GroupName}", group.Name);

        if (string.IsNullOrEmpty(group.Id))
        {
            group.Id = $"folder/{Guid.NewGuid():N}";
        }

        await _storage.UpsertGroupAsync(group, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("分组 {GroupName} 添加成功", group.Name);
        return group;
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("更新分组: {GroupName}", group.Name);

        await _storage.UpsertGroupAsync(group, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("分组 {GroupName} 更新成功", group.Name);
        return group;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("删除分组: {GroupName}", group.Name);

        var result = await _storage.DeleteGroupAsync(group.Id, cancellationToken).ConfigureAwait(false);

        if (result)
        {
            _logger.LogInformation("分组 {GroupName} 删除成功", group.Name);
        }
        else
        {
            _logger.LogWarning("分组 {GroupName} 删除失败", group.Name);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<RssFeed?> AddFeedAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("添加订阅源: {FeedName} ({FeedUrl})", feed.Name, feed.Url);

        if (string.IsNullOrEmpty(feed.Id))
        {
            feed.Id = $"feed/{feed.Url}";
        }

        await _storage.UpsertFeedAsync(feed, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("订阅源 {FeedName} 添加成功", feed.Name);
        return feed;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFeedAsync(
        RssFeed newFeed,
        RssFeed oldFeed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(newFeed);

        _logger.LogDebug("更新订阅源: {FeedName}", newFeed.Name);

        await _storage.UpsertFeedAsync(newFeed, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("订阅源 {FeedName} 更新成功", newFeed.Name);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFeedAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("删除订阅源: {FeedName}", feed.Name);

        var result = await _storage.DeleteFeedAsync(feed.Id, cancellationToken).ConfigureAwait(false);

        if (result)
        {
            _logger.LogInformation("订阅源 {FeedName} 删除成功", feed.Name);
        }
        else
        {
            _logger.LogWarning("订阅源 {FeedName} 删除失败", feed.Name);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkArticlesAsReadAsync(
        IEnumerable<string> articleIds,
        CancellationToken cancellationToken = default)
    {
        var idList = articleIds.ToList();
        _logger.LogDebug("标记 {Count} 篇文章为已读", idList.Count);

        await _storage.MarkAsReadAsync(idList, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("成功标记 {Count} 篇文章为已读", idList.Count);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkFeedAsReadAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("将订阅源 {FeedName} 下的所有文章标记为已读", feed.Name);

        await _storage.MarkFeedAsReadAsync(feed.Id, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("订阅源 {FeedName} 的所有文章已标记为已读", feed.Name);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkGroupAsReadAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("将分组 {GroupName} 下的所有文章标记为已读", group.Name);

        // 获取分组下的所有订阅源
        var allFeeds = await _storage.GetAllFeedsAsync(cancellationToken).ConfigureAwait(false);
        var groupFeeds = allFeeds.Where(f => f.GroupIds?.Contains(group.Id, StringComparison.Ordinal) == true).ToList();

        foreach (var feed in groupFeeds)
        {
            await _storage.MarkFeedAsReadAsync(feed.Id, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation("分组 {GroupName} 的所有文章已标记为已读", group.Name);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ImportOpmlAsync(
        string opmlContent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("开始导入 OPML");

        try
        {
            var (groups, feeds) = OpmlHelper.ParseOpml(opmlContent);

            await _storage.UpsertGroupsAsync(groups, cancellationToken).ConfigureAwait(false);
            await _storage.UpsertFeedsAsync(feeds, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("OPML 导入成功，共导入 {GroupCount} 个分组和 {FeedCount} 个订阅源", groups.Count, feeds.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OPML 导入失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string> ExportOpmlAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("开始导出 OPML");

        var groups = await _storage.GetAllGroupsAsync(cancellationToken).ConfigureAwait(false);
        var feeds = await _storage.GetAllFeedsAsync(cancellationToken).ConfigureAwait(false);

        var opml = OpmlHelper.GenerateOpml(groups, feeds, "Local RSS Subscriptions");

        _logger.LogInformation("OPML 导出成功");
        return opml;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _httpClient.Dispose();
            _disposed = true;
        }
    }

    private HttpClient CreateDefaultHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = _options.Timeout,
        };

        if (!string.IsNullOrEmpty(_options.UserAgent))
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
        }
        else
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RodelReader", "1.0"));
        }

        return client;
    }

    private async Task FetchFeedWithSemaphoreAsync(
        RssFeed feed,
        List<RssFeedDetail> results,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var detail = await GetFeedDetailAsync(feed, cancellationToken).ConfigureAwait(false);
            if (detail != null)
            {
                lock (results)
                {
                    results.Add(detail);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<(FeedChannel Channel, IReadOnlyList<FeedItem> Items)> FetchFeedContentAsync(
        string feedUrl,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(new Uri(feedUrl), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = await Richasy.RodelReader.Utilities.FeedParser.Readers.FeedReader.CreateAsync(stream, cancellationToken).ConfigureAwait(false);

        var channel = await reader.ReadChannelAsync(cancellationToken).ConfigureAwait(false);
        var items = await reader.ReadAllItemsAsync(cancellationToken).ConfigureAwait(false);

        return (channel, items);
    }

    private static RssArticle ConvertToArticle(FeedItem item, string feedId)
    {
        var primaryLink = item.GetPrimaryLink();
        return new RssArticle
        {
            Id = item.Id ?? primaryLink?.ToString() ?? Guid.NewGuid().ToString("N"),
            FeedId = feedId,
            Title = item.Title ?? string.Empty,
            Url = primaryLink?.ToString(),
            Summary = item.Description,
            Content = item.Content,
            CoverUrl = item.ImageUrl,
            Author = item.Contributors.Count > 0 ? item.Contributors[0].Name : null,
            PublishTime = item.PublishedAt?.ToString("O"),
        };
    }
}
