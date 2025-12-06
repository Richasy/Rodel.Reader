// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 书籍数据仓库.
/// </summary>
internal sealed class BookRepository
{
    private readonly BookDatabase _database;
    private readonly ILogger? _logger;
    private readonly BookEntityRepository<BookDatabase> _repository = new();

    public BookRepository(BookDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(_database, cancellationToken).ConfigureAwait(false);
        var books = entities.Select(e => e.ToModel()).ToList();
        _logger?.LogDebug("Retrieved {Count} books.", books.Count);
        return books;
    }

    public async Task<Book?> GetByIdAsync(string bookId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, bookId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task<Book?> GetByPathAsync(string localPath, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Books WHERE LocalPath = @path LIMIT 1";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@path", localPath);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return BookEntityRepository<BookDatabase>.MapToEntity(reader).ToModel();
        }

        return null;
    }

    public async Task<Book?> GetByHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Books WHERE FileHash = @hash LIMIT 1";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@hash", fileHash);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return BookEntityRepository<BookDatabase>.MapToEntity(reader).ToModel();
        }

        return null;
    }

    public async Task<IReadOnlyList<Book>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM Books
            WHERE Title LIKE @keyword
               OR Authors LIKE @keyword
               OR Tags LIKE @keyword
               OR Series LIKE @keyword
            ORDER BY LastOpenedAt DESC
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Book>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(BookEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        return results;
    }

    public async Task<IReadOnlyList<Book>> GetByFormatAsync(BookFormat format, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Books WHERE Format = @format ORDER BY AddedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@format", (int)format);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Book>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(BookEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        return results;
    }

    public async Task<IReadOnlyList<Book>> GetByTrackStatusAsync(BookTrackStatus status, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Books WHERE TrackStatus = @status ORDER BY LastOpenedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@status", (int)status);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Book>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(BookEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        return results;
    }

    public async Task<IReadOnlyList<Book>> GetBySourceTypeAsync(BookSourceType sourceType, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Books WHERE SourceType = @sourceType ORDER BY AddedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@sourceType", (int)sourceType);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Book>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(BookEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        return results;
    }

    public async Task UpsertAsync(Book book, CancellationToken cancellationToken = default)
    {
        var entity = BookEntity.FromModel(book);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted book: {BookId}", book.Id);
    }

    public async Task UpsertManyAsync(IEnumerable<Book> books, CancellationToken cancellationToken = default)
    {
        var entities = books.Select(BookEntity.FromModel);
        await _repository.UpsertManyAsync(_database, entities, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted multiple books in batch.");
    }

    public async Task<bool> DeleteAsync(string bookId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, bookId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted book: {BookId}, affected: {Affected}", bookId, affected);
        return affected;
    }
}
