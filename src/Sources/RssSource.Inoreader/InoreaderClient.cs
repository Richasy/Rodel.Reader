// Copyright (c) Richasy. All rights reserved.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Richasy.RodelReader.Sources.Rss.Inoreader.Internal;

namespace Richasy.RodelReader.Sources.Rss.Inoreader;

/// <summary>
/// Inoreader RSS 客户端.
/// 通过 Inoreader API 管理订阅源和文章.
/// </summary>
public sealed class InoreaderClient : IRssClient
{
    private readonly InoreaderClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<InoreaderClient> _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly SemaphoreSlim _tokenRefreshLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="InoreaderClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端（可选，用于测试注入）.</param>
    /// <param name="logger">日志记录器.</param>
    public InoreaderClient(
        InoreaderClientOptions options,
        HttpClient? httpClient = null,
        ILogger<InoreaderClient>? logger = null)
    {
        _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<InoreaderClient>.Instance;
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
        _httpClient = httpClient ?? HttpClientHelper.CreateHttpClient(_options.Timeout);
    }

    /// <inheritdoc/>
    public IRssSourceCapabilities Capabilities => KnownRssSources.Inoreader;

    /// <inheritdoc/>
    public bool IsAuthenticated => !string.IsNullOrEmpty(_options.AccessToken);

    /// <inheritdoc/>
    public Task<bool> SignInAsync(CancellationToken cancellationToken = default)
    {
        // Inoreader 使用 OAuth 认证，需要通过 InoreaderAuthHelper 获取授权 URL
        // 然后在应用层处理浏览器跳转和回调
        // 这里仅检查是否已有有效 Token
        _logger.LogDebug("检查 Inoreader 认证状态");

        if (IsAuthenticated)
        {
            _logger.LogInformation("Inoreader 已认证");
            return Task.FromResult(true);
        }

        _logger.LogWarning("Inoreader 未认证，需要通过 OAuth 流程获取 Token");
        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public Task<bool> SignOutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Inoreader 登出");

        // 清除本地 Token
        _options.AccessToken = null;
        _options.RefreshToken = null;
        _options.ExpireTime = null;

        _logger.LogInformation("Inoreader 已登出");
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RssFeedGroup> Groups, IReadOnlyList<RssFeed> Feeds)> GetFeedListAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("获取 Inoreader 订阅源列表");

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        // 获取订阅列表
        var subscriptionRequest = CreateRequest("/subscription/list", HttpMethod.Get);
        var subscriptionResponse = await SendRequestAsync(subscriptionRequest, cancellationToken).ConfigureAwait(false);
        var subscriptionContent = await subscriptionResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var subscriptionData = JsonSerializer.Deserialize(subscriptionContent, InoreaderJsonContext.Default.InoreaderSubscriptionResponse)
            ?? throw new InvalidOperationException("Failed to parse subscription response.");

        // 获取标签/文件夹列表
        var tagRequest = CreateRequest("/tag/list", HttpMethod.Get);
        var tagResponse = await SendRequestAsync(tagRequest, cancellationToken).ConfigureAwait(false);
        var tagContent = await tagResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var tagData = JsonSerializer.Deserialize(tagContent, InoreaderJsonContext.Default.InoreaderTagListResponse)
            ?? throw new InvalidOperationException("Failed to parse tag list response.");

        // 获取排序偏好
        var prefRequest = CreateRequest("/preference/stream/list", HttpMethod.Get);
        var prefResponse = await SendRequestAsync(prefRequest, cancellationToken).ConfigureAwait(false);
        var prefContent = await prefResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var prefData = JsonSerializer.Deserialize(prefContent, InoreaderJsonContext.Default.InoreaderPreferenceResponse);

        // 解析分组
        var groups = new List<RssFeedGroup>();
        var folderTags = tagData.Tags.Where(t => t.Type == "folder").ToList();

        // 尝试按排序顺序排列分组
        var folderOrders = prefData?.StreamPrefs
            .FirstOrDefault(p => p.Key.EndsWith("state/com.google/root", StringComparison.OrdinalIgnoreCase))
            .Value?.FirstOrDefault(p => p.Id == "subscription-ordering")?.Value;

