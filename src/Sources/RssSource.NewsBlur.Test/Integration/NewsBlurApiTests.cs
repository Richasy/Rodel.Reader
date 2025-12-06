// Copyright (c) Richasy. All rights reserved.

namespace RssSource.NewsBlur.Test.Integration;

/// <summary>
/// NewsBlur API 集成测试.
/// 这些测试使用真实的 NewsBlur API，需要有效的账户凭据.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public sealed class NewsBlurApiTests : NewsBlurIntegrationTestBase
{
    [TestInitialize]
    public async Task SetupAsync()
    {
        await InitializeAsync();
    }

    [TestMethod]
    public void SignIn_WithValidCredentials_ShouldSucceed()
    {
        // Assert - 已在 InitializeAsync 中登录
        Assert.IsTrue(Client.IsAuthenticated);
    }

    [TestMethod]
    public async Task GetFeedListAsync_ShouldReturnFeedsAndGroups()
    {
        // Act
        var (groups, feeds) = await Client.GetFeedListAsync();

        // Assert
        Assert.IsNotNull(groups);
        Assert.IsNotNull(feeds);

        // 输出调试信息
        Console.WriteLine($"获取到 {groups.Count} 个分组和 {feeds.Count} 个订阅源");

        foreach (var group in groups)
        {
            Console.WriteLine($"分组: {group.Name} (Id: {group.Id})");
        }

        foreach (var feed in feeds.Take(5))
        {
            Console.WriteLine($"订阅源: {feed.Name} (Id: {feed.Id}, Groups: {string.Join(",", feed.GetGroupIdList())})");
        }
    }

    [TestMethod]
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

        Console.WriteLine($"订阅源 {feed.Name} 获取到 {detail.Articles.Count} 篇文章");

        foreach (var article in detail.Articles.Take(3))
        {
            Console.WriteLine($"  - {article.Title} ({article.GetPublishTime()?.ToString("yyyy-MM-dd HH:mm")})");
        }
    }

    [TestMethod]
    public async Task AddAndDeleteFeedAsync_ShouldWork()
    {
        // 注意：此测试会修改账户数据，使用不常用的订阅源进行测试

        // Arrange
        var testFeedUrl = DotNetBlogRssUrl;
        var newFeed = new RssFeed { Url = testFeedUrl };

        // Act - 添加订阅源
        var addedFeed = await Client.AddFeedAsync(newFeed);

        // 如果订阅源已存在，先尝试删除再添加
        if (addedFeed == null)
        {
            var (_, existingFeeds) = await Client.GetFeedListAsync();
            var existing = existingFeeds.FirstOrDefault(f => f.Url?.Contains("devblogs.microsoft.com") == true);
            if (existing != null)
            {
                await Client.DeleteFeedAsync(existing);
                await Task.Delay(1000); // 等待服务器处理
                addedFeed = await Client.AddFeedAsync(newFeed);
            }
        }

        if (addedFeed == null)
        {
            Assert.Inconclusive("无法添加测试订阅源");
            return;
        }

        Console.WriteLine($"添加订阅源成功: {addedFeed.Name} (Id: {addedFeed.Id})");

        // Act - 删除订阅源
        var deleteResult = await Client.DeleteFeedAsync(addedFeed);

        // Assert
        Assert.IsTrue(deleteResult);
        Console.WriteLine("删除订阅源成功");
    }

    [TestMethod]
    public async Task AddUpdateAndDeleteGroupAsync_ShouldWork()
    {
        // Arrange
        var testGroupName = $"TestGroup_{DateTime.Now:yyyyMMddHHmmss}";
        var newGroup = new RssFeedGroup { Name = testGroupName };

        // Act - 添加分组
        var addedGroup = await Client.AddGroupAsync(newGroup);

        Assert.IsNotNull(addedGroup);
        Console.WriteLine($"添加分组成功: {addedGroup.Name} (Id: {addedGroup.Id})");

        // Act - 更新分组名称
        var updatedGroupName = testGroupName + "_Updated";
        addedGroup.Name = updatedGroupName;
        var updatedGroup = await Client.UpdateGroupAsync(addedGroup);

        Assert.IsNotNull(updatedGroup);
        Assert.AreEqual(updatedGroupName, updatedGroup.Name);
        Console.WriteLine($"更新分组成功: {updatedGroup.Name}");

        // Act - 删除分组
        var deleteResult = await Client.DeleteGroupAsync(updatedGroup);

        Assert.IsTrue(deleteResult);
        Console.WriteLine("删除分组成功");
    }

    [TestMethod]
    public async Task MarkArticlesAsReadAsync_ShouldSucceed()
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
    public async Task ExportOpmlAsync_ShouldReturnValidOpml()
    {
        // Act
        var opml = await Client.ExportOpmlAsync();

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(opml));
        Assert.IsTrue(opml.Contains("<opml") || opml.Contains("<?xml"));

        Console.WriteLine($"OPML 导出成功，长度: {opml.Length} 字符");
        Console.WriteLine($"前 500 个字符:\n{opml[..Math.Min(500, opml.Length)]}");
    }

    [TestMethod]
    public async Task FullWorkflow_ShouldWork()
    {
        // 这是一个完整的工作流测试

        // 1. 获取订阅源列表
        Console.WriteLine("=== 步骤 1: 获取订阅源列表 ===");
        var (groups, feeds) = await Client.GetFeedListAsync();
        Console.WriteLine($"获取到 {groups.Count} 个分组和 {feeds.Count} 个订阅源");

        // 2. 如果没有订阅源，添加测试订阅源
        if (feeds.Count == 0)
        {
            Console.WriteLine("=== 步骤 2: 添加测试订阅源 ===");
            var testFeeds = new[]
            {
                new RssFeed { Url = IthomeRssUrl },
                new RssFeed { Url = GeekparkRssUrl },
            };

            foreach (var testFeed in testFeeds)
            {
                var added = await Client.AddFeedAsync(testFeed);
                if (added != null)
                {
                    Console.WriteLine($"添加订阅源: {added.Name}");
                }
            }

            // 重新获取列表
            (groups, feeds) = await Client.GetFeedListAsync();
        }

        // 3. 获取文章详情
        if (feeds.Count > 0)
        {
            Console.WriteLine("=== 步骤 3: 获取文章详情 ===");
            var detail = await Client.GetFeedDetailAsync(feeds[0]);
            if (detail != null)
            {
                Console.WriteLine($"订阅源 {detail.Feed.Name} 有 {detail.Articles.Count} 篇文章");

                // 4. 标记文章已读
                if (detail.Articles.Count > 0)
                {
                    Console.WriteLine("=== 步骤 4: 标记文章已读 ===");
                    var articleId = detail.Articles[0].Id;
                    var markResult = await Client.MarkArticlesAsReadAsync([articleId]);
                    Console.WriteLine($"标记已读结果: {markResult}");
                }
            }
        }

        // 5. 导出 OPML
        Console.WriteLine("=== 步骤 5: 导出 OPML ===");
        var opml = await Client.ExportOpmlAsync();
        Console.WriteLine($"OPML 导出成功，长度: {opml.Length} 字符");

        // 6. 登出
        Console.WriteLine("=== 步骤 6: 登出 ===");
        var logoutResult = await Client.SignOutAsync();
        Console.WriteLine($"登出结果: {logoutResult}");
        Assert.IsFalse(Client.IsAuthenticated);
    }
}
