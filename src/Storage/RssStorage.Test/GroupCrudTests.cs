// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Test;

/// <summary>
/// 分组 CRUD 操作测试.
/// </summary>
[TestClass]
public class GroupCrudTests
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
    public async Task UpsertGroup_NewGroup_ShouldInsert()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "group1", Name = "Technology" };

        // Act
        await _storage.UpsertGroupAsync(group);
        var retrieved = await _storage.GetGroupAsync("group1");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("group1", retrieved.Id);
        Assert.AreEqual("Technology", retrieved.Name);
    }

    [TestMethod]
    public async Task UpsertGroup_ExistingGroup_ShouldUpdate()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "group1", Name = "Technology" };
        await _storage.UpsertGroupAsync(group);

        // Act
        group.Name = "Tech";
        await _storage.UpsertGroupAsync(group);
        var retrieved = await _storage.GetGroupAsync("group1");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Tech", retrieved.Name);
    }

    [TestMethod]
    public async Task UpsertGroups_BatchInsert_ShouldInsertAll()
    {
        // Arrange
        var groups = new[]
        {
            new RssFeedGroup { Id = "group1", Name = "Tech" },
            new RssFeedGroup { Id = "group2", Name = "News" },
            new RssFeedGroup { Id = "group3", Name = "Sports" },
        };

        // Act
        await _storage.UpsertGroupsAsync(groups);
        var allGroups = await _storage.GetAllGroupsAsync();

        // Assert
        Assert.AreEqual(3, allGroups.Count);
    }

    [TestMethod]
    public async Task GetGroups_ShouldReturnAllGroups()
    {
        // Arrange
        await _storage.UpsertGroupAsync(new RssFeedGroup { Id = "group1", Name = "Tech" });
        await _storage.UpsertGroupAsync(new RssFeedGroup { Id = "group2", Name = "News" });

        // Act
        var groups = await _storage.GetAllGroupsAsync();

        // Assert
        Assert.AreEqual(2, groups.Count);
    }

    [TestMethod]
    public async Task GetGroupById_NonExistent_ShouldReturnNull()
    {
        // Act
        var group = await _storage.GetGroupAsync("nonexistent");

        // Assert
        Assert.IsNull(group);
    }

    [TestMethod]
    public async Task DeleteGroup_ExistingGroup_ShouldReturnTrue()
    {
        // Arrange
        await _storage.UpsertGroupAsync(new RssFeedGroup { Id = "group1", Name = "Tech" });

        // Act
        var result = await _storage.DeleteGroupAsync("group1");
        var group = await _storage.GetGroupAsync("group1");

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(group);
    }

    [TestMethod]
    public async Task DeleteGroup_NonExistent_ShouldReturnFalse()
    {
        // Act
        var result = await _storage.DeleteGroupAsync("nonexistent");

        // Assert
        Assert.IsFalse(result);
    }
}
