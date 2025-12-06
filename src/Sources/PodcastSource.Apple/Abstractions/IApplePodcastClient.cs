// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Podcast.Apple;

/// <summary>
/// Apple Podcast 客户端接口.
/// </summary>
public interface IApplePodcastClient : IDisposable
{
    /// <summary>
    /// 获取分类提供器.
    /// </summary>
    ICategoryProvider Categories { get; }

    /// <summary>
    /// 获取搜索器.
    /// </summary>
    IPodcastSearcher Search { get; }

    /// <summary>
    /// 获取详情提供器.
    /// </summary>
    IPodcastDetailProvider Details { get; }

    /// <summary>
    /// 获取客户端配置.
    /// </summary>
    ApplePodcastClientOptions Options { get; }
}
