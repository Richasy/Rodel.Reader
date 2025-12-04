// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Exceptions;

/// <summary>
/// Legado 异常基类.
/// </summary>
public class LegadoException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoException"/> class.
    /// </summary>
    public LegadoException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public LegadoException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public LegadoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
