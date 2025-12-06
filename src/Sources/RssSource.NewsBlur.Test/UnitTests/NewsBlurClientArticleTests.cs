// Copyright (c) Richasy. All rights reserved.

namespace RssSource.NewsBlur.Test.UnitTests;

/// <summary>
/// NewsBlurClient 文章操作单元测试.
/// </summary>
[TestClass]
public sealed class NewsBlurClientArticleTests
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
    public async Task GetFeedDetailAsync_ShouldReturnArticles()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed();
        _mockHandler.SetupResponse("/reader/feed/123", TestDataFactory.CreateStoriesResponse());

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(feed.Id, result.Feed.Id);
        Assert.IsTrue(result.Articles.Count > 0);

        // 文章按发布时间降序排列，所以最新的文章排在前面
        var article = result.Articles.FirstOrDefault(a => a.Id == "123:abc");
        Assert.IsNotNull(article);
        Assert.AreEqual("Test Article 1", article.Title);
        Assert.IsNotNull(article.Content);
        Assert.AreEqual("https://example.com/article1", article.Url);
        Assert.AreEqual("John Doe", article.Author);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithEmptyStories_ShouldReturnEmptyList()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed();
        _mockHandler.SetupResponse("/reader/feed/123", TestDataFactory.CreateEmptyStoriesResponse());

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Articles.Count);
    }

    [TestMethod]
    public async Task GetFeedDetailListAsync_ShouldReturnMultipleDetails()
    {
        // Arrange
        var feeds = new List<RssFeed>
        {
            TestDataFactory.CreateTestFeed("123", "Feed 1"),
            TestDataFactory.CreateTestFeed("456", "Feed 2"),
        };
        _mockHandler.SetupResponse("/reader/feed/123", TestDataFactory.CreateStoriesResponse());
        _mockHandler.SetupResponse("/reader/feed/456", TestDataFactory.CreateEmptyStoriesResponse());

        // Act
        var results = await _client.GetFeedDetailListAsync(feeds);

        // Assert
        Assert.AreEqual(2, results.Count);
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_ShouldSucceed()
    {
        // Arrange
        var articleIds = new[] { "123:abc", "123:def" };
        _mockHandler.SetupResponse("/reader/mark_story_hashes_as_read", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_WithEmptyList_ShouldReturnTrue()
    {
        // Arrange
        var articleIds = Array.Empty<string>();

        // Act
        var result = await _client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var articleIds = new[] { "123:abc" };
        _mockHandler.SetupErrorResponse("/reader/mark_story_hashes_as_read", HttpStatusCode.InternalServerError, "Server error");

        // Act
        var result = await _client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_ShouldSucceed()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed();
        _mockHandler.SetupResponse("/reader/mark_feed_as_read", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.MarkFeedAsReadAsync(feed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed();
        _mockHandler.SetupErrorResponse("/reader/mark_feed_as_read", HttpStatusCode.InternalServerError, "Server error");

        // Act
        var result = await _client.MarkFeedAsReadAsync(feed);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_ShouldMarkAllFeedsInGroup()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "Tech", Name = "Tech" };
        _mockHandler.SetupResponse("/reader/feeds", TestDataFactory.CreateFeedsResponse());
        _mockHandler.SetupResponse("/reader/mark_feed_as_read", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.MarkGroupAsReadAsync(group);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.GetFeedDetailAsync(null!));
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.MarkFeedAsReadAsync(null!));
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.MarkGroupAsReadAsync(null!));
    }
}
