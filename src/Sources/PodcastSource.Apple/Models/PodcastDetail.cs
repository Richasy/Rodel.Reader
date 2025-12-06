// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Podcast.Apple.Models;

/// <summary>
/// 播客详情.
/// </summary>
public sealed record PodcastDetail
{
    /// <summary>
    /// 播客 ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 播客名称.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 封面图片 URL.
    /// </summary>
    public string? Cover { get; init; }

    /// <summary>
    /// 详细描述.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 播客官网.
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// RSS Feed URL.
    /// </summary>
    public string? FeedUrl { get; init; }

    /// <summary>
    /// 分类 ID 列表.
    /// </summary>
    public IReadOnlyList<string> CategoryIds { get; init; } = [];

    /// <summary>
    /// 单集列表.
    /// </summary>
    public IReadOnlyList<PodcastEpisode> Episodes { get; init; } = [];
}
