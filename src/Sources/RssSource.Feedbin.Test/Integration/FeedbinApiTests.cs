// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Feedbin.Test.Integration;

/// <summary>
/// Feedbin API 集成测试.
/// 使用真实 Feedbin API 进行测试.
/// </summary>
/// <remarks>
/// 注意：这些测试会实际调用 Feedbin API，可能会产生费用或影响账户数据。
/// 默认情况下这些测试被标记为 [Ignore]，需要手动启用运行。
/// </remarks>
[TestClass]
public sealed class FeedbinApiTests : FeedbinIntegrationTestBase
{
    [TestInitialize]
    public async Task TestInitialize()
    {
        await InitializeAsync();
    }

    [TestMethod]
    [TestCategory("Integration")]
    public Task SignInAsync_WithValidCredentials_ShouldSucceed()
    {
        // Assert - InitializeAsync 中已经登录成功
        Assert.IsTrue(Client.IsAuthenticated);
        return Task.CompletedTask;
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task GetFeedListAsync_ShouldReturnSubscriptions()
    {
        // Act
        var (groups, feeds) = await Client.GetFeedListAsync();

        // Assert
        Assert.IsNotNull(groups);
        Assert.IsNotNull(feeds);

        // 输出结果供调试
        Console.WriteLine($"获取到 {groups.Count} 个分组:");
        foreach (var group in groups)
        {
            Console.WriteLine($"  - {group.Name} (ID: {group.Id})");
        }

        Console.WriteLine($"获取到 {feeds.Count} 个订阅源:");
        foreach (var feed in feeds)
        {
            var groupNames = string.Join(", ", feed.GetGroupIdList());
            Console.WriteLine($"  - {feed.Name} (FeedId: {feed.Id}, SubId: {feed.Comment}, Groups: [{groupNames}])");
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task GetFeedDetailAsync_ShouldReturnArticles()
    {
        // Arrange
        var (_, feeds) = await Client.GetFeedListAsync();

        if (feeds.Count == 0)
        {
            Assert.Inconclusive("没有可用的订阅源进行测试");
            return;
        }

        var feed = feeds[0];

        // Act
        var detail = await Client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(detail);
        Assert.AreEqual(feed.Id, detail.Feed.Id);
        Console.WriteLine($"订阅源 '{feed.Name}' 包含 {detail.Articles.Count} 篇文章:");
        foreach (var article in detail.Articles.Take(5))
        {
            Console.WriteLine($"  - [{article.Id}] {article.Title}");
            Console.WriteLine($"    作者: {article.Author}, 发布时间: {article.PublishTime}");
            if (!string.IsNullOrEmpty(article.CoverUrl))
            {
                Console.WriteLine($"    封面: {article.CoverUrl}");
            }
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task ExportOpmlAsync_ShouldGenerateValidOpml()
    {
        // Act
        var opml = await Client.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(opml);
        Assert.IsTrue(opml.Contains("<opml", StringComparison.OrdinalIgnoreCase));
        Console.WriteLine("导出的 OPML 内容（前 2000 字符）:");
        Console.WriteLine(opml.Length > 2000 ? opml[..2000] + "..." : opml);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task AddAndDeleteFeedAsync_ShouldWorkCorrectly()
    {
        // Arrange - 使用 .NET Blog RSS
        var testFeedUrl = DotNetBlogRssUrl;
        var testFeed = new RssFeed
        {
            Name = ".NET Blog (Test)",
            Url = testFeedUrl,
        };

        // Act - 添加订阅
        var addedFeed = await Client.AddFeedAsync(testFeed);

        // Assert - 添加成功
        Assert.IsNotNull(addedFeed);
        Assert.IsFalse(string.IsNullOrEmpty(addedFeed.Id));
        Assert.IsFalse(string.IsNullOrEmpty(addedFeed.Comment));
        Console.WriteLine($"成功添加订阅: {addedFeed.Name} (FeedId: {addedFeed.Id}, SubId: {addedFeed.Comment})");

        // Act - 删除订阅
        var deleteResult = await Client.DeleteFeedAsync(addedFeed);

        // Assert - 删除成功
        Assert.IsTrue(deleteResult);
        Console.WriteLine($"成功删除订阅: {addedFeed.Name}");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task AddFeedWithTagAsync_ShouldCreateTagging()
    {
        // Arrange - 使用极客公园 RSS
        var testFeedUrl = GeekparkRssUrl;
        var testFeed = new RssFeed
        {
            Name = "极客公园 (Test)",
            Url = testFeedUrl,
        };
        testFeed.SetGroupIdList(["测试分组"]);

        // Act - 添加订阅
        var addedFeed = await Client.AddFeedAsync(testFeed);

        // Assert
        Assert.IsNotNull(addedFeed);
        Console.WriteLine($"成功添加订阅: {addedFeed.Name}，分组: {string.Join(", ", addedFeed.GetGroupIdList())}");

        // 验证分组已创建
        var (groups, _) = await Client.GetFeedListAsync();
        var testGroup = groups.FirstOrDefault(g => g.Name == "测试分组");
        Assert.IsNotNull(testGroup);

        // 清理 - 删除订阅
        await Client.DeleteFeedAsync(addedFeed);
        Console.WriteLine("测试清理完成");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task GetFeedDetailListAsync_ShouldBatchFetchFeeds()
    {
        // Arrange
        var (_, feeds) = await Client.GetFeedListAsync();

        if (feeds.Count == 0)
        {
            Assert.Inconclusive("没有可用的订阅源进行测试");
            return;
        }

        var feedsToFetch = feeds.Take(3).ToList();

        // Act
        var details = await Client.GetFeedDetailListAsync(feedsToFetch);

        // Assert
        Assert.IsNotNull(details);
        Console.WriteLine($"批量获取 {feedsToFetch.Count} 个订阅源，成功获取 {details.Count} 个:");
        foreach (var detail in details)
        {
            Console.WriteLine($"  - {detail.Feed.Name}: {detail.Articles.Count} 篇文章");
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task MarkArticlesAsReadAsync_ShouldSucceed()
    {
        // Arrange
        var (_, feeds) = await Client.GetFeedListAsync();

        if (feeds.Count == 0)
        {
            Assert.Inconclusive("没有可用的订阅源进行测试");
            return;
        }

        var detail = await Client.GetFeedDetailAsync(feeds[0]);
        if (detail == null || detail.Articles.Count == 0)
        {
            Assert.Inconclusive("没有可用的文章进行测试");
            return;
        }

        var articleIds = detail.Articles.Take(2).Select(a => a.Id).ToList();

        // Act
        var result = await Client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsTrue(result);
        Console.WriteLine($"成功标记 {articleIds.Count} 篇文章为已读");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task ImportOpmlAsync_ShouldSubmitImport()
    {
        // Arrange - 使用真实的 IT之家 RSS
        var opmlContent = $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="2.0">
            <head>
                <title>Test Import</title>
            </head>
            <body>
                <outline text="科技资讯" title="科技资讯">
                    <outline text="IT之家" type="rss" xmlUrl="{IthomeRssUrl}" htmlUrl="https://www.ithome.com" />
                </outline>
            </body>
        </opml>
        """;

        // Act
        var result = await Client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsTrue(result);
        Console.WriteLine("OPML 导入已提交");
    }
}
