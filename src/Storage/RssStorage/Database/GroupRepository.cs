// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 分组数据仓库.
/// </summary>
internal sealed class GroupRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;
    private readonly RssFeedGroupEntityRepository _repository = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupRepository"/> class.
    /// </summary>
    public GroupRepository(RssDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有分组.
    /// </summary>
    public async Task<IReadOnlyList<RssFeedGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(_database, cancellationToken).ConfigureAwait(false);
        var groups = entities.Select(e => e.ToModel()).ToList();
        _logger?.LogDebug("Retrieved {Count} groups.", groups.Count);
        return groups;
    }

    /// <summary>
    /// 根据 ID 获取分组.
    /// </summary>
    public async Task<RssFeedGroup?> GetByIdAsync(string groupId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, groupId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    /// <summary>
    /// 添加或更新分组.
    /// </summary>
    public async Task UpsertAsync(RssFeedGroup group, CancellationToken cancellationToken = default)
    {
        var entity = RssFeedGroupEntity.FromModel(group);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted group: {GroupId}", group.Id);
    }

    /// <summary>
    /// 批量添加或更新分组.
    /// </summary>
    public async Task UpsertManyAsync(IEnumerable<RssFeedGroup> groups, CancellationToken cancellationToken = default)
    {
        var entities = groups.Select(RssFeedGroupEntity.FromModel);
        await _repository.UpsertManyAsync(_database, entities, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted multiple groups in batch.");
    }

    /// <summary>
    /// 删除分组.
    /// </summary>
    public async Task<bool> DeleteAsync(string groupId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, groupId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted group: {GroupId}, affected: {Affected}", groupId, affected);
        return affected;
    }
}
