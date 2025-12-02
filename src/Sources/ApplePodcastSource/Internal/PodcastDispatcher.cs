// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Internal;

/// <summary>
/// HTTP 请求分发器实现.
/// </summary>
internal sealed class PodcastDispatcher : IPodcastDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastDispatcher"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public PodcastDispatcher(HttpClient httpClient, ILogger logger)
    {
        _httpClient = Guard.NotNull(httpClient);
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public async Task<T?> GetJsonAsync<T>(Uri uri, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
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
                throw new ApplePodcastException($"Request failed with status code {statusCode}", statusCode, uri);
            }

            _logger.LogDebug("Request to {Uri} succeeded", uri);

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync(stream, jsonTypeInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to {Uri} failed", uri);
            throw new ApplePodcastException($"HTTP request failed: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from {Uri}", uri);
            throw new ApplePodcastException($"Failed to parse response: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(uri);

        _logger.LogDebug("Sending GET request to {Uri}", uri);

        try
        {
            var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                _logger.LogWarning("Request to {Uri} failed with status code {StatusCode}", uri, statusCode);
                throw new ApplePodcastException($"Request failed with status code {statusCode}", statusCode, uri);
            }

            _logger.LogDebug("Request to {Uri} succeeded", uri);
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to {Uri} failed", uri);
            throw new ApplePodcastException($"HTTP request failed: {ex.Message}", ex);
        }
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
                throw new ApplePodcastException($"Request failed with status code {statusCode}", statusCode, uri);
            }

            _logger.LogDebug("Request to {Uri} succeeded", uri);
            return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to {Uri} failed", uri);
            throw new ApplePodcastException($"HTTP request failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
            _logger.LogDebug("PodcastDispatcher disposed");
        }
    }
}
