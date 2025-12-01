// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 属性操作接口.
/// </summary>
public interface IPropertyOperator
{
    /// <summary>
    /// 执行 PROPFIND 操作，获取资源属性.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>PROPFIND 响应.</returns>
    Task<PropfindResponse> PropfindAsync(Uri requestUri, PropfindParameters? parameters = null);

    /// <summary>
    /// 执行 PROPPATCH 操作，设置或删除资源属性.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>PROPPATCH 响应.</returns>
    Task<ProppatchResponse> ProppatchAsync(Uri requestUri, ProppatchParameters parameters);
}
