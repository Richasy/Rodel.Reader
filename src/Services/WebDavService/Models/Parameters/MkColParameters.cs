// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// MKCOL 操作参数.
/// </summary>
public sealed class MkColParameters : RequestParameters
{
    /// <summary>
    /// 获取或设置锁令牌.
    /// </summary>
    public string? LockToken { get; set; }
}
