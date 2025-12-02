// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit.Models;

/// <summary>
/// 远程文件信息.
/// </summary>
public sealed class RemoteFileInfo
{
    /// <summary>
    /// 获取或设置文件大小（字节）.
    /// </summary>
    /// <remarks>
    /// 如果服务器未返回 Content-Length，则为 null.
    /// </remarks>
    public long? ContentLength { get; set; }

    /// <summary>
    /// 获取或设置内容类型.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 获取或设置最后修改时间.
    /// </summary>
    public DateTimeOffset? LastModified { get; set; }

    /// <summary>
    /// 获取或设置 ETag.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// 获取或设置是否支持断点续传.
    /// </summary>
    public bool AcceptRanges { get; set; }

    /// <summary>
    /// 获取或设置文件名（从 Content-Disposition 头中提取）.
    /// </summary>
    public string? FileName { get; set; }
}
