// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// FB2 解析失败时抛出的异常。
/// </summary>
public class Fb2ParseException : Exception
{
    /// <summary>
    /// 初始化 <see cref="Fb2ParseException"/> 类的新实例。
    /// </summary>
    public Fb2ParseException()
    {
    }

    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="Fb2ParseException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    public Fb2ParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化 <see cref="Fb2ParseException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    /// <param name="innerException">导致当前异常的异常。</param>
    public Fb2ParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
