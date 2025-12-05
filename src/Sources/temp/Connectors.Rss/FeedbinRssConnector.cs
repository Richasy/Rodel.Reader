// Copyright (c) Reader Copilot. All rights reserved.

using Microsoft.Extensions.Logging;
using Richasy.ReaderKernel.Connectors.Rss.Models.Feedbin;
using Richasy.ReaderKernel.Models;
using Richasy.ReaderKernel.Models.Config;
using Richasy.ReaderKernel.Models.Rss;
using Richasy.ReaderKernel.Services;
using Richasy.ReaderKernel.Toolkits;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace Richasy.ReaderKernel.Connectors.Rss;

/// <summary>
/// Feedbin RSS 连接器.
/// </summary>
public sealed class FeedbinRssConnector : IRssConnector
{
    private readonly HttpClient _httpClient;
    private readonly IKernelSettingToolkit _settingToolkit;
    private readonly ILogger<FeedbinRssConnector> _logger;
    private readonly IRssDataService _dataService;
    private string _token;
    private string _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbinRssConnector"/> class.
    /// </summary>
    public FeedbinRssConnector(
        IKernelSettingToolkit settingToolkit,
        IRssDataService dataService,
        ILogger<FeedbinRssConnector> logger)
    {
        _httpClient = Utils.GetHttpClient();
        _settingToolkit = settingToolkit;
        _logger = logger;
        _dataService = dataService;
        _token = _settingToolkit.ReadSetting(KernelSettingNames.FeedbinToken, string.Empty)!;
        _server = _settingToolkit.ReadSetting<string>(KernelSettingNames.FeedbinServer, string.Empty)!;
    }

