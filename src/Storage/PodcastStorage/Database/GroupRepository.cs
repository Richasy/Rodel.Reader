// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Database;

/// <summary>
/// 播客分组数据仓库.
/// </summary>
internal sealed class GroupRepository
{
    private readonly PodcastDatabase _database;
    private readonly ILogger? _logger;
    private readonly PodcastGroupEntityRepository<PodcastDatabase> _repository = new();

    public GroupRepository(PodcastDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PodcastGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(_database, cancellationToken).ConfigureAwait(false);
        var groups = entities.Select(e => e.ToModel()).OrderBy(g => g.SortIndex).ToList();
        _logger?.LogDebug("Retrieved {Count} podcast groups.", groups.Count);
        return groups;
    }

    public async Task<PodcastGroup?> GetByIdAsync(string groupId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, groupId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task<bool> IsNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default)
    {
        var sql = excludeId is null
            ? "SELECT COUNT(*) FROM PodcastGroups WHERE Name = @name"
            : "SELECT COUNT(*) FROM PodcastGroups WHERE Name = @name AND Id != @excludeId";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@name", name);
        if (excludeId is not null)
        {
            cmd.Parameters.AddWithValue("@excludeId", excludeId);
        }

        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        return count > 0;
    }

    public async Task UpsertAsync(PodcastGroup group, CancellationToken cancellationToken = default)
    {
        var entity = PodcastGroupEntity.FromModel(group);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted podcast group: {GroupId}", group.Id);
    }

    public async Task UpsertManyAsync(IEnumerable<PodcastGroup> groups, CancellationToken cancellationToken = default)
    {
        var entities = groups.Select(PodcastGroupEntity.FromModel);
        await _repository.UpsertManyAsync(_database, entities, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted multiple podcast groups in batch.");
    }

    public async Task<bool> DeleteAsync(string groupId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, groupId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted podcast group: {GroupId}, affected: {Affected}", groupId, affected);
        return affected;
    }
}
