// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast;

/// <summary>
/// HTTP 请求分发器接口.
/// </summary>
public interface IPodcastDispatcher : IDisposable
{
    /// <summary>
    /// 发送 GET 请求并反序列化 JSON 响应.
    /// </summary>
    /// <typeparam name="T">响应类型.</typeparam>
    /// <param name="uri">请求 URI.</param>
    /// <param name="jsonTypeInfo">AOT 兼容的 JSON 类型信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>反序列化后的响应对象.</returns>
    Task<T?> GetJsonAsync<T>(Uri uri, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 GET 请求并返回字符串响应.
    /// </summary>
    /// <param name="uri">请求 URI.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应字符串.</returns>
    Task<string> GetStringAsync(Uri uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 GET 请求并返回流.
    /// </summary>
    /// <param name="uri">请求 URI.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应流.</returns>
    Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default);
}
