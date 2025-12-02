// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Models;

/// <summary>
/// 播客摘要信息（用于列表展示）.
/// </summary>
public sealed class PodcastSummary
{
    /// <summary>
    /// iTunes ID.
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
    /// 简介.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// RSS Feed URL.
    /// </summary>
    public string? FeedUrl { get; init; }

    /// <summary>
    /// 艺术家/作者.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// iTunes 页面 URL.
    /// </summary>
    public string? ITunesUrl { get; init; }
}
