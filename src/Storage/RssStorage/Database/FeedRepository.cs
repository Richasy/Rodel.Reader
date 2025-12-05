// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 订阅源数据仓库.
/// </summary>
internal sealed class FeedRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedRepository"/> class.
    /// </summary>
    public FeedRepository(RssDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有订阅源.
    /// </summary>
    public async Task<IReadOnlyList<RssFeed>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Url, Website, Description, IconUrl, GroupIds, Comment, IsFullContentRequired
            FROM Feeds
            ORDER BY Name
            """;

        await using var cmd = _database.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var feeds = new List<RssFeed>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            feeds.Add(MapToFeed(reader));
        }

        _logger?.LogDebug("Retrieved {Count} feeds.", feeds.Count);
        return feeds;
    }

    /// <summary>
    /// 根据 ID 获取订阅源.
    /// </summary>
    public async Task<RssFeed?> GetByIdAsync(string feedId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Url, Website, Description, IconUrl, GroupIds, Comment, IsFullContentRequired
            FROM Feeds
            WHERE Id = @id
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", feedId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return MapToFeed(reader);
        }

        return null;
    }

    /// <summary>
    /// 添加或更新订阅源.
    /// </summary>
    public async Task UpsertAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Feeds (Id, Name, Url, Website, Description, IconUrl, GroupIds, Comment, IsFullContentRequired)
            VALUES (@id, @name, @url, @website, @description, @iconUrl, @groupIds, @comment, @isFullContentRequired)
            ON CONFLICT(Id) DO UPDATE SET
                Name = excluded.Name,
                Url = excluded.Url,
                Website = excluded.Website,
                Description = excluded.Description,
                IconUrl = excluded.IconUrl,
                GroupIds = excluded.GroupIds,
                Comment = excluded.Comment,
                IsFullContentRequired = excluded.IsFullContentRequired
            """;

        await using var cmd = _database.CreateCommand(sql);
        AddFeedParameters(cmd, feed);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted feed: {FeedId}", feed.Id);
    }

    /// <summary>
    /// 批量添加或更新订阅源.
    /// </summary>
    public async Task UpsertManyAsync(IEnumerable<RssFeed> feeds, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var feed in feeds)
            {
                const string sql = """
                    INSERT INTO Feeds (Id, Name, Url, Website, Description, IconUrl, GroupIds, Comment, IsFullContentRequired)
                    VALUES (@id, @name, @url, @website, @description, @iconUrl, @groupIds, @comment, @isFullContentRequired)
                    ON CONFLICT(Id) DO UPDATE SET
                        Name = excluded.Name,
                        Url = excluded.Url,
                        Website = excluded.Website,
                        Description = excluded.Description,
                        IconUrl = excluded.IconUrl,
                        GroupIds = excluded.GroupIds,
                        Comment = excluded.Comment,
                        IsFullContentRequired = excluded.IsFullContentRequired
                    """;

                await using var cmd = _database.CreateCommand(sql);
                cmd.Transaction = transaction;
                AddFeedParameters(cmd, feed);

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Upserted multiple feeds in batch.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 删除订阅源.
    /// </summary>
    public async Task<bool> DeleteAsync(string feedId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Feeds WHERE Id = @id";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", feedId);

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted feed: {FeedId}, affected: {Affected}", feedId, affected);

        return affected > 0;
    }

    private static RssFeed MapToFeed(SqliteDataReader reader)
    {
        return new RssFeed
        {
            Id = reader.GetString(0),
            Name = reader.GetString(1),
            Url = reader.GetString(2),
            Website = reader.IsDBNull(3) ? null : reader.GetString(3),
            Description = reader.IsDBNull(4) ? null : reader.GetString(4),
            IconUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
            GroupIds = reader.IsDBNull(6) ? null : reader.GetString(6),
            Comment = reader.IsDBNull(7) ? null : reader.GetString(7),
            IsFullContentRequired = reader.GetInt32(8) == 1,
        };
    }

    private static void AddFeedParameters(SqliteCommand cmd, RssFeed feed)
    {
        cmd.Parameters.AddWithValue("@id", feed.Id);
        cmd.Parameters.AddWithValue("@name", feed.Name);
        cmd.Parameters.AddWithValue("@url", feed.Url);
        cmd.Parameters.AddWithValue("@website", (object?)feed.Website ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@description", (object?)feed.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@iconUrl", (object?)feed.IconUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@groupIds", (object?)feed.GroupIds ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@comment", (object?)feed.Comment ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@isFullContentRequired", feed.IsFullContentRequired ? 1 : 0);
    }
}
