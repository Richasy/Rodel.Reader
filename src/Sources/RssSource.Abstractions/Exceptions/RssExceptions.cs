// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 客户端异常.
/// </summary>
public class RssClientException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RssClientException"/> class.
    /// </summary>
    public RssClientException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssClientException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public RssClientException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssClientException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public RssClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// RSS 认证异常.
/// </summary>
public class RssAuthenticationException : RssClientException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RssAuthenticationException"/> class.
    /// </summary>
    public RssAuthenticationException()
        : base("Authentication failed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssAuthenticationException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public RssAuthenticationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssAuthenticationException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public RssAuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// RSS 网络异常.
/// </summary>
public class RssNetworkException : RssClientException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RssNetworkException"/> class.
    /// </summary>
    public RssNetworkException()
        : base("Network error occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssNetworkException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public RssNetworkException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssNetworkException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public RssNetworkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// RSS Feed 解析异常.
/// </summary>
public class RssFeedParseException : RssClientException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RssFeedParseException"/> class.
    /// </summary>
    public RssFeedParseException()
        : base("Failed to parse RSS feed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssFeedParseException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    public RssFeedParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssFeedParseException"/> class.
    /// </summary>
    /// <param name="message">错误消息.</param>
    /// <param name="innerException">内部异常.</param>
    public RssFeedParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
