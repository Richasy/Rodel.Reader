// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 播客详情.
/// </summary>
public sealed class RssPodcastDetail
{
    /// <summary>
    /// 播客信息.
    /// </summary>
    public RssPodcast Podcast { get; set; }

    /// <summary>
    /// 单集列表.
    /// </summary>
    public List<RssEpisodeBase> Episodes { get; set; }
}
