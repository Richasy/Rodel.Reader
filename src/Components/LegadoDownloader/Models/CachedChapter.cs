// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Models;

/// <summary>
/// 缓存的章节信息.
/// </summary>
public sealed class CachedChapter
{
    /// <summary>
    /// 章节索引.
    /// </summary>
    [JsonPropertyName("index")]
    public required int ChapterIndex { get; init; }

    /// <summary>
    /// 章节链接.
    /// </summary>
    [JsonPropertyName("url")]
    public required string ChapterUrl { get; init; }

    /// <summary>
    /// 章节标题.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>
    /// 是否为卷标题.
    /// </summary>
    [JsonPropertyName("isVolume")]
    public bool IsVolume { get; init; }

    /// <summary>
    /// 章节状态.
    /// </summary>
    [JsonPropertyName("status")]
    public ChapterStatus Status { get; set; }

    /// <summary>
    /// HTML 内容.
    /// </summary>
    [JsonPropertyName("html")]
    public string? HtmlContent { get; set; }

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
