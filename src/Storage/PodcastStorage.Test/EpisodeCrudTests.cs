// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Test;

/// <summary>
/// 单集 CRUD 测试.
/// </summary>
[TestClass]
public class EpisodeCrudTests
{
    private string _testDbPath = null!;
    private PodcastStorage _storage = null!;
    private Podcast _testPodcast = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"podcast_episode_test_{Guid.NewGuid()}.db");
        var options = new PodcastStorageOptions { DatabasePath = _testDbPath };
        _storage = new PodcastStorage(options);
        await _storage.InitializeAsync();

        // Create a test podcast
        _testPodcast = CreateTestPodcast();
        await _storage.UpsertPodcastAsync(_testPodcast);
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
    public async Task UpsertEpisode_NewEpisode_ShouldInsert()
    {
        // Arrange
        var episode = CreateTestEpisode(_testPodcast.Id);

        // Act
        await _storage.UpsertEpisodeAsync(episode);
        var retrieved = await _storage.GetEpisodeAsync(episode.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(episode.Id, retrieved.Id);
        Assert.AreEqual(episode.Title, retrieved.Title);
        Assert.AreEqual(episode.PodcastId, retrieved.PodcastId);
    }

    [TestMethod]
    public async Task UpsertEpisode_ExistingEpisode_ShouldUpdate()
    {
        // Arrange
        var episode = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodeAsync(episode);

        // Act
        episode.Title = "Updated Episode Title";
        episode.Duration = 7200;
        await _storage.UpsertEpisodeAsync(episode);
        var retrieved = await _storage.GetEpisodeAsync(episode.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Updated Episode Title", retrieved.Title);
        Assert.AreEqual(7200, retrieved.Duration);
    }

    [TestMethod]
    public async Task GetEpisodesByPodcast_ShouldReturnEpisodesForPodcast()
    {
        // Arrange
        var episode1 = CreateTestEpisode(_testPodcast.Id);
        var episode2 = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodesAsync([episode1, episode2]);

        // Act
        var episodes = await _storage.GetEpisodesByPodcastAsync(_testPodcast.Id);

        // Assert
        Assert.AreEqual(2, episodes.Count);
    }

    [TestMethod]
    public async Task GetEpisodesByPodcast_WithLimitAndOffset_ShouldPaginate()
    {
        // Arrange
        var episodes = Enumerable.Range(1, 10)
            .Select(i =>
            {
                var ep = CreateTestEpisode(_testPodcast.Id);
                ep.PublishDate = DateTimeOffset.UtcNow.AddDays(-i);
                return ep;
            })
            .ToList();
        await _storage.UpsertEpisodesAsync(episodes);

        // Act
        var firstPage = await _storage.GetEpisodesByPodcastAsync(_testPodcast.Id, limit: 5, offset: 0);
        var secondPage = await _storage.GetEpisodesByPodcastAsync(_testPodcast.Id, limit: 5, offset: 5);

        // Assert
        Assert.AreEqual(5, firstPage.Count);
        Assert.AreEqual(5, secondPage.Count);
    }

    [TestMethod]
    public async Task GetRecentEpisodes_ShouldReturnRecentEpisodes()
    {
        // Arrange
        var recentEpisode = CreateTestEpisode(_testPodcast.Id);
        recentEpisode.PublishDate = DateTimeOffset.UtcNow.AddDays(-1);

        var oldEpisode = CreateTestEpisode(_testPodcast.Id);
        oldEpisode.PublishDate = DateTimeOffset.UtcNow.AddDays(-30);

        await _storage.UpsertEpisodesAsync([recentEpisode, oldEpisode]);

        // Act
        var recent = await _storage.GetRecentEpisodesAsync(days: 7);

        // Assert
        Assert.AreEqual(1, recent.Count);
        Assert.AreEqual(recentEpisode.Id, recent[0].Id);
    }

    [TestMethod]
    public async Task GetEpisodeCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var episode1 = CreateTestEpisode(_testPodcast.Id);
        var episode2 = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodesAsync([episode1, episode2]);

        // Act
        var count = await _storage.GetEpisodeCountAsync(_testPodcast.Id);

        // Assert
        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public async Task DeleteEpisode_ShouldRemoveEpisode()
    {
        // Arrange
        var episode = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodeAsync(episode);

        // Act
        var deleted = await _storage.DeleteEpisodeAsync(episode.Id);
        var retrieved = await _storage.GetEpisodeAsync(episode.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsNull(retrieved);
    }

    [TestMethod]
    public async Task DeleteEpisodesByPodcast_ShouldRemoveAllEpisodes()
    {
        // Arrange
        var episode1 = CreateTestEpisode(_testPodcast.Id);
        var episode2 = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodesAsync([episode1, episode2]);

        // Act
        var deleted = await _storage.DeleteEpisodesByPodcastAsync(_testPodcast.Id);
        var episodes = await _storage.GetEpisodesByPodcastAsync(_testPodcast.Id);

        // Assert
        Assert.AreEqual(2, deleted);
        Assert.AreEqual(0, episodes.Count);
    }

    [TestMethod]
    public async Task DeletePodcast_ShouldCascadeDeleteEpisodes()
    {
        // Arrange
        var episode = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodeAsync(episode);

        // Act
        await _storage.DeletePodcastAsync(_testPodcast.Id);
        var episodes = await _storage.GetEpisodesByPodcastAsync(_testPodcast.Id);

        // Assert
        Assert.AreEqual(0, episodes.Count);
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

    private static Episode CreateTestEpisode(string podcastId)
    {
        return new Episode
        {
            Id = Guid.NewGuid().ToString(),
            PodcastId = podcastId,
            Title = "Test Episode",
            MediaUrl = "https://example.com/episode.mp3",
            Duration = 3600,
            PublishDate = DateTimeOffset.UtcNow,
            CachedAt = DateTimeOffset.UtcNow,
        };
    }
}
