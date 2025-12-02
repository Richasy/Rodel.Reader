// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Exceptions;

/// <summary>
/// ZLibrary 异常基类.
/// </summary>
public class ZLibraryException : Exception
{
    /// <summary>
    /// 初始化 <see cref="ZLibraryException"/> 类的新实例.
    /// </summary>
    public ZLibraryException()
    {
    }

    /// <summary>
    /// 初始化 <see cref="ZLibraryException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public ZLibraryException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="ZLibraryException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public ZLibraryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
