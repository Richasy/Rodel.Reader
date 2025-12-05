// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 阅读状态数据仓库.
/// </summary>
internal sealed class ReadStatusRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadStatusRepository"/> class.
    /// </summary>
    public ReadStatusRepository(RssDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// 标记文章为已读.
    /// </summary>
    public async Task MarkAsReadAsync(IEnumerable<string> articleIds, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var articleId in articleIds)
            {
                const string sql = """
                    INSERT INTO ReadStatus (ArticleId, ReadAt)
                    VALUES (@articleId, @readAt)
                    ON CONFLICT(ArticleId) DO NOTHING
                    """;

                await using var cmd = _database.CreateCommand(sql);
                cmd.Transaction = transaction;
                cmd.Parameters.AddWithValue("@articleId", articleId);
                cmd.Parameters.AddWithValue("@readAt", DateTimeOffset.UtcNow.ToString("O"));

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Marked articles as read.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 标记文章为未读.
    /// </summary>
    public async Task MarkAsUnreadAsync(IEnumerable<string> articleIds, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var articleId in articleIds)
            {
                const string sql = "DELETE FROM ReadStatus WHERE ArticleId = @articleId";

                await using var cmd = _database.CreateCommand(sql);
                cmd.Transaction = transaction;
                cmd.Parameters.AddWithValue("@articleId", articleId);

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Marked articles as unread.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 将订阅源下所有文章标记为已读.
    /// </summary>
    public async Task MarkFeedAsReadAsync(string feedId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT OR IGNORE INTO ReadStatus (ArticleId, ReadAt)
            SELECT Id, @readAt FROM Articles WHERE FeedId = @feedId
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@feedId", feedId);
        cmd.Parameters.AddWithValue("@readAt", DateTimeOffset.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Marked all articles in feed {FeedId} as read.", feedId);
    }

    /// <summary>
    /// 将所有文章标记为已读.
    /// </summary>
    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT OR IGNORE INTO ReadStatus (ArticleId, ReadAt)
            SELECT Id, @readAt FROM Articles
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@readAt", DateTimeOffset.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogInformation("Marked all articles as read.");
    }

    /// <summary>
    /// 检查文章是否已读.
    /// </summary>
    public async Task<bool> IsReadAsync(string articleId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT 1 FROM ReadStatus WHERE ArticleId = @articleId";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@articleId", articleId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is not null;
    }
}
