// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Database;

/// <summary>
/// 收听进度数据仓库.
/// </summary>
internal sealed class ProgressRepository
{
    private readonly PodcastDatabase _database;
    private readonly ILogger? _logger;
    private readonly ListeningProgressEntityRepository<PodcastDatabase> _repository = new();

    public ProgressRepository(PodcastDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<ListeningProgress?> GetByEpisodeAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, episodeId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task<IReadOnlyList<(Episode Episode, ListeningProgress Progress)>> GetInProgressAsync(
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT e.*, p.Position as p_Position, p.Duration as p_Duration, p.Progress as p_Progress,
                   p.PlaybackRate as p_PlaybackRate, p.UpdatedAt as p_UpdatedAt
            FROM ListeningProgress p
            INNER JOIN Episodes e ON p.EpisodeId = e.Id
            WHERE p.Progress > 0 AND p.Progress < 0.95
            ORDER BY p.UpdatedAt DESC
            LIMIT @limit
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@limit", limit);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<(Episode, ListeningProgress)>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var episode = EpisodeEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel();
            var durationOrdinal = reader.GetOrdinal("p_Duration");
            var playbackRateOrdinal = reader.GetOrdinal("p_PlaybackRate");
            var progress = new ListeningProgress
            {
                EpisodeId = episode.Id,
                Position = reader.GetInt32(reader.GetOrdinal("p_Position")),
                Duration = await reader.IsDBNullAsync(durationOrdinal, cancellationToken).ConfigureAwait(false) ? null : reader.GetInt32(durationOrdinal),
                Progress = reader.GetDouble(reader.GetOrdinal("p_Progress")),
                PlaybackRate = await reader.IsDBNullAsync(playbackRateOrdinal, cancellationToken).ConfigureAwait(false) ? null : reader.GetDouble(playbackRateOrdinal),
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(reader.GetOrdinal("p_UpdatedAt"))),
            };
            results.Add((episode, progress));
        }

        _logger?.LogDebug("Retrieved {Count} in-progress episodes.", results.Count);
        return results;
    }

    public async Task UpsertAsync(ListeningProgress progress, CancellationToken cancellationToken = default)
    {
        var entity = ListeningProgressEntity.FromModel(progress);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted listening progress for episode: {EpisodeId}", progress.EpisodeId);
    }

    public async Task<bool> DeleteAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, episodeId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted listening progress for episode: {EpisodeId}, affected: {Affected}", episodeId, affected);
        return affected;
    }
}
