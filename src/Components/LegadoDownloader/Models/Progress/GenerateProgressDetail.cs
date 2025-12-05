// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Models;

/// <summary>
/// 生成进度详情.
/// </summary>
public sealed record GenerateProgressDetail
{
    /// <summary>
    /// 当前步骤描述.
    /// </summary>
    public string? Step { get; init; }

    /// <summary>
    /// 已处理章节数.
    /// </summary>
    public int ProcessedChapters { get; init; }

    /// <summary>
    /// 总章节数.
    /// </summary>
    public int TotalChapters { get; init; }

    /// <summary>
    /// 进度百分比（0-100）.
    /// </summary>
    public double Percentage => TotalChapters > 0
        ? ProcessedChapters * 100.0 / TotalChapters
        : 0;
}
