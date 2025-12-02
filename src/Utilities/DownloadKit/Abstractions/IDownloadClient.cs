// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit;

/// <summary>
/// 下载客户端接口.
/// </summary>
public interface IDownloadClient : IDisposable
{
    /// <summary>
    /// 异步下载文件.
    /// </summary>
    /// <param name="url">下载地址.</param>
    /// <param name="destinationPath">目标文件路径.</param>
    /// <param name="options">下载选项.</param>
    /// <param name="progress">进度报告器.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>下载结果.</returns>
    Task<DownloadResult> DownloadAsync(
        string url,
        string destinationPath,
        DownloadOptions? options = null,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步下载文件.
    /// </summary>
    /// <param name="uri">下载 URI.</param>
    /// <param name="destinationPath">目标文件路径.</param>
    /// <param name="options">下载选项.</param>
    /// <param name="progress">进度报告器.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>下载结果.</returns>
    Task<DownloadResult> DownloadAsync(
        Uri uri,
        string destinationPath,
        DownloadOptions? options = null,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步获取远程文件信息（不下载内容）.
    /// </summary>
    /// <param name="url">文件地址.</param>
    /// <param name="options">下载选项.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>文件信息，包含大小等元数据.</returns>
    Task<RemoteFileInfo> GetFileInfoAsync(
        string url,
        DownloadOptions? options = null,
        CancellationToken cancellationToken = default);
}
