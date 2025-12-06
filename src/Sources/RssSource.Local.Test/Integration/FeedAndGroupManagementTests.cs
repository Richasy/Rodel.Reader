// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.Integration;

/// <summary>
/// 订阅源和分组管理集成测试.
/// 测试实际与存储层的交互.
/// </summary>
[TestClass]
public sealed class FeedAndGroupManagementTests : IntegrationTestBase
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
    public async Task AddGroup_ThenGetFeedList_ShouldReturnGroup()
    {
        // Arrange
        var group = new RssFeedGroup { Name = "科技新闻" };

        // Act
        var addedGroup = await Client.AddGroupAsync(group);
        var (groups, _) = await Client.GetFeedListAsync();

        // Assert
        Assert.IsNotNull(addedGroup);
        Assert.IsTrue(!string.IsNullOrEmpty(addedGroup.Id));
        Assert.AreEqual(1, groups.Count);
        Assert.AreEqual("科技新闻", groups[0].Name);
    }

    [TestMethod]
    public async Task AddFeed_ThenGetFeedList_ShouldReturnFeed()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "IT之家",
            Url = "https://www.ithome.com/rss",
            Website = "https://www.ithome.com",
        };

        // Act
        var addedFeed = await Client.AddFeedAsync(feed);
        var (_, feeds) = await Client.GetFeedListAsync();

        // Assert
        Assert.IsNotNull(addedFeed);
        Assert.AreEqual("feed/https://www.ithome.com/rss", addedFeed.Id);
        Assert.AreEqual(1, feeds.Count);
        Assert.AreEqual("IT之家", feeds[0].Name);
    }

    [TestMethod]
    public async Task AddFeedToGroup_ShouldAssociateFeedWithGroup()
    {
        // Arrange
        var group = await Client.AddGroupAsync(new RssFeedGroup { Name = "科技" });
        var feed = new RssFeed
        {
            Name = "IT之家",
            Url = "https://www.ithome.com/rss",
            GroupIds = group!.Id,
        };

        // Act
        await Client.AddFeedAsync(feed);
        var (groups, feeds) = await Client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(1, groups.Count);
        Assert.AreEqual(1, feeds.Count);
        Assert.IsTrue(feeds[0].GroupIds?.Contains(group.Id, StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task UpdateGroup_ShouldPersistChanges()
    {
        // Arrange
        var group = await Client.AddGroupAsync(new RssFeedGroup { Name = "原始名称" });

        // Act
        group!.Name = "更新后的名称";
        await Client.UpdateGroupAsync(group);
        var (groups, _) = await Client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(1, groups.Count);
        Assert.AreEqual("更新后的名称", groups[0].Name);
    }

    [TestMethod]
    public async Task UpdateFeed_ShouldPersistChanges()
    {
        // Arrange
        var oldFeed = await Client.AddFeedAsync(new RssFeed
        {
            Name = "原始名称",
            Url = "https://example.com/rss",
        });

        // Act
        var newFeed = new RssFeed
        {
            Id = oldFeed!.Id,
            Name = "更新后的名称",
            Url = oldFeed.Url,
            Description = "新描述",
        };
        await Client.UpdateFeedAsync(newFeed, oldFeed);
        var (_, feeds) = await Client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(1, feeds.Count);
        Assert.AreEqual("更新后的名称", feeds[0].Name);
        Assert.AreEqual("新描述", feeds[0].Description);
    }

    [TestMethod]
    public async Task DeleteGroup_ShouldRemoveGroup()
    {
        // Arrange
        var group = await Client.AddGroupAsync(new RssFeedGroup { Name = "待删除分组" });

        // Act
        var deleteResult = await Client.DeleteGroupAsync(group!);
        var (groups, _) = await Client.GetFeedListAsync();

        // Assert
        Assert.IsTrue(deleteResult);
        Assert.AreEqual(0, groups.Count);
    }

    [TestMethod]
    public async Task DeleteFeed_ShouldRemoveFeed()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(new RssFeed
        {
            Name = "待删除订阅源",
            Url = "https://example.com/rss",
        });

        // Act
        var deleteResult = await Client.DeleteFeedAsync(feed!);
        var (_, feeds) = await Client.GetFeedListAsync();

        // Assert
        Assert.IsTrue(deleteResult);
        Assert.AreEqual(0, feeds.Count);
    }

    [TestMethod]
    public async Task AddMultipleGroupsAndFeeds_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var techGroup = await Client.AddGroupAsync(new RssFeedGroup { Name = "科技" });
        var newsGroup = await Client.AddGroupAsync(new RssFeedGroup { Name = "新闻" });

        await Client.AddFeedAsync(new RssFeed
        {
            Name = "IT之家",
            Url = "https://www.ithome.com/rss",
            GroupIds = techGroup!.Id,
        });
        await Client.AddFeedAsync(new RssFeed
        {
            Name = "极客公园",
            Url = "https://www.geekpark.net/rss",
            GroupIds = techGroup.Id,
        });
        await Client.AddFeedAsync(new RssFeed
        {
            Name = ".NET Blog",
            Url = "https://devblogs.microsoft.com/dotnet/feed/",
            GroupIds = $"{techGroup.Id},{newsGroup!.Id}",
        });

        var (groups, feeds) = await Client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(3, feeds.Count);
    }
}
