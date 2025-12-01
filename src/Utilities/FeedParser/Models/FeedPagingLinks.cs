// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 分页链接（RFC 5005）.
/// </summary>
/// <remarks>
/// 支持 Feed 归档和分页遍历.
/// 参考: <see href="https://tools.ietf.org/html/rfc5005"/>.
/// </remarks>
public sealed record FeedPagingLinks
{
    /// <summary>
    /// 第一页链接.
    /// </summary>
    public Uri? First { get; init; }

    /// <summary>
    /// 上一页链接.
    /// </summary>
    public Uri? Previous { get; init; }

    /// <summary>
    /// 下一页链接.
    /// </summary>
    public Uri? Next { get; init; }

    /// <summary>
    /// 最后一页链接.
    /// </summary>
    public Uri? Last { get; init; }

    /// <summary>
    /// 当前 Feed 链接（self）.
    /// </summary>
    public Uri? Current { get; init; }

    /// <summary>
    /// 获取一个值，指示是否有分页信息.
    /// </summary>
    public bool HasPaging => First != null || Previous != null || Next != null || Last != null;

    /// <summary>
    /// 获取一个值，指示是否有下一页.
    /// </summary>
    public bool HasNext => Next != null;

    /// <summary>
    /// 获取一个值，指示是否有上一页.
    /// </summary>
    public bool HasPrevious => Previous != null;
}
