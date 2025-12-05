// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 客户端接口.
/// 定义与 RSS 在线服务交互的核心能力.
/// </summary>
public interface IRssClient : IDisposable
{
    /// <summary>
    /// 获取当前是否已通过身份验证.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 登录到 RSS 服务.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>登录是否成功.</returns>
    Task<bool> SignInAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 从 RSS 服务登出.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>登出是否成功.</returns>
    Task<bool> SignOutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取订阅源列表（包含分组信息）.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>分组列表和订阅源列表的元组.</returns>
    Task<(IReadOnlyList<RssFeedGroup> Groups, IReadOnlyList<RssFeed> Feeds)> GetFeedListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取订阅源详情（包含文章列表）.
    /// </summary>
    /// <param name="feed">订阅源信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>订阅源详情.</returns>
    Task<RssFeedDetail?> GetFeedDetailAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量获取多个订阅源的详情.
    /// </summary>
    /// <param name="feeds">订阅源列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>订阅源详情列表.</returns>
    Task<IReadOnlyList<RssFeedDetail>> GetFeedDetailListAsync(
        IEnumerable<RssFeed> feeds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加分组.
    /// </summary>
    /// <param name="group">分组信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>添加后的分组（可能包含服务端生成的 ID）.</returns>
    Task<RssFeedGroup?> AddGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新分组.
    /// </summary>
    /// <param name="group">分组信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>更新后的分组.</returns>
    Task<RssFeedGroup?> UpdateGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除分组.
    /// </summary>
    /// <param name="group">分组信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteGroupAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加订阅源.
    /// </summary>
    /// <param name="feed">订阅源信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>添加后的订阅源（可能包含服务端生成的 ID）.</returns>
    Task<RssFeed?> AddFeedAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新订阅源.
    /// </summary>
    /// <param name="newFeed">新的订阅源信息.</param>
    /// <param name="oldFeed">旧的订阅源信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否更新成功.</returns>
    Task<bool> UpdateFeedAsync(
        RssFeed newFeed,
        RssFeed oldFeed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除订阅源.
    /// </summary>
    /// <param name="feed">订阅源信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteFeedAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 将文章标记为已读.
    /// </summary>
    /// <param name="articleIds">文章 ID 列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否标记成功.</returns>
    Task<bool> MarkArticlesAsReadAsync(
        IEnumerable<string> articleIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 将订阅源下的所有文章标记为已读.
    /// </summary>
    /// <param name="feed">订阅源信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否标记成功.</returns>
    Task<bool> MarkFeedAsReadAsync(
        RssFeed feed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 将分组下的所有文章标记为已读.
    /// </summary>
    /// <param name="group">分组信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否标记成功.</returns>
    Task<bool> MarkGroupAsReadAsync(
        RssFeedGroup group,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 导入 OPML 文件.
    /// </summary>
    /// <param name="opmlContent">OPML 文件内容.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否导入成功.</returns>
    Task<bool> ImportOpmlAsync(
        string opmlContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 导出为 OPML 格式.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>OPML 文件内容.</returns>
    Task<string> ExportOpmlAsync(CancellationToken cancellationToken = default);
}
