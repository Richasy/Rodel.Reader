// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 锚点信息（章节内的子节/书签）.
/// </summary>
public sealed class AnchorInfo
{
    /// <summary>
    /// 锚点 ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 锚点标题.
    /// </summary>
    public required string Title { get; init; }
}
