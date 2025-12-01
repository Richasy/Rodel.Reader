// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPFIND 请求类型.
/// </summary>
public enum PropfindRequestType
{
    /// <summary>
    /// 隐式请求所有属性（空请求体）.
    /// </summary>
    AllPropertiesImplied,

    /// <summary>
    /// 显式请求所有属性.
    /// </summary>
    AllProperties,

    /// <summary>
    /// 仅请求属性名称.
    /// </summary>
    NamedProperties,
}
