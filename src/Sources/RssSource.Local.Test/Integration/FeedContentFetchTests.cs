// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.Integration;

/// <summary>
/// 订阅源内容获取集成测试.
/// 使用真实的 RSS 订阅源进行测试.
/// </summary>
[TestClass]
[TestCategory("Network")]
public sealed class FeedContentFetchTests : IntegrationTestBase
{
    [TestInitialize]
    public async Task Setup()
    {
        await InitializeAsync();
    }

    [TestCleanup]
    public new void Dispose()
    {
        base.Dispose();
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithIthome_ShouldReturnArticles()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(IthomeFeed);

        // Act
        var detail = await Client.GetFeedDetailAsync(feed!);

        // Assert
        Assert.IsNotNull(detail);
        Assert.IsNotNull(detail.Feed);
        Assert.IsNotNull(detail.Articles);
        Assert.IsTrue(detail.Articles.Count > 0, "IT之家应该有文章");

        // 验证文章内容
        var firstArticle = detail.Articles[0];
        Assert.IsTrue(!string.IsNullOrEmpty(firstArticle.Id), "文章应有 ID");
        Assert.IsTrue(!string.IsNullOrEmpty(firstArticle.Title), "文章应有标题");
        Assert.AreEqual(feed!.Id, firstArticle.FeedId, "文章的 FeedId 应匹配");

        Console.WriteLine($"IT之家获取到 {detail.Articles.Count} 篇文章");
        Console.WriteLine($"第一篇文章: {firstArticle.Title}");
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithGeekpark_ShouldReturnArticles()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(GeekparkFeed);

        // Act
        var detail = await Client.GetFeedDetailAsync(feed!);

        // Assert
        Assert.IsNotNull(detail);
        Assert.IsNotNull(detail.Articles);
        Assert.IsTrue(detail.Articles.Count > 0, "极客公园应该有文章");

        Console.WriteLine($"极客公园获取到 {detail.Articles.Count} 篇文章");
        Console.WriteLine($"第一篇文章: {detail.Articles[0].Title}");
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithDotNetBlog_ShouldReturnArticles()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(DotNetBlogFeed);

        // Act
        var detail = await Client.GetFeedDetailAsync(feed!);

        // Assert
        Assert.IsNotNull(detail);
        Assert.IsNotNull(detail.Articles);
        Assert.IsTrue(detail.Articles.Count > 0, ".NET Blog 应该有文章");

        // .NET Blog 通常有作者信息
        Console.WriteLine($".NET Blog 获取到 {detail.Articles.Count} 篇文章");
        Console.WriteLine($"第一篇文章: {detail.Articles[0].Title}");
        Console.WriteLine($"作者: {detail.Articles[0].Author ?? "(无)"}");
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_WithInvalidUrl_ShouldReturnNull()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(new RssFeed
        {
            Name = "无效订阅源",
            Url = "https://invalid-url-that-does-not-exist.example.com/rss",
        });

        // Act
        var detail = await Client.GetFeedDetailAsync(feed!);

        // Assert
        Assert.IsNull(detail, "无效 URL 应返回 null");
    }

    [TestMethod]
    public async Task GetFeedDetailListAsync_WithMultipleFeeds_ShouldReturnAllDetails()
    {
        // Arrange
        var feed1 = await Client.AddFeedAsync(IthomeFeed);
        var feed2 = await Client.AddFeedAsync(DotNetBlogFeed);

        var feeds = new List<RssFeed> { feed1!, feed2! };

        // Act
        var details = await Client.GetFeedDetailListAsync(feeds);

        // Assert
        Assert.IsNotNull(details);
        Assert.AreEqual(2, details.Count, "应返回两个订阅源的详情");

        foreach (var detail in details)
        {
            Assert.IsNotNull(detail.Feed);
            Assert.IsTrue(detail.Articles.Count > 0);
            Console.WriteLine($"{detail.Feed.Name}: {detail.Articles.Count} 篇文章");
        }
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_ShouldUpdateFeedMetadata()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(new RssFeed
        {
            Name = "临时名称",
            Url = "https://devblogs.microsoft.com/dotnet/feed/",
        });

        // Act
        var detail = await Client.GetFeedDetailAsync(feed!);

        // Assert
        Assert.IsNotNull(detail);
        Assert.IsNotNull(detail.Feed);

        // 从 RSS Feed 获取的信息应该更新订阅源元数据
        Console.WriteLine($"更新后的名称: {detail.Feed.Name}");
        Console.WriteLine($"描述: {detail.Feed.Description ?? "(无)"}");
        Console.WriteLine($"网站: {detail.Feed.Website ?? "(无)"}");
    }

    [TestMethod]
    public async Task GetFeedDetailAsync_ArticlesShouldHaveValidPublishTime()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(IthomeFeed);

        // Act
        var detail = await Client.GetFeedDetailAsync(feed!);

        // Assert
        Assert.IsNotNull(detail);
        Assert.IsTrue(detail.Articles.Count > 0);

        var articlesWithPublishTime = detail.Articles
            .Where(a => !string.IsNullOrEmpty(a.PublishTime))
            .ToList();

        Assert.IsTrue(articlesWithPublishTime.Count > 0, "应该有带发布时间的文章");

        foreach (var article in articlesWithPublishTime.Take(3))
        {
            Console.WriteLine($"{article.Title} - 发布于: {article.PublishTime}");
        }
    }
}
