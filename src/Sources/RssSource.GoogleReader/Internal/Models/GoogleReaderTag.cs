// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.GoogleReader.Internal;

/// <summary>
/// 标签列表响应.
/// </summary>
internal sealed class GoogleReaderTagListResponse
{
    /// <summary>
    /// 标签列表.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<GoogleReaderTag> Tags { get; set; } = [];
}

/// <summary>
/// 标签/分组项.
/// </summary>
internal sealed class GoogleReaderTag
{
    /// <summary>
    /// 标签 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 排序 ID.
    /// </summary>
    [JsonPropertyName("sortid")]
    public string? SortId { get; set; }

    /// <summary>
    /// 标签类型 (folder, tag 等).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 未读数量.
    /// </summary>
    [JsonPropertyName("unread_count")]
    public int UnreadCount { get; set; }
}