    /// <inheritdoc/>
    public bool IsServiceAvailable()
        => !string.IsNullOrEmpty(_token)
            && !string.IsNullOrEmpty(_server);

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(RssConfig? config = null)
    {
        var server = config!.Server!.TrimEnd('/');
        var url = server + "/authentication.json";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.UserName}:{config.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            _token = token;
            _server = server;
            _settingToolkit.WriteSetting(KernelSettingNames.FeedbinToken, _token);
            _settingToolkit.WriteSetting(KernelSettingNames.FeedbinServer, _server);
            var data = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
            data.Feedbin = new RssTokenConfig
            {
                Server = _server,
                Token = _token,
            };
            await Utils.WriteLibraryConfigurationAsync(data, _settingToolkit).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feedbin login failed");
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> SignOutAsync()
    {
        _server = string.Empty;
        _token = string.Empty;
        _settingToolkit.DeleteSetting(KernelSettingNames.FeedbinServer);
        _settingToolkit.DeleteSetting(KernelSettingNames.FeedbinToken);
        await _dataService.ClearCacheAsync(RssConnectorType.Feedbin).ConfigureAwait(false);
        var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
        config.Feedbin = default;
        await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        var url = $"/feeds/{feed.Id}/entries.json";
        var request = GetBasicRequest(url, HttpMethod.Get);
        try
        {
            var data = await GetResponseAsync(request, JsonGenContext.Default.ListFeedbinEntity, cancellationToken).ConfigureAwait(false);
            var rssArticles = new List<RssArticleBase>();
            foreach (var item in data)
            {
                var art = new RssArticleBase
                {
                    Id = item.id.ToString(),
                    Title = item.title,
                    Url = item.url,
                    PublishDate = item.published.ToLocalTime().ToString(),
                    Summary = item.summary,
                    Content = item.content,
                    Author = item.author ?? feed.Name,
                    FeedId = feed.Id,
                    Cover = Toolkits.Feed.Utils.XmlUtils.GetCover(item.content),
                };

                rssArticles.Add(art);
            }

            return new RssFeedDetail
            {
                Articles = rssArticles,
                Feed = feed,
            };
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<List<RssFeedDetail>> GetFeedDetailListAsync(List<RssFeed> feeds, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();

        // 最大任务数为 20
        var semaphore = new SemaphoreSlim(20);
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
        await GetAndWriteRecentReadAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc/>
    public async Task<(List<RssFeedGroup> Groups, List<RssFeed> Feeds)> GetFeedListAsync(CancellationToken cancellationToken = default)
    {
        var groupRequest = GetBasicRequest("/taggings.json", HttpMethod.Get);
        var feedRequest = GetBasicRequest("/subscriptions.json", HttpMethod.Get);
        try
        {
            var groups = await GetResponseAsync(groupRequest, JsonGenContext.Default.ListFeedbinTagging, cancellationToken).ConfigureAwait(false);
            var feeds = await GetResponseAsync(feedRequest, JsonGenContext.Default.ListFeedbinSubscription, cancellationToken).ConfigureAwait(false);

            var feedGroups = new List<RssFeedGroup>();
            var rssFeeds = new List<RssFeed>();
            foreach (var feed in feeds)
            {
                var f = new RssFeed
                {
                    Id = feed.feed_id.ToString(),
                    Name = feed.title,
                    Url = feed.feed_url ?? string.Empty,
                    Website = feed.site_url,
                    Comment = feed.id.ToString(),
                };

                f.SetGroupIds(groups.Where(p => p.feed_id == feed.feed_id).Select(p => p.name));
                rssFeeds.Add(f);
            }

            foreach (var group in groups)
            {
                if (feedGroups.Any(p => p.Name == group.name))
                {
                    continue;
                }

                var g = new RssFeedGroup
                {
                    Id = group.name,
                    Name = group.name,
                };

                feedGroups.Add(g);
            }

            return (feedGroups, rssFeeds);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<RssFeed?> AddFeedAsync(RssFeed feed)
    {
        // 第一步，创建订阅源.
        var firstRequest = GetBasicRequest("/subscriptions.json", HttpMethod.Post);
        var data = new FeedbinAddFeedRequest { feed_url = feed.Url };
        firstRequest.Content = JsonContent.Create(data, (JsonTypeInfo<FeedbinAddFeedRequest>)JsonGenContext.Default.FeedbinAddFeedRequest);
        try
        {
            var newFeed = await GetResponseAsync(firstRequest, JsonGenContext.Default.FeedbinSubscription).ConfigureAwait(false);

            // 第二步，修改订阅源名称.
            if (!newFeed.title.Equals(feed.Name, StringComparison.OrdinalIgnoreCase))
            {
                var secondRequest = GetBasicRequest($"/subscriptions/{newFeed.id}.json", HttpMethod.Patch);
                newFeed = await GetResponseAsync(secondRequest, JsonGenContext.Default.FeedbinSubscription).ConfigureAwait(false);
            }

            // 第三步，添加分组.
            if (feed.GetGroupIds().Count > 0)
            {
                var tasks = new List<Task>();
                var succeedIds = new List<string>();
                foreach (var gid in feed.GetGroupIds())
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var id = gid;
                            var tempRequest = GetBasicRequest("/taggings.json", HttpMethod.Post);
                            var tempJson = $"{{\"feed_id\":{newFeed.feed_id},\"name\":\"{id}\"}}";
                            tempRequest.Content = new StringContent(tempJson, Encoding.UTF8, "application/json");
                            var response = await _httpClient.SendAsync(tempRequest).ConfigureAwait(false);
                            _ = response.EnsureSuccessStatusCode();
                            succeedIds.Add(id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Add feed group failed");
                        }
                    }));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
                feed.SetGroupIds(succeedIds);
            }

            var resultFeed = new RssFeed
            {
                Id = newFeed.feed_id.ToString(),
                Name = newFeed.title,
                Url = newFeed.feed_url ?? string.Empty,
                GroupIds = feed.GroupIds,
                Website = newFeed.site_url,
                Comment = newFeed.id.ToString(),
            };

            return resultFeed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Add feed failed");
        }

        return default;
    }

    /// <inheritdoc/>
    public Task<RssFeedGroup> AddGroupAsync(RssFeedGroup group)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public async Task<bool> DeleteFeedAsync(RssFeed feed)
    {
        var url = $"/subscriptions/{feed.Comment}.json";
        var request = GetBasicRequest(url, HttpMethod.Delete);
        try
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<bool> DeleteGroupAsync(RssFeedGroup group)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public async Task<bool> MarkAllReadAsync(RssFeed feed)
    {
        var articles = await _dataService.GetUnreadArticleIdsByFeedIdAsync(RssConnectorType.Feedbin, feed.Id).ConfigureAwait(false);
        return await MarkReadInternalAsync([.. articles]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAllReadAsync(RssFeedGroup group)
    {
        var feeds = await _dataService.GetFeedsByGroupIdAsync(RssConnectorType.Feedbin, group.Id).ConfigureAwait(false);
        var articles = new List<string>();
        foreach (var feed in feeds)
        {
            var tempArticles = await _dataService.GetUnreadArticleIdsByFeedIdAsync(RssConnectorType.Feedbin, feed.Id).ConfigureAwait(false);
            articles.AddRange(tempArticles);
        }

        return await MarkReadInternalAsync([.. articles.Distinct()]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<bool> MarkReadAsync(params string[] articleIds)
        => MarkReadInternalAsync(articleIds);

    /// <inheritdoc/>
    public Task<bool> UpdateFeedAsync(RssFeed newFeed, RssFeed oldFeed)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(RssFeedGroup group)
    {
        var url = $"tags.json";
        var data = new FeedbinUpdateGroupRequest { old_name = group.Id, new_name = group.Name };

        var request = GetBasicRequest(url, HttpMethod.Post);
        request.Content = JsonContent.Create(data, JsonGenContext.Default.FeedbinUpdateGroupRequest);
        try
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var newGroup = group.Clone();
            newGroup.Id = group.Name;
            return newGroup;
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UploadOpmlAsync(string opmlPath)
    {
        var bytes = await File.ReadAllBytesAsync(opmlPath).ConfigureAwait(false);
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml");
        var request = GetBasicRequest("/imports.json", HttpMethod.Post);
        request.Content = fileContent;
        try
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<string> GenerateOpmlAsync()
        => Utils.GenerateOpmlContentAsync(this);

    private async Task GetAndWriteRecentReadAsync(CancellationToken cancellationToken)
    {
        var request = GetBasicRequest("/recently_read_entries.json", HttpMethod.Get);
        var data = await GetResponseAsync(request, JsonGenContext.Default.ListInt64, cancellationToken).ConfigureAwait(false);
        await _dataService.AddReadArticleIdsAsync(RssConnectorType.Feedbin, [.. data.Select(p => p.ToString())]).ConfigureAwait(false);
    }

    private HttpRequestMessage GetBasicRequest(string url, HttpMethod method, string query = "")
    {
        url = $"{_server}{url}";
        if (!string.IsNullOrEmpty(query))
        {
            url += $"?{query}";
        }

        var request = new HttpRequestMessage(method, new Uri(url));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _token);
        return request;
    }

    private async Task<T> GetResponseAsync<T>(HttpRequestMessage request, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken: cancellationToken).ConfigureAwait(false))!;
    }

    private async Task<bool> MarkReadInternalAsync(params string[] articleIds)
    {
        if (articleIds.Length == 0)
        {
            return true;
        }

        var ids = articleIds.Distinct().Select(p => Convert.ToInt64(p)).ToList();
        var url = $"/updated_entries.json";
        var request = GetBasicRequest(url, HttpMethod.Delete);
        var data = new FeedbinMarkReadRequest { updated_entries = ids };
        request.Content = JsonContent.Create(data, (JsonTypeInfo<FeedbinMarkReadRequest>)JsonGenContext.Default.FeedbinMarkReadRequest);
        try
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
