// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models.Internal;

/// <summary>
/// 下载链接 API 响应.
/// </summary>
internal sealed class DownloadApiResponse
{
    /// <summary>
    /// 获取或设置是否成功.
    /// </summary>
    [JsonPropertyName("success")]
    public int Success { get; set; }

    /// <summary>
    /// 获取或设置文件信息.
    /// </summary>
    [JsonPropertyName("file")]
    public DownloadApiFile? File { get; set; }
}

/// <summary>
/// 下载文件信息.
/// </summary>
internal sealed class DownloadApiFile
{
    /// <summary>
    /// 获取或设置下载链接.
    /// </summary>
    [JsonPropertyName("downloadLink")]
    public string? DownloadLink { get; set; }

    /// <summary>
    /// 获取或设置文件描述（通常是书名）.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 获取或设置作者.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// 获取或设置文件扩展名.
    /// </summary>
    [JsonPropertyName("extension")]
    public string? Extension { get; set; }

    /// <summary>
    /// 获取或设置是否允许下载.
    /// </summary>
    [JsonPropertyName("allowDownload")]
    public bool AllowDownload { get; set; }
}
