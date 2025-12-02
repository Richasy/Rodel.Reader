// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit.Models;

/// <summary>
/// 下载结果.
/// </summary>
public sealed class DownloadResult
{
    /// <summary>
    /// 初始化 <see cref="DownloadResult"/> 类的新实例.
    /// </summary>
    private DownloadResult()
    {
    }

    /// <summary>
    /// 获取下载是否成功.
    /// </summary>
    public bool IsSuccess { get; private init; }

    /// <summary>
    /// 获取下载文件的目标路径.
    /// </summary>
    public string FilePath { get; private init; } = string.Empty;

    /// <summary>
    /// 获取下载的总字节数.
    /// </summary>
    public long TotalBytes { get; private init; }

    /// <summary>
    /// 获取下载耗时.
    /// </summary>
    public TimeSpan ElapsedTime { get; private init; }

    /// <summary>
    /// 获取最终下载状态.
    /// </summary>
    public DownloadState State { get; private init; }

    /// <summary>
    /// 获取下载过程中发生的异常.
    /// </summary>
    /// <remarks>
    /// 如果下载成功，则为 null.
    /// </remarks>
    public Exception? Error { get; private init; }

    /// <summary>
    /// 获取平均下载速度（字节/秒）.
    /// </summary>
    public double AverageSpeed => ElapsedTime.TotalSeconds > 0
        ? TotalBytes / ElapsedTime.TotalSeconds
        : 0;

    /// <summary>
    /// 创建成功的下载结果.
    /// </summary>
    /// <param name="filePath">文件路径.</param>
    /// <param name="totalBytes">总字节数.</param>
    /// <param name="elapsedTime">耗时.</param>
    /// <returns>成功的下载结果.</returns>
    public static DownloadResult Success(string filePath, long totalBytes, TimeSpan elapsedTime)
    {
        return new DownloadResult
        {
            IsSuccess = true,
            FilePath = filePath,
            TotalBytes = totalBytes,
            ElapsedTime = elapsedTime,
            State = DownloadState.Completed,
        };
    }

    /// <summary>
    /// 创建失败的下载结果.
    /// </summary>
    /// <param name="filePath">文件路径.</param>
    /// <param name="error">异常信息.</param>
    /// <param name="elapsedTime">耗时.</param>
    /// <param name="bytesDownloaded">已下载的字节数.</param>
    /// <returns>失败的下载结果.</returns>
    public static DownloadResult Failure(string filePath, Exception error, TimeSpan elapsedTime, long bytesDownloaded = 0)
    {
        return new DownloadResult
        {
            IsSuccess = false,
            FilePath = filePath,
            TotalBytes = bytesDownloaded,
            ElapsedTime = elapsedTime,
            State = DownloadState.Failed,
            Error = error,
        };
    }

    /// <summary>
    /// 创建已取消的下载结果.
    /// </summary>
    /// <param name="filePath">文件路径.</param>
    /// <param name="elapsedTime">耗时.</param>
    /// <param name="bytesDownloaded">已下载的字节数.</param>
    /// <returns>已取消的下载结果.</returns>
    public static DownloadResult Canceled(string filePath, TimeSpan elapsedTime, long bytesDownloaded = 0)
    {
        return new DownloadResult
        {
            IsSuccess = false,
            FilePath = filePath,
            TotalBytes = bytesDownloaded,
            ElapsedTime = elapsedTime,
            State = DownloadState.Canceled,
        };
    }
}
