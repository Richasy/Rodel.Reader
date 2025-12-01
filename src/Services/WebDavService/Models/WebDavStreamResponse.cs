// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 流响应（用于文件下载）.
/// </summary>
public sealed class WebDavStreamResponse : WebDavResponse, IDisposable
{
    private readonly HttpResponseMessage? _response;
    private bool _disposed;

    /// <summary>
    /// 初始化 <see cref="WebDavStreamResponse"/> 类的新实例.
    /// </summary>
    /// <param name="response">HTTP 响应消息.</param>
    /// <param name="stream">响应流.</param>
    public WebDavStreamResponse(HttpResponseMessage response, Stream stream)
        : base((int)response.StatusCode, response.ReasonPhrase)
    {
        _response = response;
        Stream = stream;
    }

    /// <summary>
    /// 初始化 <see cref="WebDavStreamResponse"/> 类的新实例（用于错误情况）.
    /// </summary>
    /// <param name="statusCode">状态码.</param>
    /// <param name="description">描述.</param>
    public WebDavStreamResponse(int statusCode, string? description)
        : base(statusCode, description)
    {
        Stream = Stream.Null;
    }

    /// <summary>
    /// 获取响应流.
    /// </summary>
    public Stream Stream { get; }

    /// <summary>
    /// 获取内容长度.
    /// </summary>
    public long? ContentLength => _response?.Content.Headers.ContentLength;

    /// <summary>
    /// 获取内容类型.
    /// </summary>
    public string? ContentType => _response?.Content.Headers.ContentType?.MediaType;

    /// <summary>
    /// 获取 ETag.
    /// </summary>
    public string? ETag => _response?.Headers.ETag?.Tag;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stream.Dispose();
        _response?.Dispose();
        _disposed = true;
    }

    /// <inheritdoc/>
    public override string ToString() => $"WebDAV Stream Response - StatusCode: {StatusCode}, ContentLength: {ContentLength}";
}
