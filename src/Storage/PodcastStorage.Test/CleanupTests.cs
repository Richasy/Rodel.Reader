// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Test;

/// <summary>
/// 清理功能测试.
/// </summary>
[TestClass]
public class CleanupTests
{
    private string _testDbPath = null!;
    private PodcastStorage _storage = null!;
    private Podcast _testPodcast = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"podcast_cleanup_test_{Guid.NewGuid()}.db");
        var options = new PodcastStorageOptions { DatabasePath = _testDbPath };
        _storage = new PodcastStorage(options);
        await _storage.InitializeAsync();

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
    public async Task CleanupOldEpisodes_ShouldRemoveOldEpisodes()
    {
        // Arrange
        var recentEpisode = CreateTestEpisode(_testPodcast.Id);
        recentEpisode.CachedAt = DateTimeOffset.UtcNow.AddDays(-10);

        var oldEpisode = CreateTestEpisode(_testPodcast.Id);
        oldEpisode.CachedAt = DateTimeOffset.UtcNow.AddDays(-100);

        await _storage.UpsertEpisodesAsync([recentEpisode, oldEpisode]);

        // Act
        var deleted = await _storage.CleanupOldEpisodesAsync(keepDays: 30);

        // Assert
        Assert.AreEqual(1, deleted);

        var episodes = await _storage.GetEpisodesByPodcastAsync(_testPodcast.Id);
        Assert.AreEqual(1, episodes.Count);
        Assert.AreEqual(recentEpisode.Id, episodes[0].Id);
    }

    [TestMethod]
    public async Task CleanupOldSessions_ShouldRemoveOldSessions()
    {
        // Arrange
        var episode = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodeAsync(episode);

        var recentSession = CreateTestSession(episode.Id, _testPodcast.Id);
        recentSession.StartedAt = DateTimeOffset.UtcNow.AddDays(-100);

        var oldSession = CreateTestSession(episode.Id, _testPodcast.Id);
        oldSession.StartedAt = DateTimeOffset.UtcNow.AddDays(-400);

        await _storage.AddSessionAsync(recentSession);
        await _storage.AddSessionAsync(oldSession);

        // Act
        var deleted = await _storage.CleanupOldSessionsAsync(keepDays: 365);

        // Assert
        Assert.AreEqual(1, deleted);

        var sessions = await _storage.GetSessionsByEpisodeAsync(episode.Id);
        Assert.AreEqual(1, sessions.Count);
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

    private static ListeningSession CreateTestSession(string episodeId, string podcastId)
    {
        var now = DateTimeOffset.UtcNow;
        return new ListeningSession
        {
            Id = Guid.NewGuid().ToString(),
            EpisodeId = episodeId,
            PodcastId = podcastId,
            StartedAt = now.AddMinutes(-30),
            EndedAt = now,
            DurationSeconds = 1800,
        };
    }
}
