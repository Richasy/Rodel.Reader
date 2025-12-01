// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 锁定范围.
/// </summary>
public enum LockScope
{
    /// <summary>
    /// 共享锁 - 允许多个用户同时锁定资源.
    /// </summary>
    Shared,

    /// <summary>
    /// 排他锁 - 只有一个用户可以锁定资源.
    /// </summary>
    Exclusive,
}
