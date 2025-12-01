// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPFIND 操作参数.
/// </summary>
public sealed class PropfindParameters : RequestParameters
{
    /// <summary>
    /// 获取或设置请求类型.
    /// </summary>
    public PropfindRequestType RequestType { get; set; } = PropfindRequestType.AllProperties;

    /// <summary>
    /// 获取或设置应用范围.
    /// </summary>
    public PropfindApplyTo? ApplyTo { get; set; }

    /// <summary>
    /// 获取或设置要请求的自定义属性.
    /// </summary>
    public IReadOnlyCollection<WebDavProperty>? CustomProperties { get; set; }

    /// <summary>
    /// 获取或设置自定义命名空间.
    /// </summary>
    public IReadOnlyCollection<NamespaceAttribute>? Namespaces { get; set; }
}

/// <summary>
/// 命名空间属性.
/// </summary>
/// <param name="Prefix">前缀.</param>
/// <param name="Namespace">命名空间 URI.</param>
public readonly record struct NamespaceAttribute(string Prefix, string Namespace);
