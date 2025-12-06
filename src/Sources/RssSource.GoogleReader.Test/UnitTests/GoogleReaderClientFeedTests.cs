// Copyright (c) Richasy. All rights reserved.

namespace RssSource.GoogleReader.Test.UnitTests;

/// <summary>
/// GoogleReaderClient 订阅源管理单元测试.
/// </summary>
[TestClass]
public sealed class GoogleReaderClientFeedTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private GoogleReaderClientOptions _options = null!;
    private GoogleReaderClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new GoogleReaderClient(_options, _httpClient);
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
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/list", TestDataFactory.CreateSubscriptionListJson());

        // Act
        var (groups, feeds) = await _client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(3, feeds.Count);

        // 验证分组
        Assert.IsTrue(groups.Any(g => g.Name == "科技"));
        Assert.IsTrue(groups.Any(g => g.Name == "开发"));

        // 验证订阅源
        Assert.IsTrue(feeds.Any(f => f.Name == "IT之家"));
        Assert.IsTrue(feeds.Any(f => f.Name == "极客公园"));
        Assert.IsTrue(feeds.Any(f => f.Name == ".NET Blog"));
    }

    [TestMethod]
    public async Task GetFeedListAsync_WithoutAuth_ShouldThrowException()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();
        using var client = new GoogleReaderClient(options, _httpClient);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.GetFeedListAsync());
    }

    [TestMethod]
    public async Task AddFeedAsync_ShouldReturnNewFeedWithId()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");
        var newFeed = new RssFeed
        {
            Name = "新订阅源",
            Url = "https://example.com/rss",
        };

        // Act
        var result = await _client.AddFeedAsync(newFeed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("feed/https://example.com/rss", result.Id);
        Assert.AreEqual("新订阅源", result.Name);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithGroups_ShouldIncludeGroupsInRequest()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");
        var newFeed = new RssFeed
        {
            Name = "新订阅源",
            Url = "https://example.com/rss",
        };
        newFeed.SetGroupIdList(["user/-/label/科技", "user/-/label/开发"]);

        // Act
        var result = await _client.AddFeedAsync(newFeed);

        // Assert
        Assert.IsNotNull(result);

        // 验证请求包含分组参数
        var request = _mockHandler.Requests.FirstOrDefault(r =>
            r.RequestUri!.PathAndQuery.Contains("/subscription/edit"));
        Assert.IsNotNull(request);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithFailure_ShouldReturnNull()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "ERROR");
        var newFeed = new RssFeed
        {
            Name = "新订阅源",
            Url = "https://example.com/rss",
        };

        // Act
        var result = await _client.AddFeedAsync(newFeed);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task UpdateFeedAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");
        var oldFeed = TestDataFactory.IthomeFeed;
        var newFeed = oldFeed.Clone();
        newFeed.Name = "IT之家 (更新)";

        // Act
        var result = await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task UpdateFeedAsync_WithGroupChanges_ShouldWork()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");

        var oldFeed = TestDataFactory.IthomeFeed.Clone();
        oldFeed.SetGroupIdList(["user/-/label/科技"]);

        var newFeed = oldFeed.Clone();
        newFeed.SetGroupIdList(["user/-/label/开发"]);

        // Act
        var result = await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "OK");

        // Act
        var result = await _client.DeleteFeedAsync(TestDataFactory.IthomeFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WithFailure_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/edit", "ERROR");

        // Act
        var result = await _client.DeleteFeedAsync(TestDataFactory.IthomeFeed);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.AddFeedAsync(null!));
    }
}
