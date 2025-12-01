// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 属性状态.
/// </summary>
public sealed class WebDavPropertyStatus
{
    /// <summary>
    /// 初始化 <see cref="WebDavPropertyStatus"/> 类的新实例.
    /// </summary>
    /// <param name="property">属性.</param>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    public WebDavPropertyStatus(WebDavProperty property, int statusCode, string? description)
    {
        Property = property;
        StatusCode = statusCode;
        Description = description;
    }

    /// <summary>
    /// 获取属性.
    /// </summary>
    public WebDavProperty Property { get; }

    /// <summary>
    /// 获取状态码.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// 获取描述.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// 获取一个值，指示操作是否成功.
    /// </summary>
    public bool IsSuccessful => StatusCode >= 200 && StatusCode <= 299;
}
