// Copyright (c) Richasy. All rights reserved.

namespace RssSource.GoogleReader.Test.Integration;

/// <summary>
/// Google Reader API 集成测试.
/// 使用真实的 FreshRSS 服务器进行测试.
/// </summary>
/// <remarks>
/// 这些测试需要网络连接和有效的测试服务器.
/// 在 CI/CD 环境中可能需要跳过这些测试.
/// </remarks>
[TestClass]
public sealed class GoogleReaderApiTests : GoogleReaderIntegrationTestBase
{
    [TestInitialize]
    public async Task Setup()
    {
        await InitializeAsync();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    [TestMethod]
    [TestCategory("Integration")]
    public void SignIn_WithValidCredentials_ShouldSucceed()
    {
        // 已经在 InitializeAsync 中登录
        // Assert
        Assert.IsTrue(Client.IsAuthenticated);
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

        Console.WriteLine($"获取到 {groups.Count} 个分组和 {feeds.Count} 个订阅源");
        foreach (var group in groups)
        {
            Console.WriteLine($"  分组: {group.Name} ({group.Id})");
        }

        foreach (var feed in feeds)
        {
            Console.WriteLine($"  订阅: {feed.Name} ({feed.Url})");
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
            Assert.Inconclusive("没有订阅源可用于测试");
            return;
        }

        var feed = feeds[0];

        // Act
        var detail = await Client.GetFeedDetailAsync(feed);

        // Assert
        Assert.IsNotNull(detail);
        Assert.AreEqual(feed.Id, detail.Feed.Id);

        Console.WriteLine($"订阅源 {feed.Name} 有 {detail.Articles.Count} 篇文章");
        foreach (var article in detail.Articles.Take(5))
        {
            Console.WriteLine($"  - {article.Title}");
            Console.WriteLine($"    URL: {article.Url}");
            Console.WriteLine($"    发布时间: {article.GetPublishTime()}");
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task GetFeedDetailListAsync_ShouldReturnMultipleResults()
    {
        // Arrange
        var (_, feeds) = await Client.GetFeedListAsync();
        if (feeds.Count < 2)
        {
            Assert.Inconclusive("订阅源数量不足，无法进行批量测试");
            return;
        }

        var feedsToFetch = feeds.Take(3).ToList();

        // Act
        var details = await Client.GetFeedDetailListAsync(feedsToFetch);

        // Assert
        Assert.IsNotNull(details);
        Assert.IsTrue(details.Count > 0);

        Console.WriteLine($"批量获取 {details.Count}/{feedsToFetch.Count} 个订阅源成功");
        foreach (var detail in details)
        {
            Console.WriteLine($"  {detail.Feed.Name}: {detail.Articles.Count} 篇文章");
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task AddAndDeleteFeed_ShouldWork()
    {
        // Arrange
        var testFeed = new RssFeed
        {
            Name = "Test Feed - " + Guid.NewGuid().ToString("N")[..8],
            Url = "https://www.ithome.com/rss", // 使用已知可用的 RSS
        };

        RssFeed? addedFeed = null;

        try
        {
            // Act 1: 添加订阅
            addedFeed = await Client.AddFeedAsync(testFeed);

            // Assert 1
            Assert.IsNotNull(addedFeed);
            Assert.IsTrue(addedFeed.Id.StartsWith("feed/", StringComparison.Ordinal));
            Console.WriteLine($"成功添加订阅: {addedFeed.Name} ({addedFeed.Id})");

            // 等待服务器同步（FreshRSS 可能有缓存延迟）
            await Task.Delay(2000);

            // 验证订阅已添加
            var (_, feeds) = await Client.GetFeedListAsync();
            Console.WriteLine($"当前订阅列表 (共 {feeds.Count} 个):");
            foreach (var f in feeds)
            {
                Console.WriteLine($"  - {f.Name}: {f.Id} ({f.Url})");
            }

            // 使用 URL 匹配而不是 ID 匹配（因为服务器可能返回不同的 ID 格式）
            var found = feeds.Any(f => f.Url == testFeed.Url || f.Id == addedFeed.Id);
            Console.WriteLine($"订阅列表中是否找到: {found}");
            Assert.IsTrue(found, "新添加的订阅应该出现在列表中");
        }
        finally
        {
            // Cleanup: 删除测试订阅
            if (addedFeed != null)
            {
                var deleteResult = await Client.DeleteFeedAsync(addedFeed);
                Console.WriteLine($"删除测试订阅: {(deleteResult ? "成功" : "失败")}");
            }
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task ExportOpmlAsync_ShouldReturnValidOpml()
    {
        // Act
        var opml = await Client.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(opml);
        Assert.IsTrue(opml.Length > 0);
        Assert.IsTrue(opml.Contains("<opml") || opml.Contains("<?xml"));

        Console.WriteLine("OPML 导出成功:");
        Console.WriteLine(opml[..Math.Min(500, opml.Length)] + "...");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task MarkArticlesAsRead_ShouldWork()
    {
        // Arrange
        var (_, feeds) = await Client.GetFeedListAsync();
        if (feeds.Count == 0)
        {
            Assert.Inconclusive("没有订阅源可用于测试");
            return;
        }

        var detail = await Client.GetFeedDetailAsync(feeds[0]);
        if (detail == null || detail.Articles.Count == 0)
        {
            Assert.Inconclusive("没有文章可用于测试");
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
    public async Task FullWorkflow_ShouldComplete()
    {
        // 这是一个完整工作流测试
        Console.WriteLine("=== 完整工作流测试 ===");

        // Step 1: 获取订阅列表
        Console.WriteLine("\n1. 获取订阅列表...");
        var (groups, feeds) = await Client.GetFeedListAsync();
        Console.WriteLine($"   获取到 {groups.Count} 个分组, {feeds.Count} 个订阅");
        Assert.IsNotNull(feeds);

        // Step 2: 获取文章
        if (feeds.Count > 0)
        {
            Console.WriteLine("\n2. 获取第一个订阅的文章...");
            var detail = await Client.GetFeedDetailAsync(feeds[0]);
            Assert.IsNotNull(detail);
            Console.WriteLine($"   {feeds[0].Name}: {detail.Articles.Count} 篇文章");

            // Step 3: 标记已读
            if (detail.Articles.Count > 0)
            {
                Console.WriteLine("\n3. 标记第一篇文章为已读...");
                var markResult = await Client.MarkArticlesAsReadAsync([detail.Articles[0].Id]);
                Assert.IsTrue(markResult);
                Console.WriteLine("   标记成功");
            }
        }

        // Step 4: 导出 OPML
        Console.WriteLine("\n4. 导出 OPML...");
        var opml = await Client.ExportOpmlAsync();
        Assert.IsTrue(!string.IsNullOrEmpty(opml));
        Console.WriteLine($"   导出成功，大小: {opml.Length} 字符");

        Console.WriteLine("\n=== 工作流测试完成 ===");
    }
}
