// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 收藏数据仓库.
/// </summary>
internal sealed class FavoriteRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoriteRepository"/> class.
    /// </summary>
    public FavoriteRepository(RssDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// 添加收藏.
    /// </summary>
    public async Task AddAsync(string articleId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Favorites (ArticleId, FavoritedAt)
            VALUES (@articleId, @favoritedAt)
            ON CONFLICT(ArticleId) DO NOTHING
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@articleId", articleId);
        cmd.Parameters.AddWithValue("@favoritedAt", DateTimeOffset.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Added article {ArticleId} to favorites.", articleId);
    }

    /// <summary>
    /// 移除收藏.
    /// </summary>
    public async Task RemoveAsync(string articleId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Favorites WHERE ArticleId = @articleId";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@articleId", articleId);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Removed article {ArticleId} from favorites.", articleId);
    }

    /// <summary>
    /// 检查文章是否已收藏.
    /// </summary>
    public async Task<bool> IsFavoriteAsync(string articleId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT 1 FROM Favorites WHERE ArticleId = @articleId";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@articleId", articleId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is not null;
    }
}
