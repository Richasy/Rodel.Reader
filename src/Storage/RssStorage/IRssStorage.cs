// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss;

/// <summary>
/// RSS 存储服务接口.
/// </summary>
public interface IRssStorage : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// 初始化数据库.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>初始化任务.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    #region Feed 操作

    /// <summary>
    /// 获取所有订阅源.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>订阅源列表.</returns>
    Task<IReadOnlyList<RssFeed>> GetAllFeedsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取订阅源.
    /// </summary>
    /// <param name="feedId">订阅源 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>订阅源，不存在则返回 null.</returns>
    Task<RssFeed?> GetFeedAsync(string feedId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新订阅源.
    /// </summary>
    /// <param name="feed">订阅源.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertFeedAsync(RssFeed feed, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加或更新订阅源.
    /// </summary>
    /// <param name="feeds">订阅源列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertFeedsAsync(IEnumerable<RssFeed> feeds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除订阅源.
    /// </summary>
    /// <param name="feedId">订阅源 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteFeedAsync(string feedId, CancellationToken cancellationToken = default);

    #endregion

    #region Group 操作

    /// <summary>
    /// 获取所有分组.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>分组列表.</returns>
    Task<IReadOnlyList<RssFeedGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取分组.
    /// </summary>
    /// <param name="groupId">分组 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>分组，不存在则返回 null.</returns>
    Task<RssFeedGroup?> GetGroupAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新分组.
    /// </summary>
    /// <param name="group">分组.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertGroupAsync(RssFeedGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加或更新分组.
    /// </summary>
    /// <param name="groups">分组列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertGroupsAsync(IEnumerable<RssFeedGroup> groups, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除分组.
    /// </summary>
    /// <param name="groupId">分组 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default);

    #endregion

    #region Article 操作

    /// <summary>
    /// 获取订阅源下的所有文章（不含内容）.
    /// </summary>
    /// <param name="feedId">订阅源 ID.</param>
    /// <param name="limit">返回数量限制（0 表示不限制）.</param>
    /// <param name="offset">偏移量.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>文章列表（不含 Content 字段）.</returns>
    Task<IReadOnlyList<RssArticle>> GetArticlesByFeedAsync(
        string feedId,
        int limit = 0,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取未读文章列表.
    /// </summary>
    /// <param name="feedId">订阅源 ID（可选，为空则获取所有未读）.</param>
    /// <param name="limit">返回数量限制.</param>
    /// <param name="offset">偏移量.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>未读文章列表.</returns>
    Task<IReadOnlyList<RssArticle>> GetUnreadArticlesAsync(
        string? feedId = null,
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取收藏文章列表.
    /// </summary>
    /// <param name="limit">返回数量限制.</param>
    /// <param name="offset">偏移量.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>收藏文章列表.</returns>
    Task<IReadOnlyList<RssArticle>> GetFavoriteArticlesAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取文章（含内容）.
    /// </summary>
    /// <param name="articleId">文章 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>文章，不存在则返回 null.</returns>
    Task<RssArticle?> GetArticleAsync(string articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文章内容.
    /// </summary>
    /// <param name="articleId">文章 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>文章 HTML 内容.</returns>
    Task<string?> GetArticleContentAsync(string articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新文章.
    /// </summary>
    /// <param name="article">文章.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertArticleAsync(RssArticle article, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加或更新文章.
    /// </summary>
    /// <param name="articles">文章列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task UpsertArticlesAsync(IEnumerable<RssArticle> articles, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除文章.
    /// </summary>
    /// <param name="articleId">文章 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteArticleAsync(string articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除订阅源下的所有文章.
    /// </summary>
    /// <param name="feedId">订阅源 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>删除的文章数量.</returns>
    Task<int> DeleteArticlesByFeedAsync(string feedId, CancellationToken cancellationToken = default);

    #endregion

    #region 阅读状态

    /// <summary>
    /// 标记文章为已读.
    /// </summary>
    /// <param name="articleIds">文章 ID 列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task MarkAsReadAsync(IEnumerable<string> articleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记文章为未读.
    /// </summary>
    /// <param name="articleIds">文章 ID 列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task MarkAsUnreadAsync(IEnumerable<string> articleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将订阅源下所有文章标记为已读.
    /// </summary>
    /// <param name="feedId">订阅源 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task MarkFeedAsReadAsync(string feedId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将所有文章标记为已读.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查文章是否已读.
    /// </summary>
    /// <param name="articleId">文章 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否已读.</returns>
    Task<bool> IsArticleReadAsync(string articleId, CancellationToken cancellationToken = default);

    #endregion

    #region 收藏

    /// <summary>
    /// 添加收藏.
    /// </summary>
    /// <param name="articleId">文章 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task AddFavoriteAsync(string articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除收藏.
    /// </summary>
    /// <param name="articleId">文章 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task RemoveFavoriteAsync(string articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查文章是否已收藏.
    /// </summary>
    /// <param name="articleId">文章 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否已收藏.</returns>
    Task<bool> IsArticleFavoriteAsync(string articleId, CancellationToken cancellationToken = default);

    #endregion

    #region 清理

    /// <summary>
    /// 清理过期文章.
    /// </summary>
    /// <param name="olderThan">清理早于此时间的文章.</param>
    /// <param name="keepFavorites">是否保留收藏文章.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>清理的文章数量.</returns>
    Task<int> CleanupOldArticlesAsync(
        DateTimeOffset olderThan,
        bool keepFavorites = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理所有数据.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>操作任务.</returns>
    Task ClearAllAsync(CancellationToken cancellationToken = default);

    #endregion
}
