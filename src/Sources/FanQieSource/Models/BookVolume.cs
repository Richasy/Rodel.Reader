// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 书籍卷信息.
/// </summary>
public sealed record BookVolume
{
    /// <summary>
    /// 卷索引.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// 卷名称.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 章节列表.
    /// </summary>
    public required IReadOnlyList<ChapterItem> Chapters { get; init; }
}
