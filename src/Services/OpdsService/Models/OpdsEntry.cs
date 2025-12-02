// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 条目（书籍或导航项）.
/// </summary>
public sealed record OpdsEntry
{
    /// <summary>
    /// 条目唯一标识符.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// 条目标题.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// 摘要描述.
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// 完整内容（可能是 HTML）.
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// 更新时间.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    /// 语言.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// 出版商.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// 标识符（如 ISBN）.
    /// </summary>
    public string? Identifier { get; init; }

    /// <summary>
    /// 作者列表.
    /// </summary>
    public IReadOnlyList<OpdsAuthor> Authors { get; init; } = [];

    /// <summary>
    /// 分类列表.
    /// </summary>
    public IReadOnlyList<OpdsCategory> Categories { get; init; } = [];

    /// <summary>
    /// 链接列表.
    /// </summary>
    public IReadOnlyList<OpdsLink> Links { get; init; } = [];

    /// <summary>
    /// 图片列表（封面、缩略图）.
    /// </summary>
    public IReadOnlyList<OpdsImage> Images { get; init; } = [];

    /// <summary>
    /// 获取链接列表（下载链接）.
    /// </summary>
    public IReadOnlyList<OpdsAcquisition> Acquisitions { get; init; } = [];

    /// <summary>
    /// 是否为导航条目（不含获取链接）.
    /// </summary>
    public bool IsNavigationEntry => Acquisitions.Count == 0;

    /// <summary>
    /// 是否为书籍条目（含获取链接）.
    /// </summary>
    public bool IsBookEntry => Acquisitions.Count > 0;

    /// <summary>
    /// 获取导航链接（如果是导航条目）.
    /// </summary>
    /// <returns>导航链接，如果不存在则返回 null.</returns>
    public OpdsLink? GetNavigationLink()
        => Links.FirstOrDefault(l =>
            l.Relation is OpdsLinkRelation.Subsection or OpdsLinkRelation.Alternate &&
            l.MediaType?.Contains("application/atom+xml", StringComparison.OrdinalIgnoreCase) == true);

    /// <summary>
    /// 获取免费下载的获取链接.
    /// </summary>
    /// <returns>免费获取链接，如果不存在则返回 null.</returns>
    public OpdsAcquisition? GetOpenAccessAcquisition()
        => Acquisitions.FirstOrDefault(a => a.Type == AcquisitionType.OpenAccess)
           ?? Acquisitions.FirstOrDefault(a => a.Type == AcquisitionType.Generic);

    /// <summary>
    /// 根据媒体类型获取获取链接.
    /// </summary>
    /// <param name="mediaType">媒体类型（如 application/epub+zip）.</param>
    /// <returns>匹配的获取链接，如果不存在则返回 null.</returns>
    public OpdsAcquisition? GetAcquisitionByMediaType(string mediaType)
        => Acquisitions.FirstOrDefault(a =>
            string.Equals(a.MediaType, mediaType, StringComparison.OrdinalIgnoreCase) ||
            a.IndirectMediaTypes.Any(m => string.Equals(m, mediaType, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// 获取所有可下载的获取链接（按优先级排序）.
    /// </summary>
    /// <returns>可下载的获取链接列表.</returns>
    public IEnumerable<OpdsAcquisition> GetDownloadableAcquisitions()
        => Acquisitions
            .Where(a => a.Type is AcquisitionType.OpenAccess or AcquisitionType.Generic)
            .OrderByDescending(a => a.Type == AcquisitionType.OpenAccess);

    /// <summary>
    /// 获取封面图片.
    /// </summary>
    /// <returns>封面图片，如果不存在则返回 null.</returns>
    public OpdsImage? GetCoverImage()
        => Images.FirstOrDefault(i => i.Relation == OpdsLinkRelation.Image);

    /// <summary>
    /// 获取缩略图.
    /// </summary>
    /// <returns>缩略图，如果不存在则返回 null.</returns>
    public OpdsImage? GetThumbnail()
        => Images.FirstOrDefault(i => i.Relation == OpdsLinkRelation.Thumbnail)
           ?? GetCoverImage();
}
