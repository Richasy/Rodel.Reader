// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 下载信息.
/// </summary>
public sealed class DownloadInfo
{
    /// <summary>
    /// 获取或设置下载链接.
    /// </summary>
    public string DownloadLink { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置文件名（不含扩展名）.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置文件扩展名.
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置作者.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 获取完整文件名（含扩展名）.
    /// </summary>
    public string FullFileName => string.IsNullOrEmpty(Extension)
        ? FileName
        : $"{FileName}.{Extension}";
}
