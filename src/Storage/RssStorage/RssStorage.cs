// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Storage.Rss.Database;

namespace Richasy.RodelReader.Storage.Rss;

/// <summary>
/// RSS 存储服务实现.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RssStorage"/> class.
/// </remarks>
/// <param name="options">存储选项.</param>
/// <param name="logger">日志记录器.</param>
public sealed class RssStorage(RssStorageOptions options, ILogger<RssStorage>? logger = null) : IRssStorage
{
    private readonly RssStorageOptions _options = options ?? throw new ArgumentNullException(nameof(options));
    private RssDatabase? _database;
    private FeedRepository? _feedRepository;
    private GroupRepository? _groupRepository;
    private ArticleRepository? _articleRepository;
    private ReadStatusRepository? _readStatusRepository;
    private FavoriteRepository? _favoriteRepository;
    private bool _initialized;
    private bool _disposed;

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_initialized)
        {
            logger?.LogDebug("Storage already initialized.");
            return;
        }

        logger?.LogInformation("Initializing RSS storage at {DatabasePath}...", _options.DatabasePath);

        // 确保目录存在
        var directory = Path.GetDirectoryName(_options.DatabasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _database = new RssDatabase(_options.DatabasePath, logger as ILogger<RssDatabase>);

        if (_options.CreateTablesOnInit)
        {
            await _database.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        // 初始化仓库
        _feedRepository = new FeedRepository(_database, logger);
        _groupRepository = new GroupRepository(_database, logger);
        _articleRepository = new ArticleRepository(_database, logger);
        _readStatusRepository = new ReadStatusRepository(_database, logger);
        _favoriteRepository = new FavoriteRepository(_database, logger);

        _initialized = true;
        logger?.LogInformation("RSS storage initialized successfully.");
    }

    #region Feeds

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RssFeed>> GetAllFeedsAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _feedRepository!.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RssFeed?> GetFeedAsync(string feedId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _feedRepository!.GetByIdAsync(feedId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertFeedAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _feedRepository!.UpsertAsync(feed, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertFeedsAsync(IEnumerable<RssFeed> feeds, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _feedRepository!.UpsertManyAsync(feeds, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFeedAsync(string feedId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _feedRepository!.DeleteAsync(feedId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Groups

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RssFeedGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _groupRepository!.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RssFeedGroup?> GetGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _groupRepository!.GetByIdAsync(groupId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertGroupAsync(RssFeedGroup group, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _groupRepository!.UpsertAsync(group, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertGroupsAsync(IEnumerable<RssFeedGroup> groups, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _groupRepository!.UpsertManyAsync(groups, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _groupRepository!.DeleteAsync(groupId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Articles

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RssArticle>> GetArticlesByFeedAsync(
        string feedId,
        int limit = 0,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _articleRepository!.GetByFeedAsync(feedId, limit, offset, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RssArticle>> GetUnreadArticlesAsync(
        string? feedId = null,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _articleRepository!.GetUnreadAsync(feedId, limit, offset, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RssArticle>> GetFavoriteArticlesAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _articleRepository!.GetFavoritesAsync(limit, offset, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RssArticle?> GetArticleAsync(string articleId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _articleRepository!.GetByIdAsync(articleId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<string?> GetArticleContentAsync(string articleId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _articleRepository!.GetContentAsync(articleId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertArticleAsync(RssArticle article, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _articleRepository!.UpsertAsync(article, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertArticlesAsync(IEnumerable<RssArticle> articles, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _articleRepository!.UpsertManyAsync(articles, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteArticleAsync(string articleId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _articleRepository!.DeleteAsync(articleId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> DeleteArticlesByFeedAsync(string feedId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _articleRepository!.DeleteByFeedAsync(feedId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Read Status

    /// <inheritdoc/>
    public async Task MarkAsReadAsync(IEnumerable<string> articleIds, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _readStatusRepository!.MarkAsReadAsync(articleIds, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task MarkAsUnreadAsync(IEnumerable<string> articleIds, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _readStatusRepository!.MarkAsUnreadAsync(articleIds, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task MarkFeedAsReadAsync(string feedId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _readStatusRepository!.MarkFeedAsReadAsync(feedId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _readStatusRepository!.MarkAllAsReadAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsArticleReadAsync(string articleId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _readStatusRepository!.IsReadAsync(articleId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Favorites

    /// <inheritdoc/>
    public async Task AddFavoriteAsync(string articleId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _favoriteRepository!.AddAsync(articleId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task RemoveFavoriteAsync(string articleId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _favoriteRepository!.RemoveAsync(articleId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsArticleFavoriteAsync(string articleId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _favoriteRepository!.IsFavoriteAsync(articleId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Cleanup

    /// <inheritdoc/>
    public async Task<int> CleanupOldArticlesAsync(
        DateTimeOffset olderThan,
        bool keepFavorites = true,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _articleRepository!.CleanupOldAsync(olderThan, keepFavorites, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        logger?.LogWarning("Clearing all RSS data...");

        await using var cmd = _database!.CreateCommand(Database.Schema.DropTablesSql);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        await using var createCmd = _database.CreateCommand(Database.Schema.CreateTablesSql);
        await createCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        logger?.LogWarning("All RSS data cleared.");
    }

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _database?.Dispose();
        _disposed = true;

        logger?.LogDebug("RSS storage disposed.");
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_database is not null)
        {
            await _database.DisposeAsync().ConfigureAwait(false);
        }

        _disposed = true;

        logger?.LogDebug("RSS storage disposed asynchronously.");
    }

    #endregion

    private void EnsureInitialized()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_initialized)
        {
            throw new InvalidOperationException("Storage has not been initialized. Call InitializeAsync first.");
        }
    }
}
