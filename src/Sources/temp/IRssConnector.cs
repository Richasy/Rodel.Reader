// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.ReaderKernel.Models.Config;
using Richasy.ReaderKernel.Models.Rss;

namespace Richasy.ReaderKernel.Connectors;

/// <summary>
/// RSS 连接器.
/// </summary>
public interface IRssConnector
{
    /// <summary>
    /// 服务是否可用.
    /// </summary>
    /// <returns>是否可用.</returns>
    bool IsServiceAvailable();

    /// <summary>
    /// 登录.
    /// </summary>
    /// <returns>登录结果.</returns>
    Task<bool> SignInAsync(RssConfig config);

    /// <summary>
    /// 登出.
    /// </summary>
    /// <returns>登出结果.</returns>
    Task<bool> SignOutAsync();

    /// <summary>
    /// 获取订阅源列表.
    /// </summary>
    /// <returns>包含分组和订阅源在内的元组.</returns>
    Task<(List<RssFeedGroup> Groups, List<RssFeed> Feeds)> GetFeedListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加分组.
    /// </summary>
    /// <param name="group">分组信息.</param>
    /// <returns><see cref="RssFeedGroup"/>.</returns>
    Task<RssFeedGroup> AddGroupAsync(RssFeedGroup group);

    /// <summary>
    /// 更新分组.
    /// </summary>
    /// <param name="group">分组信息.</param>
    /// <returns>更新结果.</returns>
    Task<RssFeedGroup?> UpdateGroupAsync(RssFeedGroup group);

    /// <summary>
    /// 删除分组.
    /// </summary>
    /// <param name="group">分组信息.</param>
    /// <returns>删除结果.</returns>
    Task<bool> DeleteGroupAsync(RssFeedGroup group);

    /// <summary>
    /// 添加订阅源.
    /// </summary>
    /// <param name="feed">订阅源信息.</param>
    /// <returns>添加后的订阅源信息.</returns>
    Task<RssFeed?> AddFeedAsync(RssFeed feed);

    /// <summary>
    /// 更新订阅源.
    /// </summary>
    /// <param name="newFeed">新订阅源信息.</param>
    /// <param name="oldFeed">旧订阅源信息.</param>
    /// <returns>更新结果.</returns>
    Task<bool> UpdateFeedAsync(RssFeed newFeed, RssFeed oldFeed);

    /// <summary>
    /// 删除订阅源.
    /// </summary>
    /// <param name="feed">订阅源信息.</param>
    /// <returns>删除结果.</returns>
    Task<bool> DeleteFeedAsync(RssFeed feed);

    /// <summary>
    /// 获取订阅源文章列表.
    /// </summary>
    /// <param name="feed">订阅源信息.</param>
    /// <param name="cancellationToken">终止令牌.</param>
    /// <returns>文章列表.</returns>
    Task<RssFeedDetail?> GetFeedDetailAsync(RssFeed feed, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取多个订阅源文章列表.
    /// </summary>
    /// <param name="feeds">订阅源列表.</param>
    /// <param name="cancellationToken">终止令牌.</param>
    /// <returns>文章列表.</returns>
    Task<List<RssFeedDetail>> GetFeedDetailListAsync(List<RssFeed> feeds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传 OPML 文件.
    /// </summary>
    /// <param name="opmlPath">OPML 路径.</param>
    /// <returns>上传结果.</returns>
    Task<bool> UploadOpmlAsync(string opmlPath);

    /// <summary>
    /// 生成 OPML 内容.
    /// </summary>
    /// <returns>OPML 文本.</returns>
    Task<string> GenerateOpmlAsync();

    /// <summary>
    /// 将文章标记为已读.
    /// </summary>
    /// <param name="articleIds">文章 Id 列表.</param>
    /// <returns>是否标记成功.</returns>
    Task<bool> MarkReadAsync(params string[] articleIds);

    /// <summary>
    /// 将订阅源下的所有文章标记为已读.
    /// </summary>
    /// <param name="feed">订阅源信息.</param>
    /// <returns>标记是否成功.</returns>
    Task<bool> MarkAllReadAsync(RssFeed feed);

    /// <summary>
    /// 将分组下的所有文章标记为已读.
    /// </summary>
    /// <param name="group">分组信息.</param>
    /// <returns>标记是否成功.</returns>
    Task<bool> MarkAllReadAsync(RssFeedGroup group);
}
