// Copyright (c) Reader Copilot. All rights reserved.

using Microsoft.Extensions.Logging;
using Richasy.ReaderKernel.Connectors.Rss.Models.GoogleReader;
using Richasy.ReaderKernel.Models;
using Richasy.ReaderKernel.Models.Config;
using Richasy.ReaderKernel.Models.Rss;
using Richasy.ReaderKernel.Services;
using Richasy.ReaderKernel.Toolkits;
using System.Text;
using System.Text.Json;

namespace Richasy.ReaderKernel.Connectors.Rss;

/// <summary>
/// Google Reader RSS 连接器.
/// </summary>
public sealed class GoogleReaderRssConnector : IRssConnector
{
    private readonly HttpClient _httpClient;
    private readonly IKernelSettingToolkit _settingToolkit;
    private readonly ILogger<GoogleReaderRssConnector> _logger;
    private readonly IRssDataService _dataService;
    private string _token;
    private string _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleReaderRssConnector"/> class.
    /// </summary>
    public GoogleReaderRssConnector(
        IKernelSettingToolkit settingToolkit,
        IRssDataService dataService,
        ILogger<GoogleReaderRssConnector> logger)
    {
        _httpClient = Utils.GetHttpClient();
        _settingToolkit = settingToolkit;
        _logger = logger;
        _dataService = dataService;
        _token = _settingToolkit.ReadSetting(KernelSettingNames.GoogleReaderToken, string.Empty)!;
        _server = _settingToolkit.ReadSetting(KernelSettingNames.GoogleReaderServer, string.Empty)!;
    }

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(RssConfig? config = null)
    {
        var url = config!.Server!.TrimEnd('/') + "/accounts/ClientLogin?output=json";
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            var dict = new Dictionary<string, string>
            {
                { "Email", config?.UserName ?? string.Empty },
                { "Passwd", config?.Password ?? string.Empty },
                { "client", "Reader Copilot" },
                { "accountType", "HOSTED_OR_GOOGLE" },
                { "service", "reader" },
            };

            request.Content = new FormUrlEncodedContent(dict);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var auth = string.Empty;

            if (responseString.StartsWith('{'))
            {
                var authResponse = JsonSerializer.Deserialize(responseString, JsonGenContext.Default.GoogleReaderAuthResponse);
                auth = authResponse!.Auth;
            }
            else
            {
                auth = Array.Find(responseString?.Split('\n') ?? [], s => s.StartsWith("Auth=", StringComparison.OrdinalIgnoreCase))![5..];
            }

            if (string.IsNullOrEmpty(auth))
            {
                return false;
            }

            _token = auth;
            _server = config!.Server.TrimEnd('/');
            _settingToolkit.WriteSetting(KernelSettingNames.GoogleReaderToken, _token);
            _settingToolkit.WriteSetting(KernelSettingNames.GoogleReaderServer, _server);
            var data = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
            data.GoogleReader = new RssTokenConfig
            {
                Server = _server,
                Token = _token,
            };
            await Utils.WriteLibraryConfigurationAsync(data, _settingToolkit).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SignOutAsync()
    {
        _server = string.Empty;
        _token = string.Empty;
        _settingToolkit.DeleteSetting(KernelSettingNames.GoogleReaderServer);
        _settingToolkit.DeleteSetting(KernelSettingNames.GoogleReaderToken);
        await _dataService.ClearCacheAsync(RssConnectorType.GoogleReader).ConfigureAwait(false);
        var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
        config.GoogleReader = default;
        await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public async Task<(List<RssFeedGroup> Groups, List<RssFeed> Feeds)> GetFeedListAsync(CancellationToken cancellationToken = default)
    {
        var url = $"/reader/api/0/subscription/list";
        try
        {
            var request = GetBasicRequest(url, HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var data = JsonSerializer.Deserialize(content, JsonGenContext.Default.GoogleReaderSubscriptionResponse);
            var groups = new List<RssFeedGroup>();
            var feeds = new List<RssFeed>();
            foreach (var item in data!.subscriptions)
            {
                var feed = new RssFeed
                {
                    Id = item.id,
                    Name = item.title,
                    Url = item.url ?? string.Empty,
                    Website = item.htmlUrl,
                    Description = string.Empty,
                };

                feed.SetGroupIds(item.categories?.Select(p => p.id) ?? []);
                if (item.categories?.Count > 0)
                {
                    foreach (var cate in item.categories)
                    {
                        if (!groups.Any(p => p.Id == cate.id))
                        {
                            groups.Add(new RssFeedGroup
                            {
                                Id = cate.id,
                                Name = cate.label,
                            });
                        }
                    }
                }

                feeds.Add(feed);
            }

            return (groups, feeds);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        var url = $"/reader/api/0/stream/contents/{feed.Id}";
        try
        {
            var request = GetBasicRequest(url, HttpMethod.Get, "n=100");
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var data = JsonSerializer.Deserialize(content, JsonGenContext.Default.GoogleReaderStreamContentResponse);
            if (data?.items == null)
            {
                return default;
            }

            var articles = new List<RssArticleBase>();
            var readIds = new List<string>();

            foreach (var item in data.items)
            {
                var article = new RssArticleBase
                {
                    Id = item.id,
                    Title = item.title,
                    Url = item.canonical?.FirstOrDefault()?.href ?? item.alternate?.FirstOrDefault()?.href ?? string.Empty,
                    PublishDate = DateTimeOffset.FromUnixTimeSeconds(item.published).ToLocalTime().ToString(),
                    Content = GetSummaryOrContent(item),
                    Summary = GetSummaryOrContent(item).DecodeHtml().Truncate(300).ClearReturnSanitizeString(),
                    Author = item.author,
                    Cover = Toolkits.Feed.Utils.XmlUtils.GetCover(item.summary?.content),
                    FeedId = feed.Id,
                };

                article.SetTags(item.categories?.Where(p => p.StartsWith("user/-/label/", StringComparison.OrdinalIgnoreCase)).Select(p => p.Replace("user/-/label/", string.Empty, StringComparison.OrdinalIgnoreCase)).ToList() ?? []);

                if (item.categories!.Contains("user/-/state/com.google/read"))
                {
                    // 该文章已读，添加到已读列表
                    readIds.Add(article.Id);
                }

                articles.Add(article);
            }

            if (readIds.Count > 0)
            {
                await _dataService.AddReadArticleIdsAsync(RssConnectorType.GoogleReader, [.. readIds]).ConfigureAwait(false);
            }

            return new RssFeedDetail
            {
                Feed = feed,
                Articles = articles,
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
    public async Task<RssFeed?> AddFeedAsync(RssFeed feed)
    {
        var query = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("ac", "subscribe"),
            new KeyValuePair<string, string>("s", $"feed/{feed.Url}"),
            new KeyValuePair<string, string>("t", feed.Name),
        };
        foreach (var id in feed.GetGroupIds())
        {
            query.Add(new KeyValuePair<string, string>("a", id));
        }

        try
        {
            var request = GetBasicRequest("/reader/api/0/subscription/edit", HttpMethod.Post);
            var content = new FormUrlEncodedContent(query);
            request.Content = content;
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseString.Contains("OK", StringComparison.OrdinalIgnoreCase))
            {
                feed.Id = $"feed/{feed.Url}";
                return feed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Add feed failed");
        }

        return default;
    }

    /// <inheritdoc/>
    public Task<RssFeedGroup> AddGroupAsync(RssFeedGroup group)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<bool> DeleteFeedAsync(RssFeed feed)
    {
        var dict = new Dictionary<string, string>
        {
            { "ac", "unsubscribe" },
            { "s", feed.Id },
        };

        try
        {
            var request = GetBasicRequest("/reader/api/0/subscription/edit", HttpMethod.Post);
            var content = new FormUrlEncodedContent(dict);
            request.Content = content;
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteGroupAsync(RssFeedGroup group)
    {
        try
        {
            HttpRequestMessage request;
            if (string.IsNullOrEmpty(group.Id))
            {
                request = GetBasicRequest("/reader/api/0/subscription/edit", HttpMethod.Post);
                var list = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("ac", "unsubscribe"),
                };

                var feeds = await _dataService.GetFeedsByGroupIdAsync(RssConnectorType.GoogleReader, string.Empty).ConfigureAwait(false);
                foreach (var feed in feeds)
                {
                    list.Add(new KeyValuePair<string, string>("s", feed.Id));
                }

                var content = new FormUrlEncodedContent(list);
                request.Content = content;
            }
            else
            {
                request = GetBasicRequest("/reader/api/0/disable-tag", HttpMethod.Post);
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "s", group.Id },
                });
            }

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFeedAsync(RssFeed newFeed, RssFeed oldFeed)
    {
        var query = new List<KeyValuePair<string, string>>
        {
            new("ac", "edit"),
            new("s", newFeed.Id),
            new("t", newFeed.Name),
        };

        foreach (var id in newFeed.GetGroupIds().Except(oldFeed.GetGroupIds()).ToList())
        {
            query.Add(new KeyValuePair<string, string>("a", id));
        }

        foreach (var id in oldFeed.GetGroupIds().Except(newFeed.GetGroupIds()).ToList())
        {
            query.Add(new KeyValuePair<string, string>("r", id));
        }

        try
        {
            var request = GetBasicRequest("/reader/api/0/subscription/edit", HttpMethod.Post);
            request.Content = new FormUrlEncodedContent(query);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update feed failed");
        }

        return default;
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(RssFeedGroup group)
    {
        try
        {
            var request = GetBasicRequest("/reader/api/0/rename-tag", HttpMethod.Post);
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "s", group.Id },
                { "dest", $"user/-/label/{group.Name}" },
            });
            request.Content = content;
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseString.Contains("OK", StringComparison.OrdinalIgnoreCase))
            {
                var newGroup = group.Clone();
                newGroup.Id = $"user/-/label/{group.Name}";
                return newGroup;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update group failed");
        }

        return default;
    }

    /// <inheritdoc/>
    public async Task<bool> UploadOpmlAsync(string opmlPath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(opmlPath).ConfigureAwait(false);
            var request = GetBasicRequest("/reader/api/0/subscription/import", HttpMethod.Post);
            request.Content = new StringContent(content, Encoding.UTF8, "application/xml");
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload OPML failed");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string> GenerateOpmlAsync()
    {
        try
        {
            var request = GetBasicRequest("/reader/api/0/subscription/export", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (content.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                return content;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generate OPML failed");
        }

        return string.Empty;
    }

    /// <inheritdoc/>
    public bool IsServiceAvailable()
        => !string.IsNullOrEmpty(_token)
            && !string.IsNullOrEmpty(_server);

    /// <inheritdoc/>
    public Task<bool> MarkReadAsync(params string[] articleIds)
        => MarkAsReadInternalAsync(articleIds);

    /// <inheritdoc/>
    public async Task<bool> MarkAllReadAsync(RssFeed feed)
    {
        var articles = await _dataService.GetUnreadArticleIdsByFeedIdAsync(RssConnectorType.GoogleReader, feed.Id).ConfigureAwait(false);
        return await MarkAsReadInternalAsync([..articles]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAllReadAsync(RssFeedGroup group)
    {
        var feeds = await _dataService.GetFeedsByGroupIdAsync(RssConnectorType.GoogleReader, group.Id).ConfigureAwait(false);
        var articles = new List<string>();
        foreach (var feed in feeds)
        {
            var tempArticles = await _dataService.GetUnreadArticleIdsByFeedIdAsync(RssConnectorType.GoogleReader, feed.Id).ConfigureAwait(false);
            articles.AddRange(tempArticles);
        }

        return await MarkAsReadInternalAsync([.. articles.Distinct()]).ConfigureAwait(false);
    }

    private static string GetSummaryOrContent(ArticleItem item, bool preferSummary = false)
    {
        var content = preferSummary ? item.summary?.content ?? item.content?.content
            : item.content?.content ?? item.summary?.content;
        return content ?? string.Empty;
    }

    private HttpRequestMessage GetBasicRequest(string url, HttpMethod method, string query = "")
    {
        url = $"{_server}{url}?output=json&{query}";
        var request = new HttpRequestMessage(method, new Uri(url));
        request.Headers.Add("Authorization", $"GoogleLogin auth={_token}");
        return request;
    }

    private async Task<bool> MarkAsReadInternalAsync(params string[] ids)
    {
        if (ids.Length == 0)
        {
            return true;
        }

        var list = new List<KeyValuePair<string, string>>
        {
            new("a", "user/-/state/com.google/read"),
        };
        foreach (var item in ids)
        {
            list.Add(new KeyValuePair<string, string>("i", item));
        }

        try
        {
            var request = GetBasicRequest("/reader/api/0/edit-tag", HttpMethod.Post);
            var content = new FormUrlEncodedContent(list);
            request.Content = content;
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
