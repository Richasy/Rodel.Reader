// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Exceptions;

/// <summary>
/// 解析异常.
/// </summary>
public class FanQieParseException : FanQieException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieParseException"/> class.
    /// </summary>
    public FanQieParseException()
        : base("Failed to parse content.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieParseException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public FanQieParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieParseException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public FanQieParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
