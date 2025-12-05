// Copyright (c) Reader Copilot. All rights reserved.

using Microsoft.Extensions.Logging;
using Richasy.ReaderKernel.Models;
using Richasy.ReaderKernel.Models.Config;
using Richasy.ReaderKernel.Models.Rss;
using Richasy.ReaderKernel.Services;
using Richasy.ReaderKernel.Toolkits;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Richasy.ReaderKernel.Connectors.Rss;

/// <summary>
/// NewsBlur RSS 服务.
/// </summary>
public sealed class NewsBlurRssConnector : IRssConnector
{
    private const string BaseUrl = "https://newsblur.com";
    private readonly HttpClient _httpClient;
    private readonly IKernelSettingToolkit _settingToolkit;
    private readonly ILogger<NewsBlurRssConnector> _logger;
    private readonly IRssDataService _dataService;
    private bool _isSignIn;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsBlurRssConnector"/> class.
    /// </summary>
    public NewsBlurRssConnector(
        IKernelSettingToolkit settingToolkit,
        IRssDataService dataService,
        ILogger<NewsBlurRssConnector> logger)
    {
        _httpClient = Utils.GetHttpClient(true);
        _logger = logger;
        _settingToolkit = settingToolkit;
        _dataService = dataService;
        var cookie = _settingToolkit.ReadSetting(KernelSettingNames.NewsBlurSessionCookie, string.Empty);

        _isSignIn = !string.IsNullOrEmpty(cookie);

        if (_isSignIn)
        {
            Utils.CookieContainer.SetCookies(new Uri(BaseUrl), cookie!);
            _isSignIn = DateTime.Now < Utils.CookieContainer.GetCookies(new Uri(BaseUrl)).First().Expires;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(RssConfig? data = null)
    {
        var url = $"{BaseUrl}/api/login";

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", data!.UserName ?? string.Empty },
                { "password", data.Password ?? string.Empty },
            });
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content,
            };
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var authResult = JsonSerializer.Deserialize(responseString, JsonGenContext.Default.NewsBlurAuthResult);
            _isSignIn = authResult?.authenticated ?? false;

            if (_isSignIn)
            {
                var cookie = response.Headers.GetValues("set-cookie").FirstOrDefault() ?? string.Empty;
                _isSignIn = !string.IsNullOrEmpty(cookie);
                _settingToolkit.WriteSetting(KernelSettingNames.NewsBlurSessionCookie, cookie);
                var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
                config.NewsBlur = new NewsBlurConfig { Cookie = cookie };
                await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sign in failed.");
            _isSignIn = false;
        }

