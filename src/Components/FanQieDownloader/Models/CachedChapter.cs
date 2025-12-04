// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 缓存的章节信息.
/// </summary>
public sealed class CachedChapter
{
    /// <summary>
    /// 章节 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public required string ChapterId { get; init; }

    /// <summary>
    /// 章节标题.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>
    /// 章节序号.
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; init; }

    /// <summary>
    /// 所属卷名.
    /// </summary>
    [JsonPropertyName("volume")]
    public string? VolumeName { get; init; }

    /// <summary>
    /// 章节状态.
    /// </summary>
    [JsonPropertyName("status")]
    public ChapterStatus Status { get; set; }

    /// <summary>
    /// HTML 内容（带番茄标记）.
    /// </summary>
    [JsonPropertyName("html")]
    public string? HtmlContent { get; set; }

    /// <summary>
    /// 字数.
    /// </summary>
    [JsonPropertyName("wordCount")]
    public int WordCount { get; set; }

    /// <summary>
    /// 章节图片引用列表.
    /// </summary>
    [JsonPropertyName("images")]
    public List<CachedImageRef>? Images { get; set; }

    /// <summary>
    /// 失败原因.
    /// </summary>
    [JsonPropertyName("failReason")]
    public string? FailureReason { get; set; }

    /// <summary>
    /// 下载时间.
    /// </summary>
    [JsonPropertyName("downloadTime")]
    public DateTimeOffset? DownloadTime { get; set; }
}

/// <summary>
/// 缓存的图片引用.
/// </summary>
public sealed class CachedImageRef
{
    /// <summary>
    /// 图片 ID（用于占位符匹配）.
    /// </summary>
    [JsonPropertyName("id")]
    public required string ImageId { get; init; }

    /// <summary>
    /// 图片 URL.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>
    /// 媒体类型.
    /// </summary>
    [JsonPropertyName("mediaType")]
    public string MediaType { get; init; } = "image/jpeg";
}
