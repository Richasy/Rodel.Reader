// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 书架数据仓库.
/// </summary>
internal sealed class ShelfRepository
{
    private readonly BookDatabase _database;
    private readonly ILogger? _logger;
    private readonly ShelfEntityRepository<BookDatabase> _repository = new();

    public ShelfRepository(BookDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Shelf>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(_database, cancellationToken).ConfigureAwait(false);
        var shelves = entities.Select(e => e.ToModel()).OrderBy(s => s.SortIndex).ToList();
        _logger?.LogDebug("Retrieved {Count} shelves.", shelves.Count);
        return shelves;
    }

    public async Task<Shelf?> GetByIdAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, shelfId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task<bool> IsNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default)
    {
        var sql = excludeId is null
            ? "SELECT COUNT(*) FROM Shelves WHERE Name = @name"
            : "SELECT COUNT(*) FROM Shelves WHERE Name = @name AND Id != @excludeId";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@name", name);
        if (excludeId is not null)
        {
            cmd.Parameters.AddWithValue("@excludeId", excludeId);
        }

        var count = (long)(await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) ?? 0L);
        return count > 0;
    }

    public async Task UpsertAsync(Shelf shelf, CancellationToken cancellationToken = default)
    {
        var entity = ShelfEntity.FromModel(shelf);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted shelf: {ShelfId}", shelf.Id);
    }

    public async Task<bool> DeleteAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, shelfId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted shelf: {ShelfId}, affected: {Affected}", shelfId, affected);
        return affected;
    }
}
