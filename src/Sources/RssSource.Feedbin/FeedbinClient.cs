// Copyright (c) Richasy. All rights reserved.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Richasy.RodelReader.Sources.Rss.Feedbin.Internal;

namespace Richasy.RodelReader.Sources.Rss.Feedbin;

/// <summary>
/// Feedbin RSS 客户端.
/// 通过 Feedbin API 管理订阅源和文章.
/// </summary>
public sealed partial class FeedbinClient : IRssClient
{
    private readonly FeedbinClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FeedbinClient> _logger;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;
    private string? _authToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbinClient"/> class.
    /// </summary>
    /// <param name="options">客户端配置选项.</param>
    /// <param name="httpClient">HTTP 客户端（可选，用于测试注入）.</param>
    /// <param name="logger">日志记录器.</param>
    public FeedbinClient(
        FeedbinClientOptions options,
        HttpClient? httpClient = null,
        ILogger<FeedbinClient>? logger = null)
    {
        _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<FeedbinClient>.Instance;
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
        _httpClient = httpClient ?? CreateFeedbinHttpClient(_options.Timeout);
    }

    /// <inheritdoc/>
    public IRssSourceCapabilities Capabilities => KnownRssSources.Feedbin;

    /// <inheritdoc/>
    public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);

    /// <inheritdoc/>
    public async Task<bool> SignInAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("开始 Feedbin 登录验证");

        if (string.IsNullOrEmpty(_options.UserName) || string.IsNullOrEmpty(_options.Password))
        {
            _logger.LogWarning("用户名或密码为空，无法登录");
            return false;
        }

        try
        {
            var token = _options.GenerateBasicAuthToken();
            var request = CreateRequest("/authentication.json", HttpMethod.Get, token);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _authToken = token;
                _logger.LogInformation("Feedbin 登录验证成功");
                return true;
            }

            _logger.LogWarning("Feedbin 登录验证失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feedbin 登录验证过程中发生异常");
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<bool> SignOutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Feedbin 登出");

        _authToken = null;

        _logger.LogInformation("Feedbin 已登出");
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RssFeedGroup> Groups, IReadOnlyList<RssFeed> Feeds)> GetFeedListAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("获取 Feedbin 订阅源列表");

        EnsureAuthenticated();

        // 并行获取订阅和标签
        var subscriptionRequest = CreateAuthenticatedRequest("/subscriptions.json", HttpMethod.Get);
        var taggingRequest = CreateAuthenticatedRequest("/taggings.json", HttpMethod.Get);

        _logger.LogDebug("发送订阅列表请求");
        var subscriptionResponse = await SendRequestAsync(subscriptionRequest, cancellationToken).ConfigureAwait(false);
        var subscriptionContent = await subscriptionResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var subscriptions = JsonSerializer.Deserialize(subscriptionContent, FeedbinJsonContext.Default.ListFeedbinSubscription)
            ?? throw new InvalidOperationException("Failed to parse subscription response.");

        _logger.LogDebug("获取到 {Count} 个订阅", subscriptions.Count);

        _logger.LogDebug("发送标签列表请求");
        var taggingResponse = await SendRequestAsync(taggingRequest, cancellationToken).ConfigureAwait(false);
        var taggingContent = await taggingResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var taggings = JsonSerializer.Deserialize(taggingContent, FeedbinJsonContext.Default.ListFeedbinTagging) ?? [];

        _logger.LogDebug("获取到 {Count} 个标签关联", taggings.Count);

        // 构建分组列表（从标签名称去重）
        var groups = taggings
            .Select(t => t.Name)
            .Distinct()
            .Select(name => new RssFeedGroup
            {
                Id = name,
                Name = name,
            })
            .ToList();

        // 构建订阅源列表
        var feeds = new List<RssFeed>();
        foreach (var sub in subscriptions)
        {
            var feed = new RssFeed
            {
                // 使用 feed_id 作为主 ID（获取文章用）
                Id = sub.FeedId.ToString(),
                Name = WebUtility.HtmlDecode(sub.Title),
                Url = sub.FeedUrl ?? string.Empty,
                Website = sub.SiteUrl,
                // 使用 Comment 字段存储 subscription_id（删除/更新用）
                Comment = sub.Id.ToString(),
            };

            // 设置该订阅源所属的分组
            var feedTaggings = taggings.Where(t => t.FeedId == sub.FeedId).ToList();
            if (feedTaggings.Count > 0)
            {
                feed.SetGroupIdList(feedTaggings.Select(t => t.Name));
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

        _logger.LogDebug("获取订阅源详情: {FeedName} (FeedId: {FeedId})", feed.Name, feed.Id);

        EnsureAuthenticated();

        var path = $"/feeds/{feed.Id}/entries.json";
        var query = $"per_page={_options.ArticlesPerRequest}";
        var request = CreateAuthenticatedRequest(path, HttpMethod.Get, query);

        try
        {
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var entries = JsonSerializer.Deserialize(content, FeedbinJsonContext.Default.ListFeedbinEntry);

            if (entries == null)
            {
                _logger.LogWarning("无法解析订阅源 {FeedName} 的响应", feed.Name);
                return null;
            }

            var articles = new List<RssArticle>();
            foreach (var entry in entries)
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
            _logger.LogError(ex, "获取订阅源 {FeedName} 失败", feed.Name);
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
    public Task<RssFeedGroup?> AddGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        // Feedbin 不支持直接创建分组
        // 分组会在创建 tagging 时自动创建
        _logger.LogWarning("Feedbin 不支持直接创建分组，分组会在添加订阅源到标签时自动创建");
        throw new NotSupportedException("Feedbin does not support creating groups directly. Groups are created when adding taggings to feeds.");
    }

    /// <inheritdoc/>
    public Task<RssFeedGroup?> UpdateGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        // Feedbin 不支持重命名分组
        // 需要删除所有旧标签并创建新标签
        _logger.LogWarning("Feedbin 不支持重命名分组");
        throw new NotSupportedException("Feedbin does not support renaming groups.");
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("删除分组: {GroupName}", group.Name);

        EnsureAuthenticated();

        // 获取所有属于该分组的 tagging
        var taggingRequest = CreateAuthenticatedRequest("/taggings.json", HttpMethod.Get);
        var taggingResponse = await SendRequestAsync(taggingRequest, cancellationToken).ConfigureAwait(false);
        var taggingContent = await taggingResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var taggings = JsonSerializer.Deserialize(taggingContent, FeedbinJsonContext.Default.ListFeedbinTagging) ?? [];

        var groupTaggings = taggings.Where(t => t.Name == group.Name || t.Name == group.Id).ToList();

        if (groupTaggings.Count == 0)
        {
            _logger.LogInformation("分组 {GroupName} 不存在或已为空", group.Name);
            return true;
        }

        _logger.LogDebug("需要删除 {Count} 个标签关联", groupTaggings.Count);

        // 删除所有 tagging
        var successCount = 0;
        foreach (var tagging in groupTaggings)
        {
            try
            {
                var deleteRequest = CreateAuthenticatedRequest($"/taggings/{tagging.Id}.json", HttpMethod.Delete);
                var deleteResponse = await _httpClient.SendAsync(deleteRequest, cancellationToken).ConfigureAwait(false);

                if (deleteResponse.IsSuccessStatusCode || deleteResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    successCount++;
                }
                else
                {
                    _logger.LogWarning("删除标签关联 {TaggingId} 失败，状态码: {StatusCode}", tagging.Id, deleteResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除标签关联 {TaggingId} 时发生异常", tagging.Id);
            }
        }

        var success = successCount == groupTaggings.Count;
        if (success)
        {
            _logger.LogInformation("分组 {GroupName} 删除成功", group.Name);
        }
        else
        {
            _logger.LogWarning("分组 {GroupName} 部分删除成功 ({SuccessCount}/{TotalCount})", group.Name, successCount, groupTaggings.Count);
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

        // 第一步：创建订阅
        var createRequest = CreateAuthenticatedRequest("/subscriptions.json", HttpMethod.Post);
        var createData = new FeedbinCreateSubscriptionRequest { FeedUrl = feed.Url };
        createRequest.Content = JsonContent.Create(createData, FeedbinJsonContext.Default.FeedbinCreateSubscriptionRequest);
        createRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

        FeedbinSubscription? subscription = null;

        try
        {
            var createResponse = await _httpClient.SendAsync(createRequest, cancellationToken).ConfigureAwait(false);

            // 201 Created 或 302 Found 都表示成功
            if (createResponse.StatusCode == HttpStatusCode.Created ||
                createResponse.StatusCode == HttpStatusCode.Found ||
                createResponse.IsSuccessStatusCode)
            {
                var responseContent = await createResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                // 如果响应内容不为空，尝试解析
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    subscription = JsonSerializer.Deserialize(responseContent, FeedbinJsonContext.Default.FeedbinSubscription);
                }
                else
                {
                    // 响应内容为空，可能订阅已存在，需要从订阅列表中查找
                    _logger.LogDebug("创建订阅响应内容为空，尝试从订阅列表中查找");
                    var subscriptionsRequest = CreateAuthenticatedRequest("/subscriptions.json", HttpMethod.Get);
                    var subscriptionsResponse = await _httpClient.SendAsync(subscriptionsRequest, cancellationToken).ConfigureAwait(false);
                    if (subscriptionsResponse.IsSuccessStatusCode)
                    {
                        var subscriptionsContent = await subscriptionsResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(subscriptionsContent))
                        {
                            var subscriptions = JsonSerializer.Deserialize(subscriptionsContent, FeedbinJsonContext.Default.ListFeedbinSubscription);
                            subscription = subscriptions?.FirstOrDefault(s =>
                                s.FeedUrl?.Equals(feed.Url, StringComparison.OrdinalIgnoreCase) == true);
                        }
                    }
                }
            }
            else if (createResponse.StatusCode == HttpStatusCode.MultipleChoices)
            {
                // 300 Multiple Choices - 发现多个 feed
                var responseContent = await createResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var discoveredFeeds = JsonSerializer.Deserialize(responseContent, FeedbinJsonContext.Default.ListFeedbinDiscoveredFeed);

                if (discoveredFeeds?.Count > 0)
                {
                    _logger.LogWarning("发现多个 Feed，使用第一个: {FeedUrl}", discoveredFeeds[0].FeedUrl);

                    // 重新使用第一个发现的 feed URL 创建订阅
                    var retryRequest = CreateAuthenticatedRequest("/subscriptions.json", HttpMethod.Post);
                    var retryData = new FeedbinCreateSubscriptionRequest { FeedUrl = discoveredFeeds[0].FeedUrl ?? feed.Url };
                    retryRequest.Content = JsonContent.Create(retryData, FeedbinJsonContext.Default.FeedbinCreateSubscriptionRequest);
                    retryRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

                    var retryResponse = await _httpClient.SendAsync(retryRequest, cancellationToken).ConfigureAwait(false);
                    if (retryResponse.IsSuccessStatusCode || retryResponse.StatusCode == HttpStatusCode.Created)
                    {
                        var retryContent = await retryResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        subscription = JsonSerializer.Deserialize(retryContent, FeedbinJsonContext.Default.FeedbinSubscription);
                    }
                }
            }
            else
            {
                _logger.LogWarning("创建订阅失败，状态码: {StatusCode}", createResponse.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建订阅时发生异常");
            return null;
        }

        if (subscription == null)
        {
            _logger.LogWarning("无法解析创建的订阅");
            return null;
        }

        _logger.LogDebug("订阅创建成功，FeedId: {FeedId}, SubscriptionId: {SubscriptionId}", subscription.FeedId, subscription.Id);

        // 第二步：如果标题不同，更新标题
        if (!string.IsNullOrEmpty(feed.Name) && !subscription.Title.Equals(feed.Name, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("更新订阅标题: {OldTitle} -> {NewTitle}", subscription.Title, feed.Name);

            var updateRequest = CreateAuthenticatedRequest($"/subscriptions/{subscription.Id}.json", HttpMethod.Patch);
            var updateData = new FeedbinUpdateSubscriptionRequest { Title = feed.Name };
            updateRequest.Content = JsonContent.Create(updateData, FeedbinJsonContext.Default.FeedbinUpdateSubscriptionRequest);
            updateRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

            try
            {
                var updateResponse = await _httpClient.SendAsync(updateRequest, cancellationToken).ConfigureAwait(false);
                if (updateResponse.IsSuccessStatusCode)
                {
                    subscription.Title = feed.Name;
                    _logger.LogDebug("标题更新成功");
                }
                else
                {
                    _logger.LogWarning("更新标题失败，状态码: {StatusCode}", updateResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新标题时发生异常");
            }
        }

        // 第三步：添加标签/分组
        var groupIds = feed.GetGroupIdList();
        if (groupIds.Count > 0)
        {
            _logger.LogDebug("为订阅添加 {Count} 个分组", groupIds.Count);

            foreach (var groupId in groupIds)
            {
                try
                {
                    var taggingRequest = CreateAuthenticatedRequest("/taggings.json", HttpMethod.Post);
                    var taggingData = new FeedbinCreateTaggingRequest
                    {
                        FeedId = subscription.FeedId,
                        Name = groupId,
                    };
                    taggingRequest.Content = JsonContent.Create(taggingData, FeedbinJsonContext.Default.FeedbinCreateTaggingRequest);
                    taggingRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

                    var taggingResponse = await _httpClient.SendAsync(taggingRequest, cancellationToken).ConfigureAwait(false);

                    if (taggingResponse.IsSuccessStatusCode || taggingResponse.StatusCode == HttpStatusCode.Created || taggingResponse.StatusCode == HttpStatusCode.Found)
                    {
                        _logger.LogDebug("添加分组 {GroupId} 成功", groupId);
                    }
                    else
                    {
                        _logger.LogWarning("添加分组 {GroupId} 失败，状态码: {StatusCode}", groupId, taggingResponse.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "添加分组 {GroupId} 时发生异常", groupId);
                }
            }
        }

        var resultFeed = new RssFeed
        {
            Id = subscription.FeedId.ToString(),
            Name = subscription.Title,
            Url = subscription.FeedUrl ?? feed.Url,
            Website = subscription.SiteUrl,
            Comment = subscription.Id.ToString(),
            GroupIds = feed.GroupIds,
        };

        _logger.LogInformation("订阅源 {FeedName} 添加成功", resultFeed.Name);

        return resultFeed;
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

        var subscriptionId = oldFeed.Comment;
        if (string.IsNullOrEmpty(subscriptionId))
        {
            _logger.LogWarning("订阅源 {FeedName} 没有有效的 subscription_id", oldFeed.Name);
            return false;
        }

        var success = true;

        // 更新标题
        if (newFeed.Name != oldFeed.Name)
        {
            _logger.LogDebug("更新订阅标题: {OldTitle} -> {NewTitle}", oldFeed.Name, newFeed.Name);

            var updateRequest = CreateAuthenticatedRequest($"/subscriptions/{subscriptionId}.json", HttpMethod.Patch);
            var updateData = new FeedbinUpdateSubscriptionRequest { Title = newFeed.Name };
            updateRequest.Content = JsonContent.Create(updateData, FeedbinJsonContext.Default.FeedbinUpdateSubscriptionRequest);
            updateRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

            try
            {
                var updateResponse = await _httpClient.SendAsync(updateRequest, cancellationToken).ConfigureAwait(false);
                if (updateResponse.IsSuccessStatusCode)
                {
                    _logger.LogDebug("标题更新成功");
                }
                else
                {
                    _logger.LogWarning("更新标题失败，状态码: {StatusCode}", updateResponse.StatusCode);
                    success = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新标题时发生异常");
                success = false;
            }
        }

        // 更新分组
        var oldGroups = oldFeed.GetGroupIdList().ToHashSet();
        var newGroups = newFeed.GetGroupIdList().ToHashSet();

        var removeGroups = oldGroups.Except(newGroups).ToList();
        var addGroups = newGroups.Except(oldGroups).ToList();

        if (removeGroups.Count > 0 || addGroups.Count > 0)
        {
            _logger.LogDebug("更新分组: 删除 {RemoveCount} 个，添加 {AddCount} 个", removeGroups.Count, addGroups.Count);

            var feedId = int.Parse(newFeed.Id);

            // 获取当前 taggings
            var taggingRequest = CreateAuthenticatedRequest("/taggings.json", HttpMethod.Get);
            var taggingResponse = await SendRequestAsync(taggingRequest, cancellationToken).ConfigureAwait(false);
            var taggingContent = await taggingResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var taggings = JsonSerializer.Deserialize(taggingContent, FeedbinJsonContext.Default.ListFeedbinTagging) ?? [];

            // 删除旧分组
            foreach (var groupName in removeGroups)
            {
                var tagging = taggings.FirstOrDefault(t => t.FeedId == feedId && t.Name == groupName);
                if (tagging != null)
                {
                    try
                    {
                        var deleteRequest = CreateAuthenticatedRequest($"/taggings/{tagging.Id}.json", HttpMethod.Delete);
                        var deleteResponse = await _httpClient.SendAsync(deleteRequest, cancellationToken).ConfigureAwait(false);

                        if (deleteResponse.IsSuccessStatusCode || deleteResponse.StatusCode == HttpStatusCode.NoContent)
                        {
                            _logger.LogDebug("删除分组 {GroupName} 成功", groupName);
                        }
                        else
                        {
                            _logger.LogWarning("删除分组 {GroupName} 失败", groupName);
                            success = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "删除分组 {GroupName} 时发生异常", groupName);
                        success = false;
                    }
                }
            }

            // 添加新分组
            foreach (var groupName in addGroups)
            {
                try
                {
                    var createRequest = CreateAuthenticatedRequest("/taggings.json", HttpMethod.Post);
                    var createData = new FeedbinCreateTaggingRequest
                    {
                        FeedId = feedId,
                        Name = groupName,
                    };
                    createRequest.Content = JsonContent.Create(createData, FeedbinJsonContext.Default.FeedbinCreateTaggingRequest);
                    createRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

                    var createResponse = await _httpClient.SendAsync(createRequest, cancellationToken).ConfigureAwait(false);

                    if (createResponse.IsSuccessStatusCode || createResponse.StatusCode == HttpStatusCode.Created)
                    {
                        _logger.LogDebug("添加分组 {GroupName} 成功", groupName);
                    }
                    else
                    {
                        _logger.LogWarning("添加分组 {GroupName} 失败", groupName);
                        success = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "添加分组 {GroupName} 时发生异常", groupName);
                    success = false;
                }
            }
        }

        if (success)
        {
            _logger.LogInformation("订阅源 {FeedName} 更新成功", newFeed.Name);
        }
        else
        {
            _logger.LogWarning("订阅源 {FeedName} 更新部分失败", newFeed.Name);
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

        var subscriptionId = feed.Comment;
        if (string.IsNullOrEmpty(subscriptionId))
        {
            _logger.LogWarning("订阅源 {FeedName} 没有有效的 subscription_id", feed.Name);
            return false;
        }

        var request = CreateAuthenticatedRequest($"/subscriptions/{subscriptionId}.json", HttpMethod.Delete);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                _logger.LogInformation("订阅源 {FeedName} 删除成功", feed.Name);
                return true;
            }

            _logger.LogWarning("删除订阅源 {FeedName} 失败，状态码: {StatusCode}", feed.Name, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除订阅源 {FeedName} 时发生异常", feed.Name);
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

        // 将字符串 ID 转换为 long
        var longIds = new List<long>();
        foreach (var id in ids)
        {
            if (long.TryParse(id, out var longId))
            {
                longIds.Add(longId);
            }
            else
            {
                _logger.LogWarning("无效的文章 ID: {Id}", id);
            }
        }

        if (longIds.Count == 0)
        {
            _logger.LogWarning("没有有效的文章 ID 可标记");
            return false;
        }

        // Feedbin 限制每次最多 1000 个 ID
        const int batchSize = 1000;
        var batches = longIds
            .Select((id, index) => new { id, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.id).ToList())
            .ToList();

        var allSuccess = true;
        foreach (var batch in batches)
        {
            var request = CreateAuthenticatedRequest("/unread_entries.json", HttpMethod.Delete);
            var data = new FeedbinUnreadEntriesRequest { UnreadEntries = batch };
            request.Content = JsonContent.Create(data, FeedbinJsonContext.Default.FeedbinUnreadEntriesRequest);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("批次标记 {Count} 篇文章为已读成功", batch.Count);
                }
                else
                {
                    _logger.LogWarning("批次标记已读失败，状态码: {StatusCode}", response.StatusCode);
                    allSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "标记已读时发生异常");
                allSuccess = false;
            }
        }

        if (allSuccess)
        {
            _logger.LogInformation("成功标记 {Count} 篇文章为已读", ids.Count);
        }

        return allSuccess;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkFeedAsReadAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);

        _logger.LogDebug("将订阅源 {FeedName} 下的所有文章标记为已读", feed.Name);

        EnsureAuthenticated();

        // 先获取该订阅源的所有未读文章 ID
        var unreadRequest = CreateAuthenticatedRequest("/unread_entries.json", HttpMethod.Get);
        var unreadResponse = await SendRequestAsync(unreadRequest, cancellationToken).ConfigureAwait(false);
        var unreadContent = await unreadResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var allUnreadIds = JsonSerializer.Deserialize(unreadContent, FeedbinJsonContext.Default.ListInt64) ?? [];

        if (allUnreadIds.Count == 0)
        {
            _logger.LogInformation("没有未读文章");
            return true;
        }

        // 获取订阅源的文章以筛选出属于该订阅源的未读 ID
        var feedId = int.Parse(feed.Id);
        var entriesRequest = CreateAuthenticatedRequest($"/feeds/{feedId}/entries.json", HttpMethod.Get, "per_page=500");
        var entriesResponse = await SendRequestAsync(entriesRequest, cancellationToken).ConfigureAwait(false);
        var entriesContent = await entriesResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var entries = JsonSerializer.Deserialize(entriesContent, FeedbinJsonContext.Default.ListFeedbinEntry) ?? [];

        var feedEntryIds = entries.Select(e => e.Id).ToHashSet();
        var unreadIdsToMark = allUnreadIds.Where(id => feedEntryIds.Contains(id)).ToList();

        if (unreadIdsToMark.Count == 0)
        {
            _logger.LogInformation("订阅源 {FeedName} 没有未读文章", feed.Name);
            return true;
        }

        _logger.LogDebug("订阅源 {FeedName} 有 {Count} 篇未读文章需要标记", feed.Name, unreadIdsToMark.Count);

        return await MarkArticlesAsReadAsync(unreadIdsToMark.Select(id => id.ToString()), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkGroupAsReadAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        _logger.LogDebug("将分组 {GroupName} 下的所有文章标记为已读", group.Name);

        EnsureAuthenticated();

        // 获取分组下的所有订阅源
        var (_, feeds) = await GetFeedListAsync(cancellationToken).ConfigureAwait(false);
        var groupFeeds = feeds.Where(f => f.GetGroupIdList().Contains(group.Name) || f.GetGroupIdList().Contains(group.Id)).ToList();

        if (groupFeeds.Count == 0)
        {
            _logger.LogInformation("分组 {GroupName} 没有订阅源", group.Name);
            return true;
        }

        _logger.LogDebug("分组 {GroupName} 包含 {Count} 个订阅源", group.Name, groupFeeds.Count);

        var allSuccess = true;
        foreach (var feed in groupFeeds)
        {
            var success = await MarkFeedAsReadAsync(feed, cancellationToken).ConfigureAwait(false);
            if (!success)
            {
                allSuccess = false;
            }
        }

        return allSuccess;
    }

    /// <inheritdoc/>
    public async Task<bool> ImportOpmlAsync(
        string opmlContent,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(opmlContent);

        _logger.LogDebug("导入 OPML");

        EnsureAuthenticated();

        var request = CreateAuthenticatedRequest("/imports.json", HttpMethod.Post);
        request.Content = new StringContent(opmlContent, Encoding.UTF8, "text/xml");

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var importResponse = JsonSerializer.Deserialize(responseContent, FeedbinJsonContext.Default.FeedbinImportResponse);

                if (importResponse != null)
                {
                    _logger.LogInformation("OPML 导入已提交，导入 ID: {ImportId}，状态: {Complete}",
                        importResponse.Id, importResponse.Complete ? "已完成" : "处理中");
                }

                return true;
            }

            _logger.LogWarning("OPML 导入失败，状态码: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OPML 导入时发生异常");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string> ExportOpmlAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("导出 OPML");

        EnsureAuthenticated();

        // Feedbin 没有直接的 OPML 导出 API，本地生成
        var (groups, feeds) = await GetFeedListAsync(cancellationToken).ConfigureAwait(false);
        var opml = OpmlHelper.GenerateOpml(groups, feeds, "Feedbin Subscriptions");

        _logger.LogInformation("本地生成 OPML 成功，包含 {GroupCount} 个分组和 {FeedCount} 个订阅源", groups.Count, feeds.Count);
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

    private static RssArticle ConvertToArticle(FeedbinEntry entry, RssFeed feed)
    {
        var article = new RssArticle
        {
            Id = entry.Id.ToString(),
            FeedId = feed.Id,
            Title = WebUtility.HtmlDecode(entry.Title ?? string.Empty),
            Url = entry.Url,
            Author = entry.Author ?? feed.Name,
            Content = entry.Content,
            Summary = ExtractSummary(entry),
            CoverUrl = ExtractCover(entry.Content),
        };

        article.SetPublishTime(entry.Published);

        return article;
    }

    private static string? ExtractSummary(FeedbinEntry entry)
    {
        var content = entry.Summary ?? entry.Content;
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

    private static string? ExtractCover(string? content)
    {
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
        if (string.IsNullOrEmpty(_authToken))
        {
            throw new InvalidOperationException("未认证，请先调用 SignInAsync 方法进行登录。");
        }
    }

    private HttpRequestMessage CreateRequest(string path, HttpMethod method, string authToken)
    {
        var url = BuildUrl(path);
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        return request;
    }

    private HttpRequestMessage CreateAuthenticatedRequest(string path, HttpMethod method, string? query = null)
    {
        EnsureAuthenticated();
        var url = BuildUrl(path, query);
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authToken);
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

        if (!string.IsNullOrEmpty(query))
        {
            urlBuilder.Append('?');
            urlBuilder.Append(query);
        }

        return new Uri(urlBuilder.ToString());
    }

    private static HttpClient CreateFeedbinHttpClient(TimeSpan timeout)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };

        var client = new HttpClient(handler)
        {
            Timeout = timeout,
        };

        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
        };
        // Feedbin API 需要 Accept: application/json 来返回正确的响应
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "RodelReader/1.0");

        return client;
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

    [GeneratedRegex(@"<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"<img[^>]+src=""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex ImgSrcRegex();
}
