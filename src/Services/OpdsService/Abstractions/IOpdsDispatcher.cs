// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService;

/// <summary>
/// HTTP 请求分发器接口.
/// </summary>
internal interface IOpdsDispatcher : IDisposable
{
    /// <summary>
    /// 发送 GET 请求并返回响应流.
    /// </summary>
    /// <param name="uri">请求 URI.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应流.</returns>
    Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 GET 请求并返回响应字符串.
    /// </summary>
    /// <param name="uri">请求 URI.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应字符串.</returns>
    Task<string> GetStringAsync(Uri uri, CancellationToken cancellationToken = default);
}
