// Copyright (c) Richasy. All rights reserved.

namespace RssSource.NewsBlur.Test.UnitTests;

/// <summary>
/// NewsBlurClient 订阅源管理单元测试.
/// </summary>
[TestClass]
public sealed class NewsBlurClientFeedTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private NewsBlurClientOptions _options = null!;
    private NewsBlurClient _client = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new NewsBlurClient(_options, _httpClient);

        // 模拟登录成功
        _mockHandler.SetupResponse("/api/login", TestDataFactory.CreateLoginSuccessResponse());
        await _client.SignInAsync();
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
        _mockHandler.SetupResponse("/reader/feeds", TestDataFactory.CreateFeedsResponse());

        // Act
        var (groups, feeds) = await _client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(2, feeds.Count);

        Assert.IsTrue(groups.Any(g => g.Name == "Tech"));
        Assert.IsTrue(groups.Any(g => g.Name == "News"));

        var feed1 = feeds.First(f => f.Id == "123");
        Assert.AreEqual("Test Feed 1", feed1.Name);
        Assert.AreEqual("https://example.com/feed1.xml", feed1.Url);
        Assert.IsTrue(feed1.GetGroupIdList().Contains("Tech"));

        var feed2 = feeds.First(f => f.Id == "456");
        Assert.AreEqual("Test Feed 2", feed2.Name);
        Assert.IsTrue(feed2.GetGroupIdList().Contains("News"));
    }

    [TestMethod]
    public async Task GetFeedListAsync_WithEmptyFeeds_ShouldReturnEmptyLists()
    {
        // Arrange
        _mockHandler.SetupResponse("/reader/feeds", TestDataFactory.CreateEmptyFeedsResponse());

        // Act
        var (groups, feeds) = await _client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(0, groups.Count);
        Assert.AreEqual(0, feeds.Count);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithValidUrl_ShouldReturnNewFeed()
    {
        // Arrange
        var feed = new RssFeed { Url = "https://newsite.com/feed.xml" };
        _mockHandler.SetupResponse("/reader/add_url", TestDataFactory.CreateAddFeedSuccessResponse());

        // Act
        var result = await _client.AddFeedAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("789", result.Id);
        Assert.AreEqual("New Feed", result.Name);
        Assert.AreEqual("https://newsite.com/feed.xml", result.Url);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithInvalidUrl_ShouldReturnNull()
    {
        // Arrange
        var feed = new RssFeed { Url = "https://invalid.com/notafeed" };
        _mockHandler.SetupResponse("/reader/add_url", TestDataFactory.CreateAddFeedFailedResponse());

        // Act
        var result = await _client.AddFeedAsync(feed);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithFolder_ShouldIncludeFolderInRequest()
    {
        // Arrange
        var feed = new RssFeed { Url = "https://newsite.com/feed.xml" };
        feed.SetGroupIdList(["Tech"]);
        _mockHandler.SetupResponse("/reader/add_url", TestDataFactory.CreateAddFeedSuccessResponse());

        // Act
        var result = await _client.AddFeedAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.GetGroupIdList().Contains("Tech"));
    }

    [TestMethod]
    public async Task UpdateFeedAsync_ShouldRenameSuccessfully()
    {
        // Arrange
        var oldFeed = TestDataFactory.CreateTestFeed();
        var newFeed = oldFeed.Clone();
        newFeed.Name = "Renamed Feed";
        _mockHandler.SetupResponse("/reader/rename_feed", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task UpdateFeedAsync_WithGroupChange_ShouldMoveFeed()
    {
        // Arrange
        var oldFeed = TestDataFactory.CreateTestFeed();
        oldFeed.SetGroupIdList(["OldFolder"]);
        var newFeed = oldFeed.Clone();
        newFeed.Name = "Renamed Feed";
        newFeed.SetGroupIdList(["NewFolder"]);
        _mockHandler.SetupResponse("/reader/rename_feed", TestDataFactory.CreateOperationSuccessResponse());
        _mockHandler.SetupResponse("/reader/move_feed_to_folder", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_ShouldSucceed()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed();
        _mockHandler.SetupResponse("/reader/delete_feed", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.DeleteFeedAsync(feed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed();
        _mockHandler.SetupErrorResponse("/reader/delete_feed", HttpStatusCode.InternalServerError, "Server error");

        // Act
        var result = await _client.DeleteFeedAsync(feed);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.AddFeedAsync(null!));
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.DeleteFeedAsync(null!));
    }
}
