// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 书籍详情.
/// </summary>
public sealed record BookDetail
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
    /// 作者 ID.
    /// </summary>
    public string? AuthorId { get; init; }

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
    /// 标签列表.
    /// </summary>
    public IReadOnlyList<string>? Tags { get; init; }

    /// <summary>
    /// 字数.
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// 章节数.
    /// </summary>
    public int ChapterCount { get; init; }

    /// <summary>
    /// 连载状态.
    /// </summary>
    public BookCreationStatus CreationStatus { get; init; }

    /// <summary>
    /// 性别分类.
    /// </summary>
    public BookGender Gender { get; init; }

    /// <summary>
    /// 最后更新时间.
    /// </summary>
    public DateTimeOffset? LastUpdateTime { get; init; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTimeOffset? CreateTime { get; init; }

    /// <summary>
    /// 评分.
    /// </summary>
    public string? Score { get; init; }
}