        if (!string.IsNullOrEmpty(folderOrders))
        {
            var orderList = Enumerable
                .Range(0, folderOrders.Length / 8)
                .Select(i => folderOrders.Substring(i * 8, 8))
                .ToList();

            foreach (var sortId in orderList)
            {
                var tag = folderTags.FirstOrDefault(t => t.SortId == sortId);
                if (tag != null)
                {
                    var hasFeeds = subscriptionData.Subscriptions
                        .Any(s => s.Categories?.Any(c => c.Id == tag.Id) == true);

                    if (hasFeeds)
                    {
                        groups.Add(new RssFeedGroup
                        {
                            Id = tag.Id,
                            Name = ExtractGroupName(tag.Id),
                        });
                    }
                }
            }
        }
        else
        {
            // 没有排序信息，直接添加有订阅的分组
            foreach (var tag in folderTags)
            {
                var hasFeeds = subscriptionData.Subscriptions
                    .Any(s => s.Categories?.Any(c => c.Id == tag.Id) == true);

                if (hasFeeds)
                {
                    groups.Add(new RssFeedGroup
                    {
                        Id = tag.Id,
                        Name = ExtractGroupName(tag.Id),
                    });
                }
            }
        }

        // 解析订阅源
        var feeds = new List<RssFeed>();
        foreach (var sub in subscriptionData.Subscriptions)
        {
            var feed = new RssFeed
            {
                Id = sub.Id,
                Name = WebUtility.HtmlDecode(sub.Title),
                Url = sub.Url ?? string.Empty,
                Website = sub.HtmlUrl,
                IconUrl = sub.IconUrl,
                Description = string.Empty,
            };

            if (sub.Categories?.Count > 0)
            {
                feed.SetGroupIdList(sub.Categories.Select(c => c.Id));
            }

            feeds.Add(feed);
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

        _logger.LogDebug("获取订阅源详情: {FeedName}", feed.Name);

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var path = $"/stream/contents/{Uri.EscapeDataString(feed.Id)}?n={_options.ArticlesPerRequest}";
        var request = CreateRequest(path, HttpMethod.Get);
        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var data = JsonSerializer.Deserialize(content, InoreaderJsonContext.Default.InoreaderStreamContentResponse);

        if (data == null)
        {
            _logger.LogWarning("无法解析订阅源 {FeedName} 的响应", feed.Name);
            return null;
        }

        var articles = new List<RssArticle>();

        foreach (var item in data.Items)
        {
            var article = ConvertToArticle(item, feed.Id);
            articles.Add(article);
        }

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
    public Task<RssFeedGroup?> AddGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        // Inoreader 不支持直接创建分组
        // 分组会在添加订阅源时通过指定 categories 自动创建
        _logger.LogWarning("Inoreader 不支持直接创建分组，分组会在添加订阅源时自动创建");
        throw new NotSupportedException("Inoreader does not support creating groups directly. Groups are created when adding feeds with categories.");
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("重命名分组: {GroupId} -> {GroupName}", group.Id, group.Name);

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var request = CreateRequest("/rename-tag", HttpMethod.Post);
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["s"] = group.Id,
            ["dest"] = $"user/-/label/{group.Name}",
        });
        request.Content = content;

        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (responseText.Contains("OK", StringComparison.OrdinalIgnoreCase))
        {
            var newGroup = group.Clone();
            newGroup.Id = $"user/-/label/{group.Name}";
            _logger.LogInformation("分组 {GroupName} 重命名成功", group.Name);
            return newGroup;
        }

        _logger.LogWarning("分组 {GroupName} 重命名失败", group.Name);
        return null;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("删除分组: {GroupName}", group.Name);

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var request = CreateRequest("/disable-tag", HttpMethod.Post);
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["s"] = group.Id,
        });
        request.Content = content;

        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var success = responseText.Contains("OK", StringComparison.OrdinalIgnoreCase);
        if (success)
        {
            _logger.LogInformation("分组 {GroupName} 删除成功", group.Name);
        }
        else
        {
            _logger.LogWarning("分组 {GroupName} 删除失败", group.Name);
        }

        return success;
    }

    /// <inheritdoc/>
    public async Task<RssFeed?> AddFeedAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("添加订阅源: {FeedName} ({FeedUrl})", feed.Name, feed.Url);

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var query = new List<KeyValuePair<string, string>>
        {
            new("ac", "subscribe"),
            new("s", $"feed/{feed.Url}"),
            new("t", feed.Name),
        };

        foreach (var groupId in feed.GetGroupIdList())
        {
            query.Add(new KeyValuePair<string, string>("a", groupId));
        }

        var request = CreateRequest("/subscription/edit", HttpMethod.Post);
        request.Content = new FormUrlEncodedContent(query);

        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (responseText.Contains("OK", StringComparison.OrdinalIgnoreCase))
        {
            var newFeed = feed.Clone();
            newFeed.Id = $"feed/{feed.Url}";
            _logger.LogInformation("订阅源 {FeedName} 添加成功", feed.Name);
            return newFeed;
        }

        _logger.LogWarning("订阅源 {FeedName} 添加失败", feed.Name);
        return null;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateFeedAsync(
        RssFeed newFeed,
        RssFeed oldFeed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(newFeed);
        ArgumentNullException.ThrowIfNull(oldFeed);

        _logger.LogDebug("更新订阅源: {FeedName}", newFeed.Name);

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var query = new List<KeyValuePair<string, string>>
        {
            new("ac", "edit"),
            new("s", newFeed.Id),
            new("t", newFeed.Name),
        };

        var oldGroups = oldFeed.GetGroupIdList();
        var newGroups = newFeed.GetGroupIdList();

        var removeGroups = oldGroups.Except(newGroups).ToList();
        var addGroups = newGroups.Except(oldGroups).ToList();

        foreach (var groupId in addGroups)
        {
            query.Add(new KeyValuePair<string, string>("a", groupId));
        }

        foreach (var groupId in removeGroups)
        {
            query.Add(new KeyValuePair<string, string>("r", groupId));
        }

        var request = CreateRequest("/subscription/edit", HttpMethod.Post);
        request.Content = new FormUrlEncodedContent(query);

        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var success = responseText.Contains("OK", StringComparison.OrdinalIgnoreCase);
        if (success)
        {
            _logger.LogInformation("订阅源 {FeedName} 更新成功", newFeed.Name);
        }
        else
        {
            _logger.LogWarning("订阅源 {FeedName} 更新失败", newFeed.Name);
        }

        return success;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFeedAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("删除订阅源: {FeedName}", feed.Name);

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var request = CreateRequest("/subscription/edit", HttpMethod.Post);
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ac"] = "unsubscribe",
            ["s"] = feed.Id,
        });
        request.Content = content;

        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var success = responseText.Contains("OK", StringComparison.OrdinalIgnoreCase);
        if (success)
        {
            _logger.LogInformation("订阅源 {FeedName} 删除成功", feed.Name);
        }
        else
        {
            _logger.LogWarning("订阅源 {FeedName} 删除失败", feed.Name);
        }

        return success;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkArticlesAsReadAsync(
        IEnumerable<string> articleIds,
        CancellationToken cancellationToken = default)
    {
        var ids = articleIds.ToList();
        if (ids.Count == 0)
        {
            return true;
        }

        _logger.LogDebug("标记 {Count} 篇文章为已读", ids.Count);

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var query = new List<KeyValuePair<string, string>>
        {
            new("a", "user/-/state/com.google/read"),
        };

        foreach (var id in ids)
        {
            query.Add(new KeyValuePair<string, string>("i", id));
        }

        var request = CreateRequest("/edit-tag", HttpMethod.Post);
        request.Content = new FormUrlEncodedContent(query);

        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var success = responseText.Contains("OK", StringComparison.OrdinalIgnoreCase);
        if (success)
        {
            _logger.LogInformation("成功标记 {Count} 篇文章为已读", ids.Count);
        }
        else
        {
            _logger.LogWarning("标记文章为已读失败");
        }

        return success;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkFeedAsReadAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("将订阅源 {FeedName} 下的所有文章标记为已读", feed.Name);

        return await MarkStreamAsReadAsync(feed.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkGroupAsReadAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("将分组 {GroupName} 下的所有文章标记为已读", group.Name);

        return await MarkStreamAsReadAsync(group.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ImportOpmlAsync(
        string opmlContent,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(opmlContent);

        _logger.LogDebug("导入 OPML");

        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var request = CreateRequest("/subscription/import", HttpMethod.Post);
        request.Content = new StringContent(opmlContent, Encoding.UTF8, "application/xml");

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("OPML 导入成功");
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
        _logger.LogDebug("导出 OPML");

        var (groups, feeds) = await GetFeedListAsync(cancellationToken).ConfigureAwait(false);
        var opml = OpmlHelper.GenerateOpml(groups, feeds, "Inoreader Subscriptions");

        _logger.LogInformation("OPML 导出成功");
        return opml;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _tokenRefreshLock.Dispose();
            _httpClient.Dispose();
            _disposed = true;
        }
    }

    private HttpRequestMessage CreateRequest(string path, HttpMethod method)
    {
        // 确保 path 不以 / 开头，以便正确追加到基础 URL
        var relativePath = path.TrimStart('/');
        var baseUrl = _options.GetApiBaseUrl().ToString().TrimEnd('/');
        var fullUrl = $"{baseUrl}/{relativePath}";

        var request = new HttpRequestMessage(method, fullUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        return request;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return response;
    }

    private async Task EnsureTokenValidAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.AccessToken))
        {
            throw new InvalidOperationException("Access token is not set. Please authenticate first.");
        }

        // 检查是否需要刷新 Token
        if (_options.ExpireTime.HasValue && DateTimeOffset.Now >= _options.ExpireTime.Value.AddMinutes(-5))
        {
            await RefreshTokenAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RefreshTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.RefreshToken))
        {
            _logger.LogWarning("没有刷新令牌，无法刷新 Token");
            return;
        }

        await _tokenRefreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // 再次检查，防止其他线程已经刷新过
            if (_options.ExpireTime.HasValue && DateTimeOffset.Now < _options.ExpireTime.Value.AddMinutes(-5))
            {
                return;
            }

            _logger.LogDebug("刷新 Inoreader Token");

            var result = await InoreaderAuthHelper.RefreshTokenAsync(
                _options.RefreshToken,
                _options,
                _httpClient,
                cancellationToken).ConfigureAwait(false);

            _options.AccessToken = result.AccessToken;
            _options.RefreshToken = result.RefreshToken;
            _options.ExpireTime = result.ExpireTime;

            // 通知应用层保存新的 Token
            _options.OnTokenUpdated?.Invoke(result);

            _logger.LogInformation("Token 刷新成功，新过期时间: {ExpireTime}", result.ExpireTime);
        }
        finally
        {
            _tokenRefreshLock.Release();
        }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取订阅源 {FeedName} 失败", feed.Name);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<bool> MarkStreamAsReadAsync(string streamId, CancellationToken cancellationToken)
    {
        await EnsureTokenValidAsync(cancellationToken).ConfigureAwait(false);

        var request = CreateRequest("/mark-all-as-read", HttpMethod.Post);
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["s"] = streamId,
            ["ts"] = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
        });
        request.Content = content;

        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var success = responseText.Contains("OK", StringComparison.OrdinalIgnoreCase);
        if (success)
        {
            _logger.LogInformation("流 {StreamId} 的所有文章已标记为已读", streamId);
        }
        else
        {
            _logger.LogWarning("标记流 {StreamId} 为已读失败", streamId);
        }

        return success;
    }

    private static string ExtractGroupName(string groupId)
    {
        // 分组 ID 格式: user/-/label/分组名
        var parts = groupId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[^1] : groupId;
    }

    private static RssArticle ConvertToArticle(InoreaderArticleItem item, string feedId)
    {
        var article = new RssArticle
        {
            Id = item.Id,
            FeedId = feedId,
            Title = WebUtility.HtmlDecode(item.Title ?? string.Empty),
            Url = item.Canonical?.FirstOrDefault()?.Href ?? item.Alternate?.FirstOrDefault()?.Href,
            Author = item.Author,
            Content = item.Content?.Content ?? item.Summary?.Content,
            Summary = ExtractSummary(item),
            CoverUrl = ExtractCover(item),
        };

        // 设置发布时间
        if (item.Published > 0)
        {
            var publishTime = DateTimeOffset.FromUnixTimeSeconds(item.Published);
            article.SetPublishTime(publishTime);
        }

        // 设置标签
        if (item.Categories != null)
        {
            var tags = item.Categories
                .Where(c => c.StartsWith("user/-/label/", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Replace("user/-/label/", string.Empty, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (tags.Count > 0)
            {
                article.SetTagList(tags);
            }
        }

        return article;
    }

    private static string? ExtractSummary(InoreaderArticleItem item)
    {
        var content = item.Summary?.Content ?? item.Content?.Content;
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        // 移除 HTML 标签，截取摘要
        var text = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]*>", string.Empty);
        text = WebUtility.HtmlDecode(text);
        text = text.Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Trim();

        return text.Length > 300 ? text[..300] + "..." : text;
    }

    private static string? ExtractCover(InoreaderArticleItem item)
    {
        var content = item.Summary?.Content ?? item.Content?.Content;
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        // 尝试从内容中提取第一张图片
        var match = System.Text.RegularExpressions.Regex.Match(content, @"<img[^>]+src=""([^""]+)""");
        return match.Success ? match.Groups[1].Value : null;
    }
}
