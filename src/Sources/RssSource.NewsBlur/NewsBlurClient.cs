// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Sources.Rss.NewsBlur;

/// <summary>
/// NewsBlur RSS 客户端.
/// 通过 NewsBlur API 管理订阅源和文章.
/// </summary>
public sealed partial class NewsBlurClient : IRssClient
{
    private const string BaseUrl = NewsBlurClientOptions.BaseUrl;

    private readonly NewsBlurClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;
    private readonly ILogger<NewsBlurClient> _logger;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;
    private bool _isAuthenticated;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsBlurClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端（可选，用于测试注入）.</param>
    /// <param name="cookieContainer">Cookie 容器（可选，用于测试注入）.</param>
    /// <param name="logger">日志记录器.</param>
    public NewsBlurClient(
        NewsBlurClientOptions options,
        HttpClient? httpClient = null,
        CookieContainer? cookieContainer = null,
        ILogger<NewsBlurClient>? logger = null)
    {
        _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<NewsBlurClient>.Instance;
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
        _cookieContainer = cookieContainer ?? new CookieContainer();
        _httpClient = httpClient ?? HttpClientHelper.CreateHttpClient(_cookieContainer, _options.Timeout);
    }

    /// <inheritdoc/>
    public IRssSourceCapabilities Capabilities => KnownRssSources.NewsBlur;

    /// <inheritdoc/>
    public bool IsAuthenticated => _isAuthenticated;

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("开始 NewsBlur 登录验证");

