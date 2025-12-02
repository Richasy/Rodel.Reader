// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast;

/// <summary>
/// 分类提供器接口.
/// </summary>
public interface ICategoryProvider
{
    /// <summary>
    /// 获取所有分类.
    /// </summary>
    /// <returns>分类列表.</returns>
    IReadOnlyList<PodcastCategory> GetCategories();

    /// <summary>
    /// 获取指定分类下的热门播客.
    /// </summary>
    /// <param name="categoryId">分类 ID.</param>
    /// <param name="region">区域代码（如 "us", "cn"）.</param>
    /// <param name="limit">返回数量限制.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>播客列表.</returns>
    Task<IReadOnlyList<PodcastSummary>> GetTopPodcastsAsync(
        string categoryId,
        string? region = null,
        int? limit = null,
        CancellationToken cancellationToken = default);
}
