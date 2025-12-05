// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Test;

/// <summary>
/// 订阅源 CRUD 操作测试.
/// </summary>
[TestClass]
public class FeedCrudTests
{
    private string _testDbPath = null!;
    private RssStorage _storage = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"rss_test_{Guid.NewGuid()}.db");
        var options = new RssStorageOptions { DatabasePath = _testDbPath };
        _storage = new RssStorage(options);
        await _storage.InitializeAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _storage.DisposeAsync();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [TestMethod]
    public async Task UpsertFeed_NewFeed_ShouldInsert()
    {
        // Arrange
        var feed = new RssFeed
        {
            Id = "feed1",
            Name = "Test Feed",
            Url = "https://example.com/feed.xml",
        };

        // Act
        await _storage.UpsertFeedAsync(feed);
        var retrieved = await _storage.GetFeedAsync("feed1");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("feed1", retrieved.Id);
        Assert.AreEqual("Test Feed", retrieved.Name);
        Assert.AreEqual("https://example.com/feed.xml", retrieved.Url);
    }

    [TestMethod]
    public async Task UpsertFeed_ExistingFeed_ShouldUpdate()
    {
        // Arrange
        var feed = new RssFeed
        {
            Id = "feed1",
            Name = "Test Feed",
            Url = "https://example.com/feed.xml",
        };
        await _storage.UpsertFeedAsync(feed);

        // Act
        feed.Name = "Updated Feed";
        await _storage.UpsertFeedAsync(feed);
        var retrieved = await _storage.GetFeedAsync("feed1");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Updated Feed", retrieved.Name);
    }

    [TestMethod]
    public async Task UpsertFeeds_BatchInsert_ShouldInsertAll()
    {
        // Arrange
        var feeds = new[]
        {
            new RssFeed { Id = "feed1", Name = "Feed 1", Url = "https://example.com/1" },
            new RssFeed { Id = "feed2", Name = "Feed 2", Url = "https://example.com/2" },
            new RssFeed { Id = "feed3", Name = "Feed 3", Url = "https://example.com/3" },
        };

        // Act
        await _storage.UpsertFeedsAsync(feeds);
        var allFeeds = await _storage.GetAllFeedsAsync();

        // Assert
        Assert.AreEqual(3, allFeeds.Count);
    }

    [TestMethod]
    public async Task GetFeeds_ShouldReturnAllFeeds()
    {
        // Arrange
        await _storage.UpsertFeedAsync(new RssFeed { Id = "feed1", Name = "Feed 1", Url = "https://a.com" });
        await _storage.UpsertFeedAsync(new RssFeed { Id = "feed2", Name = "Feed 2", Url = "https://b.com" });

        // Act
        var feeds = await _storage.GetAllFeedsAsync();

        // Assert
        Assert.AreEqual(2, feeds.Count);
    }

    [TestMethod]
    public async Task GetFeedById_NonExistent_ShouldReturnNull()
    {
        // Act
        var feed = await _storage.GetFeedAsync("nonexistent");

        // Assert
        Assert.IsNull(feed);
    }

    [TestMethod]
    public async Task DeleteFeed_ExistingFeed_ShouldReturnTrue()
    {
        // Arrange
        await _storage.UpsertFeedAsync(new RssFeed { Id = "feed1", Name = "Feed", Url = "https://a.com" });

        // Act
        var result = await _storage.DeleteFeedAsync("feed1");
        var feed = await _storage.GetFeedAsync("feed1");

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(feed);
    }

    [TestMethod]
    public async Task DeleteFeed_NonExistent_ShouldReturnFalse()
    {
        // Act
        var result = await _storage.DeleteFeedAsync("nonexistent");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Feed_AllProperties_ShouldPersist()
    {
        // Arrange
        var feed = new RssFeed
        {
            Id = "feed1",
            Name = "Test Feed",
            Url = "https://example.com/feed.xml",
            Website = "https://example.com",
            Description = "A test feed",
            IconUrl = "https://example.com/icon.png",
            GroupIds = "group1,group2",
            Comment = "My favorite feed",
            IsFullContentRequired = true,
        };

        // Act
        await _storage.UpsertFeedAsync(feed);
        var retrieved = await _storage.GetFeedAsync("feed1");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Test Feed", retrieved.Name);
        Assert.AreEqual("https://example.com", retrieved.Website);
        Assert.AreEqual("A test feed", retrieved.Description);
        Assert.AreEqual("https://example.com/icon.png", retrieved.IconUrl);
        Assert.AreEqual("group1,group2", retrieved.GroupIds);
        Assert.AreEqual("My favorite feed", retrieved.Comment);
        Assert.IsTrue(retrieved.IsFullContentRequired);
    }
}
