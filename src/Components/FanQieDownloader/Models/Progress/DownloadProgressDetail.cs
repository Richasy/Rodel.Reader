// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 下载进度详情.
/// </summary>
public sealed record DownloadProgressDetail
{
    /// <summary>
    /// 已完成数.
    /// </summary>
    public int Completed { get; init; }

    /// <summary>
    /// 总数.
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// 失败数.
    /// </summary>
    public int Failed { get; init; }

    /// <summary>
    /// 跳过数（已存在）.
    /// </summary>
    public int Skipped { get; init; }

    /// <summary>
    /// 当前章节标题.
    /// </summary>
    public string? CurrentChapter { get; init; }

    /// <summary>
    /// 进度百分比（0-100）.
    /// </summary>
    public double Percentage => Total > 0
        ? (Completed + Skipped) * 100.0 / Total
        : 0;
}
