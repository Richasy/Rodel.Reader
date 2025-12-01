// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPPATCH 操作参数.
/// </summary>
public sealed class ProppatchParameters : RequestParameters
{
    /// <summary>
    /// 获取或设置要设置的属性.
    /// </summary>
    public IReadOnlyCollection<WebDavProperty>? PropertiesToSet { get; set; }

    /// <summary>
    /// 获取或设置要删除的属性.
    /// </summary>
    public IReadOnlyCollection<WebDavProperty>? PropertiesToRemove { get; set; }

    /// <summary>
    /// 获取或设置自定义命名空间.
    /// </summary>
    public IReadOnlyCollection<NamespaceAttribute>? Namespaces { get; set; }

    /// <summary>
    /// 获取或设置锁令牌.
    /// </summary>
    public string? LockToken { get; set; }
}
