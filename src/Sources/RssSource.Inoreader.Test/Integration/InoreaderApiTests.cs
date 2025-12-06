// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test.Integration;

/// <summary>
/// Inoreader API 集成测试.
/// 测试完整的 API 交互流程.
/// </summary>
[TestClass]
public sealed class InoreaderApiTests : InoreaderIntegrationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        Initialize();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    [TestMethod]
    public async Task FullWorkflow_AddFeedAndGetArticles()
    {
        // Arrange
        SetupAllStreamContentResponses();

        // Act 1: 添加订阅源
        var newFeed = new RssFeed
        {
            Name = "新订阅源",
            Url = "https://example.com/rss",
        };
        newFeed.SetGroupIdList(["user/-/label/科技"]);

        var addedFeed = await Client.AddFeedAsync(newFeed);

        // Assert 1
        Assert.IsNotNull(addedFeed);
        Assert.AreEqual("feed/https://example.com/rss", addedFeed.Id);

        // Act 2: 获取订阅列表
        var (groups, feeds) = await Client.GetFeedListAsync();

        // Assert 2
        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(3, feeds.Count);

        // Act 3: 获取文章
        var ithomeFeed = feeds.First(f => f.Name == "IT之家");
        SetupStreamContentResponse(ithomeFeed.Id);
        var detail = await Client.GetFeedDetailAsync(ithomeFeed);

        // Assert 3
        Assert.IsNotNull(detail);
        Assert.AreEqual(2, detail.Articles.Count);
    }

    [TestMethod]
    public async Task FullWorkflow_GetFeedsAndMarkAsRead()
    {
        // Arrange
        SetupAllStreamContentResponses();

        // Act 1: 获取订阅列表
        var (groups, feeds) = await Client.GetFeedListAsync();

        // Act 2: 获取第一个订阅源的文章
        var firstFeed = feeds[0];
        SetupStreamContentResponse(firstFeed.Id);
        var detail = await Client.GetFeedDetailAsync(firstFeed);

        // Act 3: 标记所有文章为已读
        var articleIds = detail!.Articles.Select(a => a.Id).ToList();
        var markResult = await Client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsTrue(markResult);

        // 验证请求被发送
        var markRequest = MockHandler.Requests.FirstOrDefault(r =>
            r.RequestUri!.PathAndQuery.Contains("/edit-tag"));
        Assert.IsNotNull(markRequest);
    }

    [TestMethod]
    public async Task FullWorkflow_ManageGroups()
    {
        // Act 1: 获取分组列表
        var (groups, _) = await Client.GetFeedListAsync();
        Assert.AreEqual(2, groups.Count);

        // Act 2: 重命名分组
        var techGroup = groups.First(g => g.Name == "科技");
        techGroup.Name = "科技新闻";
        var updatedGroup = await Client.UpdateGroupAsync(techGroup);

        // Assert 2
        Assert.IsNotNull(updatedGroup);
        Assert.AreEqual("user/-/label/科技新闻", updatedGroup.Id);

        // Act 3: 删除分组
        var deleteResult = await Client.DeleteGroupAsync(updatedGroup);

        // Assert 3
        Assert.IsTrue(deleteResult);
    }

    [TestMethod]
    public async Task FullWorkflow_BatchFetchFeeds()
    {
        // Arrange
        SetupAllStreamContentResponses();

        // Act: 获取所有订阅源
        var (_, feeds) = await Client.GetFeedListAsync();

        // 批量获取文章
        var details = await Client.GetFeedDetailListAsync(feeds);

        // Assert
        Assert.AreEqual(3, details.Count);
        Assert.IsTrue(details.All(d => d.Articles.Count > 0));
    }

    [TestMethod]
    public async Task FullWorkflow_ImportAndExportOpml()
    {
        // Arrange
        var opmlContent = """
            <?xml version="1.0" encoding="UTF-8"?>
            <opml version="2.0">
                <head><title>Test OPML</title></head>
                <body>
                    <outline text="科技" title="科技">
                        <outline text="IT之家" type="rss" xmlUrl="https://www.ithome.com/rss" htmlUrl="https://www.ithome.com"/>
                    </outline>
                </body>
            </opml>
            """;

        // Act 1: 导入 OPML
        var importResult = await Client.ImportOpmlAsync(opmlContent);

        // Assert 1
        Assert.IsTrue(importResult);

        // Act 2: 导出 OPML
        var exportedOpml = await Client.ExportOpmlAsync();

        // Assert 2
        Assert.IsNotNull(exportedOpml);
        Assert.IsTrue(exportedOpml.Contains("opml"));
        Assert.IsTrue(exportedOpml.Contains("IT之家") || exportedOpml.Contains("ithome"));
    }

    [TestMethod]
    public async Task TokenRefresh_ShouldAutomaticallyRefreshExpiredToken()
    {
        // Arrange: 设置即将过期的 Token 和新的客户端
        var expiredOptions = TestDataFactory.CreateDefaultOptions();
        expiredOptions.ExpireTime = DateTimeOffset.Now.AddMinutes(-1);
        expiredOptions.RefreshToken = "valid_refresh_token";

        var tokenUpdated = false;
        expiredOptions.OnTokenUpdated = args =>
        {
            tokenUpdated = true;
            Assert.AreEqual("new_access_token", args.AccessToken);
            Assert.AreEqual("new_refresh_token", args.RefreshToken);
        };

        // 使用新的 MockHandler 和 HttpClient
        using var newMockHandler = new MockHttpMessageHandler();
        // 设置 Token 刷新响应
        newMockHandler.SetupTextResponse("/oauth2/token", TestDataFactory.CreateAuthTokenJson());
        // 设置订阅列表响应
        newMockHandler.SetupTextResponse("/subscription/list", TestDataFactory.CreateSubscriptionListJson());
        newMockHandler.SetupTextResponse("/tag/list", TestDataFactory.CreateTagListJson());
        newMockHandler.SetupTextResponse("/preference/stream/list", TestDataFactory.CreatePreferenceJson());

        using var newHttpClient = new HttpClient(newMockHandler);
        using var client = new InoreaderClient(expiredOptions, newHttpClient);

        // Act: 发起请求（应该触发 Token 刷新）
        var (groups, feeds) = await client.GetFeedListAsync();

        // Assert
        Assert.IsTrue(tokenUpdated);
        Assert.IsNotNull(groups);
        Assert.IsNotNull(feeds);
    }

    [TestMethod]
    public async Task AuthorizationHeader_ShouldBeIncludedInAllRequests()
    {
        // Arrange
        SetupAllStreamContentResponses();

        // Act: 执行多个操作
        await Client.GetFeedListAsync();

        var feed = TestDataFactory.IthomeFeed;
        SetupStreamContentResponse(feed.Id);
        await Client.GetFeedDetailAsync(feed);

        await Client.MarkFeedAsReadAsync(feed);

        // Assert: 所有请求都应包含 Authorization 头
        foreach (var request in MockHandler.Requests)
        {
            // 跳过可能的 OAuth 请求
            if (request.RequestUri!.PathAndQuery.Contains("oauth2"))
            {
                continue;
            }

            Assert.IsNotNull(request.Headers.Authorization, $"Request to {request.RequestUri} missing Authorization header");
            Assert.AreEqual("Bearer", request.Headers.Authorization.Scheme);
        }
    }

    [TestMethod]
    public async Task ErrorHandling_ShouldHandleApiErrors()
    {
        // Arrange: 设置错误响应
        MockHandler.SetupErrorResponse("/subscription/list", HttpStatusCode.Unauthorized);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => Client.GetFeedListAsync());
    }

    [TestMethod]
    public async Task UpdateFeed_ShouldCorrectlyHandleGroupChanges()
    {
        // Arrange
        var oldFeed = TestDataFactory.IthomeFeed;
        oldFeed.SetGroupIdList(["user/-/label/科技", "user/-/label/新闻"]);

        var newFeed = oldFeed.Clone();
        newFeed.Name = "IT之家 (更新)";
        newFeed.SetGroupIdList(["user/-/label/科技", "user/-/label/热门"]); // 移除"新闻"，添加"热门"

        // Act
        var result = await Client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);

        // 验证请求内容
        var updateRequest = MockHandler.Requests.First(r =>
            r.RequestUri!.PathAndQuery.Contains("/subscription/edit"));
        var content = await updateRequest.Content!.ReadAsStringAsync();

        // 应该有添加和移除操作
        Assert.IsTrue(content.Contains("ac=edit"));
    }
}
