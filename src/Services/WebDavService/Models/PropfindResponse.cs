// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// PROPFIND 响应.
/// </summary>
public sealed class PropfindResponse : WebDavResponse
{
    /// <summary>
    /// 初始化 <see cref="PropfindResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    public PropfindResponse(int statusCode)
        : this(statusCode, null, [])
    {
    }

    /// <summary>
    /// 初始化 <see cref="PropfindResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="resources">资源集合.</param>
    public PropfindResponse(int statusCode, IEnumerable<WebDavResource> resources)
        : this(statusCode, null, resources)
    {
    }

    /// <summary>
    /// 初始化 <see cref="PropfindResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    public PropfindResponse(int statusCode, string? description)
        : this(statusCode, description, [])
    {
    }

    /// <summary>
    /// 初始化 <see cref="PropfindResponse"/> 类的新实例.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    /// <param name="resources">资源集合.</param>
    public PropfindResponse(int statusCode, string? description, IEnumerable<WebDavResource> resources)
        : base(statusCode, description)
    {
        Resources = resources.ToList().AsReadOnly();
    }

    /// <summary>
    /// 获取 WebDAV 资源集合.
    /// </summary>
    public IReadOnlyCollection<WebDavResource> Resources { get; }

    /// <inheritdoc/>
    public override string ToString() => $"PROPFIND Response - StatusCode: {StatusCode}, Resources: {Resources.Count}";
}
