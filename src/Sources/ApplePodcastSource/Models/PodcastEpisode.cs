// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Models;

/// <summary>
/// 播客单集信息.
/// </summary>
public sealed class PodcastEpisode
{
    /// <summary>
    /// 单集 ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 单集标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 单集描述.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 音频 URL.
    /// </summary>
    public string? AudioUrl { get; init; }

    /// <summary>
    /// 音频 MIME 类型.
    /// </summary>
    public string? AudioMimeType { get; init; }

    /// <summary>
    /// 音频时长（秒）.
    /// </summary>
    public int? DurationInSeconds { get; init; }

    /// <summary>
    /// 音频文件大小（字节）.
    /// </summary>
    public long? FileSizeInBytes { get; init; }

    /// <summary>
    /// 发布日期.
    /// </summary>
    public DateTimeOffset? PublishedDate { get; init; }

    /// <summary>
    /// 季数（如果有）.
    /// </summary>
    public int? Season { get; init; }

    /// <summary>
    /// 集数（如果有）.
    /// </summary>
    public int? Episode { get; init; }

    /// <summary>
    /// 单集封面（如果与播客封面不同）.
    /// </summary>
    public string? Cover { get; init; }

    /// <summary>
    /// 单集类型（full, trailer, bonus）.
    /// </summary>
    public string? EpisodeType { get; init; }

    /// <summary>
    /// 是否显式内容.
    /// </summary>
    public bool IsExplicit { get; init; }
}
