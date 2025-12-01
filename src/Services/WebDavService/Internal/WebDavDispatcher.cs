// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV HTTP 分发器实现.
/// </summary>
internal sealed class WebDavDispatcher : IWebDavDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private bool _disposed;

    /// <summary>
    /// 初始化 <see cref="WebDavDispatcher"/> 类的新实例.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public WebDavDispatcher(HttpClient httpClient, ILogger? logger = null)
    {
        Guard.NotNull(httpClient);
        _httpClient = httpClient;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public Uri? BaseAddress => _httpClient.BaseAddress;

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> SendAsync(
        Uri requestUri,
        HttpMethod method,
        IDictionary<string, string>? headers = null,
        HttpContent? content = null,
        MediaTypeHeaderValue? contentType = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var absoluteUri = GetAbsoluteUri(requestUri);

        _logger.LogDebug("WebDAV {Method} request to {Uri}", method.Method, absoluteUri);

        using var request = new HttpRequestMessage(method, absoluteUri);

        // 添加请求头
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // 添加请求内容
        if (content != null)
        {
            request.Content = content;
            if (contentType != null)
            {
                request.Content.Headers.ContentType = contentType;
            }
        }

        try
        {
            var response = await _httpClient.SendAsync(request, completionOption, cancellationToken)
                .ConfigureAwait(false);

            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug(
                    "WebDAV {Method} {Uri} completed in {Elapsed}ms with status {StatusCode}",
                    method.Method,
                    absoluteUri,
                    stopwatch.ElapsedMilliseconds,
                    (int)response.StatusCode);
            }
            else
            {
                _logger.LogWarning(
                    "WebDAV {Method} {Uri} returned {StatusCode} {Reason} in {Elapsed}ms",
                    method.Method,
                    absoluteUri,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "WebDAV {Method} {Uri} failed after {Elapsed}ms: {Message}",
                method.Method,
                absoluteUri,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw new WebDavException($"Request to {absoluteUri} failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "WebDAV {Method} {Uri} timed out after {Elapsed}ms",
                method.Method,
                absoluteUri,
                stopwatch.ElapsedMilliseconds);

            throw new WebDavException($"Request to {absoluteUri} timed out", ex);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogDebug("Disposing WebDavDispatcher");
        _httpClient.Dispose();
        _disposed = true;
    }

    private Uri GetAbsoluteUri(Uri uri)
    {
        if (uri.IsAbsoluteUri)
        {
            return uri;
        }

        if (_httpClient.BaseAddress == null)
        {
            throw new InvalidOperationException(
                "The request URI must be an absolute URI or BaseAddress must be set.");
        }

        return new Uri(_httpClient.BaseAddress, uri);
    }
}
