// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 锁操作接口.
/// </summary>
public interface ILockOperator
{
    /// <summary>
    /// 锁定资源.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>锁响应.</returns>
    Task<LockResponse> LockAsync(Uri requestUri, LockParameters? parameters = null);

    /// <summary>
    /// 解锁资源.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="lockToken">锁令牌.</param>
    /// <returns>WebDAV 响应.</returns>
    Task<WebDavResponse> UnlockAsync(Uri requestUri, string lockToken);

    /// <summary>
    /// 解锁资源.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>WebDAV 响应.</returns>
    Task<WebDavResponse> UnlockAsync(Uri requestUri, UnlockParameters parameters);
}
