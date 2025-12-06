// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Feedbin.Internal;

/// <summary>
/// Feedbin 订阅项.
/// </summary>
internal sealed class FeedbinSubscription
{
    /// <summary>
    /// 订阅 ID（用于更新/删除操作）.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Feed ID（用于获取文章）.
    /// </summary>
    [JsonPropertyName("feed_id")]
    public int FeedId { get; set; }

    /// <summary>
    /// 订阅标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 订阅源 URL.
    /// </summary>
    [JsonPropertyName("feed_url")]
    public string? FeedUrl { get; set; }

    /// <summary>
    /// 网站 URL.
    /// </summary>
    [JsonPropertyName("site_url")]
    public string? SiteUrl { get; set; }
}
