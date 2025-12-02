// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast;

/// <summary>
/// 播客搜索器接口.
/// </summary>
public interface IPodcastSearcher
{
    /// <summary>
    /// 搜索播客.
    /// </summary>
    /// <param name="keyword">搜索关键词.</param>
    /// <param name="limit">返回数量限制.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客列表.</returns>
    Task<IReadOnlyList<PodcastSummary>> SearchAsync(
        string keyword,
        int? limit = null,
        CancellationToken cancellationToken = default);
}
