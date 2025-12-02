// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Exceptions;

/// <summary>
/// 登录失败异常.
/// </summary>
public sealed class LoginFailedException : ZLibraryException
{
    /// <summary>
    /// 初始化 <see cref="LoginFailedException"/> 类的新实例.
    /// </summary>
    public LoginFailedException()
        : base("Login failed.")
    {
    }

    /// <summary>
    /// 初始化 <see cref="LoginFailedException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    public LoginFailedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="LoginFailedException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public LoginFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
