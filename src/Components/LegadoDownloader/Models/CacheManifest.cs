// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Models;

/// <summary>
/// 缓存清单.
/// </summary>
public sealed class CacheManifest
{
    /// <summary>
    /// 书籍链接（唯一标识）.
    /// </summary>
    [JsonPropertyName("bookUrl")]
    public required string BookUrl { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// 书源链接.
    /// </summary>
    [JsonPropertyName("bookSource")]
    public string? BookSource { get; init; }

    /// <summary>
    /// 服务地址.
    /// </summary>
    [JsonPropertyName("serverUrl")]
    public string? ServerUrl { get; init; }

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
    /// 已缓存的章节索引列表.
    /// </summary>
    [JsonPropertyName("cachedChapterIndexes")]
    public List<int> CachedChapterIndexes { get; init; } = [];

    /// <summary>
    /// 失败的章节索引列表.
    /// </summary>
    [JsonPropertyName("failedChapterIndexes")]
    public List<int> FailedChapterIndexes { get; init; } = [];
}
