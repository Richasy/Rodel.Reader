// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 阅读时段数据仓库.
/// </summary>
internal sealed class SessionRepository
{
    private readonly BookDatabase _database;
    private readonly ILogger? _logger;
    private readonly ReadingSessionEntityRepository<BookDatabase> _repository = new();

    public SessionRepository(BookDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReadingSession>> GetByBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM ReadingSessions WHERE BookId = @bookId ORDER BY StartedAt DESC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@bookId", bookId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ReadingSession>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ReadingSessionEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} sessions for book {BookId}.", results.Count, bookId);
        return results;
    }

    public async Task<IReadOnlyList<ReadingSession>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days).ToString("O");
        const string sql = "SELECT * FROM ReadingSessions WHERE StartedAt >= @cutoff ORDER BY StartedAt DESC";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@cutoff", cutoff);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ReadingSession>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ReadingSessionEntityRepository<BookDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} recent sessions within {Days} days.", results.Count, days);
        return results;
    }

    public async Task<BookReadingStats> GetStatsAsync(string bookId, CancellationToken cancellationToken = default)
    {
        var sessions = await GetByBookAsync(bookId, cancellationToken).ConfigureAwait(false);

        if (sessions.Count == 0)
        {
            return new BookReadingStats { BookId = bookId };
        }

        var totalSeconds = sessions.Sum(s => s.DurationSeconds);
        var totalPages = sessions.Where(s => s.PagesRead.HasValue).Sum(s => s.PagesRead!.Value);
        var uniqueDays = sessions
            .Select(s => DateTimeOffset.TryParse(s.StartedAt, out var dt) ? dt.Date : (DateTime?)null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .Distinct()
            .ToList();

        var firstSession = sessions[^1]; // Last item in DESC order = first reading session
        var lastSession = sessions[0];   // First item in DESC order = most recent session

        DateOnly? firstDate = null;
        DateOnly? lastDate = null;

        if (DateTimeOffset.TryParse(firstSession.StartedAt, out var first))
        {
            firstDate = DateOnly.FromDateTime(first.Date);
        }

        if (DateTimeOffset.TryParse(lastSession.StartedAt, out var last))
        {
            lastDate = DateOnly.FromDateTime(last.Date);
        }

        var totalHours = totalSeconds / 3600.0;
        double? pagesPerHour = totalHours > 0 && totalPages > 0 ? totalPages / totalHours : null;

        return new BookReadingStats
        {
            BookId = bookId,
            TotalReadingTime = TimeSpan.FromSeconds(totalSeconds),
            TotalSessionCount = sessions.Count,
            AverageSessionDuration = TimeSpan.FromSeconds(totalSeconds / sessions.Count),
            ReadingDays = uniqueDays.Count,
            FirstReadDate = firstDate,
            LastReadDate = lastDate,
            PagesPerHour = pagesPerHour,
        };
    }

    public async Task AddAsync(ReadingSession session, CancellationToken cancellationToken = default)
    {
        var entity = ReadingSessionEntity.FromModel(session);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Added reading session: {SessionId}", session.Id);
    }

    public async Task DeleteByBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM ReadingSessions WHERE BookId = @bookId";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@bookId", bookId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted all sessions for book: {BookId}", bookId);
    }
}
