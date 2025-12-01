// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPPATCH 响应.
/// </summary>
public sealed class ProppatchResponse : WebDavResponse
{
    /// <summary>
    /// 初始化 <see cref="ProppatchResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    public ProppatchResponse(int statusCode)
        : this(statusCode, null, [])
    {
    }

    /// <summary>
    /// 初始化 <see cref="ProppatchResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    public ProppatchResponse(int statusCode, string? description)
        : this(statusCode, description, [])
    {
    }

    /// <summary>
    /// 初始化 <see cref="ProppatchResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    /// <param name="propertyStatuses">属性状态集合.</param>
    public ProppatchResponse(int statusCode, string? description, IEnumerable<WebDavPropertyStatus> propertyStatuses)
        : base(statusCode, description)
    {
        PropertyStatuses = propertyStatuses.ToList().AsReadOnly();
    }

    /// <summary>
    /// 获取属性状态集合.
    /// </summary>
    public IReadOnlyCollection<WebDavPropertyStatus> PropertyStatuses { get; }

    /// <inheritdoc/>
    public override string ToString() => $"PROPPATCH Response - StatusCode: {StatusCode}, Properties: {PropertyStatuses.Count}";
}