        if (!_options.HasValidCredentials)
        {
            _logger.LogWarning("未提供有效的认证信息（用户名或密码）");
            return false;
        }

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", _options.UserName! },
                { "password", _options.Password! },
            });

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/login")
            {
                Content = content,
            };

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var authResult = JsonSerializer.Deserialize(responseString, NewsBlurJsonContext.Default.NewsBlurAuthResult);

            _isAuthenticated = authResult?.Authenticated ?? false;

            if (_isAuthenticated)
            {
                _logger.LogInformation("NewsBlur 登录成功");
            }
            else
            {
                _logger.LogWarning("NewsBlur 登录失败");
            }

            return _isAuthenticated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NewsBlur 登录过程中发生异常");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SignOutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NewsBlur 登出");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/logout");
            await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NewsBlur 登出请求失败，但仍清除本地状态");
        }

        _isAuthenticated = false;
        _logger.LogInformation("NewsBlur 已登出");
        return true;
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RssFeedGroup> Groups, IReadOnlyList<RssFeed> Feeds)> GetFeedListAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("获取 NewsBlur 订阅源列表");

        EnsureAuthenticated();

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/reader/feeds");
        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var feeds = new List<RssFeed>();
        var groups = new List<RssFeedGroup>();

        // 解析响应
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        // 解析订阅源
        if (root.TryGetProperty("feeds", out var feedsElement))
        {
            foreach (var feedProp in feedsElement.EnumerateObject())
            {
                var feedId = feedProp.Name;
                var feedObj = feedProp.Value;

                var feed = new RssFeed
                {
                    Id = feedId,
                    Name = WebUtility.HtmlDecode(feedObj.GetProperty("feed_title").GetString() ?? string.Empty),
                    Url = feedObj.TryGetProperty("feed_address", out var addr) ? addr.GetString() ?? string.Empty : string.Empty,
                    Website = feedObj.TryGetProperty("feed_link", out var link) ? link.GetString() : null,
                    IconUrl = feedObj.TryGetProperty("favicon_url", out var icon) ? icon.GetString() : null,
                };

                feeds.Add(feed);
            }
        }

        // 解析文件夹结构
        if (root.TryGetProperty("folders", out var foldersElement))
        {
            ParseFolders(foldersElement, feeds, groups);
        }

        _logger.LogInformation("获取到 {GroupCount} 个分组和 {FeedCount} 个订阅源", groups.Count, feeds.Count);

        return (groups, feeds);
    }

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("获取订阅源详情: {FeedName} (Id: {FeedId})", feed.Name, feed.Id);

        EnsureAuthenticated();

        var articles = new List<RssArticle>();
        var tasks = new List<Task>();
        var lockObj = new object();

        // 并行获取多页文章
        for (var page = 1; page <= _options.PagesToFetch; page++)
        {
            var currentPage = page;
            tasks.Add(Task.Run(
                async () =>
                {
                    try
                    {
                        var url = $"{BaseUrl}/reader/feed/{feed.Id}?page={currentPage}&read_filter=all";
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                        var storiesResponse = JsonSerializer.Deserialize(content, NewsBlurJsonContext.Default.NewsBlurStoriesResponse);
                        if (storiesResponse?.Stories != null)
                        {
                            foreach (var story in storiesResponse.Stories)
                            {
                                var article = ConvertToArticle(story, feed);
                                lock (lockObj)
                                {
                                    articles.Add(article);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "获取订阅源 {FeedName} 第 {Page} 页文章失败", feed.Name, currentPage);
                    }
                },
                cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        // 去重并按时间排序
        articles = articles
            .DistinctBy(a => a.Id)
            .OrderByDescending(a => a.GetPublishTime() ?? DateTimeOffset.MinValue)
            .ToList();

        _logger.LogInformation("获取订阅源 {FeedName} 的 {ArticleCount} 篇文章", feed.Name, articles.Count);

        return new RssFeedDetail
        {
            Feed = feed,
            Articles = articles,
        };
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

        EnsureAuthenticated();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "folder", group.Name },
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/add_folder")
        {
            Content = content,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("分组 {GroupName} 添加成功", group.Name);
                return new RssFeedGroup
                {
                    Id = group.Name,  // NewsBlur 使用文件夹名作为 ID
                    Name = group.Name,
                };
            }

            _logger.LogWarning("添加分组失败，状态码: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加分组 {GroupName} 失败", group.Name);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("更新分组: {GroupId} -> {GroupName}", group.Id, group.Name);

        EnsureAuthenticated();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "folder_to_rename", group.Id },
            { "new_folder_name", group.Name },
            { "in_folder", string.Empty },
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/rename_folder")
        {
            Content = content,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("分组 {GroupName} 更新成功", group.Name);
                return new RssFeedGroup
                {
                    Id = group.Name,
                    Name = group.Name,
                };
            }

            _logger.LogWarning("更新分组失败，状态码: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新分组 {GroupId} 失败", group.Id);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("删除分组: {GroupName} ({GroupId})", group.Name, group.Id);

        EnsureAuthenticated();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "folder_to_delete", group.Id },
            { "in_folder", string.Empty },
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/delete_folder")
        {
            Content = content,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("分组 {GroupName} 删除成功", group.Name);
                return true;
            }

            _logger.LogWarning("删除分组失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除分组 {GroupName} 失败", group.Name);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<RssFeed?> AddFeedAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("添加订阅源: {FeedUrl}", feed.Url);

        EnsureAuthenticated();

        var parameters = new Dictionary<string, string>
        {
            { "url", feed.Url },
        };

        // 如果有分组，设置文件夹
        var groupIds = feed.GetGroupIdList();
        if (groupIds.Count > 0)
        {
            parameters["folder"] = groupIds[0];
        }

        var content = new FormUrlEncodedContent(parameters);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/add_url")
        {
            Content = content,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var addResponse = JsonSerializer.Deserialize(responseContent, NewsBlurJsonContext.Default.NewsBlurAddFeedResponse);

            if (addResponse?.Feed != null)
            {
                var newFeed = new RssFeed
                {
                    Id = addResponse.Feed.Id.ToString(),
                    Name = WebUtility.HtmlDecode(addResponse.Feed.FeedTitle),
                    Url = addResponse.Feed.FeedAddress ?? feed.Url,
                    Website = addResponse.Feed.FeedLink,
                    IconUrl = addResponse.Feed.FaviconUrl,
                };

                if (groupIds.Count > 0)
                {
                    newFeed.SetGroupIdList(groupIds);
                }

                _logger.LogInformation("订阅源 {FeedName} 添加成功，ID: {FeedId}", newFeed.Name, newFeed.Id);
                return newFeed;
            }

            _logger.LogWarning("添加订阅源失败: {Message}", addResponse?.Message ?? "未知错误");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加订阅源 {FeedUrl} 失败", feed.Url);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFeedAsync(
        RssFeed newFeed,
        RssFeed oldFeed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(newFeed);
        ArgumentNullException.ThrowIfNull(oldFeed);

        _logger.LogDebug("更新订阅源: {FeedId}", oldFeed.Id);

        EnsureAuthenticated();

        // 重命名订阅源
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "feed_id", oldFeed.Id },
            { "feed_title", newFeed.Name },
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/rename_feed")
        {
            Content = content,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("订阅源 {FeedName} 更新成功", newFeed.Name);

                // 如果分组有变化，移动订阅源
                var newGroups = newFeed.GetGroupIdList();
                var oldGroups = oldFeed.GetGroupIdList();
                if (newGroups.Count > 0 && (oldGroups.Count == 0 || newGroups[0] != oldGroups[0]))
                {
                    await MoveFeedToFolderAsync(oldFeed.Id, oldGroups.Count > 0 ? oldGroups[0] : string.Empty, newGroups[0], cancellationToken).ConfigureAwait(false);
                }

                return true;
            }

            _logger.LogWarning("更新订阅源失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新订阅源 {FeedId} 失败", oldFeed.Id);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFeedAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("删除订阅源: {FeedName} ({FeedId})", feed.Name, feed.Id);

        EnsureAuthenticated();

        var groupIds = feed.GetGroupIdList();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "feed_id", feed.Id },
            { "in_folder", groupIds.Count > 0 ? groupIds[0] : string.Empty },
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/delete_feed")
        {
            Content = content,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("订阅源 {FeedName} 删除成功", feed.Name);
                return true;
            }

            _logger.LogWarning("删除订阅源失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除订阅源 {FeedName} 失败", feed.Name);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> MarkArticlesAsReadAsync(
        IEnumerable<string> articleIds,
        CancellationToken cancellationToken = default)
    {
        var idList = articleIds.ToList();
        if (idList.Count == 0)
        {
            return true;
        }

        _logger.LogDebug("标记 {Count} 篇文章为已读", idList.Count);

        EnsureAuthenticated();

        // NewsBlur 需要为每个 story_hash 添加单独的参数
        var parameters = idList.Select(id => new KeyValuePair<string, string>("story_hash", id)).ToList();
        var content = new FormUrlEncodedContent(parameters);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/mark_story_hashes_as_read")
        {
            Content = content,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("成功标记 {Count} 篇文章为已读", idList.Count);
                return true;
            }

            _logger.LogWarning("标记已读失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记文章已读失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> MarkFeedAsReadAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("标记订阅源 {FeedName} 全部已读", feed.Name);

        EnsureAuthenticated();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "feed_id", feed.Id },
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/mark_feed_as_read")
        {
            Content = content,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("订阅源 {FeedName} 已标记全部已读", feed.Name);
                return true;
            }

            _logger.LogWarning("标记订阅源已读失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记订阅源 {FeedName} 已读失败", feed.Name);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> MarkGroupAsReadAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("标记分组 {GroupName} 全部已读", group.Name);

        // NewsBlur 没有直接标记文件夹已读的 API，需要先获取该分组下的所有订阅源，然后逐个标记
        // 但为了简化，我们可以使用 mark_all_as_read，但这会标记所有内容
        // 更好的方法是获取分组下的订阅源列表，然后批量标记

        EnsureAuthenticated();

        // 获取订阅源列表
        var (groups, feeds) = await GetFeedListAsync(cancellationToken).ConfigureAwait(false);

        // 找到属于该分组的订阅源
        var feedsInGroup = feeds.Where(f => f.GetGroupIdList().Contains(group.Id)).ToList();

        if (feedsInGroup.Count == 0)
        {
            _logger.LogInformation("分组 {GroupName} 下没有订阅源", group.Name);
            return true;
        }

        // 批量标记
        var success = true;
        foreach (var feed in feedsInGroup)
        {
            var result = await MarkFeedAsReadAsync(feed, cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                success = false;
            }
        }

        return success;
    }

    /// <inheritdoc/>
    public async Task<bool> ImportOpmlAsync(
        string opmlContent,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(opmlContent);

        _logger.LogDebug("导入 OPML");

        EnsureAuthenticated();

        // 使用 multipart/form-data 上传 OPML
        using var formContent = new MultipartFormDataContent();
        var fileContent = new StringContent(opmlContent);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        formContent.Add(fileContent, "file", "import.opml");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/import/opml_upload")
        {
            Content = formContent,
        };

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("OPML 导入成功");
                return true;
            }

            _logger.LogWarning("OPML 导入失败，状态码: {StatusCode}", response.StatusCode);
            return false;
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
        _logger.LogDebug("导出 OPML");

        EnsureAuthenticated();

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/import/opml_export");

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (content.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
                content.StartsWith("<opml", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("OPML 导出成功");
                return content;
            }

            _logger.LogWarning("OPML 导出失败：响应不是有效的 XML");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OPML 导出失败");
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _semaphore.Dispose();
        _httpClient.Dispose();
        _disposed = true;
    }

    #region Private Methods

    private void EnsureAuthenticated()
    {
        if (!_isAuthenticated)
        {
            throw new InvalidOperationException("尚未登录，请先调用 SignInAsync 进行认证。");
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task FetchFeedWithSemaphoreAsync(
        RssFeed feed,
        List<RssFeedDetail> results,
        CancellationToken cancellationToken)
    {
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取订阅源 {FeedName} 详情失败", feed.Name);
        }
    }

    private static void ParseFolders(JsonElement foldersElement, List<RssFeed> feeds, List<RssFeedGroup> groups)
    {
        foreach (var item in foldersElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object)
            {
                // 这是一个文件夹
                foreach (var prop in item.EnumerateObject())
                {
                    var groupName = prop.Name;
                    var group = new RssFeedGroup
                    {
                        Id = groupName,
                        Name = groupName,
                    };

                    if (!groups.Any(g => g.Id == groupName))
                    {
                        groups.Add(group);
                    }

                    // 解析文件夹中的订阅源
                    foreach (var feedIdElement in prop.Value.EnumerateArray())
                    {
                        if (feedIdElement.ValueKind == JsonValueKind.Number)
                        {
                            var feedId = feedIdElement.GetInt64().ToString();
                            var feed = feeds.Find(f => f.Id == feedId);
                            if (feed != null)
                            {
                                var existingGroups = feed.GetGroupIdList().ToList();
                                if (!existingGroups.Contains(groupName))
                                {
                                    existingGroups.Add(groupName);
                                    feed.SetGroupIdList(existingGroups);
                                }
                            }
                        }
                        else if (feedIdElement.ValueKind == JsonValueKind.Object)
                        {
                            // 嵌套文件夹，递归处理
                            ParseNestedFolder(feedIdElement, feeds, groups, groupName);
                        }
                    }
                }
            }
            else if (item.ValueKind == JsonValueKind.Number)
            {
                // 顶级订阅源（没有文件夹）
                // 不需要额外处理
            }
        }
    }

    private static void ParseNestedFolder(JsonElement folderElement, List<RssFeed> feeds, List<RssFeedGroup> groups, string parentFolder)
    {
        foreach (var prop in folderElement.EnumerateObject())
        {
            var groupName = prop.Name;
            var fullGroupName = $"{parentFolder}/{groupName}";

            var group = new RssFeedGroup
            {
                Id = fullGroupName,
                Name = groupName,
            };

            if (!groups.Any(g => g.Id == fullGroupName))
            {
                groups.Add(group);
            }

            foreach (var feedIdElement in prop.Value.EnumerateArray())
            {
                if (feedIdElement.ValueKind == JsonValueKind.Number)
                {
                    var feedId = feedIdElement.GetInt64().ToString();
                    var feed = feeds.Find(f => f.Id == feedId);
                    if (feed != null)
                    {
                        var existingGroups = feed.GetGroupIdList().ToList();
                        if (!existingGroups.Contains(fullGroupName))
                        {
                            existingGroups.Add(fullGroupName);
                            feed.SetGroupIdList(existingGroups);
                        }
                    }
                }
            }
        }
    }

    private async Task MoveFeedToFolderAsync(
        string feedId,
        string fromFolder,
        string toFolder,
        CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "feed_id", feedId },
            { "in_folder", fromFolder },
            { "to_folder", toFolder },
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reader/move_feed_to_folder")
        {
            Content = content,
        };

        try
        {
            await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "移动订阅源到文件夹失败");
        }
    }

    private static RssArticle ConvertToArticle(NewsBlurStory story, RssFeed feed)
    {
        var article = new RssArticle
        {
            Id = story.StoryHash,
            FeedId = feed.Id,
            Title = WebUtility.HtmlDecode(story.StoryTitle),
            Content = story.StoryContent,
            Url = story.StoryPermalink,
            Author = string.IsNullOrEmpty(story.StoryAuthors) ? feed.Name : story.StoryAuthors,
            CoverUrl = story.ImageUrls?.FirstOrDefault(),
            ExtraData = story.Id,
        };

        // 解析发布时间
        if (!string.IsNullOrEmpty(story.StoryTimestamp) && long.TryParse(story.StoryTimestamp, out var timestamp))
        {
            article.SetPublishTime(DateTimeOffset.FromUnixTimeSeconds(timestamp));
        }
        else if (!string.IsNullOrEmpty(story.StoryDate) && DateTimeOffset.TryParse(story.StoryDate, out var date))
        {
            article.SetPublishTime(date);
        }

        // 设置标签
        if (story.StoryTags?.Count > 0)
        {
            article.SetTagList(story.StoryTags);
        }

        // 生成摘要
        if (!string.IsNullOrEmpty(story.StoryContent))
        {
            article.Summary = TruncateHtmlContent(story.StoryContent, 300);
        }

        return article;
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    private static string TruncateHtmlContent(string html, int maxLength)
    {
        // 移除 HTML 标签
        var text = HtmlTagRegex().Replace(html, string.Empty);
        text = WebUtility.HtmlDecode(text);
        text = text.Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Trim();

        // 截断
        if (text.Length > maxLength)
        {
            text = text[..maxLength] + "...";
        }

        return text;
    }

    #endregion
}
