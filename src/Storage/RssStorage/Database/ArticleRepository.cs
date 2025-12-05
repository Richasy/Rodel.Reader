// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 文章数据仓库.
/// </summary>
internal sealed class ArticleRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;

    // 不包含 Content 的字段列表（用于列表查询）
    private const string ArticleFieldsWithoutContent = """
        Id, FeedId, Title, Summary, CoverUrl, Url, Author, PublishTime, Tags, ExtraData, CachedAt
        """;

    // 包含 Content 的字段列表（用于详情查询）
    private const string ArticleFieldsWithContent = """
        Id, FeedId, Title, Summary, Content, CoverUrl, Url, Author, PublishTime, Tags, ExtraData, CachedAt
        """;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleRepository"/> class.
    /// </summary>
    public ArticleRepository(RssDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// 获取订阅源下的文章列表（不含内容）.
    /// </summary>
    public async Task<IReadOnlyList<RssArticle>> GetByFeedAsync(
        string feedId,
        int limit = 0,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
            SELECT {ArticleFieldsWithoutContent}
            FROM Articles
            WHERE FeedId = @feedId
            ORDER BY PublishTime DESC
            """;

        if (limit > 0)
        {
            sql += " LIMIT @limit OFFSET @offset";
        }

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@feedId", feedId);

        if (limit > 0)
        {
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@offset", offset);
        }

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var articles = new List<RssArticle>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            articles.Add(MapToArticleWithoutContent(reader));
        }

        _logger?.LogDebug("Retrieved {Count} articles for feed {FeedId}.", articles.Count, feedId);
        return articles;
    }

    /// <summary>
    /// 获取未读文章列表.
    /// </summary>
    public async Task<IReadOnlyList<RssArticle>> GetUnreadAsync(
        string? feedId = null,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
            SELECT {ArticleFieldsWithoutContent}
            FROM Articles a
            WHERE NOT EXISTS (SELECT 1 FROM ReadStatus r WHERE r.ArticleId = a.Id)
            """;

        if (!string.IsNullOrEmpty(feedId))
        {
            sql += " AND a.FeedId = @feedId";
        }

        sql += " ORDER BY a.PublishTime DESC LIMIT @limit OFFSET @offset";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@limit", limit);
        cmd.Parameters.AddWithValue("@offset", offset);

        if (!string.IsNullOrEmpty(feedId))
        {
            cmd.Parameters.AddWithValue("@feedId", feedId);
        }

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var articles = new List<RssArticle>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            articles.Add(MapToArticleWithoutContent(reader));
        }

        _logger?.LogDebug("Retrieved {Count} unread articles.", articles.Count);
        return articles;
    }

    /// <summary>
    /// 获取收藏文章列表.
    /// </summary>
    public async Task<IReadOnlyList<RssArticle>> GetFavoritesAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
            SELECT {ArticleFieldsWithoutContent}
            FROM Articles a
            WHERE EXISTS (SELECT 1 FROM Favorites f WHERE f.ArticleId = a.Id)
            ORDER BY a.PublishTime DESC
            LIMIT @limit OFFSET @offset
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@limit", limit);
        cmd.Parameters.AddWithValue("@offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var articles = new List<RssArticle>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            articles.Add(MapToArticleWithoutContent(reader));
        }

        _logger?.LogDebug("Retrieved {Count} favorite articles.", articles.Count);
        return articles;
    }

    /// <summary>
    /// 根据 ID 获取文章（含内容）.
    /// </summary>
    public async Task<RssArticle?> GetByIdAsync(string articleId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            SELECT {ArticleFieldsWithContent}
            FROM Articles
            WHERE Id = @id
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", articleId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return MapToArticleWithContent(reader);
        }

        return null;
    }

    /// <summary>
    /// 获取文章内容.
    /// </summary>
    public async Task<string?> GetContentAsync(string articleId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Content FROM Articles WHERE Id = @id";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", articleId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result as string;
    }

    /// <summary>
    /// 添加或更新文章.
    /// </summary>
    public async Task UpsertAsync(RssArticle article, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Articles (Id, FeedId, Title, Summary, Content, CoverUrl, Url, Author, PublishTime, Tags, ExtraData, CachedAt)
            VALUES (@id, @feedId, @title, @summary, @content, @coverUrl, @url, @author, @publishTime, @tags, @extraData, @cachedAt)
            ON CONFLICT(Id) DO UPDATE SET
                FeedId = excluded.FeedId,
                Title = excluded.Title,
                Summary = excluded.Summary,
                Content = excluded.Content,
                CoverUrl = excluded.CoverUrl,
                Url = excluded.Url,
                Author = excluded.Author,
                PublishTime = excluded.PublishTime,
                Tags = excluded.Tags,
                ExtraData = excluded.ExtraData,
                CachedAt = excluded.CachedAt
            """;

        await using var cmd = _database.CreateCommand(sql);
        AddArticleParameters(cmd, article);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted article: {ArticleId}", article.Id);
    }

    /// <summary>
    /// 批量添加或更新文章.
    /// </summary>
    public async Task UpsertManyAsync(IEnumerable<RssArticle> articles, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var article in articles)
            {
                const string sql = """
                    INSERT INTO Articles (Id, FeedId, Title, Summary, Content, CoverUrl, Url, Author, PublishTime, Tags, ExtraData, CachedAt)
                    VALUES (@id, @feedId, @title, @summary, @content, @coverUrl, @url, @author, @publishTime, @tags, @extraData, @cachedAt)
                    ON CONFLICT(Id) DO UPDATE SET
                        FeedId = excluded.FeedId,
                        Title = excluded.Title,
                        Summary = excluded.Summary,
                        Content = excluded.Content,
                        CoverUrl = excluded.CoverUrl,
                        Url = excluded.Url,
                        Author = excluded.Author,
                        PublishTime = excluded.PublishTime,
                        Tags = excluded.Tags,
                        ExtraData = excluded.ExtraData,
                        CachedAt = excluded.CachedAt
                    """;

                await using var cmd = _database.CreateCommand(sql);
                cmd.Transaction = transaction;
                AddArticleParameters(cmd, article);

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Upserted multiple articles in batch.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 删除文章.
    /// </summary>
    public async Task<bool> DeleteAsync(string articleId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Articles WHERE Id = @id";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", articleId);

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted article: {ArticleId}, affected: {Affected}", articleId, affected);

        return affected > 0;
    }

    /// <summary>
    /// 删除订阅源下的所有文章.
    /// </summary>
    public async Task<int> DeleteByFeedAsync(string feedId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Articles WHERE FeedId = @feedId";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@feedId", feedId);

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted {Count} articles for feed {FeedId}.", affected, feedId);

        return affected;
    }

    /// <summary>
    /// 清理过期文章.
    /// </summary>
    public async Task<int> CleanupOldAsync(
        DateTimeOffset olderThan,
        bool keepFavorites,
        CancellationToken cancellationToken = default)
    {
        var sql = "DELETE FROM Articles WHERE CachedAt < @olderThan";

        if (keepFavorites)
        {
            sql += " AND NOT EXISTS (SELECT 1 FROM Favorites f WHERE f.ArticleId = Articles.Id)";
        }

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@olderThan", olderThan.ToString("O"));

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogInformation("Cleaned up {Count} old articles.", affected);

        return affected;
    }

    private static RssArticle MapToArticleWithoutContent(SqliteDataReader reader)
    {
        return new RssArticle
        {
            Id = reader.GetString(0),
            FeedId = reader.GetString(1),
            Title = reader.GetString(2),
            Summary = reader.IsDBNull(3) ? null : reader.GetString(3),
            CoverUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
            Url = reader.IsDBNull(5) ? null : reader.GetString(5),
            Author = reader.IsDBNull(6) ? null : reader.GetString(6),
            PublishTime = reader.IsDBNull(7) ? null : reader.GetString(7),
            Tags = reader.IsDBNull(8) ? null : reader.GetString(8),
            ExtraData = reader.IsDBNull(9) ? null : reader.GetString(9),
        };
    }

    private static RssArticle MapToArticleWithContent(SqliteDataReader reader)
    {
        return new RssArticle
        {
            Id = reader.GetString(0),
            FeedId = reader.GetString(1),
            Title = reader.GetString(2),
            Summary = reader.IsDBNull(3) ? null : reader.GetString(3),
            Content = reader.IsDBNull(4) ? null : reader.GetString(4),
            CoverUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
            Url = reader.IsDBNull(6) ? null : reader.GetString(6),
            Author = reader.IsDBNull(7) ? null : reader.GetString(7),
            PublishTime = reader.IsDBNull(8) ? null : reader.GetString(8),
            Tags = reader.IsDBNull(9) ? null : reader.GetString(9),
            ExtraData = reader.IsDBNull(10) ? null : reader.GetString(10),
        };
    }

    private static void AddArticleParameters(SqliteCommand cmd, RssArticle article)
    {
        cmd.Parameters.AddWithValue("@id", article.Id);
        cmd.Parameters.AddWithValue("@feedId", article.FeedId);
        cmd.Parameters.AddWithValue("@title", article.Title);
        cmd.Parameters.AddWithValue("@summary", (object?)article.Summary ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@content", (object?)article.Content ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@coverUrl", (object?)article.CoverUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@url", (object?)article.Url ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@author", (object?)article.Author ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@publishTime", (object?)article.PublishTime ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tags", (object?)article.Tags ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@extraData", (object?)article.ExtraData ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@cachedAt", DateTimeOffset.UtcNow.ToString("O"));
    }
}
