// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 缓存清单.
/// </summary>
public sealed class CacheManifest
{
    /// <summary>
    /// 书籍 ID.
    /// </summary>
    [JsonPropertyName("bookId")]
    public required string BookId { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// 目录哈希（用于验证目录是否变化）.
    /// </summary>
    [JsonPropertyName("tocHash")]
    public required string TocHash { get; init; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 最后更新时间.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// 已缓存的章节 ID 列表.
    /// </summary>
    [JsonPropertyName("cachedChapterIds")]
    public List<string> CachedChapterIds { get; init; } = [];

    /// <summary>
    /// 失败的章节 ID 列表.
    /// </summary>
    [JsonPropertyName("failedChapterIds")]
    public List<string> FailedChapterIds { get; init; } = [];
}
