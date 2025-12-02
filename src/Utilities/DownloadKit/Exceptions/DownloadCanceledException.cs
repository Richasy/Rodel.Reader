// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit;

/// <summary>
/// 下载已取消异常.
/// </summary>
public class DownloadCanceledException : DownloadException
{
    /// <summary>
    /// 初始化 <see cref="DownloadCanceledException"/> 类的新实例.
    /// </summary>
    public DownloadCanceledException()
        : base("下载已取消。")
    {
    }

    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="DownloadCanceledException"/> 类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    public DownloadCanceledException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化 <see cref="DownloadCanceledException"/> 类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    /// <param name="innerException">导致当前异常的异常.</param>
    public DownloadCanceledException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 使用指定的 URI 初始化 <see cref="DownloadCanceledException"/> 类的新实例.
    /// </summary>
    /// <param name="requestUri">被取消请求的 URI.</param>
    /// <param name="bytesDownloaded">取消时已下载的字节数.</param>
    public DownloadCanceledException(Uri? requestUri, long bytesDownloaded)
        : base($"下载已取消。已下载 {bytesDownloaded} 字节。")
    {
        RequestUri = requestUri;
        BytesDownloaded = bytesDownloaded;
    }

    /// <summary>
    /// 获取请求的 URI.
    /// </summary>
    public new Uri? RequestUri { get; }

    /// <summary>
    /// 获取取消时已下载的字节数.
    /// </summary>
    public long BytesDownloaded { get; }
}
