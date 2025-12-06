// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test.UnitTests;

/// <summary>
/// InoreaderClient 订阅源操作单元测试.
/// </summary>
[TestClass]
public sealed class InoreaderClientFeedTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private InoreaderClientOptions _options = null!;
    private InoreaderClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new InoreaderClient(_options, _httpClient);

        // 设置默认响应
        _mockHandler.SetupTextResponse("/subscription/list", TestDataFactory.CreateSubscriptionListJson());
        _mockHandler.SetupTextResponse("/tag/list", TestDataFactory.CreateTagListJson());
        _mockHandler.SetupTextResponse("/preference/stream/list", TestDataFactory.CreatePreferenceJson());
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public async Task GetFeedListAsync_ShouldReturnGroupsAndFeeds()
    {
        // Act
        var (groups, feeds) = await _client.GetFeedListAsync();

        // Assert
        Assert.IsNotNull(groups);
        Assert.IsNotNull(feeds);
        Assert.AreEqual(2, groups.Count); // 科技, 开发
        Assert.AreEqual(3, feeds.Count); // IT之家, 极客公园, .NET Blog

        // 验证分组
        Assert.IsTrue(groups.Any(g => g.Name == "科技"));
        Assert.IsTrue(groups.Any(g => g.Name == "开发"));

        // 验证订阅源
        Assert.IsTrue(feeds.Any(f => f.Name == "IT之家"));
        Assert.IsTrue(feeds.Any(f => f.Name == "极客公园"));
        Assert.IsTrue(feeds.Any(f => f.Name == ".NET Blog"));
    }

    [TestMethod]
    public async Task GetFeedListAsync_FeedsShouldHaveCorrectGroupIds()
    {
        // Act
        var (_, feeds) = await _client.GetFeedListAsync();

        // Assert
        var ithome = feeds.First(f => f.Name == "IT之家");
        var groupIds = ithome.GetGroupIdList();
        Assert.AreEqual(1, groupIds.Count);
        Assert.AreEqual("user/-/label/科技", groupIds[0]);
    }

    [TestMethod]
    public async Task GetFeedListAsync_ShouldSendAuthorizationHeader()
    {
        // Act
        await _client.GetFeedListAsync();

        // Assert
        Assert.IsTrue(_mockHandler.Requests.Count > 0);
        var request = _mockHandler.Requests[0];
        Assert.IsNotNull(request.Headers.Authorization);
        Assert.AreEqual("Bearer", request.Headers.Authorization.Scheme);
        Assert.AreEqual("test_access_token", request.Headers.Authorization.Parameter);
    }

    [TestMethod]
    public async Task AddFeedAsync_ShouldReturnFeedWithId()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");

        var newFeed = new RssFeed
        {
            Name = "New Feed",
            Url = "https://example.com/rss",
        };

        // Act
        var result = await _client.AddFeedAsync(newFeed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("feed/https://example.com/rss", result.Id);
        Assert.AreEqual("New Feed", result.Name);
    }

    [TestMethod]
    public async Task AddFeedAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");

        var newFeed = new RssFeed
        {
            Name = "Test Feed",
            Url = "https://test.com/rss",
        };
        newFeed.SetGroupIdList(["user/-/label/测试"]);

        // Act
        await _client.AddFeedAsync(newFeed);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/subscription/edit"));
        Assert.AreEqual(HttpMethod.Post, request.Method);

        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("ac=subscribe"));
        Assert.IsTrue(content.Contains("t=Test+Feed") || content.Contains("t=Test%20Feed"));
    }

    [TestMethod]
    public async Task AddFeedAsync_WhenFailed_ShouldReturnNull()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "ERROR");

        var newFeed = new RssFeed
        {
            Name = "New Feed",
            Url = "https://example.com/rss",
        };

        // Act
        var result = await _client.AddFeedAsync(newFeed);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task UpdateFeedAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");

        var oldFeed = TestDataFactory.IthomeFeed;
        oldFeed.SetGroupIdList(["user/-/label/科技"]);

        var newFeed = oldFeed.Clone();
        newFeed.Name = "IT之家 (Updated)";
        newFeed.SetGroupIdList(["user/-/label/新闻"]);

        // Act
        var result = await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task UpdateFeedAsync_ShouldSendCorrectGroupChanges()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");

        var oldFeed = TestDataFactory.IthomeFeed;
        oldFeed.SetGroupIdList(["user/-/label/旧分组"]);

        var newFeed = oldFeed.Clone();
        newFeed.SetGroupIdList(["user/-/label/新分组"]);

        // Act
        await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/subscription/edit"));
        var content = await request.Content!.ReadAsStringAsync();

        // 应该添加新分组，移除旧分组
        Assert.IsTrue(content.Contains("a=user%2F-%2Flabel%2F") || content.Contains("a=user/-/label/"));
        Assert.IsTrue(content.Contains("r=user%2F-%2Flabel%2F") || content.Contains("r=user/-/label/"));
    }

    [TestMethod]
    public async Task DeleteFeedAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");

        // Act
        var result = await _client.DeleteFeedAsync(TestDataFactory.IthomeFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_ShouldSendUnsubscribeAction()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");

        // Act
        await _client.DeleteFeedAsync(TestDataFactory.IthomeFeed);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/subscription/edit"));
        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("ac=unsubscribe"));
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "ERROR");

        // Act
        var result = await _client.DeleteFeedAsync(TestDataFactory.IthomeFeed);

        // Assert
        Assert.IsFalse(result);
    }
}
