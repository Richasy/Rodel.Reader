// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Feedbin.Internal;

/// <summary>
/// Feedbin 文章条目.
/// </summary>
internal sealed class FeedbinEntry
{
    /// <summary>
    /// 文章 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Feed ID.
    /// </summary>
    [JsonPropertyName("feed_id")]
    public int FeedId { get; set; }

    /// <summary>
    /// 文章标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 文章作者.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// 文章摘要.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// 文章内容.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// 文章 URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 提取内容的 URL.
    /// </summary>
    [JsonPropertyName("extracted_content_url")]
    public string? ExtractedContentUrl { get; set; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    [JsonPropertyName("published")]
    public DateTimeOffset Published { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
