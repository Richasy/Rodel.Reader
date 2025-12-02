// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Exceptions;

/// <summary>
/// 空查询异常.
/// </summary>
public sealed class EmptyQueryException : ZLibraryException
{
    /// <summary>
    /// 初始化 <see cref="EmptyQueryException"/> 类的新实例.
    /// </summary>
    public EmptyQueryException()
        : base("Search query cannot be empty.")
    {
    }

    /// <summary>
    /// 初始化 <see cref="EmptyQueryException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public EmptyQueryException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="EmptyQueryException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public EmptyQueryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
