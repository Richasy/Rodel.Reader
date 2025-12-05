// Copyright (c) Reader Copilot. All rights reserved.

using Microsoft.Extensions.Logging;
using Richasy.ReaderKernel.Connectors.Rss.Models.GoogleReader;
using Richasy.ReaderKernel.Models;
using Richasy.ReaderKernel.Models.Config;
using Richasy.ReaderKernel.Models.Rss;
using Richasy.ReaderKernel.Services;
using Richasy.ReaderKernel.Toolkits;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Richasy.ReaderKernel.Connectors.Rss;

/// <summary>
/// Inoreader RSS 连接器.
/// </summary>
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
public sealed class InoreaderRssConnector : IRssConnector
{
    private const string ClientId = "999999903";
    private const string ClientSecret = "Zu7l4W4QYOgznj1n7D1hUpGiM_NZgFt0";
    private readonly HttpClient _httpClient;
    private readonly IKernelSettingToolkit _settingToolkit;
    private readonly ILogger<InoreaderRssConnector> _logger;
    private readonly IRssDataService _dataService;
    private InoreaderDataSource _dataSource;
    private string _accessToken;
    private string _tempCode;
    private DateTimeOffset _expireTime;
    private TaskCompletionSource<bool> _signInTaskCompletionSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="InoreaderRssConnector"/> class.
    /// </summary>
    public InoreaderRssConnector(
        IKernelSettingToolkit settingToolkit,
        IRssDataService dataService,
        ILogger<InoreaderRssConnector> logger)
    {
        _httpClient = Utils.GetHttpClient();
        _logger = logger;
        _settingToolkit = settingToolkit;
        _dataService = dataService;
        _dataSource = _settingToolkit.ReadSetting(KernelSettingNames.InoreaderSource, InoreaderDataSource.Default);
        _accessToken = _settingToolkit.ReadSetting(KernelSettingNames.InoreaderAccessToken, string.Empty)!;
        _expireTime = _settingToolkit.ReadSetting(KernelSettingNames.InoreaderExpireTime, DateTimeOffset.MinValue);
    }

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(RssConfig? config = null)
    {
        _tempCode = string.Empty;
        _settingToolkit.DeleteSetting(KernelSettingNames.InoreaderTempCode);
        _dataSource = _settingToolkit.ReadSetting(KernelSettingNames.InoreaderSource, InoreaderDataSource.Default);
        _signInTaskCompletionSource = new TaskCompletionSource<bool>();
        var url = $"{GetBaseUrl()}/oauth2/auth?client_id={ClientId}&redirect_uri={Uri.EscapeDataString("readercop://inoreader")}&response_type=code&scope=read%20write&state=readercopilot";

        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true,
        };
        _ = Process.Start(psi);

        return await _signInTaskCompletionSource.Task.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> SignOutAsync()
    {
        _accessToken = string.Empty;
        _expireTime = DateTimeOffset.MinValue;
        _tempCode = string.Empty;
        _settingToolkit.DeleteSetting(KernelSettingNames.InoreaderAccessToken);
        _settingToolkit.DeleteSetting(KernelSettingNames.InoreaderRefreshToken);
        _settingToolkit.DeleteSetting(KernelSettingNames.InoreaderExpireTime);
        _settingToolkit.DeleteSetting(KernelSettingNames.InoreaderTempCode);
        _settingToolkit.DeleteSetting(KernelSettingNames.InoreaderSource);
        await _dataService.ClearCacheAsync(RssConnectorType.Inoreader).ConfigureAwait(false);
        var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
        config.Inoreader = default;
        await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public bool IsServiceAvailable()
        => !string.IsNullOrEmpty(_accessToken);

    /// <inheritdoc/>
    public async Task<(List<RssFeedGroup> Groups, List<RssFeed> Feeds)> GetFeedListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await GetBasicRequestAsync("/subscription/list", HttpMethod.Get).ConfigureAwait(false);
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
                    Name = HttpUtility.HtmlDecode(item.title),
                    Url = item.url ?? string.Empty,
                    Website = item.htmlUrl,
                    Description = string.Empty,
                };

