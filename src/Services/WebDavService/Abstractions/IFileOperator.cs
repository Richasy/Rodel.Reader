// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 文件操作接口.
/// </summary>
public interface IFileOperator
{
    /// <summary>
    /// 获取文件（原始内容，不经服务器处理）.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>流响应.</returns>
    Task<WebDavStreamResponse> GetRawFileAsync(Uri requestUri, GetFileParameters? parameters = null);

    /// <summary>
    /// 获取文件（服务器可能处理响应）.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>流响应.</returns>
    Task<WebDavStreamResponse> GetProcessedFileAsync(Uri requestUri, GetFileParameters? parameters = null);

    /// <summary>
    /// 上传文件.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="stream">文件流.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>WebDAV 响应.</returns>
    Task<WebDavResponse> PutFileAsync(Uri requestUri, Stream stream, PutFileParameters? parameters = null);

    /// <summary>
    /// 上传文件.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="content">HTTP 内容.</param>
    /// <param name="parameters">操作参数.</param>
    /// <returns>WebDAV 响应.</returns>
    Task<WebDavResponse> PutFileAsync(Uri requestUri, HttpContent content, PutFileParameters? parameters = null);
}