        return _isSignIn;
    }

    /// <inheritdoc/>
    public async Task<bool> SignOutAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/api/logout");
        _ = await _httpClient.SendAsync(request).ConfigureAwait(false);
        _settingToolkit.DeleteSetting(KernelSettingNames.NewsBlurSessionCookie);
        await _dataService.ClearCacheAsync(RssConnectorType.NewsBlur).ConfigureAwait(false);
        _isSignIn = false;
        var config = await Utils.GetLibraryConfigurationAsync(_settingToolkit).ConfigureAwait(false);
        config.NewsBlur = default;
        await Utils.WriteLibraryConfigurationAsync(config, _settingToolkit).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();
        var articles = new List<RssArticleBase>();
        var readIds = new List<string>();

        for (var i = 1; i <= 4; i++)
        {
            tasks.Add(Task.Run(
                async () =>
                {
                    var page = i;
                    var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/reader/feed/" + feed.Id + $"?page={i}&read_filter=unread");
                    try
                    {
                        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var responseData = JsonSerializer.Deserialize(responseString, JsonGenContext.Default.NewsBlurFeedResponse);
                        foreach (var item in responseData!.stories)
                        {
                            var article = new RssArticleBase
                            {
                                Id = item.story_hash,
                                Title = item.story_title,
                                Url = item.story_permalink,
                                Summary = item.story_content.DecodeHtml().Truncate(300).ClearReturnSanitizeString(),
                                Content = item.story_content,
                                PublishDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(item.story_timestamp)).ToLocalTime().ToString(),
                                Cover = item.image_urls?.FirstOrDefault() ?? string.Empty,
                                Author = string.IsNullOrEmpty(item.story_authors) ? feed.Name : item.story_authors,
                                FeedId = feed.Id,
                                ExtraParameter = item.id,
                            };

                            if (item.read_status == 1)
                            {
                                readIds.Add(article.Id);
                            }

                            articles.Add(article);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Get feed detail failed.");
                    }
                },
                cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
        articles = [.. articles.Distinct().OrderByDescending(a => a.PublishDate?.ConvertDateString() ?? DateTimeOffset.MinValue)];

        if(readIds.Count > 0)
        {
            await _dataService.AddReadArticleIdsAsync(RssConnectorType.NewsBlur, [.. readIds]).ConfigureAwait(false);
        }

        return new RssFeedDetail
        {
            Feed = feed,
            Articles = articles,
        };
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
                        _logger.LogError(ex, "Get feed detail list failed.");
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
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/reader/feeds");
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var jsonEle = JsonDocument.Parse(responseString).RootElement;
            var feeds = new List<RssFeed>();
            var groups = new List<RssFeedGroup>();
            if (jsonEle.TryGetProperty("feeds", out var feedsEle))
            {
                var feedJson = feedsEle.GetRawText();
                var feedList = JsonSerializer.Deserialize(feedJson, JsonGenContext.Default.DictionaryStringNewsBlurFeed);
                foreach (var item in feedList!)
                {
                    var feed = new RssFeed
                    {
                        Id = item.Key,
                        Name = item.Value.feed_title,
                        Url = item.Value.feed_address ?? string.Empty,
                        Website = item.Value.feed_link,
                        GroupIds = string.Empty,
                    };
                    feeds.Add(feed);
                }
            }

            if (jsonEle.TryGetProperty("folders", out var folderEle))
            {
                foreach (var item in folderEle.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var jp in item.EnumerateObject())
                        {
                            var groupName = jp.Name;
                            var group = new RssFeedGroup
                            {
                                Id = groupName,
                                Name = groupName,
                            };
                            groups.Add(group);

                            foreach (var ele in jp.Value.EnumerateArray())
                            {
                                if (ele.ValueKind == JsonValueKind.Number)
                                {
                                    var feed = feeds.Find(f => f.Id == ele.GetInt64().ToString());
                                    if (!feed?.GetGroupIds()?.Contains(group.Id) ?? false)
                                    {
                                        feed!.AddGroupId(group.Id);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (groups, feeds);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public bool IsServiceAvailable()
        => _isSignIn;

    /// <inheritdoc/>
    public Task<RssFeed?> AddFeedAsync(RssFeed feed)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public async Task<RssFeedGroup> AddGroupAsync(RssFeedGroup group)
    {
        var url = $"{BaseUrl}/reader/add_folder";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "folder", group.Name },
        });

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content,
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        group.Id = group.Name;
        return group;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteFeedAsync(RssFeed feed)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public Task<bool> DeleteGroupAsync(RssFeedGroup group)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public async Task<bool> MarkAllReadAsync(RssFeed feed)
    {
        var articles = await _dataService.GetUnreadArticleIdsByFeedIdAsync(RssConnectorType.NewsBlur, feed.Id).ConfigureAwait(false);
        return await MarkAsReadInternalAsync([.. articles]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAllReadAsync(RssFeedGroup group)
    {
        var feeds = await _dataService.GetFeedsByGroupIdAsync(RssConnectorType.NewsBlur, group.Id).ConfigureAwait(false);
        var articles = new List<string>();
        foreach (var feed in feeds)
        {
            var tempArticles = await _dataService.GetUnreadArticleIdsByFeedIdAsync(RssConnectorType.NewsBlur, feed.Id).ConfigureAwait(false);
            articles.AddRange([..tempArticles]);
        }

        return await MarkAsReadInternalAsync([.. articles.Distinct()]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkReadAsync(params string[] articleIds)
        => await MarkAsReadInternalAsync(articleIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<bool> UpdateFeedAsync(RssFeed newFeed, RssFeed oldFeed)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(RssFeedGroup group)
    {
        var url = $"{BaseUrl}/reader/rename_folder";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "folder_to_rename", group.Id },
            { "new_folder_name", group.Name },
            { "in_folder", string.Empty },
        });

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content,
            };

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var newGroup = group.Clone();
            newGroup.Id = group.Name;
            return newGroup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update group failed.");
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UploadOpmlAsync(string opmlPath)
    {
        var bytes = await File.ReadAllBytesAsync(opmlPath).ConfigureAwait(false);
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml");
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/import/opml_upload")
        {
            Content = fileContent,
        };
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
    public async Task<string> GenerateOpmlAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/import/opml_export");
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

    private async Task<bool> MarkAsReadInternalAsync(params string[] storyHashes)
    {
        if (storyHashes.Length == 0)
        {
            return true;
        }

        var url = $"{BaseUrl}/reader/mark_story_hashes_as_read";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "story_hash", string.Join("&", storyHashes) },
        });

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content,
            };
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
