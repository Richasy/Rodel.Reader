// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 批注数据仓库.
/// </summary>
internal sealed class AnnotationRepository
{
    private readonly BookDatabase _database;
    private readonly ILogger? _logger;
    private readonly AnnotationEntityRepository<BookDatabase> _repository = new();

    public AnnotationRepository(BookDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Annotation>> GetByBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Annotations WHERE BookId = @bookId ORDER BY CreatedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@bookId", bookId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<Annotation>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(AnnotationEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} annotations for book {BookId}.", results.Count, bookId);
        return results;
    }

    public async Task<Annotation?> GetByIdAsync(string annotationId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, annotationId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    public async Task UpsertAsync(Annotation annotation, CancellationToken cancellationToken = default)
    {
        var entity = AnnotationEntity.FromModel(annotation);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted annotation: {AnnotationId}", annotation.Id);
    }

    public async Task<bool> DeleteAsync(string annotationId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, annotationId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted annotation: {AnnotationId}, affected: {Affected}", annotationId, affected);
        return affected;
    }

    public async Task DeleteByBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Annotations WHERE BookId = @bookId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@bookId", bookId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted all annotations for book: {BookId}", bookId);
    }
}
