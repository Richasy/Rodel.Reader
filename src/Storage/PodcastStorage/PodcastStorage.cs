// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Storage.Podcast.Database;

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 播客存储服务实现.
/// </summary>
public sealed class PodcastStorage : IPodcastStorage
{
    private readonly PodcastStorageOptions _options;
    private readonly ILogger<PodcastStorage>? _logger;

    private PodcastDatabase? _database;
    private PodcastRepository? _podcastRepository;
    private GroupRepository? _groupRepository;
    private EpisodeRepository? _episodeRepository;
    private ProgressRepository? _progressRepository;
    private SessionRepository? _sessionRepository;
    private bool _initialized;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastStorage"/> class.
    /// </summary>
    /// <param name="options">存储选项.</param>
    /// <param name="logger">日志记录器.</param>
    public PodcastStorage(PodcastStorageOptions options, ILogger<PodcastStorage>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_initialized)
        {
            _logger?.LogDebug("Storage already initialized.");
            return;
        }

        _logger?.LogInformation("Initializing Podcast storage at {DatabasePath}...", _options.DatabasePath);

        var directory = Path.GetDirectoryName(_options.DatabasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _database = new PodcastDatabase(_options.DatabasePath, _logger as ILogger<PodcastDatabase>);

        if (_options.CreateTablesOnInit)
        {
            await _database.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        _podcastRepository = new PodcastRepository(_database, _logger);
        _groupRepository = new GroupRepository(_database, _logger);
        _episodeRepository = new EpisodeRepository(_database, _logger);
        _progressRepository = new ProgressRepository(_database, _logger);
        _sessionRepository = new SessionRepository(_database, _logger);

        _initialized = true;
        _logger?.LogInformation("Podcast storage initialized successfully.");
    }

    #region Podcasts

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Podcast>> GetAllPodcastsAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _podcastRepository!.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Podcast>> GetSubscribedPodcastsAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _podcastRepository!.GetSubscribedAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Podcast?> GetPodcastAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _podcastRepository!.GetByIdAsync(podcastId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Podcast>> GetPodcastsByGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _podcastRepository!.GetByGroupAsync(groupId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Podcast>> GetPodcastsBySourceTypeAsync(PodcastSourceType sourceType, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _podcastRepository!.GetBySourceTypeAsync(sourceType, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Podcast>> SearchPodcastsAsync(string keyword, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _podcastRepository!.SearchAsync(keyword, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertPodcastAsync(Podcast podcast, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _podcastRepository!.UpsertAsync(podcast, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertPodcastsAsync(IEnumerable<Podcast> podcasts, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _podcastRepository!.UpsertManyAsync(podcasts, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePodcastAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        // 级联删除会自动处理 Episodes、Progress 和 Sessions
        return await _podcastRepository!.DeleteAsync(podcastId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Groups

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PodcastGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _groupRepository!.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<PodcastGroup?> GetGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _groupRepository!.GetByIdAsync(groupId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsGroupNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _groupRepository!.IsNameExistsAsync(name, excludeId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertGroupAsync(PodcastGroup group, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _groupRepository!.UpsertAsync(group, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertGroupsAsync(IEnumerable<PodcastGroup> groups, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _groupRepository!.UpsertManyAsync(groups, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        // 先从所有播客中移除该分组 ID
        var podcasts = await _podcastRepository!.GetByGroupAsync(groupId, cancellationToken).ConfigureAwait(false);
        foreach (var podcast in podcasts)
        {
            podcast.RemoveGroupId(groupId);
            await _podcastRepository.UpsertAsync(podcast, cancellationToken).ConfigureAwait(false);
        }

        return await _groupRepository!.DeleteAsync(groupId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Episodes

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Episode>> GetEpisodesByPodcastAsync(
        string podcastId,
        int limit = 0,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _episodeRepository!.GetByPodcastAsync(podcastId, limit, offset, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Episode?> GetEpisodeAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _episodeRepository!.GetByIdAsync(episodeId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Episode>> GetRecentEpisodesAsync(int days = 7, int limit = 50, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _episodeRepository!.GetRecentAsync(days, limit, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Episode>> GetUnlistenedEpisodesAsync(string? podcastId = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _episodeRepository!.GetUnlistenedAsync(podcastId, limit, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> GetEpisodeCountAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _episodeRepository!.GetCountByPodcastAsync(podcastId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertEpisodeAsync(Episode episode, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _episodeRepository!.UpsertAsync(episode, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertEpisodesAsync(IEnumerable<Episode> episodes, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _episodeRepository!.UpsertManyAsync(episodes, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteEpisodeAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _episodeRepository!.DeleteAsync(episodeId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> DeleteEpisodesByPodcastAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _episodeRepository!.DeleteByPodcastAsync(podcastId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Progress

    /// <inheritdoc/>
    public async Task<ListeningProgress?> GetProgressAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _progressRepository!.GetByEpisodeAsync(episodeId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<(Episode Episode, ListeningProgress Progress)>> GetInProgressEpisodesAsync(
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _progressRepository!.GetInProgressAsync(limit, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertProgressAsync(ListeningProgress progress, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _progressRepository!.UpsertAsync(progress, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteProgressAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _progressRepository!.DeleteAsync(episodeId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Sessions

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ListeningSession>> GetSessionsByEpisodeAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetByEpisodeAsync(episodeId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ListeningSession>> GetSessionsByPodcastAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetByPodcastAsync(podcastId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ListeningSession>> GetRecentSessionsAsync(int days = 30, int limit = 100, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetRecentAsync(days, limit, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddSessionAsync(ListeningSession session, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _sessionRepository!.AddAsync(session, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ListeningStats> GetPodcastStatsAsync(string podcastId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetPodcastStatsAsync(podcastId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ListeningStats> GetEpisodeStatsAsync(string episodeId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetEpisodeStatsAsync(episodeId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ListeningStats> GetOverallStatsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetOverallStatsAsync(days, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Cleanup

    /// <inheritdoc/>
    public async Task<int> CleanupOldEpisodesAsync(int keepDays = 90, int? keepCount = null, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _episodeRepository!.CleanupOldAsync(keepDays, keepCount, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> CleanupOldSessionsAsync(int keepDays = 365, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.CleanupOldAsync(keepDays, cancellationToken).ConfigureAwait(false);
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

        _logger?.LogDebug("PodcastStorage disposed.");
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

        _logger?.LogDebug("PodcastStorage disposed asynchronously.");
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
