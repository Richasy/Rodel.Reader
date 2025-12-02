// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models.Enums;

/// <summary>
/// OPDS 链接关系类型.
/// </summary>
public enum OpdsLinkRelation
{
    /// <summary>
    /// 替代表示.
    /// </summary>
    Alternate,

    /// <summary>
    /// 自身链接.
    /// </summary>
    Self,

    /// <summary>
    /// 起始/根目录.
    /// </summary>
    Start,

    /// <summary>
    /// 子部分/子目录.
    /// </summary>
    Subsection,

    /// <summary>
    /// 相关资源.
    /// </summary>
    Related,

    /// <summary>
    /// 搜索.
    /// </summary>
    Search,

    /// <summary>
    /// 分面导航.
    /// </summary>
    Facet,

    /// <summary>
    /// 封面图片.
    /// </summary>
    Image,

    /// <summary>
    /// 缩略图.
    /// </summary>
    Thumbnail,

    /// <summary>
    /// 获取链接（通用）.
    /// </summary>
    Acquisition,

    /// <summary>
    /// 开放获取.
    /// </summary>
    AcquisitionOpenAccess,

    /// <summary>
    /// 借阅.
    /// </summary>
    AcquisitionBorrow,

    /// <summary>
    /// 购买.
    /// </summary>
    AcquisitionBuy,

    /// <summary>
    /// 样本.
    /// </summary>
    AcquisitionSample,

    /// <summary>
    /// 订阅.
    /// </summary>
    AcquisitionSubscribe,

    /// <summary>
    /// 下一页.
    /// </summary>
    Next,

    /// <summary>
    /// 上一页.
    /// </summary>
    Previous,

    /// <summary>
    /// 第一页.
    /// </summary>
    First,

    /// <summary>
    /// 最后一页.
    /// </summary>
    Last,

    /// <summary>
    /// 爬取链接.
    /// </summary>
    Crawlable,

    /// <summary>
    /// 排行榜/流行.
    /// </summary>
    Popular,

    /// <summary>
    /// 推荐.
    /// </summary>
    Featured,

    /// <summary>
    /// 新书.
    /// </summary>
    New,

    /// <summary>
    /// 书架.
    /// </summary>
    Shelf,

    /// <summary>
    /// 未知/其他类型.
    /// </summary>
    Other,
}
