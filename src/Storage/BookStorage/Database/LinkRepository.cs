// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 书架-书籍关联数据仓库.
/// </summary>
internal sealed class LinkRepository
{
    private readonly BookDatabase _database;
    private readonly ILogger? _logger;
    private readonly ShelfBookLinkEntityRepository<BookDatabase> _repository = new();

    public LinkRepository(BookDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ShelfBookLink>> GetByShelfAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ShelfBookLinks WHERE ShelfId = @shelfId ORDER BY SortIndex";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@shelfId", shelfId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ShelfBookLink>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ShelfBookLinkEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        return results;
    }

    public async Task<IReadOnlyList<ShelfBookLink>> GetByGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ShelfBookLinks WHERE GroupId = @groupId ORDER BY SortIndex";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@groupId", groupId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ShelfBookLink>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ShelfBookLinkEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        return results;
    }

    public async Task<IReadOnlyList<ShelfBookLink>> GetByBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ShelfBookLinks WHERE BookId = @bookId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@bookId", bookId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ShelfBookLink>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ShelfBookLinkEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        return results;
    }

    public async Task<ShelfBookLink?> GetLinkAsync(string bookId, string shelfId, CancellationToken cancellationToken = default)
    {
        var linkId = $"{bookId}_{shelfId}";
        var entity = await _repository.GetByIdAsync(_database, linkId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task UpsertAsync(ShelfBookLink link, CancellationToken cancellationToken = default)
    {
        var entity = ShelfBookLinkEntity.FromModel(link);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted link: {LinkId}", link.Id);
    }

    public async Task<bool> DeleteAsync(string bookId, string shelfId, CancellationToken cancellationToken = default)
    {
        var linkId = $"{bookId}_{shelfId}";
        var affected = await _repository.DeleteAsync(_database, linkId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted link: {LinkId}, affected: {Affected}", linkId, affected);
        return affected;
    }

    public async Task DeleteByBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM ShelfBookLinks WHERE BookId = @bookId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@bookId", bookId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted all links for book: {BookId}", bookId);
    }

    public async Task DeleteByShelfAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM ShelfBookLinks WHERE ShelfId = @shelfId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@shelfId", shelfId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted all links for shelf: {ShelfId}", shelfId);
    }

    public async Task ClearGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE ShelfBookLinks SET GroupId = NULL WHERE GroupId = @groupId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@groupId", groupId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Cleared group from all links: {GroupId}", groupId);
    }
}
