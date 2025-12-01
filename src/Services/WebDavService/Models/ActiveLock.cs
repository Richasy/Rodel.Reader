// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 活动锁信息.
/// </summary>
public sealed class ActiveLock
{
    /// <summary>
    /// 初始化 <see cref="ActiveLock"/> 类的新实例.
    /// </summary>
    public ActiveLock()
    {
    }

    /// <summary>
    /// 获取或设置锁定范围.
    /// </summary>
    public LockScope LockScope { get; init; }

    /// <summary>
    /// 获取或设置锁定令牌.
    /// </summary>
    public string? LockToken { get; init; }

    /// <summary>
    /// 获取或设置锁定深度.
    /// </summary>
    public string? Depth { get; init; }

    /// <summary>
    /// 获取或设置锁定超时.
    /// </summary>
    public string? Timeout { get; init; }

    /// <summary>
    /// 获取或设置锁定所有者.
    /// </summary>
    public LockOwner? Owner { get; init; }

    /// <summary>
    /// 获取或设置锁定根资源.
    /// </summary>
    public string? LockRoot { get; init; }
}
