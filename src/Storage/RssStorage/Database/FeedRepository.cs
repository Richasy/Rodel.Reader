// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 订阅源数据仓库.
/// </summary>
internal sealed class FeedRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;
    private readonly RssFeedEntityRepository<RssDatabase> _repository = new();

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
        var entities = await _repository.GetAllAsync(_database, cancellationToken).ConfigureAwait(false);
        var feeds = entities.Select(e => e.ToModel()).ToList();
        _logger?.LogDebug("Retrieved {Count} feeds.", feeds.Count);
        return feeds;
    }

    /// <summary>
    /// 根据 ID 获取订阅源.
    /// </summary>
    public async Task<RssFeed?> GetByIdAsync(string feedId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, feedId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    /// <summary>
    /// 添加或更新订阅源.
    /// </summary>
    public async Task UpsertAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        var entity = RssFeedEntity.FromModel(feed);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted feed: {FeedId}", feed.Id);
    }

    /// <summary>
    /// 批量添加或更新订阅源.
    /// </summary>
    public async Task UpsertManyAsync(IEnumerable<RssFeed> feeds, CancellationToken cancellationToken = default)
    {
        var entities = feeds.Select(RssFeedEntity.FromModel);
        await _repository.UpsertManyAsync(_database, entities, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted multiple feeds in batch.");
    }

    /// <summary>
    /// 删除订阅源.
    /// </summary>
    public async Task<bool> DeleteAsync(string feedId, CancellationToken cancellationToken = default)
    {
        var affected = await _repository.DeleteAsync(_database, feedId, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted feed: {FeedId}, affected: {Affected}", feedId, affected);
        return affected;
    }
}
