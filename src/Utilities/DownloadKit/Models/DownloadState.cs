// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit.Models;

/// <summary>
/// 下载状态枚举.
/// </summary>
public enum DownloadState
{
    /// <summary>
    /// 等待中，任务已创建但尚未开始.
    /// </summary>
    Pending,

    /// <summary>
    /// 下载中.
    /// </summary>
    Downloading,

    /// <summary>
    /// 已完成.
    /// </summary>
    Completed,

    /// <summary>
    /// 已取消.
    /// </summary>
    Canceled,

    /// <summary>
    /// 失败.
    /// </summary>
    Failed,
}
