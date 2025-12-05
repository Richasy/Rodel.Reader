// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 订阅源详情.
/// </summary>
public sealed class RssFeedDetail
{
    /// <summary>
    /// 元数据.
    /// </summary>
    public RssFeed Feed { get; set; }

    /// <summary>
    /// 文章列表.
    /// </summary>
    public List<RssArticleBase> Articles { get; set; }
}
