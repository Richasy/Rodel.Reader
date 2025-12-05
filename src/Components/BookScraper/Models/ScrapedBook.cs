// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.BookScraper.Models;

/// <summary>
/// 刮削书籍信息.
/// </summary>
public sealed record ScrapedBook
{
    /// <summary>
    /// 书籍 Id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 评分（1-5分制）.
    /// </summary>
    public int Rating { get; init; }

    /// <summary>
    /// 副标题.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 封面链接.
    /// </summary>
    public string? Cover { get; init; }

    /// <summary>
    /// 网页链接.
    /// </summary>
    public string? WebLink { get; init; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// 译者.
    /// </summary>
    public string? Translator { get; init; }

    /// <summary>
    /// 出版商.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// 页数.
    /// </summary>
    public int? PageCount { get; init; }

    /// <summary>
    /// 出版日期.
    /// </summary>
    public string? PublishDate { get; init; }

    /// <summary>
    /// ISBN编号.
    /// </summary>
    public string? ISBN { get; init; }

    /// <summary>
    /// 分类.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// 数据来源.
    /// </summary>
    public ScraperType Source { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id, Source);

    /// <inheritdoc/>
    public bool Equals(ScrapedBook? other)
        => other is not null && Id == other.Id && Source == other.Source;
}
