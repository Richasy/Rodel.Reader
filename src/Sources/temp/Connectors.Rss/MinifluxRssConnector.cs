// Copyright (c) Reader Copilot. All rights reserved.

using Microsoft.Extensions.Logging;
using Richasy.ReaderKernel.Connectors.Rss.Models.Miniflux;
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
/// Miniflux RSS 连接器.
/// </summary>
public sealed class MinifluxRssConnector : IRssConnector
{
    private readonly HttpClient _httpClient;
    private readonly IKernelSettingToolkit _settingToolkit;
    private readonly ILogger<MinifluxRssConnector> _logger;
    private readonly IRssDataService _dataService;
    private string _token;
    private string _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinifluxRssConnector"/> class.
    /// </summary>
    public MinifluxRssConnector(
        IKernelSettingToolkit settingToolkit,
        IRssDataService dataService,
        ILogger<MinifluxRssConnector> logger)
    {
        _httpClient = Utils.GetHttpClient();
        _logger = logger;
        _settingToolkit = settingToolkit;
        _dataService = dataService;
        _token = _settingToolkit.ReadSetting(KernelSettingNames.MinifluxToken, string.Empty)!;
        _server = _settingToolkit.ReadSetting(KernelSettingNames.MinifluxServer, string.Empty)!;
    }

