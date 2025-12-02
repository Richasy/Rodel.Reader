// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Exceptions;

/// <summary>
/// 未认证异常，表示用户未登录.
/// </summary>
public sealed class NotAuthenticatedException : ZLibraryException
{
    /// <summary>
    /// 初始化 <see cref="NotAuthenticatedException"/> 类的新实例.
    /// </summary>
    public NotAuthenticatedException()
        : base("You must log in to your account before performing this operation.")
    {
    }

    /// <summary>
    /// 初始化 <see cref="NotAuthenticatedException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public NotAuthenticatedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="NotAuthenticatedException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public NotAuthenticatedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
