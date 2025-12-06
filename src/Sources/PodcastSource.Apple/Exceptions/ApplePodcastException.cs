// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Podcast.Apple;

/// <summary>
/// Apple Podcast 客户端异常.
/// </summary>
public class ApplePodcastException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastException"/> class.
    /// </summary>
    public ApplePodcastException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public ApplePodcastException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public ApplePodcastException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplePodcastException"/> class.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="statusCode">HTTP 状态码.</param>
    /// <param name="requestUri">请求 URI.</param>
    public ApplePodcastException(string message, int statusCode, Uri? requestUri = null)
        : base(message)
    {
        StatusCode = statusCode;
        RequestUri = requestUri;
    }

    /// <summary>
    /// 获取 HTTP 状态码（如果有）.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// 获取请求 URI（如果有）.
    /// </summary>
    public Uri? RequestUri { get; }
}
