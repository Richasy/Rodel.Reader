// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Exceptions;

/// <summary>
/// Legado 认证异常.
/// </summary>
public class LegadoAuthException : LegadoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoAuthException"/> class.
    /// </summary>
    public LegadoAuthException()
        : base("Authentication failed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoAuthException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public LegadoAuthException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoAuthException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public LegadoAuthException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
