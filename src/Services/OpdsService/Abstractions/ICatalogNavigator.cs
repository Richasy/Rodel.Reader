// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService;

/// <summary>
/// 目录导航器接口.
/// </summary>
public interface ICatalogNavigator
{
    /// <summary>
    /// 获取指定 URI 的 Feed.
    /// </summary>
    /// <param name="uri">Feed URI.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>OPDS Feed.</returns>
    Task<OpdsFeed> GetFeedAsync(Uri uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取根目录 Feed.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>根目录 OPDS Feed.</returns>
    Task<OpdsFeed> GetRootAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取下一页 Feed.
    /// </summary>
    /// <param name="currentFeed">当前 Feed.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>下一页 OPDS Feed，如果没有下一页则返回 null.</returns>
    Task<OpdsFeed?> GetNextPageAsync(OpdsFeed currentFeed, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取上一页 Feed.
    /// </summary>
    /// <param name="currentFeed">当前 Feed.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>上一页 OPDS Feed，如果没有上一页则返回 null.</returns>
    Task<OpdsFeed?> GetPreviousPageAsync(OpdsFeed currentFeed, CancellationToken cancellationToken = default);

    /// <summary>
    /// 导航到指定条目.
    /// </summary>
    /// <param name="entry">导航条目.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>目标 OPDS Feed，如果条目不是导航条目则返回 null.</returns>
    Task<OpdsFeed?> NavigateToEntryAsync(OpdsEntry entry, CancellationToken cancellationToken = default);
}
