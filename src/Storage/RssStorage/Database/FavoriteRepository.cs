// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 收藏数据仓库.
/// </summary>
internal sealed class FavoriteRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;
    private readonly FavoriteEntityRepository _repository = new();

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
        var entity = new FavoriteEntity { ArticleId = articleId };
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Added article {ArticleId} to favorites.", articleId);
    }

    /// <summary>
    /// 移除收藏.
    /// </summary>
    public async Task RemoveAsync(string articleId, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(_database, articleId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Removed article {ArticleId} from favorites.", articleId);
    }

    /// <summary>
    /// 检查文章是否已收藏.
    /// </summary>
    public async Task<bool> IsFavoriteAsync(string articleId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, articleId, cancellationToken).ConfigureAwait(false);
        return entity is not null;
    }
}
