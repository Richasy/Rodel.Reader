// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Internal;

/// <summary>
/// HTTP 请求分发器实现.
/// </summary>
internal sealed class OpdsDispatcher : IOpdsDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private bool _disposed;

    /// <summary>
    /// 初始化 <see cref="OpdsDispatcher"/> 类的新实例.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public OpdsDispatcher(HttpClient httpClient, ILogger logger)
    {
        _httpClient = Guard.NotNull(httpClient);
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(uri);

        _logger.LogDebug("Sending GET request to {Uri}", uri);

        try
        {
            var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                _logger.LogWarning("Request to {Uri} failed with status code {StatusCode}", uri, statusCode);
                throw new OpdsException($"Request failed with status code {statusCode}", statusCode, uri);
            }

            _logger.LogDebug("Request to {Uri} succeeded with status code {StatusCode}", uri, (int)response.StatusCode);

            return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to {Uri} failed", uri);
            throw new OpdsException($"HTTP request failed: {ex.Message}", 0, uri, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request to {Uri} timed out", uri);
            throw new OpdsException("Request timed out", 0, uri, ex);
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(uri);

        _logger.LogDebug("Sending GET request (string) to {Uri}", uri);

        try
        {
            var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                _logger.LogWarning("Request to {Uri} failed with status code {StatusCode}", uri, statusCode);
                throw new OpdsException($"Request failed with status code {statusCode}", statusCode, uri);
            }

            _logger.LogDebug("Request to {Uri} succeeded with status code {StatusCode}", uri, (int)response.StatusCode);

            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to {Uri} failed", uri);
            throw new OpdsException($"HTTP request failed: {ex.Message}", 0, uri, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request to {Uri} timed out", uri);
            throw new OpdsException("Request timed out", 0, uri, ex);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
    }
}
