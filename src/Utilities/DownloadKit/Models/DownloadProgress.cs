// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit.Models;

/// <summary>
/// 下载进度信息.
/// </summary>
public sealed class DownloadProgress
{
    /// <summary>
    /// 初始化 <see cref="DownloadProgress"/> 类的新实例.
    /// </summary>
    public DownloadProgress()
    {
    }

    /// <summary>
    /// 初始化 <see cref="DownloadProgress"/> 类的新实例.
    /// </summary>
    /// <param name="bytesReceived">已接收字节数.</param>
    /// <param name="totalBytes">总字节数.</param>
    /// <param name="bytesPerSecond">下载速度.</param>
    /// <param name="state">下载状态.</param>
    public DownloadProgress(long bytesReceived, long? totalBytes, double bytesPerSecond, DownloadState state)
    {
        BytesReceived = bytesReceived;
        TotalBytes = totalBytes;
        BytesPerSecond = bytesPerSecond;
        State = state;
    }

    /// <summary>
    /// 获取或设置已接收的字节数.
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// 获取或设置总字节数.
    /// </summary>
    /// <remarks>
    /// 如果服务器未返回 Content-Length，则为 null.
    /// </remarks>
    public long? TotalBytes { get; set; }

    /// <summary>
    /// 获取下载完成百分比.
    /// </summary>
    /// <remarks>
    /// 如果总字节数未知，则为 null.
    /// </remarks>
    public double? Percentage => TotalBytes.HasValue && TotalBytes.Value > 0
        ? (double)BytesReceived / TotalBytes.Value * 100
        : null;

    /// <summary>
    /// 获取或设置每秒下载的字节数.
    /// </summary>
    public double BytesPerSecond { get; set; }

    /// <summary>
    /// 获取预计剩余时间.
    /// </summary>
    /// <remarks>
    /// 如果总字节数未知或下载速度为零，则为 null.
    /// </remarks>
    public TimeSpan? EstimatedRemaining
    {
        get
        {
            if (!TotalBytes.HasValue || BytesPerSecond <= 0)
            {
                return null;
            }

            var remainingBytes = TotalBytes.Value - BytesReceived;
            if (remainingBytes <= 0)
            {
                return TimeSpan.Zero;
            }

            var seconds = remainingBytes / BytesPerSecond;
            return TimeSpan.FromSeconds(seconds);
        }
    }

    /// <summary>
    /// 获取或设置当前下载状态.
    /// </summary>
    public DownloadState State { get; set; }

    /// <summary>
    /// 获取格式化的下载速度字符串.
    /// </summary>
    /// <returns>格式化的速度字符串，如 "1.5 MB/s".</returns>
    public string GetFormattedSpeed()
    {
        return BytesPerSecond switch
        {
            >= 1024 * 1024 * 1024 => $"{BytesPerSecond / (1024 * 1024 * 1024):F2} GB/s",
            >= 1024 * 1024 => $"{BytesPerSecond / (1024 * 1024):F2} MB/s",
            >= 1024 => $"{BytesPerSecond / 1024:F2} KB/s",
            _ => $"{BytesPerSecond:F0} B/s",
        };
    }

    /// <summary>
    /// 获取格式化的已下载大小字符串.
    /// </summary>
    /// <returns>格式化的大小字符串.</returns>
    public string GetFormattedBytesReceived()
    {
        return FormatBytes(BytesReceived);
    }

    /// <summary>
    /// 获取格式化的总大小字符串.
    /// </summary>
    /// <returns>格式化的大小字符串，如果总大小未知则返回 "未知".</returns>
    public string GetFormattedTotalBytes()
    {
        return TotalBytes.HasValue ? FormatBytes(TotalBytes.Value) : "未知";
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024 * 1024):F2} GB",
            >= 1024 * 1024 => $"{bytes / (1024.0 * 1024):F2} MB",
            >= 1024 => $"{bytes / 1024.0:F2} KB",
            _ => $"{bytes} B",
        };
    }
}
