// Copyright (c) Richasy. All rights reserved.

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Richasy.RodelReader.Sources.Rss.GoogleReader.Internal;

namespace Richasy.RodelReader.Sources.Rss.GoogleReader;

/// <summary>
/// Google Reader API 兼容客户端.
/// 可用于连接 FreshRSS、Tiny Tiny RSS 等支持 Google Reader API 的服务.
/// </summary>
public sealed partial class GoogleReaderClient : IRssClient
{
    private readonly GoogleReaderClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleReaderClient> _logger;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleReaderClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端（可选，用于测试注入）.</param>
    /// <param name="logger">日志记录器.</param>
    public GoogleReaderClient(
        GoogleReaderClientOptions options,
        HttpClient? httpClient = null,
        ILogger<GoogleReaderClient>? logger = null)
    {
        _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<GoogleReaderClient>.Instance;
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
        _httpClient = httpClient ?? HttpClientHelper.CreateHttpClient(_options.Timeout);
    }

    /// <inheritdoc/>
    public IRssSourceCapabilities Capabilities => KnownRssSources.GoogleReader;

    /// <inheritdoc/>
    public bool IsAuthenticated => !string.IsNullOrEmpty(_options.AuthToken);

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("开始 Google Reader 登录");

        if (string.IsNullOrEmpty(_options.UserName) || string.IsNullOrEmpty(_options.Password))
        {
            _logger.LogWarning("用户名或密码为空，无法登录");
            return false;
        }

        try
        {
            var loginUrl = BuildUrl("/accounts/ClientLogin");
            var request = new HttpRequestMessage(HttpMethod.Post, loginUrl);

            var formData = new Dictionary<string, string>
            {
                ["Email"] = _options.UserName,
                ["Passwd"] = _options.Password,
                ["client"] = "RodelReader",
                ["accountType"] = "HOSTED_OR_GOOGLE",
                ["service"] = "reader",
            };

            request.Content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var authToken = ParseAuthToken(responseContent);

            if (string.IsNullOrEmpty(authToken))
            {
                _logger.LogWarning("登录响应中未找到认证令牌");
                return false;
            }

            _options.AuthToken = authToken;
            _options.OnTokenUpdated?.Invoke(authToken);

            _logger.LogInformation("Google Reader 登录成功");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Reader 登录失败");
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<bool> SignOutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Google Reader 登出");

        _options.AuthToken = null;

        _logger.LogInformation("Google Reader 已登出");
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RssFeedGroup> Groups, IReadOnlyList<RssFeed> Feeds)> GetFeedListAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("获取 Google Reader 订阅源列表");

        EnsureAuthenticated();

        // 获取订阅列表
        var subscriptionRequest = CreateRequest("/reader/api/0/subscription/list", HttpMethod.Get);
        var subscriptionResponse = await SendRequestAsync(subscriptionRequest, cancellationToken).ConfigureAwait(false);
        var subscriptionContent = await subscriptionResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var subscriptionData = JsonSerializer.Deserialize(subscriptionContent, GoogleReaderJsonContext.Default.GoogleReaderSubscriptionResponse)
            ?? throw new InvalidOperationException("Failed to parse subscription response.");

        // 解析分组和订阅源
        var groups = new List<RssFeedGroup>();
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

                // 收集分组信息
                foreach (var category in sub.Categories)
                {
                    if (!groups.Any(g => g.Id == category.Id))
                    {
                        groups.Add(new RssFeedGroup
                        {
                            Id = category.Id,
                            Name = category.Label ?? ExtractGroupName(category.Id),
                        });
                    }
                }
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

        EnsureAuthenticated();

        var path = $"/reader/api/0/stream/contents/{Uri.EscapeDataString(feed.Id)}";
        var request = CreateRequest(path, HttpMethod.Get, $"n={_options.ArticlesPerRequest}");
        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var data = JsonSerializer.Deserialize(content, GoogleReaderJsonContext.Default.GoogleReaderStreamContentResponse);

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
        // Google Reader API 不支持直接创建分组
        // 分组会在添加订阅源时通过指定 categories 自动创建
        _logger.LogWarning("Google Reader API 不支持直接创建分组，分组会在添加订阅源时自动创建");
        throw new NotSupportedException("Google Reader API does not support creating groups directly. Groups are created when adding feeds with categories.");
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> UpdateGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("重命名分组: {GroupId} -> {GroupName}", group.Id, group.Name);

        EnsureAuthenticated();

        var request = CreateRequest("/reader/api/0/rename-tag", HttpMethod.Post);
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

        EnsureAuthenticated();

        var request = CreateRequest("/reader/api/0/disable-tag", HttpMethod.Post);
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

        EnsureAuthenticated();

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

        var request = CreateRequest("/reader/api/0/subscription/edit", HttpMethod.Post);
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

        EnsureAuthenticated();

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

