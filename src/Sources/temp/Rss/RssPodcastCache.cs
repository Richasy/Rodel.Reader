// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// 播客缓存.
/// </summary>
public sealed class RssPodcastCache
{
    /// <summary>
    /// 缓存时间.
    /// </summary>
    public long CacheTime { get; set; }

    /// <summary>
    /// 播客源列表.
    /// </summary>
    public List<RssPodcast> Podcasts { get; set; }
}
