// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 书签数据仓库.
/// </summary>
internal sealed class BookmarkRepository
{
    private readonly BookDatabase _database;
    private readonly ILogger? _logger;
    private readonly BookmarkEntityRepository<BookDatabase> _repository = new();

    public BookmarkRepository(BookDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Bookmark>> GetByBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Bookmarks WHERE BookId = @bookId ORDER BY CreatedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@bookId", bookId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Bookmark>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(BookmarkEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} bookmarks for book {BookId}.", results.Count, bookId);
        return results;
    }

    public async Task<Bookmark?> GetByIdAsync(string bookmarkId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, bookmarkId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task UpsertAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        var entity = BookmarkEntity.FromModel(bookmark);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted bookmark: {BookmarkId}", bookmark.Id);
    }

    public async Task<bool> DeleteAsync(string bookmarkId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, bookmarkId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted bookmark: {BookmarkId}, affected: {Affected}", bookmarkId, affected);
        return affected;
    }

    public async Task DeleteByBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Bookmarks WHERE BookId = @bookId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@bookId", bookId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted all bookmarks for book: {BookId}", bookId);
    }
}