        var request = CreateRequest("/reader/api/0/subscription/edit", HttpMethod.Post);
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

        EnsureAuthenticated();

        var request = CreateRequest("/reader/api/0/subscription/edit", HttpMethod.Post);
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

        EnsureAuthenticated();

        var query = new List<KeyValuePair<string, string>>
        {
            new("a", "user/-/state/com.google/read"),
        };

        foreach (var id in ids)
        {
            query.Add(new KeyValuePair<string, string>("i", id));
        }

        var request = CreateRequest("/reader/api/0/edit-tag", HttpMethod.Post);
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

        EnsureAuthenticated();

        var request = CreateRequest("/reader/api/0/subscription/import", HttpMethod.Post);
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

        EnsureAuthenticated();

        // 尝试使用服务端导出
        try
        {
            var request = CreateRequest("/reader/api/0/subscription/export", HttpMethod.Get);
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (content.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("<opml", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("从服务端导出 OPML 成功");
                return content;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "服务端 OPML 导出失败，尝试本地生成");
        }

        // 本地生成 OPML
        var (groups, feeds) = await GetFeedListAsync(cancellationToken).ConfigureAwait(false);
        var opml = OpmlHelper.GenerateOpml(groups, feeds, "Google Reader Subscriptions");

        _logger.LogInformation("本地生成 OPML 成功");
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

    private static string? ParseAuthToken(string response)
    {
        // 尝试解析 JSON 格式响应
        if (response.StartsWith('{'))
        {
            try
            {
                var authResponse = JsonSerializer.Deserialize(response, GoogleReaderJsonContext.Default.GoogleReaderAuthResponse);
                return authResponse?.Auth;
            }
            catch
            {
                // 忽略 JSON 解析错误，尝试文本格式
            }
        }

        // 尝试解析文本格式响应 (Auth=xxxx)
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("Auth=", StringComparison.OrdinalIgnoreCase))
            {
                return line[5..].Trim();
            }
        }

        return null;
    }

    private static string ExtractGroupName(string groupId)
    {
        // 分组 ID 格式: user/-/label/分组名 或 user/xxx/label/分组名
        var match = LabelRegex().Match(groupId);
        return match.Success ? match.Groups[1].Value : groupId;
    }

    private static RssArticle ConvertToArticle(GoogleReaderArticleItem item, string feedId)
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
                .Where(c => c.StartsWith("user/-/label/", StringComparison.OrdinalIgnoreCase) ||
                           c.Contains("/label/", StringComparison.OrdinalIgnoreCase))
                .Select(c =>
                {
                    var match = LabelRegex().Match(c);
                    return match.Success ? match.Groups[1].Value : c;
                })
                .Distinct()
                .ToList();

            if (tags.Count > 0)
            {
                article.SetTagList(tags);
            }
        }

        return article;
    }

    private static string? ExtractSummary(GoogleReaderArticleItem item)
    {
        var content = item.Summary?.Content ?? item.Content?.Content;
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        // 移除 HTML 标签，截取摘要
        var text = HtmlTagRegex().Replace(content, string.Empty);
        text = WebUtility.HtmlDecode(text);
        text = text.Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Trim();

        return text.Length > 300 ? text[..300] + "..." : text;
    }

    private static string? ExtractCover(GoogleReaderArticleItem item)
    {
        var content = item.Summary?.Content ?? item.Content?.Content;
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        // 尝试从内容中提取第一张图片
        var match = ImgSrcRegex().Match(content);
        return match.Success ? match.Groups[1].Value : null;
    }

    private void EnsureAuthenticated()
    {
        if (string.IsNullOrEmpty(_options.AuthToken))
        {
            throw new InvalidOperationException("未认证，请先调用 SignInAsync 方法进行登录。");
        }
    }

    private HttpRequestMessage CreateRequest(string path, HttpMethod method, string? query = null)
    {
        var url = BuildUrl(path, query);
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Authorization", $"GoogleLogin auth={_options.AuthToken}");
        return request;
    }

    private Uri BuildUrl(string path, string? query = null)
    {
        var baseUrl = _options.Server.TrimEnd('/');
        var urlBuilder = new StringBuilder(baseUrl);

        // 确保 path 开头有斜杠
        if (!path.StartsWith('/'))
        {
            urlBuilder.Append('/');
        }

        urlBuilder.Append(path);
        urlBuilder.Append("?output=json");

        if (!string.IsNullOrEmpty(query))
        {
            urlBuilder.Append('&');
            urlBuilder.Append(query);
        }

        return new Uri(urlBuilder.ToString());
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return response;
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
        EnsureAuthenticated();

        var request = CreateRequest("/reader/api/0/mark-all-as-read", HttpMethod.Post);
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

    [GeneratedRegex(@"/label/([^/]+)$")]
    private static partial Regex LabelRegex();

    [GeneratedRegex(@"<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"<img[^>]+src=""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex ImgSrcRegex();
}
