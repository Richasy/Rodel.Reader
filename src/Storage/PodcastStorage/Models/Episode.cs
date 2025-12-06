// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 播客单集.
/// </summary>
public sealed class Episode
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 所属播客 ID.
    /// </summary>
    public string PodcastId { get; set; } = string.Empty;

    /// <summary>
    /// 标题.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 描述.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 摘要.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 媒体 URL.
    /// </summary>
    public string MediaUrl { get; set; } = string.Empty;

    /// <summary>
    /// 媒体类型.
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// 媒体大小（字节）.
    /// </summary>
    public long? MediaSize { get; set; }

    /// <summary>
    /// 时长（秒）.
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 封面 URL.
    /// </summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// 网页链接.
    /// </summary>
    public string? WebUrl { get; set; }

    /// <summary>
    /// 发布日期.
    /// </summary>
    public DateTimeOffset? PublishDate { get; set; }

    /// <summary>
    /// 季.
    /// </summary>
    public int? Season { get; set; }

    /// <summary>
    /// 集数.
    /// </summary>
    public int? EpisodeNumber { get; set; }

    /// <summary>
    /// 内部排序索引.
    /// </summary>
    public int? SortIndex { get; set; }

    /// <summary>
    /// 缓存时间.
    /// </summary>
    public DateTimeOffset CachedAt { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Episode episode && Id == episode.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
