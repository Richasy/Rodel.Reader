// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Exceptions;

/// <summary>
/// Legado API 调用异常.
/// </summary>
public class LegadoApiException : LegadoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoApiException"/> class.
    /// </summary>
    public LegadoApiException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoApiException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public LegadoApiException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoApiException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public LegadoApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoApiException"/> class.
    /// </summary>
    /// <param name="statusCode">HTTP 状态码.</param>
    /// <param name="message">异常消息.</param>
    public LegadoApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoApiException"/> class.
    /// </summary>
    /// <param name="errorCode">API 错误码.</param>
    /// <param name="message">错误消息.</param>
    /// <param name="isApiError">标记为 API 错误.</param>
    public LegadoApiException(string errorCode, string message, bool isApiError)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// HTTP 状态码.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// API 错误码.
    /// </summary>
    public string? ErrorCode { get; }
}
