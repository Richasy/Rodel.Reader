// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Test;

/// <summary>
/// 收听时段和统计测试.
/// </summary>
[TestClass]
public class SessionAndStatsTests
{
    private string _testDbPath = null!;
    private PodcastStorage _storage = null!;
    private Podcast _testPodcast = null!;
    private Episode _testEpisode = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"podcast_session_test_{Guid.NewGuid()}.db");
        var options = new PodcastStorageOptions { DatabasePath = _testDbPath };
        _storage = new PodcastStorage(options);
        await _storage.InitializeAsync();

        // Create test podcast and episode
        _testPodcast = CreateTestPodcast();
        await _storage.UpsertPodcastAsync(_testPodcast);

        _testEpisode = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodeAsync(_testEpisode);
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
    public async Task AddSession_ShouldInsertSession()
    {
        // Arrange
        var session = CreateTestSession(_testEpisode.Id, _testPodcast.Id);

        // Act
        await _storage.AddSessionAsync(session);
        var sessions = await _storage.GetSessionsByEpisodeAsync(_testEpisode.Id);

        // Assert
        Assert.AreEqual(1, sessions.Count);
        Assert.AreEqual(session.Id, sessions[0].Id);
        Assert.AreEqual(session.DurationSeconds, sessions[0].DurationSeconds);
    }

    [TestMethod]
    public async Task GetSessionsByPodcast_ShouldReturnAllSessionsForPodcast()
    {
        // Arrange
        var episode2 = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodeAsync(episode2);

        var session1 = CreateTestSession(_testEpisode.Id, _testPodcast.Id);
        var session2 = CreateTestSession(episode2.Id, _testPodcast.Id);
        await _storage.AddSessionAsync(session1);
        await _storage.AddSessionAsync(session2);

        // Act
        var sessions = await _storage.GetSessionsByPodcastAsync(_testPodcast.Id);

        // Assert
        Assert.AreEqual(2, sessions.Count);
    }

    [TestMethod]
    public async Task GetRecentSessions_ShouldReturnSessionsWithinDays()
    {
        // Arrange
        var recentSession = CreateTestSession(_testEpisode.Id, _testPodcast.Id);
        recentSession.StartedAt = DateTimeOffset.UtcNow.AddDays(-5);

        await _storage.AddSessionAsync(recentSession);

        // Act
        var sessions = await _storage.GetRecentSessionsAsync(days: 7);

        // Assert
        Assert.AreEqual(1, sessions.Count);
    }

    [TestMethod]
    public async Task GetPodcastStats_ShouldCalculateCorrectStats()
    {
        // Arrange
        var session1 = CreateTestSession(_testEpisode.Id, _testPodcast.Id);
        session1.DurationSeconds = 1800; // 30 minutes
        session1.StartedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var session2 = CreateTestSession(_testEpisode.Id, _testPodcast.Id);
        session2.DurationSeconds = 1200; // 20 minutes
        session2.StartedAt = DateTimeOffset.UtcNow.AddDays(-2);

        await _storage.AddSessionAsync(session1);
        await _storage.AddSessionAsync(session2);

        // Act
        var stats = await _storage.GetPodcastStatsAsync(_testPodcast.Id);

        // Assert
        Assert.AreEqual(_testPodcast.Id, stats.PodcastId);
        Assert.AreEqual(2, stats.TotalSessionCount);
        Assert.AreEqual(TimeSpan.FromSeconds(3000), stats.TotalListeningTime); // 50 minutes
        Assert.AreEqual(1, stats.EpisodeCount);
        Assert.AreEqual(2, stats.ListeningDays);
    }

    [TestMethod]
    public async Task GetEpisodeStats_ShouldCalculateCorrectStats()
    {
        // Arrange
        var session1 = CreateTestSession(_testEpisode.Id, _testPodcast.Id);
        session1.DurationSeconds = 600;

        var session2 = CreateTestSession(_testEpisode.Id, _testPodcast.Id);
        session2.DurationSeconds = 400;

        await _storage.AddSessionAsync(session1);
        await _storage.AddSessionAsync(session2);

        // Act
        var stats = await _storage.GetEpisodeStatsAsync(_testEpisode.Id);

        // Assert
        Assert.AreEqual(_testEpisode.Id, stats.EpisodeId);
        Assert.AreEqual(2, stats.TotalSessionCount);
        Assert.AreEqual(TimeSpan.FromSeconds(1000), stats.TotalListeningTime);
    }

    [TestMethod]
    public async Task GetOverallStats_ShouldCalculateCorrectStats()
    {
        // Arrange
        var podcast2 = CreateTestPodcast();
        await _storage.UpsertPodcastAsync(podcast2);

        var episode2 = CreateTestEpisode(podcast2.Id);
        await _storage.UpsertEpisodeAsync(episode2);

        var session1 = CreateTestSession(_testEpisode.Id, _testPodcast.Id);
        session1.DurationSeconds = 1000;
        session1.StartedAt = DateTimeOffset.UtcNow.AddDays(-5);

        var session2 = CreateTestSession(episode2.Id, podcast2.Id);
        session2.DurationSeconds = 2000;
        session2.StartedAt = DateTimeOffset.UtcNow.AddDays(-3);

        await _storage.AddSessionAsync(session1);
        await _storage.AddSessionAsync(session2);

        // Act
        var stats = await _storage.GetOverallStatsAsync(days: 30);

        // Assert
        Assert.AreEqual(2, stats.TotalSessionCount);
        Assert.AreEqual(2, stats.PodcastCount);
        Assert.AreEqual(2, stats.EpisodeCount);
        Assert.AreEqual(TimeSpan.FromSeconds(3000), stats.TotalListeningTime);
    }

    [TestMethod]
    public async Task GetStats_EmptySessions_ShouldReturnEmptyStats()
    {
        // Act
        var stats = await _storage.GetPodcastStatsAsync(_testPodcast.Id);

        // Assert
        Assert.AreEqual(TimeSpan.Zero, stats.TotalListeningTime);
        Assert.AreEqual(0, stats.TotalSessionCount);
        Assert.IsNull(stats.FirstListenDate);
        Assert.IsNull(stats.LastListenDate);
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
            StartPosition = 0,
            EndPosition = 1800,
        };
    }
}
