// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;

namespace RssSource.Inoreader.Test.Integration;

/// <summary>
/// Inoreader 真实 API 集成测试.
/// 使用实际的 Access Token 进行测试，需要手动配置 Token.
/// </summary>
/// <remarks>
/// 使用方法:
/// 1. 将 TestAccessToken 设置为有效的 Access Token
/// 2. 将 SkipRealApiTests 设置为 false
/// 3. 运行测试
/// </remarks>
[TestClass]
public sealed class InoreaderRealApiTests : IDisposable
{
    // ==================== 配置区域 ====================
    // 设置为有效的 Access Token 以运行真实 API 测试
    private const string TestAccessToken = "a99d61c05632668fb4c3825737231da2ed2759db";

    // 设置为 false 以启用真实 API 测试
    private const bool SkipRealApiTests = false;
    // ==================================================

    private InoreaderClient? _client;
    private HttpClient? _rawHttpClient;
    private bool _disposed;

    [TestInitialize]
    public void Setup()
    {
        if (SkipRealApiTests || string.IsNullOrEmpty(TestAccessToken))
        {
            return;
        }

        var options = new InoreaderClientOptions
        {
            AccessToken = TestAccessToken,
            ExpireTime = DateTimeOffset.Now.AddDays(30), // 假设 token 有效期足够长
            DataSource = InoreaderDataSource.Default,
            Timeout = TimeSpan.FromSeconds(60),
            MaxConcurrentRequests = 5,
            ArticlesPerRequest = 20,
        };

        // 使用共享的 HttpClient 以便更好地诊断问题
        _rawHttpClient = new HttpClient();

        // 将共享的 HttpClient 传递给 InoreaderClient
        _client = new InoreaderClient(options, _rawHttpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    /// <summary>
    /// 首先测试原始 API 调用，用于诊断 Token 问题.
    /// </summary>
    [TestMethod]
    public async Task RealApi_RawApiCall_DiagnoseTokenIssues()
    {
        if (ShouldSkip())
        {
            Assert.Inconclusive("跳过真实 API 测试。请配置 TestAccessToken 并将 SkipRealApiTests 设置为 false。");
            return;
        }

        // 测试直接调用 API
        var apiUrl = "https://www.inoreader.com/reader/api/0/subscription/list";

        var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestAccessToken);

        Console.WriteLine($"请求 URL: {apiUrl}");
        Console.WriteLine($"Authorization: Bearer {TestAccessToken[..10]}...");

        var response = await _rawHttpClient!.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"状态码: {response.StatusCode}");
        Console.WriteLine($"响应内容类型: {response.Content.Headers.ContentType}");
        Console.WriteLine($"响应内容 (前 1000 字符):");
        Console.WriteLine(content.Length > 1000 ? content[..1000] + "..." : content);

        // 如果返回 HTML，说明 Token 无效或已过期
        if (content.TrimStart().StartsWith('<'))
        {
            Console.WriteLine("\n⚠️ 响应是 HTML，Token 可能已过期或无效！");
            Console.WriteLine("请检查 Token 是否正确，或尝试重新获取 Token。");
            Assert.Fail("API 返回 HTML 而非 JSON，Token 可能已过期或无效。");
        }
        else
        {
            Console.WriteLine("\n✓ 响应是 JSON，Token 有效！");
        }

        // 测试使用 InoreaderClient 构建的相同 URL
        Console.WriteLine("\n--- 测试 InoreaderClient 生成的 URL ---");

        var options = new InoreaderClientOptions
        {
            AccessToken = TestAccessToken,
            DataSource = InoreaderDataSource.Default,
        };

        var baseUrl = options.GetApiBaseUrl();
        var fullUrl = new Uri(baseUrl, "/subscription/list");

        Console.WriteLine($"BaseUrl: {baseUrl}");
        Console.WriteLine($"FullUrl: {fullUrl}");

        var request2 = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TestAccessToken);

        var response2 = await _rawHttpClient.SendAsync(request2);
        var content2 = await response2.Content.ReadAsStringAsync();

        Console.WriteLine($"状态码: {response2.StatusCode}");
        Console.WriteLine($"响应内容 (前 500 字符):");
        Console.WriteLine(content2.Length > 500 ? content2[..500] + "..." : content2);

