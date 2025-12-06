// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 订阅源详情（包含文章列表）.
/// </summary>
public sealed class RssFeedDetail
{
    /// <summary>
    /// 订阅源信息.
    /// </summary>
    public RssFeed Feed { get; set; } = new();

    /// <summary>
    /// 文章列表.
    /// </summary>
    public IReadOnlyList<RssArticle> Articles { get; set; } = [];
}
