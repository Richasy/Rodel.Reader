// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.FeedParser;

/// <summary>
/// Feed 链接关系类型.
/// </summary>
public enum FeedLinkType
{
    /// <summary>
    /// 替代链接（如网页版本）.
    /// </summary>
    Alternate,

    /// <summary>
    /// 附件（如播客音频）.
    /// </summary>
    Enclosure,

    /// <summary>
    /// 相关链接.
    /// </summary>
    Related,

    /// <summary>
    /// 自身链接.
    /// </summary>
    Self,

    /// <summary>
    /// 来源链接.
    /// </summary>
    Source,

    /// <summary>
    /// 评论链接.
    /// </summary>
    Comments,

    /// <summary>
    /// 永久链接（GUID）.
    /// </summary>
    Permalink,

    /// <summary>
    /// 第一页链接（RFC 5005）.
    /// </summary>
    First,

    /// <summary>
    /// 上一页链接（RFC 5005）.
    /// </summary>
    Previous,

    /// <summary>
    /// 下一页链接（RFC 5005）.
    /// </summary>
    Next,

    /// <summary>
    /// 最后一页链接（RFC 5005）.
    /// </summary>
    Last,

    /// <summary>
    /// 当前归档链接（RFC 5005）.
    /// </summary>
    CurrentArchive,

    /// <summary>
    /// 上一个归档链接（RFC 5005）.
    /// </summary>
    PreviousArchive,

    /// <summary>
    /// 下一个归档链接（RFC 5005）.
    /// </summary>
    NextArchive,

    /// <summary>
    /// 其他类型.
    /// </summary>
    Other,
}
