// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 只读客户端接口.
/// 定义从 RSS 源读取数据的能力.
/// </summary>
public interface IRssReader
{
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
}
