// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Models;

/// <summary>
/// 阅读进度.
/// </summary>
public sealed class BookProgress
{
    /// <summary>
    /// 书名.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 作者.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// 当前章节索引.
    /// </summary>
    [JsonPropertyName("durChapterIndex")]
    public int DurChapterIndex { get; set; }

    /// <summary>
    /// 当前章节标题.
    /// </summary>
    [JsonPropertyName("durChapterTitle")]
    public string? DurChapterTitle { get; set; }

    /// <summary>
    /// 当前章节阅读位置.
    /// </summary>
    [JsonPropertyName("durChapterPos")]
    public int DurChapterPos { get; set; }

    /// <summary>
    /// 当前阅读时间（时间戳）.
    /// </summary>
    [JsonPropertyName("durChapterTime")]
    public long DurChapterTime { get; set; }
}
