// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 目录（Feed）.
/// </summary>
public sealed record OpdsFeed
{
    /// <summary>
    /// Feed 唯一标识符.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Feed 标题.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// 副标题.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// 更新时间.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// 图标.
    /// </summary>
    public Uri? Icon { get; init; }

    /// <summary>
    /// 条目列表.
    /// </summary>
    public IReadOnlyList<OpdsEntry> Entries { get; init; } = [];

    /// <summary>
    /// 链接列表.
    /// </summary>
    public IReadOnlyList<OpdsLink> Links { get; init; } = [];

    /// <summary>
    /// 分面组列表.
    /// </summary>
    public IReadOnlyList<OpdsFacetGroup> FacetGroups { get; init; } = [];

    /// <summary>
    /// 获取自身链接.
    /// </summary>
    /// <returns>自身链接，如果不存在则返回 null.</returns>
    public OpdsLink? GetSelfLink()
        => Links.FirstOrDefault(l => l.Relation == OpdsLinkRelation.Self);

    /// <summary>
    /// 获取起始链接.
    /// </summary>
    /// <returns>起始链接，如果不存在则返回 null.</returns>
    public OpdsLink? GetStartLink()
        => Links.FirstOrDefault(l => l.Relation == OpdsLinkRelation.Start);

    /// <summary>
    /// 获取下一页链接.
    /// </summary>
    /// <returns>下一页链接，如果不存在则返回 null.</returns>
    public OpdsLink? GetNextLink()
        => Links.FirstOrDefault(l => l.Relation == OpdsLinkRelation.Next);

    /// <summary>
    /// 获取上一页链接.
    /// </summary>
    /// <returns>上一页链接，如果不存在则返回 null.</returns>
    public OpdsLink? GetPreviousLink()
        => Links.FirstOrDefault(l => l.Relation == OpdsLinkRelation.Previous);

    /// <summary>
    /// 获取搜索链接.
    /// </summary>
    /// <returns>搜索链接，如果不存在则返回 null.</returns>
    public OpdsLink? GetSearchLink()
        => Links.FirstOrDefault(l => l.Relation == OpdsLinkRelation.Search);

    /// <summary>
    /// 是否有下一页.
    /// </summary>
    public bool HasNextPage => GetNextLink() != null;

    /// <summary>
    /// 是否有上一页.
    /// </summary>
    public bool HasPreviousPage => GetPreviousLink() != null;

    /// <summary>
    /// 是否支持搜索.
    /// </summary>
    public bool SupportsSearch => GetSearchLink() != null;

    /// <summary>
    /// 获取所有导航条目.
    /// </summary>
    /// <returns>导航条目列表.</returns>
    public IEnumerable<OpdsEntry> GetNavigationEntries()
        => Entries.Where(e => e.IsNavigationEntry);

    /// <summary>
    /// 获取所有书籍条目.
    /// </summary>
    /// <returns>书籍条目列表.</returns>
    public IEnumerable<OpdsEntry> GetBookEntries()
        => Entries.Where(e => e.IsBookEntry);
}
