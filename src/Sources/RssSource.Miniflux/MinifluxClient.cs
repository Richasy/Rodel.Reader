// Copyright (c) Richasy. All rights reserved.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Richasy.RodelReader.Sources.Rss.Miniflux.Internal;

namespace Richasy.RodelReader.Sources.Rss.Miniflux;

/// <summary>
/// Miniflux RSS 客户端.
/// 通过 Miniflux API 管理订阅源和文章.
/// </summary>
public sealed partial class MinifluxClient : IRssClient
{
    private readonly MinifluxClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MinifluxClient> _logger;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;
    private bool _isAuthenticated;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinifluxClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端（可选，用于测试注入）.</param>
    /// <param name="logger">日志记录器.</param>
    public MinifluxClient(
        MinifluxClientOptions options,
        HttpClient? httpClient = null,
        ILogger<MinifluxClient>? logger = null)
    {
        _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<MinifluxClient>.Instance;
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
        _httpClient = httpClient ?? HttpClientHelper.CreateHttpClient(_options.Timeout);
    }

    /// <inheritdoc/>
    public IRssSourceCapabilities Capabilities => KnownRssSources.Miniflux;

    /// <inheritdoc/>
    public bool IsAuthenticated => _isAuthenticated;

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("开始 Miniflux 登录验证");

        if (!_options.HasValidCredentials)
        {
            _logger.LogWarning("未提供有效的认证信息（API Token 或用户名密码）");
            return false;
        }

        try
        {
            var request = CreateAuthenticatedRequest("/v1/me", HttpMethod.Get);
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _isAuthenticated = true;
                _logger.LogInformation("Miniflux 登录验证成功");
                return true;
            }

            _logger.LogWarning("Miniflux 登录验证失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Miniflux 登录验证过程中发生异常");
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<bool> SignOutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Miniflux 登出");

        _isAuthenticated = false;

