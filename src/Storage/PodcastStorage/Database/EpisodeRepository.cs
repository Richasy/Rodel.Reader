// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Database;

/// <summary>
/// 单集数据仓库.
/// </summary>
internal sealed class EpisodeRepository
{
    private readonly PodcastDatabase _database;
    private readonly ILogger? _logger;
    private readonly EpisodeEntityRepository<PodcastDatabase> _repository = new();

    public EpisodeRepository(PodcastDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Episode>> GetByPodcastAsync(
        string podcastId,
        int limit = 0,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var sql = limit > 0
            ? "SELECT * FROM Episodes WHERE PodcastId = @podcastId ORDER BY COALESCE(SortIndex, 999999), PublishDate DESC LIMIT @limit OFFSET @offset"
            : "SELECT * FROM Episodes WHERE PodcastId = @podcastId ORDER BY COALESCE(SortIndex, 999999), PublishDate DESC";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@podcastId", podcastId);
        if (limit > 0)
        {
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@offset", offset);
        }

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Episode>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(EpisodeEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} episodes for podcast {PodcastId}.", results.Count, podcastId);
        return results;
    }

    public async Task<Episode?> GetByIdAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, episodeId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task<IReadOnlyList<Episode>> GetRecentAsync(int days, int limit = 50, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days).ToUnixTimeSeconds();
        const string sql = """
            SELECT * FROM Episodes
            WHERE PublishDate >= @cutoff
            ORDER BY PublishDate DESC
            LIMIT @limit
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@cutoff", cutoff);
        cmd.Parameters.AddWithValue("@limit", limit);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Episode>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(EpisodeEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} recent episodes within {Days} days.", results.Count, days);
        return results;
    }

    public async Task<IReadOnlyList<Episode>> GetUnlistenedAsync(
        string? podcastId = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var sql = podcastId is null
            ? """
              SELECT e.* FROM Episodes e
              LEFT JOIN ListeningProgress p ON e.Id = p.EpisodeId
              WHERE p.EpisodeId IS NULL OR p.Progress < 0.9
              ORDER BY e.PublishDate DESC
              LIMIT @limit
              """
            : """
              SELECT e.* FROM Episodes e
              LEFT JOIN ListeningProgress p ON e.Id = p.EpisodeId
              WHERE e.PodcastId = @podcastId AND (p.EpisodeId IS NULL OR p.Progress < 0.9)
              ORDER BY e.PublishDate DESC
              LIMIT @limit
              """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@limit", limit);
        if (podcastId is not null)
        {
            cmd.Parameters.AddWithValue("@podcastId", podcastId);
        }

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Episode>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(EpisodeEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} unlistened episodes.", results.Count);
        return results;
    }

    public async Task<int> GetCountByPodcastAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM Episodes WHERE PodcastId = @podcastId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@podcastId", podcastId);
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        return count;
    }

    public async Task UpsertAsync(Episode episode, CancellationToken cancellationToken = default)
    {
        var entity = EpisodeEntity.FromModel(episode);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted episode: {EpisodeId}", episode.Id);
    }

    public async Task UpsertManyAsync(IEnumerable<Episode> episodes, CancellationToken cancellationToken = default)
    {
        var entities = episodes.Select(EpisodeEntity.FromModel);
        await _repository.UpsertManyAsync(_database, entities, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted multiple episodes in batch.");
    }

    public async Task<bool> DeleteAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, episodeId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted episode: {EpisodeId}, affected: {Affected}", episodeId, affected);
        return affected;
    }

    public async Task<int> DeleteByPodcastAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Episodes WHERE PodcastId = @podcastId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@podcastId", podcastId);
        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted {Count} episodes for podcast {PodcastId}.", affected, podcastId);
        return affected;
    }

    public async Task<int> CleanupOldAsync(int keepDays, int? keepCount = null, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-keepDays).ToUnixTimeSeconds();

        string sql;
        if (keepCount.HasValue)
        {
            // 保留每个播客最新的 N 个单集
            sql = """
                DELETE FROM Episodes
                WHERE Id NOT IN (
                    SELECT Id FROM (
                        SELECT Id, ROW_NUMBER() OVER (PARTITION BY PodcastId ORDER BY PublishDate DESC) as rn
                        FROM Episodes
                    ) WHERE rn <= @keepCount
                )
                AND CachedAt < @cutoff
                """;
        }
        else
        {
            sql = "DELETE FROM Episodes WHERE CachedAt < @cutoff";
        }

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@cutoff", cutoff);
        if (keepCount.HasValue)
        {
            cmd.Parameters.AddWithValue("@keepCount", keepCount.Value);
        }

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogInformation("Cleaned up {Count} old episodes (keepDays={KeepDays}, keepCount={KeepCount}).", affected, keepDays, keepCount);
        return affected;
    }
}
