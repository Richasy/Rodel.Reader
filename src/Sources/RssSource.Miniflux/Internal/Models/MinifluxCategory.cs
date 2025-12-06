// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Miniflux.Internal;

/// <summary>
/// Miniflux 分类.
/// </summary>
internal sealed class MinifluxCategory
{
    /// <summary>
    /// 分类 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 分类标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 用户 ID.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    /// <summary>
    /// 是否全局隐藏.
    /// </summary>
    [JsonPropertyName("hide_globally")]
    public bool HideGlobally { get; set; }

    /// <summary>
    /// 订阅源数量（需要 counts=true 参数）.
    /// </summary>
    [JsonPropertyName("feed_count")]
    public int? FeedCount { get; set; }

    /// <summary>
    /// 未读文章数量（需要 counts=true 参数）.
    /// </summary>
    [JsonPropertyName("total_unread")]
    public int? TotalUnread { get; set; }
}
