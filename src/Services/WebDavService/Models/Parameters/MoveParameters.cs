// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// MOVE 操作参数.
/// </summary>
public sealed class MoveParameters : RequestParameters
{
    /// <summary>
    /// 获取或设置是否覆盖目标.
    /// </summary>
    public bool Overwrite { get; set; } = true;

    /// <summary>
    /// 获取或设置源锁令牌.
    /// </summary>
    public string? SourceLockToken { get; set; }

    /// <summary>
    /// 获取或设置目标锁令牌.
    /// </summary>
    public string? DestLockToken { get; set; }
}