                feed.SetGroupIds(item.categories?.Select(p => p.id).ToList() ?? []);
                feeds.Add(feed);
            }

            var folderListRequest = await GetBasicRequestAsync("/tag/list", HttpMethod.Get).ConfigureAwait(false);
            var folderListResponse = await _httpClient.SendAsync(folderListRequest, cancellationToken).ConfigureAwait(false);
            var folderListContent = await folderListResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var folderListData = JsonSerializer.Deserialize(folderListContent, JsonGenContext.Default.InoreaderFolderListResponse);
            foreach (var item in folderListData!.tags.Where(p => p.type == "folder"))
            {
                groups.Add(new RssFeedGroup
                {
                    Id = item.id,
                    Name = item.id.Split('/', StringSplitOptions.RemoveEmptyEntries).Last(),
                });
            }

            var preferencesRequest = await GetBasicRequestAsync("/preference/stream/list", HttpMethod.Get).ConfigureAwait(false);
            var preferencesResponse = await _httpClient.SendAsync(preferencesRequest, cancellationToken).ConfigureAwait(false);
            var preferencesContent = await preferencesResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var preferencesData = JsonSerializer.Deserialize(preferencesContent, JsonGenContext.Default.InoreaderPreferenceResponse);

            var folderOrders = preferencesData!.streamprefs.FirstOrDefault(p => p.Key.EndsWith("state/com.google/root", StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault(p => p.id == "subscription-ordering")?.value;
            if (!string.IsNullOrEmpty(folderOrders))
            {
                var folderOrderList = Enumerable
                    .Range(0, folderOrders.Length / 8)
                    .Select(i => folderOrders.Substring(i * 8, 8));
                groups.Clear();
                foreach (var item in folderOrderList)
                {
                    var group = folderListData.tags.FirstOrDefault(p => p.sortid == item);
                    if (group != null)
                    {
                        var hasFeeds = data.subscriptions.Any(p => p.categories?.Any(q => q.id == group.id) == true);
                        if (!hasFeeds)
                        {
                            continue;
                        }

                        groups.Add(new RssFeedGroup
                        {
                            Id = group.id,
                            Name = group.id.Split('/', StringSplitOptions.RemoveEmptyEntries).Last(),
                        });
                    }
                }
            }

            return (groups, feeds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get feed list failed");
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        var path = "/stream/contents/" + Uri.EscapeDataString(feed.Id) + "?n=100";
        try
        {
            var request = await GetBasicRequestAsync(path, HttpMethod.Get).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var data = JsonSerializer.Deserialize(content, JsonGenContext.Default.GoogleReaderStreamContentResponse);
            var articles = new List<RssArticleBase>();
            var readIds = new List<string>();

            foreach (var item in data!.items)
            {
                var article = new RssArticleBase
                {
                    Id = item.id,
                    Title = HttpUtility.HtmlDecode(item.title),
                    Url = item.canonical?.FirstOrDefault()?.href ?? item.alternate?.FirstOrDefault()?.href ?? string.Empty,
                    PublishDate = DateTimeOffset.FromUnixTimeSeconds(item.published).ToLocalTime().ToString(),
                    Content = GetSummaryOrContent(item),
                    Summary = GetSummaryOrContent(item).DecodeHtml().Truncate(300).ClearReturnSanitizeString(),
                    Author = item.author,
                    Cover = Toolkits.Feed.Utils.XmlUtils.GetCover(item.summary?.content),
                    FeedId = feed.Id,
                };

                article.SetTags(item.categories?.Where(p => !p.StartsWith("user/-/label/", StringComparison.OrdinalIgnoreCase)).ToList() ?? []);

                if (item.categories!.Contains("user/-/state/com.google/read"))
                {
                    // 该文章已读，添加到已读列表
                    readIds.Add(item.id);
                }

                articles.Add(article);
            }

            await _dataService.AddReadArticleIdsAsync(RssConnectorType.Inoreader, [.. readIds]).ConfigureAwait(false);

            return new RssFeedDetail
            {
                Feed = feed,
                Articles = articles,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get feed detail failed");
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
                        _logger.LogError(ex, "Get feed detail failed");
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
            var request = await GetBasicRequestAsync("/subscription/edit", HttpMethod.Post).ConfigureAwait(false);
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
            var request = await GetBasicRequestAsync("/subscription/edit", HttpMethod.Post).ConfigureAwait(false);
            var content = new FormUrlEncodedContent(dict);
            request.Content = content;
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete feed failed");
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
                request = await GetBasicRequestAsync("/subscription/edit", HttpMethod.Post).ConfigureAwait(false);
                var list = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("ac", "unsubscribe"),
                };

                var feeds = await _dataService.GetFeedsByGroupIdAsync(RssConnectorType.Inoreader, string.Empty).ConfigureAwait(false);
                foreach (var feed in feeds)
                {
                    list.Add(new KeyValuePair<string, string>("s", feed.Id));
                }

                var content = new FormUrlEncodedContent(list);
                request.Content = content;
            }
            else
            {
                request = await GetBasicRequestAsync("/disable-tag", HttpMethod.Post).ConfigureAwait(false);
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "s", group.Id },
                });
                request.Content = content;
            }

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete group failed");
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

        var removeGroups = oldFeed.GetGroupIds().Except(newFeed.GetGroupIds()).ToList();
        var addGroups = newFeed.GetGroupIds().Except(oldFeed.GetGroupIds()).ToList();

        foreach (var id in addGroups)
        {
            query.Add(new KeyValuePair<string, string>("a", id));
        }

        foreach (var id in removeGroups)
        {
            query.Add(new KeyValuePair<string, string>("r", id));
        }

        try
        {
            var request = await GetBasicRequestAsync("/subscription/edit", HttpMethod.Post).ConfigureAwait(false);
            var content = new FormUrlEncodedContent(query);
            request.Content = content;
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
            var request = await GetBasicRequestAsync("/rename-tag", HttpMethod.Post).ConfigureAwait(false);
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
            var request = await GetBasicRequestAsync("/subscription/import", HttpMethod.Post).ConfigureAwait(false);
            request.Content = new StringContent(content, Encoding.UTF8, "application/xml");
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload opml failed");
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<string> GenerateOpmlAsync()
        => Utils.GenerateOpmlContentAsync(this);

    /// <inheritdoc/>
    public async Task<bool> MarkAllReadAsync(RssFeed feed)
        => await MarkAllAsReadInternalAsync(feed.Id).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<bool> MarkAllReadAsync(RssFeedGroup group)
        => await MarkAllAsReadInternalAsync(group.Id).ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<bool> MarkReadAsync(params string[] articleIds)
        => MarkAsReadInternalAsync(articleIds);

    /// <summary>
    /// 取消授权.
    /// </summary>
    public void CancelAuthorize()
    {
        _ = _signInTaskCompletionSource?.TrySetResult(false);
        _tempCode = string.Empty;
    }

    /// <summary>
    /// 是否正在授权.
    /// </summary>
    /// <returns>结果.</returns>
    public bool IsAuthorizing()
    {
        return _signInTaskCompletionSource != null
            && !_signInTaskCompletionSource.Task.IsCompleted;
    }

    /// <summary>
    /// 设置授权码.
    /// </summary>
    /// <param name="code">授权码.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetAuthorizeCodeAsync(string code)
    {
        try
        {
            _tempCode = code;
            _settingToolkit.WriteSetting(KernelSettingNames.InoreaderTempCode, code);
            var url = $"{GetBaseUrl()}/oauth2/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = _tempCode,
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["redirect_uri"] = "readercop://inoreader",
                ["scope"] = "read write",
                ["grant_type"] = "authorization_code",
            });

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content,
            };

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var authResult = JsonSerializer.Deserialize(json, JsonGenContext.Default.InoreaderAuthResult);
            _accessToken = authResult!.access_token;
            var refreshToken = authResult.refresh_token;
            var expiresIn = authResult.expires_in;
            var expireTime = DateTimeOffset.Now.AddSeconds(expiresIn);
            _expireTime = expireTime;
            _settingToolkit.WriteSetting(KernelSettingNames.InoreaderAccessToken, _accessToken);
            _settingToolkit.WriteSetting(KernelSettingNames.InoreaderRefreshToken, refreshToken);
            _settingToolkit.WriteSetting(KernelSettingNames.InoreaderExpireTime, expireTime);
            _settingToolkit.WriteSetting(KernelSettingNames.InoreaderSource, _dataSource);
            _tempCode = string.Empty;
            var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
            config.Inoreader = new InoreaderConfig
            {
                AccessToken = _accessToken,
                RefreshToken = refreshToken,
                ExpireTime = expireTime,
                Source = _dataSource,
            };
            await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
            _ = _signInTaskCompletionSource?.TrySetResult(true);
        }
        catch (Exception)
        {
            _ = _signInTaskCompletionSource?.TrySetResult(false);
        }
    }

    private static string GetSummaryOrContent(ArticleItem item, bool preferSummary = false)
    {
        var content = preferSummary ? item.summary?.content ?? item.content?.content
            : item.content?.content ?? item.summary?.content;
        return content ?? string.Empty;
    }

    private string GetBaseUrl()
    {
        var baseUrl = _dataSource switch
        {
            InoreaderDataSource.Default => "https://www.inoreader.com",
            InoreaderDataSource.Mirror => "https://www.innoreader.com",
            InoreaderDataSource.Jp => "https://jp.inoreader.com",
            _ => throw new NotImplementedException(),
        };
        return baseUrl;
    }

    private async Task<HttpRequestMessage> GetBasicRequestAsync(string path, HttpMethod method, bool shouldCheckRefresh = true)
    {
        if (shouldCheckRefresh)
        {
            await RefreshTokenIfNeededAsync().ConfigureAwait(false);
        }

        var baseUrl = GetBaseUrl();
        baseUrl += "/reader/api/0";

        var request = new HttpRequestMessage(method, baseUrl + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return request;
    }

    private async Task RefreshTokenIfNeededAsync()
    {
        if (DateTimeOffset.Now < _expireTime)
        {
            return;
        }

        var url = $"{GetBaseUrl()}/oauth2/token";
        var refreshToken = _settingToolkit.ReadSetting(KernelSettingNames.InoreaderRefreshToken, string.Empty);
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["grant_type"] = "refresh_token",
        });

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content,
        };

        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var authResult = JsonSerializer.Deserialize(json, JsonGenContext.Default.InoreaderAuthResult);
        _accessToken = authResult!.access_token;
        refreshToken = authResult.refresh_token;
        var expiresIn = authResult.expires_in;
        var expireTime = DateTimeOffset.Now.AddSeconds(expiresIn);
        _expireTime = expireTime;
        _settingToolkit.WriteSetting(KernelSettingNames.InoreaderAccessToken, _accessToken);
        _settingToolkit.WriteSetting(KernelSettingNames.InoreaderRefreshToken, refreshToken);
        _settingToolkit.WriteSetting(KernelSettingNames.InoreaderExpireTime, expireTime);
        _settingToolkit.WriteSetting(KernelSettingNames.InoreaderSource, _dataSource);
        var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
        config.Inoreader = new InoreaderConfig
        {
            AccessToken = _accessToken,
            RefreshToken = refreshToken,
            ExpireTime = expireTime,
            Source = _dataSource,
        };
        await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
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
            var request = await GetBasicRequestAsync("/edit-tag", HttpMethod.Post).ConfigureAwait(false);
            request.Content = new FormUrlEncodedContent(list);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mark as read failed");
            return false;
        }
    }

    private async Task<bool> MarkAllAsReadInternalAsync(string streamId)
    {
        try
        {
            var request = await GetBasicRequestAsync("/mark-all-as-read", HttpMethod.Post).ConfigureAwait(false);
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "s", streamId },
                { "ts", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
            });

            request.Content = content;
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return responseString.Contains("OK", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mark all as read failed");
            return false;
        }
    }
}
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
