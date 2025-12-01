// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// LOCK 操作参数.
/// </summary>
public sealed class LockParameters : RequestParameters
{
    /// <summary>
    /// 获取或设置锁定范围.
    /// </summary>
    public LockScope LockScope { get; set; } = LockScope.Exclusive;

    /// <summary>
    /// 获取或设置锁定所有者.
    /// </summary>
    public LockOwner? Owner { get; set; }

    /// <summary>
    /// 获取或设置锁定超时时间.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// 获取或设置应用范围.
    /// </summary>
    public LockApplyTo? ApplyTo { get; set; }
}
