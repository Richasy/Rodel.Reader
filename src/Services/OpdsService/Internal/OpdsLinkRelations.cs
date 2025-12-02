// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Internal;

/// <summary>
/// OPDS 链接关系类型定义.
/// </summary>
internal static class OpdsLinkRelations
{
    // 标准 Atom 关系
    public const string Alternate = "alternate";
    public const string Self = "self";
    public const string Related = "related";
    public const string Enclosure = "enclosure";
    public const string Via = "via";

    // RFC 5005 分页关系
    public const string First = "first";
    public const string Previous = "previous";
    public const string Prev = "prev"; // 别名
    public const string Next = "next";
    public const string Last = "last";

    // OPDS 导航关系
    public const string Start = "start";
    public const string Subsection = "subsection";
    public const string Search = "search";

    // OPDS 获取关系
    public const string Acquisition = "http://opds-spec.org/acquisition";
    public const string AcquisitionOpenAccess = "http://opds-spec.org/acquisition/open-access";
    public const string AcquisitionBorrow = "http://opds-spec.org/acquisition/borrow";
    public const string AcquisitionBuy = "http://opds-spec.org/acquisition/buy";
    public const string AcquisitionSample = "http://opds-spec.org/acquisition/sample";
    public const string AcquisitionSubscribe = "http://opds-spec.org/acquisition/subscribe";

    // OPDS 图片关系
    public const string Image = "http://opds-spec.org/image";
    public const string Thumbnail = "http://opds-spec.org/image/thumbnail";

    // OPDS 分面关系
    public const string Facet = "http://opds-spec.org/facet";

    // OPDS 特殊目录关系
    public const string Crawlable = "http://opds-spec.org/crawlable";
    public const string Popular = "http://opds-spec.org/sort/popular";
    public const string Featured = "http://opds-spec.org/featured";
    public const string New = "http://opds-spec.org/sort/new";
    public const string Shelf = "http://opds-spec.org/shelf";
}
