// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser.Internal;

/// <summary>
/// Atom 元素名称定义.
/// </summary>
internal static class AtomElementNames
{
    // 根元素
    public const string Feed = "feed";
    public const string Entry = "entry";

    // 通用元素
    public const string Id = "id";
    public const string Title = "title";
    public const string Updated = "updated";
    public const string Published = "published";
    public const string Link = "link";
    public const string Author = "author";
    public const string Contributor = "contributor";
    public const string Category = "category";
    public const string Rights = "rights";

    // Feed 特有元素
    public const string Subtitle = "subtitle";
    public const string Generator = "generator";
    public const string Icon = "icon";
    public const string Logo = "logo";

    // Entry 特有元素
    public const string Summary = "summary";
    public const string Content = "content";
    public const string Source = "source";

    // 人员子元素
    public const string Name = "name";
    public const string Email = "email";
    public const string Uri = "uri";

    // 属性
    public const string Href = "href";
    public const string Rel = "rel";
    public const string Type = "type";
    public const string HrefLang = "hreflang";
    public const string Length = "length";
    public const string Term = "term";
    public const string Scheme = "scheme";
    public const string Label = "label";
    public const string Src = "src";

    // 链接关系类型
    public const string RelAlternate = "alternate";
    public const string RelSelf = "self";
    public const string RelEnclosure = "enclosure";
    public const string RelRelated = "related";
    public const string RelVia = "via";

    // RFC 5005 分页链接关系类型
    public const string RelFirst = "first";
    public const string RelPrevious = "previous";
    public const string RelPrev = "prev"; // 别名
    public const string RelNext = "next";
    public const string RelLast = "last";
    public const string RelCurrentArchive = "current";
    public const string RelPreviousArchive = "prev-archive";
    public const string RelNextArchive = "next-archive";
}
