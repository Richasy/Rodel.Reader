// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 响应基类.
/// </summary>
public class WebDavResponse
{
    /// <summary>
    /// 初始化 <see cref="WebDavResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    public WebDavResponse(int statusCode)
        : this(statusCode, null)
    {
    }

    /// <summary>
    /// 初始化 <see cref="WebDavResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    public WebDavResponse(int statusCode, string? description)
    {
        StatusCode = statusCode;
        Description = description;
    }

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
    public virtual bool IsSuccessful => StatusCode >= 200 && StatusCode <= 299;

    /// <inheritdoc/>
    public override string ToString() => $"WebDAV Response - StatusCode: {StatusCode}, Description: {Description}";
}
