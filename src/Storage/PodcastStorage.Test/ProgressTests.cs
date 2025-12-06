// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Test;

/// <summary>
/// 收听进度测试.
/// </summary>
[TestClass]
public class ProgressTests
{
    private string _testDbPath = null!;
    private PodcastStorage _storage = null!;
    private Podcast _testPodcast = null!;
    private Episode _testEpisode = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"podcast_progress_test_{Guid.NewGuid()}.db");
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
    public async Task UpsertProgress_NewProgress_ShouldInsert()
    {
        // Arrange
        var progress = CreateTestProgress(_testEpisode.Id);

        // Act
        await _storage.UpsertProgressAsync(progress);
        var retrieved = await _storage.GetProgressAsync(_testEpisode.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(_testEpisode.Id, retrieved.EpisodeId);
        Assert.AreEqual(progress.Position, retrieved.Position);
        Assert.AreEqual(progress.Progress, retrieved.Progress);
    }

    [TestMethod]
    public async Task UpsertProgress_ExistingProgress_ShouldUpdate()
    {
        // Arrange
        var progress = CreateTestProgress(_testEpisode.Id);
        await _storage.UpsertProgressAsync(progress);

        // Act
        progress.Position = 1800;
        progress.Progress = 0.5;
        await _storage.UpsertProgressAsync(progress);
        var retrieved = await _storage.GetProgressAsync(_testEpisode.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(1800, retrieved.Position);
        Assert.AreEqual(0.5, retrieved.Progress);
    }

    [TestMethod]
    public async Task GetInProgressEpisodes_ShouldReturnEpisodesWithProgress()
    {
        // Arrange
        var episode1 = CreateTestEpisode(_testPodcast.Id);
        var episode2 = CreateTestEpisode(_testPodcast.Id);
        var episode3 = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodesAsync([episode1, episode2, episode3]);

        // Episode 1: in progress (50%)
        await _storage.UpsertProgressAsync(new ListeningProgress
        {
            EpisodeId = episode1.Id,
            Position = 1800,
            Progress = 0.5,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        // Episode 2: completed (100%)
        await _storage.UpsertProgressAsync(new ListeningProgress
        {
            EpisodeId = episode2.Id,
            Position = 3600,
            Progress = 1.0,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        // Episode 3: not started (no progress)

        // Act
        var inProgress = await _storage.GetInProgressEpisodesAsync();

        // Assert
        Assert.AreEqual(1, inProgress.Count);
        Assert.AreEqual(episode1.Id, inProgress[0].Episode.Id);
        Assert.AreEqual(0.5, inProgress[0].Progress.Progress);
    }

    [TestMethod]
    public async Task GetUnlistenedEpisodes_ShouldReturnEpisodesWithoutProgress()
    {
        // Arrange
        var episode1 = CreateTestEpisode(_testPodcast.Id);
        var episode2 = CreateTestEpisode(_testPodcast.Id);
        await _storage.UpsertEpisodesAsync([episode1, episode2]);

        // Episode 1: completed
        await _storage.UpsertProgressAsync(new ListeningProgress
        {
            EpisodeId = episode1.Id,
            Position = 3600,
            Progress = 1.0,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        // Episode 2: not started

        // Act
        var unlistened = await _storage.GetUnlistenedEpisodesAsync(_testPodcast.Id);

        // Assert - should include _testEpisode and episode2
        Assert.IsTrue(unlistened.Any(e => e.Id == episode2.Id));
    }

    [TestMethod]
    public async Task DeleteProgress_ShouldRemoveProgress()
    {
        // Arrange
        var progress = CreateTestProgress(_testEpisode.Id);
        await _storage.UpsertProgressAsync(progress);

        // Act
        var deleted = await _storage.DeleteProgressAsync(_testEpisode.Id);
        var retrieved = await _storage.GetProgressAsync(_testEpisode.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsNull(retrieved);
    }

    [TestMethod]
    public async Task DeleteEpisode_ShouldCascadeDeleteProgress()
    {
        // Arrange
        var progress = CreateTestProgress(_testEpisode.Id);
        await _storage.UpsertProgressAsync(progress);

        // Act
        await _storage.DeleteEpisodeAsync(_testEpisode.Id);
        var retrieved = await _storage.GetProgressAsync(_testEpisode.Id);

        // Assert
        Assert.IsNull(retrieved);
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

    private static ListeningProgress CreateTestProgress(string episodeId)
    {
        return new ListeningProgress
        {
            EpisodeId = episodeId,
            Position = 900,
            Duration = 3600,
            Progress = 0.25,
            PlaybackRate = 1.0,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }
}
