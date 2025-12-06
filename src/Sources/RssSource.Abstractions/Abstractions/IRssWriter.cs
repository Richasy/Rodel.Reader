// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 可写客户端接口.
/// 定义向 RSS 源写入数据的能力.
/// </summary>
public interface IRssWriter
{
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
}
