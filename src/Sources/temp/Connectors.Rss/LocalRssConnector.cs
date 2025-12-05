// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.ReaderKernel.Models.Config;
using Richasy.ReaderKernel.Models.Rss;
using Richasy.ReaderKernel.Models.Rss.Opml;
using Richasy.ReaderKernel.Services;

namespace Richasy.ReaderKernel.Connectors.Rss;

/// <summary>
/// 本地 RSS 服务.
/// </summary>
public sealed class LocalRssConnector : IRssConnector
{
    private readonly HttpClient _httpClient;
    private readonly IRssDataService _dataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalRssConnector"/> class.
    /// </summary>
    public LocalRssConnector(IRssDataService dataService)
    {
        _dataService = dataService;
        _httpClient = Utils.GetHttpClient();
    }

    /// <inheritdoc/>
    public async Task<(List<RssFeedGroup> Groups, List<RssFeed> Feeds)> GetFeedListAsync(CancellationToken cancellationToken = default)
    {
        var cache = await _dataService.GetFeedCacheAsync(ReaderKernel.Models.RssConnectorType.Local).ConfigureAwait(false);
        return cache != null
            ? (cache.Groups, cache.Feeds)
            : (new List<RssFeedGroup>(), new List<RssFeed>());
    }

    /// <inheritdoc/>
    public Task<RssFeed?> AddFeedAsync(RssFeed feed)
    {
        if (string.IsNullOrEmpty(feed.Id))
        {
            feed.Id = $"feed/{feed.Url}";
        }

#pragma warning disable CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。
        return Task.FromResult(feed);
#pragma warning restore CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。
    }

    /// <inheritdoc/>
    public Task<bool> UpdateFeedAsync(RssFeed newFeed, RssFeed oldFeed)
        => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<bool> DeleteFeedAsync(RssFeed feed)
        => Task.FromResult(true);

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        var data = await _httpClient.GetFeedDetailAsync(feed.Url, cancellationToken).ConfigureAwait(false);
        data!.Feed.Comment = feed.Comment;
        data.Feed.GroupIds = feed.GroupIds;
        data.Feed.Id = feed.Id;
        data.Articles.ForEach(p => p.FeedId = feed.Id);
        return data;
    }

    /// <inheritdoc/>
    public async Task<List<RssFeedDetail>> GetFeedDetailListAsync(List<RssFeed> feeds, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();

        // 最大任务数为 10
        var semaphore = new SemaphoreSlim(10);
        var result = new List<RssFeedDetail>();
        foreach (var feed in feeds)
        {
            tasks.Add(Task.Run(
                async () =>
                {
                    try
                    {
                        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                        var data = await GetFeedDetailAsync(feed, cancellationToken).ConfigureAwait(false);
                        result.Add(data!);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        _ = semaphore.Release();
                    }
                },
                cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> UploadOpmlAsync(string opmlPath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(opmlPath).ConfigureAwait(false);
            var opml = new OpmlConfiguration(content);
            var groups = new List<RssFeedGroup>();
            var feeds = new List<RssFeed>();
            foreach (var group in opml.Body.Outlines)
            {
                if (group.Outlines?.Count > 0 && string.IsNullOrEmpty(group.XMLUrl))
                {
                    var rssGroup = new RssFeedGroup
                    {
                        Id = $"/folder/label/{group.Title}",
                        Name = group.Title,
                    };
                    groups.Add(rssGroup);
                    foreach (var feed in group.Outlines)
                    {
                        if (feeds.Any(p => p.Id == $"feed/{feed.XMLUrl}"))
                        {
                            var sourceFeed = feeds.First(p => p.Id == $"feed/{feed.XMLUrl}");
                            sourceFeed.AddGroupId(rssGroup.Id);
                            continue;
                        }

                        var rssFeed = new RssFeed
                        {
                            Id = $"feed/{feed.XMLUrl}",
                            Name = feed.Title,
                            GroupIds = rssGroup.Id,
                            Url = feed.XMLUrl ?? string.Empty,
                            Website = feed.HTMLUrl,
                            Description = feed.Description,
                        };

                        feeds.Add(rssFeed);
                    }
                }
                else
                {
                    if (feeds.Any(p => p.Id == $"feed/{group.XMLUrl}"))
                    {
                        continue;
                    }

                    var rssFeed = new RssFeed
                    {
                        Id = $"feed/{group.XMLUrl}",
                        Name = group.Title,
                        Url = group.XMLUrl ?? string.Empty,
                        Website = group.HTMLUrl,
                        Description = group.Description,
                        GroupIds = string.Empty,
                    };
                    feeds.Add(rssFeed);
                }
            }

            await _dataService.RefreshFeedsAsync(ReaderKernel.Models.RssConnectorType.Local, groups, feeds).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public Task<RssFeedGroup> AddGroupAsync(RssFeedGroup group)
    {
        group.Id = Guid.NewGuid().ToString("N");
        return Task.FromResult(group);
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(RssFeedGroup group)
        => await Task.FromResult(group).ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<bool> DeleteGroupAsync(RssFeedGroup group)
        => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<bool> SignInAsync(RssConfig? config = default)
        => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<bool> SignOutAsync()
        => Task.FromResult(true);

    /// <inheritdoc/>
    public bool IsServiceAvailable()
        => true;

    /// <inheritdoc/>
    public Task<bool> MarkReadAsync(params string[] articleIds)
        => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<bool> MarkAllReadAsync(RssFeed feed)
        => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<bool> MarkAllReadAsync(RssFeedGroup group)
        => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<string> GenerateOpmlAsync()
        => Utils.GenerateOpmlContentAsync(this);
}