    /// <inheritdoc/>
    public bool IsServiceAvailable()
        => !string.IsNullOrEmpty(_token)
            && !string.IsNullOrEmpty(_server);

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(RssConfig? data = null)
    {
        var server = data!.Server!.TrimEnd('/');
        var url = $"{server}/v1/me";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{data.UserName}:{data.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            _token = token;
            _server = server;
            _settingToolkit.WriteSetting(KernelSettingNames.MinifluxToken, _token);
            _settingToolkit.WriteSetting(KernelSettingNames.MinifluxServer, _server);
            var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
            config.Miniflux = new RssTokenConfig
            {
                Server = _server,
                Token = _token,
            };
            await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录失败");
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> SignOutAsync()
    {
        _server = string.Empty;
        _token = string.Empty;
        _settingToolkit.DeleteSetting(KernelSettingNames.MinifluxServer);
        _settingToolkit.DeleteSetting(KernelSettingNames.MinifluxToken);
        await _dataService.ClearCacheAsync(RssConnectorType.Miniflux).ConfigureAwait(false);
        var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
        config.Miniflux = default;
        await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        var url = $"/v1/feeds/{feed.Id}/entries";
        var request = GetBasicRequest(url, HttpMethod.Get);
        try
        {
            var data = await GetResponseAsync(request, JsonGenContext.Default.MinifluxEntriesResponse, cancellationToken).ConfigureAwait(false);
            var rssArticles = new List<RssArticleBase>();
            var readIds = new List<string>();
            foreach (var item in data.entries)
            {
                var art = new RssArticleBase
                {
                    Id = item.id.ToString(),
                    Title = item.title,
                    Url = item.url,
                    PublishDate = item.published_at.ToLocalTime().ToString(),
                    Summary = item.content.DecodeHtml().Truncate(300).ClearReturnSanitizeString(),
                    Content = item.content,
                    Author = item.author ?? feed.Name,
                    FeedId = feed.Id,
                    Cover = Toolkits.Feed.Utils.XmlUtils.GetCover(item.content),
                };

                if (item.status == "read")
                {
                    readIds.Add(art.Id);
                }

                rssArticles.Add(art);
            }

            if (readIds.Count > 0)
            {
                await _dataService.AddReadArticleIdsAsync(RssConnectorType.Miniflux, [.. readIds]).ConfigureAwait(false);
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
        return result;
    }

    /// <inheritdoc/>
    public async Task<(List<RssFeedGroup> Groups, List<RssFeed> Feeds)> GetFeedListAsync(CancellationToken cancellationToken = default)
    {
        const string url = "/v1/feeds";
        var request = GetBasicRequest(url, HttpMethod.Get);
        try
        {
            var feeds = await GetResponseAsync(request, JsonGenContext.Default.ListMinifluxFeed, cancellationToken).ConfigureAwait(false);
            var feedGroups = new List<RssFeedGroup>();
            var rssFeeds = new List<RssFeed>();

            foreach (var feed in feeds)
            {
                var f = new RssFeed
                {
                    Id = feed.id.ToString(),
                    Name = feed.title,
                    Url = feed.feed_url ?? string.Empty,
                    Website = feed.site_url,
                };

                if (feed.category != null)
                {
                    f.GroupIds = feed.category.id.ToString();
                    if (!feedGroups.Any(x => x.Id == feed.category.id.ToString()))
                    {
                        var g = new RssFeedGroup
                        {
                            Id = feed.category.id.ToString(),
                            Name = feed.category.title,
                        };

                        feedGroups.Add(g);
                    }
                }
                else
                {
                    f.GroupIds = string.Empty;
                }

                rssFeeds.Add(f);
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
        var firstRequest = GetBasicRequest("/v1/feeds", HttpMethod.Post);
        firstRequest.Content = JsonContent.Create(
            new MinifluxAddFeedRequest
            {
                feed_url = feed.Url,
                category_id = Convert.ToInt32(feed.GetGroupIds().FirstOrDefault() ?? "0"),
            },
            JsonGenContext.Default.MinifluxAddFeedRequest);
        try
        {
            var createResponse = await GetResponseAsync(firstRequest, JsonGenContext.Default.MinifluxFeedCreateResponse).ConfigureAwait(false);

            // 第二步，获取订阅源信息.
            var secondRequest = GetBasicRequest($"/v1/feeds/{createResponse.feed_id}", HttpMethod.Get);
            var newFeed = await GetResponseAsync(secondRequest, JsonGenContext.Default.MinifluxFeed).ConfigureAwait(false);

            var resultFeed = new RssFeed
            {
                Id = newFeed.id.ToString(),
                Name = newFeed.title,
                Url = newFeed.feed_url ?? string.Empty,
                Website = newFeed.site_url,
            };

            if (newFeed != null)
            {
                resultFeed.GroupIds = newFeed.category.id.ToString();
            }

            return resultFeed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Add feed failed.");
        }

        return default;
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup> AddGroupAsync(RssFeedGroup group)
    {
        const string url = "/v1/categories";
        var request = GetBasicRequest(url, HttpMethod.Post);
        request.Content = JsonContent.Create(
            new MinifluxAddGroupRequest
            {
                title = group.Name,
            },
            JsonGenContext.Default.MinifluxAddGroupRequest);

        var newGroup = await GetResponseAsync(request, JsonGenContext.Default.MinifluxCategory).ConfigureAwait(false);
        return new RssFeedGroup
        {
            Id = newGroup.id.ToString(),
            Name = newGroup.title,
        };
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFeedAsync(RssFeed feed)
    {
        var url = $"/v1/feeds/{feed.Id}";
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
        var url = $"/v1/feeds/{feed.Id}/mark-all-as-read";
        var request = GetBasicRequest(url, HttpMethod.Put);
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
    public async Task<bool> MarkAllReadAsync(RssFeedGroup group)
    {
        var url = $"/v1/categories/{group.Id}/mark-all-as-read";
        var request = GetBasicRequest(url, HttpMethod.Put);
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
    public async Task<bool> MarkReadAsync(params string[] articleIds)
    {
        const string url = "/v1/entries";
        var request = GetBasicRequest(url, HttpMethod.Put);
        request.Content = JsonContent.Create(
            new MinifluxMarkReadRequest
            {
                entry_ids = [.. articleIds.Select(p => Convert.ToInt32(p))],
                status = "read",
            },
            JsonGenContext.Default.MinifluxMarkReadRequest);

        try
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mark read failed.");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFeedAsync(RssFeed newFeed, RssFeed oldFeed)
    {
        var url = $"/v1/feeds/{oldFeed.Id}";
        var request = GetBasicRequest(url, HttpMethod.Put);
        request.Content = JsonContent.Create(
            new MinifluxUpdateFeedRequest
            {
                category_id = Convert.ToInt32(newFeed.GetGroupIds().FirstOrDefault() ?? "0"),
                title = newFeed.Name,
            },
            JsonGenContext.Default.MinifluxUpdateFeedRequest);

        try
        {
            var response = await GetResponseAsync(request, JsonGenContext.Default.MinifluxFeed).ConfigureAwait(false);
            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update feed failed.");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(RssFeedGroup group)
    {
        var url = $"/v1/categories/{group.Id}";
        var request = GetBasicRequest(url, HttpMethod.Put);
        request.Content = JsonContent.Create(
            new MinifluxUpdateGroupRequest
            {
                title = group.Name,
            },
            JsonGenContext.Default.MinifluxUpdateGroupRequest);

        try
        {
            var response = await GetResponseAsync(request, JsonGenContext.Default.MinifluxCategory).ConfigureAwait(false);
            return new RssFeedGroup
            {
                Id = response.id.ToString(),
                Name = response.title,
            };
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UploadOpmlAsync(string opmlPath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(opmlPath).ConfigureAwait(false);
            var request = GetBasicRequest("/v1/import", HttpMethod.Post);
            request.Content = new StringContent(content, Encoding.UTF8, "application/xml");
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
    public async Task<string> GenerateOpmlAsync()
    {
        try
        {
            var request = GetBasicRequest("/v1/export", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (content.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                return content;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generate OPML failed.");
        }

        return string.Empty;
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
        return (await response.Content.ReadFromJsonAsync<T>(typeInfo, cancellationToken: cancellationToken).ConfigureAwait(false))!;
    }
}
