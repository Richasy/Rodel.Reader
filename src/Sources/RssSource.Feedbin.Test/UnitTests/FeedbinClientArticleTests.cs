// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Feedbin.Test.UnitTests;

/// <summary>
/// FeedbinClient 文章获取单元测试.
/// </summary>
[TestClass]
public sealed class FeedbinClientArticleTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private FeedbinClientOptions _options = null!;
    private FeedbinClient _client = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _mockHandler.SetupTextResponse("/authentication.json", "{}", HttpStatusCode.OK);
        _client = new FeedbinClient(_options, _httpClient);
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
    public async Task GetFeedDetailAsync_ShouldReturnFeedWithArticles()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse($"/feeds/{feed.Id}/entries.json", TestDataFactory.CreateEntriesListJson());

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(feed.Id, result.Feed.Id);
        Assert.AreEqual(2, result.Articles.Count);

        var firstArticle = result.Articles[0];
        Assert.AreEqual("10001", firstArticle.Id);
        Assert.AreEqual("新款 iPhone 发布", firstArticle.Title);
        Assert.AreEqual(feed.Id, firstArticle.FeedId);
        Assert.IsNotNull(firstArticle.Content);
        Assert.IsNotNull(firstArticle.CoverUrl);
        Assert.AreEqual("https://example.com/iphone.jpg", firstArticle.CoverUrl);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.GetFeedDetailAsync(null!));
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithServerError_ShouldReturnNull()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupErrorResponse($"/feeds/{feed.Id}/entries.json", HttpStatusCode.InternalServerError);

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetFeedDetailListAsync_ShouldReturnMultipleFeedDetails()
    {
        // Arrange
        var feeds = new[] { TestDataFactory.IthomeFeed, TestDataFactory.GeekparkFeed };
        _mockHandler.SetupTextResponse("/feeds/123/entries.json", TestDataFactory.CreateEntriesListJson());
        _mockHandler.SetupTextResponse("/feeds/124/entries.json", "[]");

        // Act
        var results = await _client.GetFeedDetailListAsync(feeds);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(2, results.Count);
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_ShouldReturnTrueOnSuccess()
    {
        // Arrange
        var articleIds = new[] { "10001", "10002", "10003" };
        _mockHandler.SetupTextResponse("/unread_entries.json", "[10001, 10002, 10003]", HttpStatusCode.OK);

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
    public async Task MarkArticlesAsReadAsync_WithInvalidIds_ShouldReturnFalse()
    {
        // Arrange
        var articleIds = new[] { "invalid", "not_a_number" };

        // Act
        var result = await _client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_ShouldMarkAllFeedArticlesAsRead()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse("/unread_entries.json", TestDataFactory.CreateUnreadEntriesJson());
        _mockHandler.SetupTextResponse($"/feeds/{feed.Id}/entries.json", TestDataFactory.CreateEntriesListJson());

        _mockHandler.SetupResponse("/unread_entries.json", req =>
        {
            if (req.Method == HttpMethod.Delete)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(TestDataFactory.CreateUnreadEntriesJson(), System.Text.Encoding.UTF8, "application/json"),
            };
        });

        // Act
        var result = await _client.MarkFeedAsReadAsync(feed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_WithNoUnreadArticles_ShouldReturnTrue()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse("/unread_entries.json", TestDataFactory.CreateEmptyUnreadEntriesJson());

        // Act
        var result = await _client.MarkFeedAsReadAsync(feed);

        // Assert
        Assert.IsTrue(result);
    }
}
