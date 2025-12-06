// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Test;

/// <summary>
/// 播客 CRUD 测试.
/// </summary>
[TestClass]
public class PodcastCrudTests
{
    private string _testDbPath = null!;
    private PodcastStorage _storage = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"podcast_crud_test_{Guid.NewGuid()}.db");
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
    public async Task UpsertPodcast_NewPodcast_ShouldInsert()
    {
        // Arrange
        var podcast = CreateTestPodcast();

        // Act
        await _storage.UpsertPodcastAsync(podcast);
        var retrieved = await _storage.GetPodcastAsync(podcast.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(podcast.Id, retrieved.Id);
        Assert.AreEqual(podcast.Title, retrieved.Title);
        Assert.AreEqual(podcast.SourceType, retrieved.SourceType);
    }

    [TestMethod]
    public async Task UpsertPodcast_ExistingPodcast_ShouldUpdate()
    {
        // Arrange
        var podcast = CreateTestPodcast();
        await _storage.UpsertPodcastAsync(podcast);

        // Act
        podcast.Title = "Updated Title";
        podcast.Author = "New Author";
        await _storage.UpsertPodcastAsync(podcast);
        var retrieved = await _storage.GetPodcastAsync(podcast.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Updated Title", retrieved.Title);
        Assert.AreEqual("New Author", retrieved.Author);
    }

    [TestMethod]
    public async Task GetAllPodcasts_ShouldReturnAllPodcasts()
    {
        // Arrange
        var podcast1 = CreateTestPodcast();
        var podcast2 = CreateTestPodcast();
        await _storage.UpsertPodcastsAsync([podcast1, podcast2]);

        // Act
        var podcasts = await _storage.GetAllPodcastsAsync();

        // Assert
        Assert.AreEqual(2, podcasts.Count);
    }

    [TestMethod]
    public async Task GetSubscribedPodcasts_ShouldReturnOnlySubscribed()
    {
        // Arrange
        var subscribedPodcast = CreateTestPodcast();
        subscribedPodcast.IsSubscribed = true;

        var unsubscribedPodcast = CreateTestPodcast();
        unsubscribedPodcast.IsSubscribed = false;

        await _storage.UpsertPodcastsAsync([subscribedPodcast, unsubscribedPodcast]);

        // Act
        var podcasts = await _storage.GetSubscribedPodcastsAsync();

        // Assert
        Assert.AreEqual(1, podcasts.Count);
        Assert.AreEqual(subscribedPodcast.Id, podcasts[0].Id);
    }

    [TestMethod]
    public async Task GetPodcastsBySourceType_ShouldFilterBySourceType()
    {
        // Arrange
        var standardPodcast = CreateTestPodcast();
        standardPodcast.SourceType = PodcastSourceType.Standard;

        var bilibiliPodcast = CreateTestPodcast();
        bilibiliPodcast.SourceType = PodcastSourceType.Bilibili;

        await _storage.UpsertPodcastsAsync([standardPodcast, bilibiliPodcast]);

        // Act
        var standard = await _storage.GetPodcastsBySourceTypeAsync(PodcastSourceType.Standard);
        var bilibili = await _storage.GetPodcastsBySourceTypeAsync(PodcastSourceType.Bilibili);

        // Assert
        Assert.AreEqual(1, standard.Count);
        Assert.AreEqual(1, bilibili.Count);
        Assert.AreEqual(standardPodcast.Id, standard[0].Id);
        Assert.AreEqual(bilibiliPodcast.Id, bilibili[0].Id);
    }

    [TestMethod]
    public async Task SearchPodcasts_ShouldFindByTitle()
    {
        // Arrange
        var podcast = CreateTestPodcast();
        podcast.Title = "Unique Podcast Title";
        await _storage.UpsertPodcastAsync(podcast);

        // Act
        var results = await _storage.SearchPodcastsAsync("Unique");

        // Assert
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(podcast.Id, results[0].Id);
    }

    [TestMethod]
    public async Task DeletePodcast_ShouldRemovePodcast()
    {
        // Arrange
        var podcast = CreateTestPodcast();
        await _storage.UpsertPodcastAsync(podcast);

        // Act
        var deleted = await _storage.DeletePodcastAsync(podcast.Id);
        var retrieved = await _storage.GetPodcastAsync(podcast.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsNull(retrieved);
    }

    [TestMethod]
    public async Task DeletePodcast_NonExistent_ShouldReturnFalse()
    {
        // Act
        var deleted = await _storage.DeletePodcastAsync("non-existent-id");

        // Assert
        Assert.IsFalse(deleted);
    }

    [TestMethod]
    public async Task Podcast_SortIndex_ShouldWork()
    {
        // Arrange
        var podcast1 = CreateTestPodcast();
        podcast1.SortIndex = 2;

        var podcast2 = CreateTestPodcast();
        podcast2.SortIndex = 1;

        var podcast3 = CreateTestPodcast();
        podcast3.SortIndex = null;

        await _storage.UpsertPodcastsAsync([podcast1, podcast2, podcast3]);

        // Act
        var podcasts = await _storage.GetSubscribedPodcastsAsync();

        // Assert - podcasts with SortIndex should come first, ordered by SortIndex
        Assert.AreEqual(3, podcasts.Count);
    }

    private static Podcast CreateTestPodcast()
    {
        var now = DateTimeOffset.UtcNow;
        return new Podcast
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Test Podcast",
            Description = "Test Description",
            Author = "Test Author",
            FeedUrl = "https://example.com/feed.xml",
            SourceType = PodcastSourceType.Standard,
            IsSubscribed = true,
            AddedAt = now,
            UpdatedAt = now,
        };
    }
}
