// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.NewsBlur.Internal;

/// <summary>
/// NewsBlur 订阅源.
/// </summary>
internal sealed class NewsBlurFeed
{
    /// <summary>
    /// 订阅源 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 订阅源标题.
    /// </summary>
    [JsonPropertyName("feed_title")]
    public string FeedTitle { get; set; } = string.Empty;

    /// <summary>
    /// 订阅源 RSS 地址.
    /// </summary>
    [JsonPropertyName("feed_address")]
    public string? FeedAddress { get; set; }

    /// <summary>
    /// 网站链接.
    /// </summary>
    [JsonPropertyName("feed_link")]
    public string? FeedLink { get; set; }

    /// <summary>
    /// 订阅者数量.
    /// </summary>
    [JsonPropertyName("num_subscribers")]
    public int NumSubscribers { get; set; }

    /// <summary>
    /// 最后更新时间.
    /// </summary>
    [JsonPropertyName("updated")]
    public string? Updated { get; set; }

    /// <summary>
    /// 是否活跃.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// 图标 URL.
    /// </summary>
    [JsonPropertyName("favicon_url")]
    public string? FaviconUrl { get; set; }

    /// <summary>
    /// 正面评分文章数.
    /// </summary>
    [JsonPropertyName("ps")]
    public int PositiveCount { get; set; }

    /// <summary>
    /// 中性评分文章数.
    /// </summary>
    [JsonPropertyName("nt")]
    public int NeutralCount { get; set; }

    /// <summary>
    /// 负面评分文章数.
    /// </summary>
    [JsonPropertyName("ng")]
    public int NegativeCount { get; set; }
}
