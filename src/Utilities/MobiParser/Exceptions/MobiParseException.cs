// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// Mobi 解析失败时抛出的异常。
/// </summary>
public class MobiParseException : Exception
{
    /// <summary>
    /// 初始化 <see cref="MobiParseException"/> 类的新实例。
    /// </summary>
    public MobiParseException()
    {
    }

    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="MobiParseException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    public MobiParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化 <see cref="MobiParseException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    /// <param name="innerException">导致当前异常的异常。</param>
    public MobiParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
