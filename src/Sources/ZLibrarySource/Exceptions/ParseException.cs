// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Exceptions;

/// <summary>
/// HTML 解析异常.
/// </summary>
public sealed class ParseException : ZLibraryException
{
    /// <summary>
    /// 初始化 <see cref="ParseException"/> 类的新实例.
    /// </summary>
    public ParseException()
        : base("Failed to parse HTML content.")
    {
    }

    /// <summary>
    /// 初始化 <see cref="ParseException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public ParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="ParseException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public ParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
