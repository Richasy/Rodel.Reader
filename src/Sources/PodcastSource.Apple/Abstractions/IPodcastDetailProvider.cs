// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Podcast.Apple;

/// <summary>
/// 播客详情提供器接口.
/// </summary>
public interface IPodcastDetailProvider
{
    /// <summary>
    /// 通过 iTunes ID 获取播客详情.
    /// </summary>
    /// <param name="podcastId">iTunes 播客 ID.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客详情，如果获取失败则返回 null.</returns>
    Task<PodcastDetail?> GetDetailByIdAsync(string podcastId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过 RSS Feed URL 获取播客详情.
    /// </summary>
    /// <param name="feedUrl">RSS Feed URL.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客详情，如果获取失败则返回 null.</returns>
    Task<PodcastDetail?> GetDetailByFeedUrlAsync(string feedUrl, CancellationToken cancellationToken = default);
}
