// Copyright (c) Richasy. All rights reserved.

namespace RssSource.GoogleReader.Test.UnitTests;

/// <summary>
/// GoogleReaderClient 文章管理单元测试.
/// </summary>
[TestClass]
public sealed class GoogleReaderClientArticleTests
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
    public async Task GetFeedDetailAsync_ShouldReturnArticles()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse(Uri.EscapeDataString(feed.Id), TestDataFactory.CreateStreamContentJson(feed.Id));

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(feed, result.Feed);
        Assert.AreEqual(2, result.Articles.Count);

        // 验证文章内容
        var firstArticle = result.Articles[0];
        Assert.AreEqual("测试文章标题 1", firstArticle.Title);
        Assert.AreEqual("测试作者", firstArticle.Author);
        Assert.IsNotNull(firstArticle.Url);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_ShouldExtractCoverFromContent()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse(Uri.EscapeDataString(feed.Id), TestDataFactory.CreateStreamContentJson(feed.Id));

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        var articleWithImage = result.Articles.FirstOrDefault(a => a.CoverUrl != null);
        Assert.IsNotNull(articleWithImage);
        Assert.AreEqual("https://example.com/image.jpg", articleWithImage.CoverUrl);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_ShouldSetPublishTime()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse(Uri.EscapeDataString(feed.Id), TestDataFactory.CreateStreamContentJson(feed.Id));

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        foreach (var article in result.Articles)
        {
            var publishTime = article.GetPublishTime();
            Assert.IsNotNull(publishTime);
        }
    }

    [TestMethod]
    public async Task GetFeedDetailListAsync_ShouldReturnMultipleResults()
    {
        // Arrange
        var feeds = new List<RssFeed>
        {
            TestDataFactory.IthomeFeed,
            TestDataFactory.GeekparkFeed,
            TestDataFactory.DotNetBlogFeed,
        };

        foreach (var feed in feeds)
        {
            _mockHandler.SetupTextResponse(Uri.EscapeDataString(feed.Id), TestDataFactory.CreateStreamContentJson(feed.Id));
        }

        // Act
        var results = await _client.GetFeedDetailListAsync(feeds);

        // Assert
        Assert.AreEqual(3, results.Count);
        Assert.IsTrue(results.All(r => r.Articles.Count > 0));
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/edit-tag", "OK");
        var articleIds = new[] { "article1", "article2", "article3" };

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
    public async Task MarkArticlesAsReadAsync_WithFailure_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/edit-tag", "ERROR");
        var articleIds = new[] { "article1" };

        // Act
        var result = await _client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/mark-all-as-read", "OK");

        // Act
        var result = await _client.MarkFeedAsReadAsync(TestDataFactory.IthomeFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/mark-all-as-read", "OK");

        // Act
        var result = await _client.MarkGroupAsReadAsync(TestDataFactory.TechGroup);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.GetFeedDetailAsync(null!));
    }
}
