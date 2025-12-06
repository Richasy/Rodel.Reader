// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Database;

/// <summary>
/// 收听时段数据仓库.
/// </summary>
internal sealed class SessionRepository
{
    private readonly PodcastDatabase _database;
    private readonly ILogger? _logger;
    private readonly ListeningSessionEntityRepository<PodcastDatabase> _repository = new();

    public SessionRepository(PodcastDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ListeningSession>> GetByEpisodeAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ListeningSessions WHERE EpisodeId = @episodeId ORDER BY StartedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@episodeId", episodeId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ListeningSession>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ListeningSessionEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} sessions for episode {EpisodeId}.", results.Count, episodeId);
        return results;
    }

    public async Task<IReadOnlyList<ListeningSession>> GetByPodcastAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ListeningSessions WHERE PodcastId = @podcastId ORDER BY StartedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@podcastId", podcastId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ListeningSession>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ListeningSessionEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} sessions for podcast {PodcastId}.", results.Count, podcastId);
        return results;
    }

    public async Task<IReadOnlyList<ListeningSession>> GetRecentAsync(int days, int limit = 100, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days).ToUnixTimeSeconds();
        const string sql = "SELECT * FROM ListeningSessions WHERE StartedAt >= @cutoff ORDER BY StartedAt DESC LIMIT @limit";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@cutoff", cutoff);
        cmd.Parameters.AddWithValue("@limit", limit);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ListeningSession>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ListeningSessionEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} recent sessions within {Days} days.", results.Count, days);
        return results;
    }

    public async Task AddAsync(ListeningSession session, CancellationToken cancellationToken = default)
    {
        var entity = ListeningSessionEntity.FromModel(session);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Added listening session: {SessionId}", session.Id);
    }

    public async Task<ListeningStats> GetPodcastStatsAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        var sessions = await GetByPodcastAsync(podcastId, cancellationToken).ConfigureAwait(false);
        return CalculateStats(sessions, podcastId: podcastId);
    }

    public async Task<ListeningStats> GetEpisodeStatsAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        var sessions = await GetByEpisodeAsync(episodeId, cancellationToken).ConfigureAwait(false);
        return CalculateStats(sessions, episodeId: episodeId);
    }

    public async Task<ListeningStats> GetOverallStatsAsync(int days, CancellationToken cancellationToken = default)
    {
        var sessions = await GetRecentAsync(days, limit: int.MaxValue, cancellationToken).ConfigureAwait(false);
        return CalculateStats(sessions);
    }

    public async Task<int> CleanupOldAsync(int keepDays, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-keepDays).ToUnixTimeSeconds();
        const string sql = "DELETE FROM ListeningSessions WHERE StartedAt < @cutoff";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@cutoff", cutoff);
        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogInformation("Cleaned up {Count} old sessions (keepDays={KeepDays}).", affected, keepDays);
        return affected;
    }

    private static ListeningStats CalculateStats(
        IReadOnlyList<ListeningSession> sessions,
        string? podcastId = null,
        string? episodeId = null)
    {
        if (sessions.Count == 0)
        {
            return new ListeningStats
            {
                PodcastId = podcastId,
                EpisodeId = episodeId,
            };
        }

        var totalSeconds = sessions.Sum(s => s.DurationSeconds);

        var uniqueDays = sessions
            .Select(s => s.StartedAt.Date)
            .Distinct()
            .ToList();

        var uniqueEpisodes = sessions.Select(s => s.EpisodeId).Distinct().Count();
        var uniquePodcasts = sessions.Select(s => s.PodcastId).Distinct().Count();

        var sortedSessions = sessions.OrderBy(s => s.StartedAt).ToList();
        var firstSession = sortedSessions[0];
        var lastSession = sortedSessions[^1];

        var firstDate = DateOnly.FromDateTime(firstSession.StartedAt.Date);
        var lastDate = DateOnly.FromDateTime(lastSession.StartedAt.Date);

        return new ListeningStats
        {
            PodcastId = podcastId,
            EpisodeId = episodeId,
            TotalListeningTime = TimeSpan.FromSeconds(totalSeconds),
            TotalSessionCount = sessions.Count,
            AverageSessionDuration = TimeSpan.FromSeconds(totalSeconds / sessions.Count),
            ListeningDays = uniqueDays.Count,
            FirstListenDate = firstDate,
            LastListenDate = lastDate,
            EpisodeCount = uniqueEpisodes,
            PodcastCount = uniquePodcasts,
        };
    }
}
