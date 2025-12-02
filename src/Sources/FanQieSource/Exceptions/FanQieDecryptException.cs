// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Exceptions;

/// <summary>
/// 内容解密异常.
/// </summary>
public class FanQieDecryptException : FanQieException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieDecryptException"/> class.
    /// </summary>
    public FanQieDecryptException()
        : base("Failed to decrypt content.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieDecryptException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public FanQieDecryptException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieDecryptException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public FanQieDecryptException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
