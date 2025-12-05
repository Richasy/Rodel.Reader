// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 订阅源缓存.
/// </summary>
public sealed class RssFeedCache
{
    /// <summary>
    /// 缓存时间.
    /// </summary>
    public long CacheTime { get; set; }

    /// <summary>
    /// 分组列表.
    /// </summary>
    public List<RssFeedGroup> Groups { get; set; }

    /// <summary>
    /// 订阅源列表.
    /// </summary>
    public List<RssFeed> Feeds { get; set; }
}
