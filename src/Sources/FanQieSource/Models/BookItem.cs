// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 书籍基本信息（搜索结果）.
/// </summary>
public sealed record BookItem
{
    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public required string BookId { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// 简介.
    /// </summary>
    public string? Abstract { get; init; }

    /// <summary>
    /// 封面 URL.
    /// </summary>
    public string? CoverUrl { get; init; }

    /// <summary>
    /// 分类.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// 评分.
    /// </summary>
    public string? Score { get; init; }

    /// <summary>
    /// 连载状态.
    /// </summary>
    public BookCreationStatus CreationStatus { get; init; }
}
