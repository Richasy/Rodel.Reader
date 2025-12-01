// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 锁所有者的基类.
/// </summary>
public abstract class LockOwner
{
    /// <summary>
    /// 获取所有者值.
    /// </summary>
    public abstract string Value { get; }
}

/// <summary>
/// 主体锁所有者（用户名）.
/// </summary>
public sealed class PrincipalLockOwner : LockOwner
{
    /// <summary>
    /// 初始化 <see cref="PrincipalLockOwner"/> 类的新实例.
    /// </summary>
    /// <param name="principal">主体名称.</param>
    public PrincipalLockOwner(string principal)
    {
        Principal = principal;
    }

    /// <summary>
    /// 获取主体名称.
    /// </summary>
    public string Principal { get; }

    /// <inheritdoc/>
    public override string Value => Principal;
}

/// <summary>
/// URI 锁所有者.
/// </summary>
public sealed class UriLockOwner : LockOwner
{
    /// <summary>
    /// 初始化 <see cref="UriLockOwner"/> 类的新实例.
    /// </summary>
    /// <param name="uri">所有者 URI.</param>
    public UriLockOwner(Uri uri)
    {
        Uri = uri;
    }

    /// <summary>
    /// 获取所有者 URI.
    /// </summary>
    public Uri Uri { get; }

    /// <inheritdoc/>
    public override string Value => Uri.ToString();
}
