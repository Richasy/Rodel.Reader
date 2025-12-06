// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 阅读进度数据仓库.
/// </summary>
internal sealed class ProgressRepository
{
    private readonly BookDatabase _database;
    private readonly ILogger? _logger;
    private readonly ReadProgressEntityRepository<BookDatabase> _repository = new();

    public ProgressRepository(BookDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<ReadProgress?> GetByBookIdAsync(string bookId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, bookId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task UpsertAsync(ReadProgress progress, CancellationToken cancellationToken = default)
    {
        var entity = ReadProgressEntity.FromModel(progress);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted progress for book: {BookId}", progress.BookId);
    }

    public async Task<bool> DeleteAsync(string bookId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, bookId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted progress for book: {BookId}, affected: {Affected}", bookId, affected);
        return affected;
    }
}
