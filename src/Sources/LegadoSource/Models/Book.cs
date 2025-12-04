// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Models;

/// <summary>
/// 书籍信息.
/// </summary>
public sealed class Book
{
    /// <summary>
    /// 书籍链接（唯一标识）.
    /// </summary>
    [JsonPropertyName("bookUrl")]
    public string BookUrl { get; set; } = string.Empty;

    /// <summary>
    /// 目录链接.
    /// </summary>
    [JsonPropertyName("tocUrl")]
    public string? TocUrl { get; set; }

    /// <summary>
    /// 书源链接.
    /// </summary>
    [JsonPropertyName("origin")]
    public string? Origin { get; set; }

    /// <summary>
    /// 书源名称.
    /// </summary>
    [JsonPropertyName("originName")]
    public string? OriginName { get; set; }

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
    /// 分类/标签.
    /// </summary>
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    /// <summary>
    /// 封面链接.
    /// </summary>
    [JsonPropertyName("coverUrl")]
    public string? CoverUrl { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    [JsonPropertyName("intro")]
    public string? Intro { get; set; }

    /// <summary>
    /// 书籍类型 (0: 小说, 1: 漫画, 2: 音频).
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 分组.
    /// </summary>
    [JsonPropertyName("group")]
    public int Group { get; set; }

    /// <summary>
    /// 最新章节标题.
    /// </summary>
    [JsonPropertyName("latestChapterTitle")]
    public string? LatestChapterTitle { get; set; }

    /// <summary>
    /// 最新章节时间（时间戳）.
    /// </summary>
    [JsonPropertyName("latestChapterTime")]
    public long LatestChapterTime { get; set; }

    /// <summary>
    /// 最后检查时间（时间戳）.
    /// </summary>
    [JsonPropertyName("lastCheckTime")]
    public long LastCheckTime { get; set; }

    /// <summary>
    /// 最后检查新章节数.
    /// </summary>
    [JsonPropertyName("lastCheckCount")]
    public int LastCheckCount { get; set; }

    /// <summary>
    /// 总章节数.
    /// </summary>
    [JsonPropertyName("totalChapterNum")]
    public int TotalChapterNum { get; set; }

    /// <summary>
    /// 当前阅读章节标题.
    /// </summary>
    [JsonPropertyName("durChapterTitle")]
    public string? DurChapterTitle { get; set; }

    /// <summary>
    /// 当前阅读章节索引.
    /// </summary>
    [JsonPropertyName("durChapterIndex")]
    public int DurChapterIndex { get; set; }

    /// <summary>
    /// 当前阅读章节位置.
    /// </summary>
    [JsonPropertyName("durChapterPos")]
    public int DurChapterPos { get; set; }

    /// <summary>
    /// 当前阅读时间（时间戳）.
    /// </summary>
    [JsonPropertyName("durChapterTime")]
    public long DurChapterTime { get; set; }

    /// <summary>
    /// 字数.
    /// </summary>
    [JsonPropertyName("wordCount")]
    public string? WordCount { get; set; }

    /// <summary>
    /// 是否可更新.
    /// </summary>
    [JsonPropertyName("canUpdate")]
    public bool CanUpdate { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// 书源排序.
    /// </summary>
    [JsonPropertyName("originOrder")]
    public int OriginOrder { get; set; }

    /// <summary>
    /// 是否使用替换规则.
    /// </summary>
    [JsonPropertyName("useReplaceRule")]
    public bool UseReplaceRule { get; set; }

    /// <summary>
    /// 变量.
    /// </summary>
    [JsonPropertyName("variable")]
    public string? Variable { get; set; }

    /// <summary>
    /// 是否在书架中.
    /// </summary>
    [JsonPropertyName("isInShelf")]
    public bool IsInShelf { get; set; }
}
