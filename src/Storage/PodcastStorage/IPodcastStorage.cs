// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 播客存储服务接口.
/// </summary>
public interface IPodcastStorage : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// 初始化数据库.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>初始化任务.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    #region Podcast 操作

    /// <summary>
    /// 获取所有播客.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客列表.</returns>
    Task<IReadOnlyList<Podcast>> GetAllPodcastsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取已订阅的播客.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>已订阅的播客列表.</returns>
    Task<IReadOnlyList<Podcast>> GetSubscribedPodcastsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取播客.
    /// </summary>
    /// <param name="podcastId">播客 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客，不存在则返回 null.</returns>
    Task<Podcast?> GetPodcastAsync(string podcastId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据分组获取播客.
    /// </summary>
    /// <param name="groupId">分组 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客列表.</returns>
    Task<IReadOnlyList<Podcast>> GetPodcastsByGroupAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据来源类型获取播客.
    /// </summary>
    /// <param name="sourceType">来源类型.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客列表.</returns>
    Task<IReadOnlyList<Podcast>> GetPodcastsBySourceTypeAsync(PodcastSourceType sourceType, CancellationToken cancellationToken = default);

    /// <summary>
    /// 搜索播客.
    /// </summary>
    /// <param name="keyword">关键词.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>搜索结果.</returns>
    Task<IReadOnlyList<Podcast>> SearchPodcastsAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新播客.
    /// </summary>
    /// <param name="podcast">播客.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertPodcastAsync(Podcast podcast, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加或更新播客.
    /// </summary>
    /// <param name="podcasts">播客列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertPodcastsAsync(IEnumerable<Podcast> podcasts, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除播客（同时删除相关单集、进度和时段）.
    /// </summary>
    /// <param name="podcastId">播客 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeletePodcastAsync(string podcastId, CancellationToken cancellationToken = default);

    #endregion

    #region Group 操作

    /// <summary>
    /// 获取所有分组.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>分组列表.</returns>
    Task<IReadOnlyList<PodcastGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取分组.
    /// </summary>
    /// <param name="groupId">分组 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>分组，不存在则返回 null.</returns>
    Task<PodcastGroup?> GetGroupAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查分组名称是否存在.
    /// </summary>
    /// <param name="name">分组名称.</param>
    /// <param name="excludeId">排除的分组 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否存在.</returns>
    Task<bool> IsGroupNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新分组.
    /// </summary>
    /// <param name="group">分组.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertGroupAsync(PodcastGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加或更新分组.
    /// </summary>
    /// <param name="groups">分组列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertGroupsAsync(IEnumerable<PodcastGroup> groups, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除分组（播客不会被删除，只移除关联）.
    /// </summary>
    /// <param name="groupId">分组 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default);

    #endregion

    #region Episode 操作

    /// <summary>
    /// 获取播客的单集.
    /// </summary>
    /// <param name="podcastId">播客 ID.</param>
    /// <param name="limit">返回数量限制（0 表示不限制）.</param>
    /// <param name="offset">偏移量.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>单集列表.</returns>
    Task<IReadOnlyList<Episode>> GetEpisodesByPodcastAsync(
        string podcastId,
        int limit = 0,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取单集.
    /// </summary>
    /// <param name="episodeId">单集 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>单集，不存在则返回 null.</returns>
    Task<Episode?> GetEpisodeAsync(string episodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最近发布的单集.
    /// </summary>
    /// <param name="days">天数.</param>
    /// <param name="limit">返回数量限制.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>单集列表.</returns>
    Task<IReadOnlyList<Episode>> GetRecentEpisodesAsync(int days = 7, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取未收听的单集.
    /// </summary>
    /// <param name="podcastId">播客 ID（可选）.</param>
    /// <param name="limit">返回数量限制.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>单集列表.</returns>
    Task<IReadOnlyList<Episode>> GetUnlistenedEpisodesAsync(string? podcastId = null, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取播客的单集数量.
    /// </summary>
    /// <param name="podcastId">播客 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>单集数量.</returns>
    Task<int> GetEpisodeCountAsync(string podcastId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新单集.
    /// </summary>
    /// <param name="episode">单集.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertEpisodeAsync(Episode episode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加或更新单集.
    /// </summary>
    /// <param name="episodes">单集列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertEpisodesAsync(IEnumerable<Episode> episodes, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除单集.
    /// </summary>
    /// <param name="episodeId">单集 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteEpisodeAsync(string episodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除播客的所有单集.
    /// </summary>
    /// <param name="podcastId">播客 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>删除的数量.</returns>
    Task<int> DeleteEpisodesByPodcastAsync(string podcastId, CancellationToken cancellationToken = default);

    #endregion

    #region 收听进度

    /// <summary>
    /// 获取单集的收听进度.
    /// </summary>
    /// <param name="episodeId">单集 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>收听进度，不存在则返回 null.</returns>
    Task<ListeningProgress?> GetProgressAsync(string episodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取正在收听的单集（有进度但未完成）.
    /// </summary>
    /// <param name="limit">返回数量限制.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>单集和进度列表.</returns>
    Task<IReadOnlyList<(Episode Episode, ListeningProgress Progress)>> GetInProgressEpisodesAsync(
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新收听进度.
    /// </summary>
    /// <param name="progress">收听进度.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertProgressAsync(ListeningProgress progress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除收听进度.
    /// </summary>
    /// <param name="episodeId">单集 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteProgressAsync(string episodeId, CancellationToken cancellationToken = default);

    #endregion

    #region 收听时段

    /// <summary>
    /// 获取单集的收听时段.
    /// </summary>
    /// <param name="episodeId">单集 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>收听时段列表.</returns>
    Task<IReadOnlyList<ListeningSession>> GetSessionsByEpisodeAsync(string episodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取播客的收听时段.
    /// </summary>
    /// <param name="podcastId">播客 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>收听时段列表.</returns>
    Task<IReadOnlyList<ListeningSession>> GetSessionsByPodcastAsync(string podcastId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最近的收听时段.
    /// </summary>
    /// <param name="days">天数.</param>
    /// <param name="limit">返回数量限制.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>收听时段列表.</returns>
    Task<IReadOnlyList<ListeningSession>> GetRecentSessionsAsync(int days = 30, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加收听时段.
    /// </summary>
    /// <param name="session">收听时段.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task AddSessionAsync(ListeningSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取播客的收听统计.
    /// </summary>
    /// <param name="podcastId">播客 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>收听统计.</returns>
    Task<ListeningStats> GetPodcastStatsAsync(string podcastId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取单集的收听统计.
    /// </summary>
    /// <param name="episodeId">单集 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>收听统计.</returns>
    Task<ListeningStats> GetEpisodeStatsAsync(string episodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取整体收听统计.
    /// </summary>
    /// <param name="days">统计天数.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>收听统计.</returns>
    Task<ListeningStats> GetOverallStatsAsync(int days = 30, CancellationToken cancellationToken = default);

    #endregion

    #region 清理

    /// <summary>
    /// 清理旧单集.
    /// </summary>
    /// <param name="keepDays">保留天数.</param>
    /// <param name="keepCount">每个播客保留的单集数量（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>删除的数量.</returns>
    Task<int> CleanupOldEpisodesAsync(int keepDays = 90, int? keepCount = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理旧收听时段.
    /// </summary>
    /// <param name="keepDays">保留天数.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>删除的数量.</returns>
    Task<int> CleanupOldSessionsAsync(int keepDays = 365, CancellationToken cancellationToken = default);

    #endregion
}
