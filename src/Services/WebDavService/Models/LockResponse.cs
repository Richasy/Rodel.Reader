// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// LOCK 响应.
/// </summary>
public sealed class LockResponse : WebDavResponse
{
    /// <summary>
    /// 初始化 <see cref="LockResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    public LockResponse(int statusCode)
        : this(statusCode, null, null)
    {
    }

    /// <summary>
    /// 初始化 <see cref="LockResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    public LockResponse(int statusCode, string? description)
        : this(statusCode, description, null)
    {
    }

    /// <summary>
    /// 初始化 <see cref="LockResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    /// <param name="activeLock">活动锁信息.</param>
    public LockResponse(int statusCode, string? description, ActiveLock? activeLock)
        : base(statusCode, description)
    {
        ActiveLock = activeLock;
    }

    /// <summary>
    /// 获取活动锁信息.
    /// </summary>
    public ActiveLock? ActiveLock { get; }

    /// <summary>
    /// 获取锁令牌.
    /// </summary>
    public string? LockToken => ActiveLock?.LockToken;

    /// <inheritdoc/>
    public override string ToString() => $"LOCK Response - StatusCode: {StatusCode}, LockToken: {LockToken}";
}
