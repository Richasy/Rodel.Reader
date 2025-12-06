// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Miniflux.Test.Integration;

/// <summary>
/// Miniflux API 集成测试.
/// 使用真实的 Miniflux 服务器进行测试.
/// </summary>
/// <remarks>
/// 这些测试需要网络连接和有效的测试服务器.
/// 在 CI/CD 环境中可能需要跳过这些测试.
/// </remarks>
[TestClass]
public sealed class MinifluxApiTests : MinifluxIntegrationTestBase
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
        // 首先获取一个可用的分类
        var (groups, _) = await Client.GetFeedListAsync();
        var testFeed = new RssFeed
        {
            Name = "Test Feed - " + Guid.NewGuid().ToString("N")[..8],
            Url = IthomeRssUrl,
        };

        // 如果有分组，添加到第一个分组
        if (groups.Count > 0)
        {
            testFeed.SetGroupIdList([groups[0].Id]);
        }

        RssFeed? addedFeed = null;

        try
        {
            // Act 1: 添加订阅
            addedFeed = await Client.AddFeedAsync(testFeed);

            // Assert 1
            Assert.IsNotNull(addedFeed);
            Console.WriteLine($"成功添加订阅: {addedFeed.Name} ({addedFeed.Id})");

            // 等待服务器同步
            await Task.Delay(2000);

            // 验证订阅已添加
            var (_, feeds) = await Client.GetFeedListAsync();
            Console.WriteLine($"当前订阅列表 (共 {feeds.Count} 个):");
            foreach (var f in feeds)
            {
                Console.WriteLine($"  - {f.Name}: {f.Id} ({f.Url})");
            }

            var found = feeds.Any(f => f.Id == addedFeed.Id);
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
    public async Task AddAndDeleteGroup_ShouldWork()
    {
        // Arrange
        var testGroup = new RssFeedGroup
        {
            Name = "TestGroup-" + Guid.NewGuid().ToString("N")[..8],
        };

        RssFeedGroup? addedGroup = null;

        try
        {
            // Act 1: 添加分组
            addedGroup = await Client.AddGroupAsync(testGroup);

            // Assert 1
            Assert.IsNotNull(addedGroup);
            Console.WriteLine($"成功添加分组: {addedGroup.Name} ({addedGroup.Id})");

            // Act 2: 更新分组名称
            addedGroup.Name = "Updated-" + addedGroup.Name;
            var updatedGroup = await Client.UpdateGroupAsync(addedGroup);

            // Assert 2
            Assert.IsNotNull(updatedGroup);
            Assert.IsTrue(updatedGroup.Name.StartsWith("Updated-", StringComparison.Ordinal));
            Console.WriteLine($"分组更新后: {updatedGroup.Name}");
        }
        finally
        {
            // Cleanup: 删除测试分组
            if (addedGroup != null)
            {
                var deleteResult = await Client.DeleteGroupAsync(addedGroup);
                Console.WriteLine($"删除测试分组: {(deleteResult ? "成功" : "失败")}");
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
    public async Task MarkFeedAsRead_ShouldWork()
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
        var result = await Client.MarkFeedAsReadAsync(feed);

        // Assert
        Assert.IsTrue(result);
        Console.WriteLine($"成功标记订阅源 {feed.Name} 的所有文章为已读");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task UpdateFeed_ShouldWork()
    {
        // Arrange
        var (_, feeds) = await Client.GetFeedListAsync();
        if (feeds.Count == 0)
        {
            Assert.Inconclusive("没有订阅源可用于测试");
            return;
        }

        var feed = feeds[0];
        var originalName = feed.Name;
        var testName = $"Test-{Guid.NewGuid().ToString("N")[..4]}-{originalName}";

        var newFeed = feed.Clone();
        newFeed.Name = testName;

        try
        {
            // Act: 更新名称
            var updateResult = await Client.UpdateFeedAsync(newFeed, feed);

            // Assert
            Assert.IsTrue(updateResult);
            Console.WriteLine($"订阅源名称更新成功: {originalName} -> {testName}");

            // 验证更新
            var (_, updatedFeeds) = await Client.GetFeedListAsync();
            var updatedFeed = updatedFeeds.FirstOrDefault(f => f.Id == feed.Id);
            Assert.IsNotNull(updatedFeed);
            Assert.AreEqual(testName, updatedFeed.Name);
        }
        finally
        {
            // Cleanup: 恢复原名称
            var restoreFeed = feed.Clone();
            restoreFeed.Name = originalName;
            await Client.UpdateFeedAsync(restoreFeed, feed);
            Console.WriteLine($"已恢复订阅源名称: {originalName}");
        }
    }
}
