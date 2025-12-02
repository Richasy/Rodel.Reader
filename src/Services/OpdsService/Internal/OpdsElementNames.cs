// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Internal;

/// <summary>
/// OPDS 元素名称定义.
/// </summary>
internal static class OpdsElementNames
{
    // Atom 根元素
    public const string Feed = "feed";
    public const string Entry = "entry";

    // Atom 通用元素
    public const string Id = "id";
    public const string Title = "title";
    public const string Updated = "updated";
    public const string Published = "published";
    public const string Link = "link";
    public const string Author = "author";
    public const string Contributor = "contributor";
    public const string Category = "category";
    public const string Rights = "rights";
    public const string Summary = "summary";
    public const string Content = "content";

    // Atom Feed 特有元素
    public const string Subtitle = "subtitle";
    public const string Generator = "generator";
    public const string Icon = "icon";
    public const string Logo = "logo";

    // 人员子元素
    public const string Name = "name";
    public const string Email = "email";
    public const string Uri = "uri";

    // Atom 属性
    public const string Href = "href";
    public const string Rel = "rel";
    public const string Type = "type";
    public const string HrefLang = "hreflang";
    public const string Length = "length";
    public const string Term = "term";
    public const string Scheme = "scheme";
    public const string Label = "label";
    public const string Src = "src";

    // Dublin Core 元素
    public const string DcLanguage = "language";
    public const string DcPublisher = "publisher";
    public const string DcIdentifier = "identifier";
    public const string DcIssued = "issued";

    // OPDS 特有元素
    public const string Price = "price";
    public const string IndirectAcquisition = "indirectAcquisition";

    // OPDS Facet 属性
    public const string FacetGroup = "facetGroup";
    public const string ActiveFacet = "activeFacet";
    public const string Count = "count";

    // OpenSearch 元素
    public const string OpenSearchDescription = "OpenSearchDescription";
    public const string Url = "Url";
    public const string Template = "template";
}
