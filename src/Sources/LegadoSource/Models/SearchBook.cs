// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Models;

/// <summary>
/// 搜索结果书籍.
/// </summary>
public sealed class SearchBook
{
    /// <summary>
    /// 书籍链接.
    /// </summary>
    [JsonPropertyName("bookUrl")]
    public string BookUrl { get; set; } = string.Empty;

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
    /// 最新章节.
    /// </summary>
    [JsonPropertyName("latestChapterTitle")]
    public string? LatestChapterTitle { get; set; }

    /// <summary>
    /// 字数.
    /// </summary>
    [JsonPropertyName("wordCount")]
    public string? WordCount { get; set; }

    /// <summary>
    /// 书籍类型.
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 时间.
    /// </summary>
    [JsonPropertyName("time")]
    public long Time { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [JsonPropertyName("originOrder")]
    public int OriginOrder { get; set; }
}
