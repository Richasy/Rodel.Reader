// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Podcast.Apple;

/// <summary>
/// 播客 Feed 解析器接口.
/// </summary>
public interface IPodcastFeedParser
{
    /// <summary>
    /// 解析播客 Feed 内容.
    /// </summary>
    /// <param name="feedContent">Feed XML 内容.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客详情.</returns>
    Task<PodcastDetail?> ParseAsync(string feedContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解析播客 Feed 流.
    /// </summary>
    /// <param name="feedStream">Feed XML 流.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客详情.</returns>
    Task<PodcastDetail?> ParseAsync(Stream feedStream, CancellationToken cancellationToken = default);
}
