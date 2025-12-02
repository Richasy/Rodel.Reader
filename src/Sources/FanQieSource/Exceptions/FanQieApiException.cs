// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Exceptions;

/// <summary>
/// API 请求异常.
/// </summary>
public class FanQieApiException : FanQieException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieApiException"/> class.
    /// </summary>
    /// <param name="code">错误码.</param>
    /// <param name="message">错误消息.</param>
    public FanQieApiException(int code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FanQieApiException"/> class.
    /// </summary>
    /// <param name="code">错误码.</param>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public FanQieApiException(int code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    /// <summary>
    /// 错误码.
    /// </summary>
    public int Code { get; }
}