        _logger.LogInformation("Miniflux 已登出");
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RssFeedGroup> Groups, IReadOnlyList<RssFeed> Feeds)> GetFeedListAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("获取 Miniflux 订阅源列表");

        EnsureAuthenticated();

        var request = CreateAuthenticatedRequest("/v1/feeds", HttpMethod.Get);
        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var feeds = JsonSerializer.Deserialize(content, MinifluxJsonContext.Default.ListMinifluxFeed)
            ?? throw new InvalidOperationException("Failed to parse feeds response.");

        var groups = new List<RssFeedGroup>();
        var rssFeeds = new List<RssFeed>();

        foreach (var feed in feeds)
        {
            var rssFeed = new RssFeed
            {
                Id = feed.Id.ToString(),
                Name = WebUtility.HtmlDecode(feed.Title),
                Url = feed.FeedUrl ?? string.Empty,
                Website = feed.SiteUrl,
            };

            // 处理分类
            if (feed.Category != null)
            {
                rssFeed.SetGroupIdList([feed.Category.Id.ToString()]);

                // 收集分类信息
                if (!groups.Any(g => g.Id == feed.Category.Id.ToString()))
                {
                    groups.Add(new RssFeedGroup
                    {
                        Id = feed.Category.Id.ToString(),
                        Name = feed.Category.Title,
                    });
                }
            }

            rssFeeds.Add(rssFeed);
        }

        _logger.LogInformation("获取到 {GroupCount} 个分组和 {FeedCount} 个订阅源", groups.Count, rssFeeds.Count);

        return (groups, rssFeeds);
    }

    /// <inheritdoc/>
    public async Task<RssFeedDetail?> GetFeedDetailAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("获取订阅源详情: {FeedName} (Id: {FeedId})", feed.Name, feed.Id);

        EnsureAuthenticated();

        var path = $"/v1/feeds/{feed.Id}/entries";
        var query = $"limit={_options.ArticlesPerRequest}&order=published_at&direction=desc";
        var request = CreateAuthenticatedRequest(path, HttpMethod.Get, query);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var data = JsonSerializer.Deserialize(content, MinifluxJsonContext.Default.MinifluxEntriesResponse);

            if (data == null)
            {
                _logger.LogWarning("无法解析订阅源 {FeedName} 的响应", feed.Name);
                return null;
            }

            var articles = new List<RssArticle>();
            foreach (var entry in data.Entries)
            {
                var article = ConvertToArticle(entry, feed);
                articles.Add(article);
            }

            _logger.LogInformation("获取订阅源 {FeedName} 的 {ArticleCount} 篇文章", feed.Name, articles.Count);

            return new RssFeedDetail
            {
                Feed = feed,
                Articles = articles,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取订阅源 {FeedName} 详情失败", feed.Name);
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

        EnsureAuthenticated();

        var request = CreateAuthenticatedRequest("/v1/categories", HttpMethod.Post);
        var createRequest = new MinifluxCreateCategoryRequest
        {
            Title = group.Name,
        };
        request.Content = JsonContent.Create(createRequest, MinifluxJsonContext.Default.MinifluxCreateCategoryRequest);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var category = JsonSerializer.Deserialize(content, MinifluxJsonContext.Default.MinifluxCategory);

            if (category == null)
            {
                _logger.LogWarning("添加分组失败：无法解析响应");
                return null;
            }

            _logger.LogInformation("分组 {GroupName} 添加成功，ID: {GroupId}", category.Title, category.Id);

            return new RssFeedGroup
            {
                Id = category.Id.ToString(),
                Name = category.Title,
            };
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

        var request = CreateAuthenticatedRequest($"/v1/categories/{group.Id}", HttpMethod.Put);
        var updateRequest = new MinifluxUpdateCategoryRequest
        {
            Title = group.Name,
        };
        request.Content = JsonContent.Create(updateRequest, MinifluxJsonContext.Default.MinifluxUpdateCategoryRequest);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var category = JsonSerializer.Deserialize(content, MinifluxJsonContext.Default.MinifluxCategory);

            if (category == null)
            {
                _logger.LogWarning("更新分组失败：无法解析响应");
                return null;
            }

            _logger.LogInformation("分组 {GroupName} 更新成功", category.Title);

            return new RssFeedGroup
            {
                Id = category.Id.ToString(),
                Name = category.Title,
            };
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

        var request = CreateAuthenticatedRequest($"/v1/categories/{group.Id}", HttpMethod.Delete);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent || response.IsSuccessStatusCode)
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

        // 第一步：创建订阅源
        var request = CreateAuthenticatedRequest("/v1/feeds", HttpMethod.Post);
        var createRequest = new MinifluxCreateFeedRequest
        {
            FeedUrl = feed.Url,
        };

        // 如果有分组，设置分类 ID
        var groupIds = feed.GetGroupIdList();
        if (groupIds.Count > 0 && long.TryParse(groupIds[0], out var categoryId))
        {
            createRequest.CategoryId = categoryId;
        }

        request.Content = JsonContent.Create(createRequest, MinifluxJsonContext.Default.MinifluxCreateFeedRequest);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var createResponse = JsonSerializer.Deserialize(content, MinifluxJsonContext.Default.MinifluxCreateFeedResponse);

            if (createResponse == null)
            {
                _logger.LogWarning("添加订阅源失败：无法解析响应");
                return null;
            }

            // 第二步：获取订阅源详细信息
            var getRequest = CreateAuthenticatedRequest($"/v1/feeds/{createResponse.FeedId}", HttpMethod.Get);
            var getResponse = await SendRequestAsync(getRequest, cancellationToken).ConfigureAwait(false);
            var getContent = await getResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var newFeed = JsonSerializer.Deserialize(getContent, MinifluxJsonContext.Default.MinifluxFeed);

            if (newFeed == null)
            {
                _logger.LogWarning("获取新订阅源信息失败");
                return null;
            }

            var resultFeed = new RssFeed
            {
                Id = newFeed.Id.ToString(),
                Name = WebUtility.HtmlDecode(newFeed.Title),
                Url = newFeed.FeedUrl ?? string.Empty,
                Website = newFeed.SiteUrl,
            };

            if (newFeed.Category != null)
            {
                resultFeed.SetGroupIdList([newFeed.Category.Id.ToString()]);
            }

            _logger.LogInformation("订阅源 {FeedName} 添加成功，ID: {FeedId}", resultFeed.Name, resultFeed.Id);

            return resultFeed;
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

        var request = CreateAuthenticatedRequest($"/v1/feeds/{oldFeed.Id}", HttpMethod.Put);
        var updateRequest = new MinifluxUpdateFeedRequest
        {
            Title = newFeed.Name,
        };

        // 如果有分组变更，设置新的分类 ID
        var groupIds = newFeed.GetGroupIdList();
        if (groupIds.Count > 0 && long.TryParse(groupIds[0], out var categoryId))
        {
            updateRequest.CategoryId = categoryId;
        }

        request.Content = JsonContent.Create(updateRequest, MinifluxJsonContext.Default.MinifluxUpdateFeedRequest);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("订阅源 {FeedName} 更新成功", newFeed.Name);
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

        var request = CreateAuthenticatedRequest($"/v1/feeds/{feed.Id}", HttpMethod.Delete);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent || response.IsSuccessStatusCode)
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
        var ids = articleIds.ToList();
        if (ids.Count == 0)
        {
            return true;
        }

        _logger.LogDebug("标记 {Count} 篇文章为已读", ids.Count);

        EnsureAuthenticated();

        var request = CreateAuthenticatedRequest("/v1/entries", HttpMethod.Put);
        var updateRequest = new MinifluxUpdateEntriesRequest
        {
            EntryIds = ids.Select(id => long.TryParse(id, out var lid) ? lid : 0).Where(id => id > 0).ToList(),
            Status = "read",
        };
        request.Content = JsonContent.Create(updateRequest, MinifluxJsonContext.Default.MinifluxUpdateEntriesRequest);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent || response.IsSuccessStatusCode)
            {
                _logger.LogInformation("成功标记 {Count} 篇文章为已读", ids.Count);
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

        _logger.LogDebug("标记订阅源 {FeedName} 的所有文章为已读", feed.Name);

        EnsureAuthenticated();

        var request = CreateAuthenticatedRequest($"/v1/feeds/{feed.Id}/mark-all-as-read", HttpMethod.Put);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent || response.IsSuccessStatusCode)
            {
                _logger.LogInformation("订阅源 {FeedName} 的所有文章已标记为已读", feed.Name);
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

        _logger.LogDebug("标记分组 {GroupName} 的所有文章为已读", group.Name);

        EnsureAuthenticated();

        var request = CreateAuthenticatedRequest($"/v1/categories/{group.Id}/mark-all-as-read", HttpMethod.Put);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent || response.IsSuccessStatusCode)
            {
                _logger.LogInformation("分组 {GroupName} 的所有文章已标记为已读", group.Name);
                return true;
            }

            _logger.LogWarning("标记分组已读失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记分组 {GroupName} 已读失败", group.Name);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ImportOpmlAsync(
        string opmlContent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(opmlContent))
        {
            _logger.LogWarning("OPML 内容为空");
            return false;
        }

        _logger.LogDebug("导入 OPML");

        EnsureAuthenticated();

        var request = CreateAuthenticatedRequest("/v1/import", HttpMethod.Post);
        request.Content = new StringContent(opmlContent, Encoding.UTF8, "application/xml");

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Created || response.IsSuccessStatusCode)
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

        var request = CreateAuthenticatedRequest("/v1/export", HttpMethod.Get);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (content.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("<opml", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("OPML 导出成功");
                return content;
            }

            _logger.LogWarning("导出的内容不是有效的 OPML");
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
            throw new InvalidOperationException("客户端未经认证，请先调用 SignInAsync 方法。");
        }
    }

    private HttpRequestMessage CreateAuthenticatedRequest(string path, HttpMethod method, string? query = null)
    {
        var baseUrl = _options.GetServerBaseUrl().ToString().TrimEnd('/');
        var url = query != null
            ? $"{baseUrl}{path}?{query}"
            : $"{baseUrl}{path}";

        var request = new HttpRequestMessage(method, url);

        // 优先使用 API Token
        if (_options.HasApiToken)
        {
            request.Headers.Add("X-Auth-Token", _options.ApiToken);
        }
        else if (_options.HasBasicAuth)
        {
            var token = _options.GenerateBasicAuthToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

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

    private async Task FetchFeedWithSemaphoreAsync(
        RssFeed feed,
        List<RssFeedDetail> results,
        CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
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
            _logger.LogWarning(ex, "获取订阅源 {FeedName} 详情时出错", feed.Name);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static RssArticle ConvertToArticle(MinifluxEntry entry, RssFeed feed)
    {
        var article = new RssArticle
        {
            Id = entry.Id.ToString(),
            FeedId = feed.Id,
            Title = WebUtility.HtmlDecode(entry.Title),
            Url = entry.Url,
            Author = entry.Author ?? feed.Name,
            Content = entry.Content,
            Summary = ExtractSummary(entry.Content, 300),
            CoverUrl = ExtractCoverUrl(entry.Content),
        };

        article.SetPublishTime(entry.PublishedAt);

        if (entry.Tags?.Count > 0)
        {
            article.SetTagList(entry.Tags);
        }

        return article;
    }

    private static string? ExtractSummary(string? content, int maxLength)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        // 移除 HTML 标签
        var text = StripHtmlTagsRegex().Replace(content, string.Empty);

        // 解码 HTML 实体
        text = WebUtility.HtmlDecode(text);

        // 清理空白字符
        text = CleanWhitespaceRegex().Replace(text, " ").Trim();

        if (text.Length <= maxLength)
        {
            return text;
        }

        return text[..maxLength] + "...";
    }

    private static string? ExtractCoverUrl(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        var match = ImgSrcRegex().Match(content);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex StripHtmlTagsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex CleanWhitespaceRegex();

    [GeneratedRegex(@"<img[^>]+src\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ImgSrcRegex();

    #endregion
}
