// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 章节目录项.
/// </summary>
public sealed record ChapterItem
{
    /// <summary>
    /// 章节 ID（item_id）.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// 章节标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 章节序号.
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// 所属卷名称.
    /// </summary>
    public string? VolumeName { get; init; }

    /// <summary>
    /// 是否锁定.
    /// </summary>
    public bool IsLocked { get; init; }

    /// <summary>
    /// 是否需要付费.
    /// </summary>
    public bool NeedPay { get; init; }

    /// <summary>
    /// 首次发布时间.
    /// </summary>
    public DateTimeOffset? FirstPassTime { get; init; }
}
