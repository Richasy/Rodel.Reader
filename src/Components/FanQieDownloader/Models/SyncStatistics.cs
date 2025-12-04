// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 同步统计.
/// </summary>
public sealed record SyncStatistics
{
    /// <summary>
    /// 总章节数.
    /// </summary>
    public int TotalChapters { get; init; }

    /// <summary>
    /// 新下载章节数.
    /// </summary>
    public int NewlyDownloaded { get; init; }

    /// <summary>
    /// 复用章节数（从现有 EPUB）.
    /// </summary>
    public int Reused { get; init; }

    /// <summary>
    /// 失败章节数.
    /// </summary>
    public int Failed { get; init; }

    /// <summary>
    /// 从缓存恢复的章节数.
    /// </summary>
    public int RestoredFromCache { get; init; }

    /// <summary>
    /// 下载的图片数.
    /// </summary>
    public int ImagesDownloaded { get; init; }

    /// <summary>
    /// 锁定/付费章节数.
    /// </summary>
    public int LockedChapters { get; init; }

    /// <summary>
    /// 总耗时.
    /// </summary>
    public TimeSpan Duration { get; init; }
}
