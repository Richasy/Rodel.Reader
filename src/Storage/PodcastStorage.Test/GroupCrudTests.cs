// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Test;

/// <summary>
/// 分组 CRUD 测试.
/// </summary>
[TestClass]
public class GroupCrudTests
{
    private string _testDbPath = null!;
    private PodcastStorage _storage = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"podcast_group_test_{Guid.NewGuid()}.db");
        var options = new PodcastStorageOptions { DatabasePath = _testDbPath };
        _storage = new PodcastStorage(options);
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
        var group = CreateTestGroup();

        // Act
        await _storage.UpsertGroupAsync(group);
        var retrieved = await _storage.GetGroupAsync(group.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(group.Id, retrieved.Id);
        Assert.AreEqual(group.Name, retrieved.Name);
        Assert.AreEqual(group.IconEmoji, retrieved.IconEmoji);
    }

    [TestMethod]
    public async Task UpsertGroup_ExistingGroup_ShouldUpdate()
    {
        // Arrange
        var group = CreateTestGroup();
        await _storage.UpsertGroupAsync(group);

        // Act
        group.Name = "Updated Name";
        group.IconEmoji = "🎧";
        await _storage.UpsertGroupAsync(group);
        var retrieved = await _storage.GetGroupAsync(group.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Updated Name", retrieved.Name);
        Assert.AreEqual("🎧", retrieved.IconEmoji);
    }

    [TestMethod]
    public async Task GetAllGroups_ShouldReturnAllGroups()
    {
        // Arrange
        var group1 = CreateTestGroup();
        group1.SortIndex = 1;

        var group2 = CreateTestGroup();
        group2.SortIndex = 0;

        await _storage.UpsertGroupsAsync([group1, group2]);

        // Act
        var groups = await _storage.GetAllGroupsAsync();

        // Assert
        Assert.AreEqual(2, groups.Count);
        // Should be ordered by SortIndex
        Assert.AreEqual(group2.Id, groups[0].Id);
        Assert.AreEqual(group1.Id, groups[1].Id);
    }

    [TestMethod]
    public async Task IsGroupNameExists_ShouldReturnTrue_WhenNameExists()
    {
        // Arrange
        var group = CreateTestGroup();
        group.Name = "Unique Name";
        await _storage.UpsertGroupAsync(group);

        // Act
        var exists = await _storage.IsGroupNameExistsAsync("Unique Name");

        // Assert
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task IsGroupNameExists_ShouldReturnFalse_WhenExcludingSelf()
    {
        // Arrange
        var group = CreateTestGroup();
        group.Name = "Unique Name";
        await _storage.UpsertGroupAsync(group);

        // Act
        var exists = await _storage.IsGroupNameExistsAsync("Unique Name", group.Id);

        // Assert
        Assert.IsFalse(exists);
    }

    [TestMethod]
    public async Task DeleteGroup_ShouldRemoveGroupAndUpdatePodcasts()
    {
        // Arrange
        var group = CreateTestGroup();
        await _storage.UpsertGroupAsync(group);

        var podcast = CreateTestPodcast();
        podcast.AddGroupId(group.Id);
        await _storage.UpsertPodcastAsync(podcast);

        // Act
        var deleted = await _storage.DeleteGroupAsync(group.Id);
        var retrievedGroup = await _storage.GetGroupAsync(group.Id);
        var retrievedPodcast = await _storage.GetPodcastAsync(podcast.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsNull(retrievedGroup);
        Assert.IsNotNull(retrievedPodcast);
        Assert.IsFalse(retrievedPodcast.GetGroupIdList().Contains(group.Id));
    }

    [TestMethod]
    public async Task GetPodcastsByGroup_ShouldReturnPodcastsInGroup()
    {
        // Arrange
        var group = CreateTestGroup();
        await _storage.UpsertGroupAsync(group);

        var podcast1 = CreateTestPodcast();
        podcast1.AddGroupId(group.Id);

        var podcast2 = CreateTestPodcast();
        // Not in group

        await _storage.UpsertPodcastsAsync([podcast1, podcast2]);

        // Act
        var podcasts = await _storage.GetPodcastsByGroupAsync(group.Id);

        // Assert
        Assert.AreEqual(1, podcasts.Count);
        Assert.AreEqual(podcast1.Id, podcasts[0].Id);
    }

    private static PodcastGroup CreateTestGroup()
    {
        return new PodcastGroup
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Group",
            IconEmoji = "📻",
            SortIndex = 0,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    private static Podcast CreateTestPodcast()
    {
        var now = DateTimeOffset.UtcNow;
        return new Podcast
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Test Podcast",
            SourceType = PodcastSourceType.Standard,
            IsSubscribed = true,
            AddedAt = now,
            UpdatedAt = now,
        };
    }
}
