// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 同步阶段.
/// </summary>
public enum SyncPhase
{
    /// <summary>
    /// 分析现有 EPUB.
    /// </summary>
    Analyzing,

    /// <summary>
    /// 获取在线目录.
    /// </summary>
    FetchingToc,

    /// <summary>
    /// 检查缓存.
    /// </summary>
    CheckingCache,

    /// <summary>
    /// 下载章节.
    /// </summary>
    DownloadingChapters,

    /// <summary>
    /// 下载图片.
    /// </summary>
    DownloadingImages,

    /// <summary>
    /// 生成 EPUB.
    /// </summary>
    GeneratingEpub,

    /// <summary>
    /// 清理缓存.
    /// </summary>
    CleaningUp,

    /// <summary>
    /// 已完成.
    /// </summary>
    Completed,

    /// <summary>
    /// 已取消.
    /// </summary>
    Cancelled,

    /// <summary>
    /// 失败.
    /// </summary>
    Failed,
}
