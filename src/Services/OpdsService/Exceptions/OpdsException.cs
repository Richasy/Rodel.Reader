// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService;

/// <summary>
/// OPDS 操作异常.
/// </summary>
public class OpdsException : Exception
{
    /// <summary>
    /// 初始化 <see cref="OpdsException"/> 类的新实例.
    /// </summary>
    public OpdsException()
    {
    }

    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="OpdsException"/> 类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    public OpdsException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 使用指定的错误消息和内部异常初始化 <see cref="OpdsException"/> 类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    /// <param name="innerException">导致当前异常的异常.</param>
    public OpdsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 使用指定的错误消息、状态码和 URI 初始化 <see cref="OpdsException"/> 类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    /// <param name="statusCode">HTTP 状态码.</param>
    /// <param name="requestUri">请求的 URI.</param>
    public OpdsException(string message, int statusCode, Uri? requestUri)
        : base(message)
    {
        StatusCode = statusCode;
        RequestUri = requestUri;
    }

    /// <summary>
    /// 使用指定的错误消息、状态码、URI 和内部异常初始化 <see cref="OpdsException"/> 类的新实例.
    /// </summary>
    /// <param name="message">描述错误的消息.</param>
    /// <param name="statusCode">HTTP 状态码.</param>
    /// <param name="requestUri">请求的 URI.</param>
    /// <param name="innerException">导致当前异常的异常.</param>
    public OpdsException(string message, int statusCode, Uri? requestUri, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        RequestUri = requestUri;
    }

    /// <summary>
    /// 获取 HTTP 状态码.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// 获取请求的 URI.
    /// </summary>
    public Uri? RequestUri { get; }
}
