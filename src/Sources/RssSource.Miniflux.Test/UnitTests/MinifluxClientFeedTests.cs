// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Miniflux.Test.UnitTests;

/// <summary>
/// MinifluxClient 订阅源操作单元测试.
/// </summary>
[TestClass]
public sealed class MinifluxClientFeedTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private MinifluxClientOptions _options = null!;
    private MinifluxClient _client = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new MinifluxClient(_options, _httpClient);

        // 模拟登录
        _mockHandler.SetupResponse("/v1/me", TestDataFactory.CreateUserResponse());
        await _client.SignInAsync();
        _mockHandler.Clear();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public async Task GetFeedListAsync_ShouldReturnFeedsAndGroups()
    {
        // Arrange
        _mockHandler.SetupResponse("/v1/feeds", TestDataFactory.CreateFeedsResponse());

        // Act
        var (groups, feeds) = await _client.GetFeedListAsync();

        // Assert
        Assert.IsNotNull(groups);
        Assert.IsNotNull(feeds);
        Assert.AreEqual(2, groups.Count); // Tech, News (第三个 feed 没有分类)
        Assert.AreEqual(3, feeds.Count);

        // 验证分组
        Assert.IsTrue(groups.Any(g => g.Name == "Tech"));
        Assert.IsTrue(groups.Any(g => g.Name == "News"));

        // 验证订阅源
        var feed1 = feeds.FirstOrDefault(f => f.Id == "1");
        Assert.IsNotNull(feed1);
        Assert.AreEqual("Test Feed 1", feed1.Name);
        Assert.AreEqual("https://example.com/feed1.xml", feed1.Url);
        Assert.AreEqual(1, feed1.GetGroupIdList().Count);
    }

    [TestMethod]
    public async Task GetFeedListAsync_NotAuthenticated_ShouldThrow()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();
        using var client = new MinifluxClient(options, _httpClient);
        // 不调用 SignInAsync

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => client.GetFeedListAsync());
    }

    [TestMethod]
    public async Task AddFeedAsync_ShouldReturnNewFeed()
    {
        // Arrange
        var feedToAdd = new RssFeed
        {
            Name = "New Feed",
            Url = "https://example.com/new-feed.xml",
        };
        feedToAdd.SetGroupIdList(["1"]);

        _mockHandler.SetupResponse("/v1/feeds", TestDataFactory.CreateAddFeedResponse(10));
        _mockHandler.SetupResponse("/v1/feeds/10", TestDataFactory.CreateFeedResponse(10, "New Feed"));

        // Act
        var result = await _client.AddFeedAsync(feedToAdd);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("10", result.Id);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithNullFeed_ShouldThrow()
    {
        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.AddFeedAsync(null!));
    }

    [TestMethod]
    public async Task UpdateFeedAsync_ShouldReturnTrue()
    {
        // Arrange
        var oldFeed = TestDataFactory.CreateTestFeed("1", "Old Name");
        var newFeed = TestDataFactory.CreateTestFeed("1", "New Name");

        _mockHandler.SetupResponse("/v1/feeds/1", TestDataFactory.CreateFeedResponse(1, "New Name"));

        // Act
        var result = await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_ShouldReturnTrue()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed("1");
        _mockHandler.SetupResponse("/v1/feeds/1", HttpStatusCode.NoContent);

        // Act
        var result = await _client.DeleteFeedAsync(feed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed("999");
        _mockHandler.SetupErrorResponse("/v1/feeds/999", HttpStatusCode.NotFound, "Feed not found");

        // Act
        var result = await _client.DeleteFeedAsync(feed);

        // Assert
        Assert.IsFalse(result);
    }
}
