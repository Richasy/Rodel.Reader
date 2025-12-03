// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 章节状态.
/// </summary>
public enum ChapterStatus
{
    /// <summary>
    /// 待下载.
    /// </summary>
    Pending,

    /// <summary>
    /// 下载成功.
    /// </summary>
    Downloaded,

    /// <summary>
    /// 下载失败（网络原因）.
    /// </summary>
    Failed,

    /// <summary>
    /// 章节被锁定/需付费.
    /// </summary>
    Locked,
}
