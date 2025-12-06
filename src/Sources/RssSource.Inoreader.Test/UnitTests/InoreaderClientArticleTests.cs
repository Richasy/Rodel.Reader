// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test.UnitTests;

/// <summary>
/// InoreaderClient 文章操作单元测试.
/// </summary>
[TestClass]
public sealed class InoreaderClientArticleTests
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
        _mockHandler.SetupTextResponse("/stream/contents/", TestDataFactory.CreateStreamContentJson(feed.Id));

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(feed.Id, result.Feed.Id);
        Assert.AreEqual(2, result.Articles.Count);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_ArticlesShouldHaveCorrectProperties()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse("/stream/contents/", TestDataFactory.CreateStreamContentJson(feed.Id));

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        var article = result!.Articles[0];
        Assert.AreEqual("tag:google.com,2005:reader/item/000000001", article.Id);
        Assert.AreEqual("测试文章标题 1", article.Title);
        Assert.AreEqual("https://example.com/article1", article.Url);
        Assert.AreEqual("测试作者", article.Author);
        Assert.AreEqual(feed.Id, article.FeedId);
        Assert.IsNotNull(article.GetPublishTime());
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_ShouldExtractCoverFromContent()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse("/stream/contents/", TestDataFactory.CreateStreamContentJson(feed.Id));

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        // 第二篇文章的内容包含图片
        var article = result!.Articles[1];
        Assert.AreEqual("https://example.com/image.jpg", article.CoverUrl);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_ShouldExtractTags()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse("/stream/contents/", TestDataFactory.CreateStreamContentJson(feed.Id));

        // Act
        var result = await _client.GetFeedDetailAsync(feed);

        // Assert
        var article = result!.Articles[0];
        var tags = article.GetTagList();
        Assert.AreEqual(1, tags.Count);
        Assert.AreEqual("科技", tags[0]);
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        var feed = TestDataFactory.IthomeFeed;
        _mockHandler.SetupTextResponse("/stream/contents/", TestDataFactory.CreateStreamContentJson(feed.Id));

        // Act
        await _client.GetFeedDetailAsync(feed);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/stream/contents/"));
        Assert.AreEqual(HttpMethod.Get, request.Method);
        Assert.IsTrue(request.RequestUri!.PathAndQuery.Contains("n=100")); // 默认请求100篇
    }

    [TestMethod]
    public async Task GetFeedDetailListAsync_ShouldReturnMultipleFeedDetails()
    {
        // Arrange
        var feeds = new[] { TestDataFactory.IthomeFeed, TestDataFactory.GeekparkFeed };

        foreach (var feed in feeds)
        {
            _mockHandler.SetupTextResponse(
                Uri.EscapeDataString(feed.Id),
                TestDataFactory.CreateStreamContentJson(feed.Id));
        }

        // Act
        var results = await _client.GetFeedDetailListAsync(feeds);

        // Assert
        Assert.AreEqual(2, results.Count);
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_ShouldReturnTrue_WhenSuccessful()
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
    public async Task MarkArticlesAsReadAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/edit-tag", "OK");

        var articleIds = new[] { "article1", "article2" };

        // Act
        await _client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/edit-tag"));
        Assert.AreEqual(HttpMethod.Post, request.Method);

        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("a=user%2F-%2Fstate%2Fcom.google%2Fread") || content.Contains("a=user/-/state/com.google/read"));
        Assert.IsTrue(content.Contains("i=article1"));
        Assert.IsTrue(content.Contains("i=article2"));
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_WithEmptyList_ShouldReturnTrue()
    {
        // Act
        var result = await _client.MarkArticlesAsReadAsync([]);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(0, _mockHandler.Requests.Count); // 不应发送请求
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/edit-tag", "ERROR");

        // Act
        var result = await _client.MarkArticlesAsReadAsync(["article1"]);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/mark-all-as-read", "OK");

        // Act
        var result = await _client.MarkFeedAsReadAsync(TestDataFactory.IthomeFeed);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/mark-all-as-read", "OK");

        var feed = TestDataFactory.IthomeFeed;

        // Act
        await _client.MarkFeedAsReadAsync(feed);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/mark-all-as-read"));
        Assert.AreEqual(HttpMethod.Post, request.Method);

        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains($"s={Uri.EscapeDataString(feed.Id)}") || content.Contains($"s={feed.Id}"));
        Assert.IsTrue(content.Contains("ts=")); // 应该包含时间戳
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/mark-all-as-read", "OK");

        // Act
        var result = await _client.MarkGroupAsReadAsync(TestDataFactory.TechGroup);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_ShouldSendGroupIdInRequest()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/mark-all-as-read", "OK");

        var group = TestDataFactory.TechGroup;

        // Act
        await _client.MarkGroupAsReadAsync(group);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/mark-all-as-read"));
        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("s=user%2F-%2Flabel%2F") || content.Contains("s=user/-/label/"));
    }
}