        if (content2.TrimStart().StartsWith('<'))
        {
            Console.WriteLine("\n⚠️ InoreaderClient URL 返回 HTML！");
        }
        else
        {
            Console.WriteLine("\n✓ InoreaderClient URL 返回 JSON！");
        }
    }

    [TestMethod]
    public async Task RealApi_GetFeedList_ShouldReturnFeeds()
    {
        if (ShouldSkip())
        {
            Assert.Inconclusive("跳过真实 API 测试。请配置 TestAccessToken 并将 SkipRealApiTests 设置为 false。");
            return;
        }

        // Act
        var (groups, feeds) = await _client!.GetFeedListAsync();

        // Assert
        Console.WriteLine($"获取到 {groups.Count} 个分组, {feeds.Count} 个订阅源");

        foreach (var group in groups)
        {
            Console.WriteLine($"分组: {group.Name} (ID: {group.Id})");
        }

        foreach (var feed in feeds)
        {
            Console.WriteLine($"订阅源: {feed.Name} (URL: {feed.Url})");
        }

        // 至少应该能成功调用 API（可能没有订阅源）
        Assert.IsNotNull(groups);
        Assert.IsNotNull(feeds);
    }

    [TestMethod]
    public async Task RealApi_GetFeedDetail_ShouldReturnArticles()
    {
        if (ShouldSkip())
        {
            Assert.Inconclusive("跳过真实 API 测试。请配置 TestAccessToken 并将 SkipRealApiTests 设置为 false。");
            return;
        }

        // Arrange - 先获取订阅列表
        var (_, feeds) = await _client!.GetFeedListAsync();

        if (feeds.Count == 0)
        {
            Assert.Inconclusive("没有订阅源，无法测试获取文章。");
            return;
        }

        // Act - 获取第一个订阅源的文章
        var firstFeed = feeds[0];
        Console.WriteLine($"获取订阅源 '{firstFeed.Name}' 的文章...");

        var detail = await _client.GetFeedDetailAsync(firstFeed);

        // Assert
        Assert.IsNotNull(detail);
        Console.WriteLine($"获取到 {detail.Articles.Count} 篇文章");

        foreach (var article in detail.Articles.Take(5))
        {
            Console.WriteLine($"  - {article.Title}");
            Console.WriteLine($"    发布时间: {article.PublishTime}");
        }
    }

    [TestMethod]
    public async Task RealApi_GetFeedDetailList_ShouldReturnMultipleFeeds()
    {
        if (ShouldSkip())
        {
            Assert.Inconclusive("跳过真实 API 测试。请配置 TestAccessToken 并将 SkipRealApiTests 设置为 false。");
            return;
        }

        // Arrange - 先获取订阅列表
        var (_, feeds) = await _client!.GetFeedListAsync();

        if (feeds.Count == 0)
        {
            Assert.Inconclusive("没有订阅源，无法测试获取文章。");
            return;
        }

        // Act - 批量获取订阅源的文章
        var feedsToFetch = feeds.Take(3).ToList();
        Console.WriteLine($"批量获取 {feedsToFetch.Count} 个订阅源的文章...");

        var details = await _client.GetFeedDetailListAsync(feedsToFetch);

        // Assert
        Assert.IsNotNull(details);
        Console.WriteLine($"获取到 {details.Count} 个订阅源详情");

        foreach (var detail in details)
        {
            Console.WriteLine($"  订阅源: {detail.Feed.Name}, 文章数: {detail.Articles.Count}");
        }
    }

    [TestMethod]
    public async Task RealApi_MarkArticlesAsRead_ShouldSucceed()
    {
        if (ShouldSkip())
        {
            Assert.Inconclusive("跳过真实 API 测试。请配置 TestAccessToken 并将 SkipRealApiTests 设置为 false。");
            return;
        }

        // Arrange - 先获取订阅列表和文章
        var (_, feeds) = await _client!.GetFeedListAsync();

        if (feeds.Count == 0)
        {
            Assert.Inconclusive("没有订阅源，无法测试标记已读。");
            return;
        }

        var firstFeed = feeds[0];
        var detail = await _client.GetFeedDetailAsync(firstFeed);

        if (detail == null || detail.Articles.Count == 0)
        {
            Assert.Inconclusive("没有文章，无法测试标记已读。");
            return;
        }

        // Act - 标记第一篇文章为已读
        var articleId = detail.Articles[0].Id;
        Console.WriteLine($"标记文章 '{detail.Articles[0].Title}' 为已读...");

        var result = await _client.MarkArticlesAsReadAsync([articleId]);

        // Assert
        Assert.IsTrue(result);
        Console.WriteLine("标记成功！");
    }

    [TestMethod]
    public async Task RealApi_ExportOpml_ShouldReturnOpmlContent()
    {
        if (ShouldSkip())
        {
            Assert.Inconclusive("跳过真实 API 测试。请配置 TestAccessToken 并将 SkipRealApiTests 设置为 false。");
            return;
        }

        // Act
        var opml = await _client!.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(opml);
        Assert.IsTrue(opml.Contains("<?xml") || opml.Contains("<opml"), "OPML 内容应该是有效的 XML");

        Console.WriteLine($"OPML 内容长度: {opml.Length} 字符");
        Console.WriteLine("OPML 前 500 字符:");
        Console.WriteLine(opml.Length > 500 ? opml[..500] + "..." : opml);
    }

    private bool ShouldSkip()
        => SkipRealApiTests || string.IsNullOrEmpty(TestAccessToken) || _client == null;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _client?.Dispose();
            _rawHttpClient?.Dispose();
        }

        _disposed = true;
    }
}
