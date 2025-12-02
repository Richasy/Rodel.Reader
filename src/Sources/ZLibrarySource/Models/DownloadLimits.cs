// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 下载限制信息.
/// </summary>
public sealed record DownloadLimits
{
    /// <summary>
    /// 获取今日已下载次数.
    /// </summary>
    public int DailyUsed { get; init; }

    /// <summary>
    /// 获取每日允许下载次数.
    /// </summary>
    public int DailyAllowed { get; init; }

    /// <summary>
    /// 获取今日剩余下载次数.
    /// </summary>
    public int DailyRemaining => DailyAllowed - DailyUsed;

    /// <summary>
    /// 获取下载次数重置时间.
    /// </summary>
    public string? ResetTime { get; init; }
}
