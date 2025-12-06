// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 书籍分组数据仓库.
/// </summary>
internal sealed class GroupRepository
{
    private readonly BookDatabase _database;
    private readonly ILogger? _logger;
    private readonly BookGroupEntityRepository<BookDatabase> _repository = new();

    public GroupRepository(BookDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BookGroup>> GetByShelfAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM BookGroups WHERE ShelfId = @shelfId ORDER BY SortIndex";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@shelfId", shelfId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<BookGroup>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(BookGroupEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} groups for shelf {ShelfId}.", results.Count, shelfId);
        return results;
    }

    public async Task<BookGroup?> GetByIdAsync(string groupId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, groupId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task UpsertAsync(BookGroup group, CancellationToken cancellationToken = default)
    {
        var entity = BookGroupEntity.FromModel(group);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted group: {GroupId}", group.Id);
    }

    public async Task<bool> DeleteAsync(string groupId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, groupId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted group: {GroupId}, affected: {Affected}", groupId, affected);
        return affected;
    }

    public async Task DeleteByShelfAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM BookGroups WHERE ShelfId = @shelfId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@shelfId", shelfId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted all groups for shelf: {ShelfId}", shelfId);
    }
}
