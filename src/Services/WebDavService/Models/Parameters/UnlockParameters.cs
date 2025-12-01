// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// UNLOCK 操作参数.
/// </summary>
public sealed class UnlockParameters : RequestParameters
{
    /// <summary>
    /// 初始化 <see cref="UnlockParameters"/> 类的新实例.
    /// </summary>
    /// <param name="lockToken">锁令牌.</param>
    public UnlockParameters(string lockToken)
    {
        LockToken = lockToken;
    }

    /// <summary>
    /// 获取锁令牌.
    /// </summary>
    public string LockToken { get; }
}
