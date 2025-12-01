// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV HTTP 分发器接口.
/// </summary>
public interface IWebDavDispatcher : IDisposable
{
    /// <summary>
    /// 获取基础地址.
    /// </summary>
    Uri? BaseAddress { get; }

    /// <summary>
    /// 发送 WebDAV 请求.
    /// </summary>
    /// <param name="requestUri">请求 URI.</param>
    /// <param name="method">HTTP 方法.</param>
    /// <param name="headers">请求头.</param>
    /// <param name="content">请求内容.</param>
    /// <param name="contentType">内容类型.</param>
    /// <param name="completionOption">完成选项.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>HTTP 响应.</returns>
    Task<HttpResponseMessage> SendAsync(
        Uri requestUri,
        HttpMethod method,
        IDictionary<string, string>? headers = null,
        HttpContent? content = null,
        MediaTypeHeaderValue? contentType = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default);
}
