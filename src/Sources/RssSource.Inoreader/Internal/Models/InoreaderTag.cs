// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Inoreader.Internal;

/// <summary>
/// 标签/文件夹列表响应.
/// </summary>
internal sealed class InoreaderTagListResponse
{
    /// <summary>
    /// 标签列表.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<InoreaderTag> Tags { get; set; } = [];
}

/// <summary>
/// 标签/文件夹.
/// </summary>
internal sealed class InoreaderTag
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
    /// 类型 (folder, tag, etc.).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 未读数量.
    /// </summary>
    [JsonPropertyName("unread_count")]
    public int UnreadCount { get; set; }

    /// <summary>
    /// 未查看数量.
    /// </summary>
    [JsonPropertyName("unseen_count")]
    public int UnseenCount { get; set; }
}
