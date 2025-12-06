// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Database;

/// <summary>
/// 播客数据仓库.
/// </summary>
internal sealed class PodcastRepository
{
    private readonly PodcastDatabase _database;
    private readonly ILogger? _logger;
    private readonly PodcastEntityRepository<PodcastDatabase> _repository = new();

    public PodcastRepository(PodcastDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Podcast>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(_database, cancellationToken).ConfigureAwait(false);
        var podcasts = entities.Select(e => e.ToModel()).ToList();
        _logger?.LogDebug("Retrieved {Count} podcasts.", podcasts.Count);
        return podcasts;
    }

    public async Task<Podcast?> GetByIdAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, podcastId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task<IReadOnlyList<Podcast>> GetByGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Podcasts WHERE GroupIds LIKE @pattern ORDER BY SortIndex, AddedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@pattern", $"%{groupId}%");
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Podcast>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var entity = PodcastEntityRepository<PodcastDatabase>.MapToEntity(reader);
            var podcast = entity.ToModel();
            // 精确匹配分组 ID
            if (podcast.GetGroupIdList().Contains(groupId))
            {
                results.Add(podcast);
            }
        }

        _logger?.LogDebug("Retrieved {Count} podcasts for group {GroupId}.", results.Count, groupId);
        return results;
    }

    public async Task<IReadOnlyList<Podcast>> GetBySourceTypeAsync(PodcastSourceType sourceType, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Podcasts WHERE SourceType = @sourceType ORDER BY SortIndex, AddedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@sourceType", (int)sourceType);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Podcast>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(PodcastEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} podcasts for source type {SourceType}.", results.Count, sourceType);
        return results;
    }

    public async Task<IReadOnlyList<Podcast>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM Podcasts
            WHERE Title LIKE @keyword
               OR Author LIKE @keyword
               OR Description LIKE @keyword
            ORDER BY SortIndex, AddedAt DESC
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Podcast>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(PodcastEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Search '{Keyword}' returned {Count} podcasts.", keyword, results.Count);
        return results;
    }

    public async Task<IReadOnlyList<Podcast>> GetSubscribedAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Podcasts WHERE IsSubscribed = 1 ORDER BY SortIndex, AddedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Podcast>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(PodcastEntityRepository<PodcastDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} subscribed podcasts.", results.Count);
        return results;
    }

    public async Task UpsertAsync(Podcast podcast, CancellationToken cancellationToken = default)
    {
        var entity = PodcastEntity.FromModel(podcast);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted podcast: {PodcastId}", podcast.Id);
    }

    public async Task UpsertManyAsync(IEnumerable<Podcast> podcasts, CancellationToken cancellationToken = default)
    {
        var entities = podcasts.Select(PodcastEntity.FromModel);
        await _repository.UpsertManyAsync(_database, entities, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted multiple podcasts in batch.");
    }

    public async Task<bool> DeleteAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, podcastId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted podcast: {PodcastId}, affected: {Affected}", podcastId, affected);
        return affected;
    }
}
