// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 资源操作接口.
/// </summary>
public interface IResourceOperator
{
    /// <summary>
    /// 创建集合（文件夹）.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>WebDAV 响应.</returns>
    Task<WebDavResponse> MkColAsync(Uri requestUri, MkColParameters? parameters = null);

    /// <summary>
    /// 删除资源.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>WebDAV 响应.</returns>
    Task<WebDavResponse> DeleteAsync(Uri requestUri, DeleteParameters? parameters = null);

    /// <summary>
    /// 复制资源.
    /// </summary>
    /// <param name="sourceUri">源 URI.</param>
    /// <param name="destUri">目标 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>WebDAV 响应.</returns>
    Task<WebDavResponse> CopyAsync(Uri sourceUri, Uri destUri, CopyParameters? parameters = null);

    /// <summary>
    /// 移动资源.
    /// </summary>
    /// <param name="sourceUri">源 URI.</param>
    /// <param name="destUri">目标 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>WebDAV 响应.</returns>
    Task<WebDavResponse> MoveAsync(Uri sourceUri, Uri destUri, MoveParameters? parameters = null);
}
