// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Exceptions;

/// <summary>
/// 番茄小说异常基类.
/// </summary>
public class FanQieException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieException"/> class.
    /// </summary>
    public FanQieException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public FanQieException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public FanQieException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
