// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Miniflux.Test.UnitTests;

/// <summary>
/// MinifluxClient 文章操作单元测试.
/// </summary>
[TestClass]
public sealed class MinifluxClientArticleTests
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
    public async Task GetFeedDetailAsync_ShouldReturnArticles()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed();
        _mockHandler.SetupResponse("/v1/feeds/1/entries", TestDataFactory.CreateEntriesResponse());

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(feed.Id, result.Feed.Id);
        Assert.AreEqual(2, result.Articles.Count);

        var article1 = result.Articles.FirstOrDefault(a => a.Id == "100");
        Assert.IsNotNull(article1);
        Assert.AreEqual("Test Article 1", article1.Title);
        Assert.AreEqual("Author 1", article1.Author);
        Assert.AreEqual("https://example.com/article1", article1.Url);
        Assert.IsNotNull(article1.CoverUrl);
        Assert.AreEqual("https://example.com/image1.jpg", article1.CoverUrl);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithNullFeed_ShouldThrow()
    {
        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.GetFeedDetailAsync(null!));
    }

    [TestMethod]
    public async Task GetFeedDetailListAsync_ShouldReturnMultipleResults()
    {
        // Arrange
        var feeds = new List<RssFeed>
        {
            TestDataFactory.CreateTestFeed("1", "Feed 1"),
            TestDataFactory.CreateTestFeed("2", "Feed 2"),
        };

        _mockHandler.SetupResponse("/v1/feeds/1/entries", TestDataFactory.CreateEntriesResponse());
        _mockHandler.SetupResponse("/v1/feeds/2/entries", TestDataFactory.CreateEntriesResponse());

        // Act
        var results = await _client.GetFeedDetailListAsync(feeds);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(2, results.Count);
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_ShouldReturnTrue()
    {
        // Arrange
        var articleIds = new[] { "100", "101" };
        _mockHandler.SetupResponse("/v1/entries", HttpStatusCode.NoContent);

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
    public async Task MarkFeedAsReadAsync_ShouldReturnTrue()
    {
        // Arrange
        var feed = TestDataFactory.CreateTestFeed();
        _mockHandler.SetupResponse("/v1/feeds/1/mark-all-as-read", HttpStatusCode.NoContent);

        // Act
        var result = await _client.MarkFeedAsReadAsync(feed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_ShouldReturnTrue()
    {
        // Arrange
        var group = TestDataFactory.CreateTestGroup();
        _mockHandler.SetupResponse("/v1/categories/1/mark-all-as-read", HttpStatusCode.NoContent);

        // Act
        var result = await _client.MarkGroupAsReadAsync(group);

        // Assert
        Assert.IsTrue(result);
    }
}
