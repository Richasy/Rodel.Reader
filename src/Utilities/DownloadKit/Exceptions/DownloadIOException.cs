// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit;

/// <summary>
/// 下载 IO 异常.
/// </summary>
public class DownloadIOException : DownloadException
{
    /// <summary>
    /// 初始化 <see cref="DownloadIOException"/> 类的新实例.
    /// </summary>
    public DownloadIOException()
    {
    }

    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="DownloadIOException"/> 类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    public DownloadIOException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化 <see cref="DownloadIOException"/> 类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    /// <param name="innerException">导致当前异常的异常.</param>
    public DownloadIOException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 使用指定的文件路径和内部异常初始化 <see cref="DownloadIOException"/> 类的新实例.
    /// </summary>
    /// <param name="filePath">出错的文件路径.</param>
    /// <param name="operation">执行的操作.</param>
    /// <param name="innerException">导致当前异常的异常.</param>
    public DownloadIOException(string filePath, string operation, Exception innerException)
        : base($"文件操作失败：{operation}，路径：{filePath}", innerException)
    {
        FilePath = filePath;
        Operation = operation;
    }

    /// <summary>
    /// 获取出错的文件路径.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// 获取执行的操作.
    /// </summary>
    public string? Operation { get; }
}
