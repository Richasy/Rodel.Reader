// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 文章数据仓库.
/// </summary>
internal sealed class ArticleRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;
    private readonly RssArticleEntityRepository<RssDatabase> _repository = new();

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
            SELECT {RssArticleEntityRepository<RssDatabase>.ListFields}
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
            articles.Add(RssArticleEntityRepository<RssDatabase>.MapToEntityList(reader).ToModel());
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
            SELECT {RssArticleEntityRepository<RssDatabase>.ListFields}
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
            articles.Add(RssArticleEntityRepository<RssDatabase>.MapToEntityList(reader).ToModel());
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
            SELECT {RssArticleEntityRepository<RssDatabase>.ListFields}
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
            articles.Add(RssArticleEntityRepository<RssDatabase>.MapToEntityList(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} favorite articles.", articles.Count);
        return articles;
    }

    /// <summary>
    /// 根据 ID 获取文章（含内容）.
    /// </summary>
    public async Task<RssArticle?> GetByIdAsync(string articleId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, articleId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
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
        var entity = RssArticleEntity.FromModel(article);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted article: {ArticleId}", article.Id);
    }

    /// <summary>
    /// 批量添加或更新文章.
    /// </summary>
    public async Task UpsertManyAsync(IEnumerable<RssArticle> articles, CancellationToken cancellationToken = default)
    {
        var entities = articles.Select(RssArticleEntity.FromModel);
        await _repository.UpsertManyAsync(_database, entities, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted multiple articles in batch.");
    }

    /// <summary>
    /// 删除文章.
    /// </summary>
    public async Task<bool> DeleteAsync(string articleId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, articleId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted article: {ArticleId}, affected: {Affected}", articleId, affected);
        return affected;
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
        cmd.Parameters.AddWithValue("@olderThan", olderThan.ToUnixTimeSeconds());

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogInformation("Cleaned up {Count} old articles.", affected);

        return affected;
    }
}
