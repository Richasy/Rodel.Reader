// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Plugin.Abstractions.Scrapers;

/// <summary>
/// 刮削书籍信息.
/// </summary>
public sealed record ScrapedBook
{
    /// <summary>
    /// 书籍 Id.
    /// 在同一刮削器内应唯一.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 刮削器标识.
    /// 用于标识此数据来源于哪个刮削器.
    /// </summary>
    public required string ScraperId { get; init; }

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
    /// 自定义扩展数据.
    /// 插件可存储特定于该刮削器的额外数据.
    /// </summary>
    public IReadOnlyDictionary<string, string>? ExtendedData { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id, ScraperId);

    /// <inheritdoc/>
    public bool Equals(ScrapedBook? other)
        => other is not null
           && Id == other.Id
           && ScraperId.Equals(other.ScraperId, StringComparison.Ordinal);
}
