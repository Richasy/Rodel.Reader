// Copyright (c) Richasy. All rights reserved.

using System.Security.Authentication;
using Richasy.RodelReader.Sources.Legado.Exceptions;
using Richasy.RodelReader.Sources.Legado.Helpers;

namespace Richasy.RodelReader.Sources.Legado.Internal;

/// <summary>
/// HTTP 请求调度器.
/// </summary>
internal sealed class LegadoDispatcher : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LegadoClientOptions _options;
    private readonly ILogger _logger;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LegadoDispatcher"/> class.
    /// </summary>
    /// <param name="options">客户端配置.</param>
    /// <param name="httpClient">HTTP 客户端（可选）.</param>
    /// <param name="logger">日志记录器（可选）.</param>
    public LegadoDispatcher(
        LegadoClientOptions options,
        HttpClient? httpClient = null,
        ILogger? logger = null)
    {
        _options = Guard.NotNull(options);
        _logger = logger ?? NullLogger.Instance;

        if (httpClient != null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = CreateHttpClient(options);
            _ownsHttpClient = true;
        }

        ConfigureHttpClient();
        _logger.LogDebug("LegadoDispatcher initialized with base URL: {BaseUrl}, server type: {ServerType}",
            options.BaseUrl, options.ServerType);
    }

    /// <summary>
    /// 发送 GET 请求.
    /// </summary>
    /// <typeparam name="T">响应数据类型.</typeparam>
    /// <param name="endpoint">API 端点.</param>
    /// <param name="typeInfo">JSON 类型信息.</param>
    /// <param name="queryParams">查询参数.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应数据.</returns>
    public async Task<T> GetAsync<T>(
        string endpoint,
        JsonTypeInfo<T> typeInfo,
        Dictionary<string, string>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(endpoint, queryParams);
        _logger.LogDebug("Sending GET request to: {Url}", uri);

        try
        {
            var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            return await HandleResponseAsync(response, typeInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not LegadoException and not OperationCanceledException)
        {
            _logger.LogError(ex, "GET request failed for endpoint: {Endpoint}", endpoint);
            throw new LegadoApiException($"Request failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 发送 GET 请求并返回原始字符串.
    /// </summary>
    /// <param name="endpoint">API 端点.</param>
    /// <param name="queryParams">查询参数.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>原始响应字符串.</returns>
    public async Task<string> GetStringAsync(
        string endpoint,
        Dictionary<string, string>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(endpoint, queryParams);
        _logger.LogDebug("Sending GET request (string) to: {Url}", uri);

        try
        {
            var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessStatusCodeAsync(response, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not LegadoException and not OperationCanceledException)
        {
            _logger.LogError(ex, "GET request (string) failed for endpoint: {Endpoint}", endpoint);
            throw new LegadoApiException($"Request failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 发送 GET 请求并返回流.
    /// </summary>
    /// <param name="endpoint">API 端点.</param>
    /// <param name="queryParams">查询参数.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应流.</returns>
    public async Task<Stream> GetStreamAsync(
        string endpoint,
        Dictionary<string, string>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(endpoint, queryParams);
        _logger.LogDebug("Sending GET request (stream) to: {Url}", uri);

        try
        {
            var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessStatusCodeAsync(response, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not LegadoException and not OperationCanceledException)
        {
            _logger.LogError(ex, "GET request (stream) failed for endpoint: {Endpoint}", endpoint);
            throw new LegadoApiException($"Request failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 发送 POST 请求.
    /// </summary>
    /// <typeparam name="TRequest">请求数据类型.</typeparam>
    /// <typeparam name="TResponse">响应数据类型.</typeparam>
    /// <param name="endpoint">API 端点.</param>
    /// <param name="data">请求数据.</param>
    /// <param name="requestTypeInfo">请求数据的 JSON 类型信息.</param>
    /// <param name="responseTypeInfo">响应数据的 JSON 类型信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应数据.</returns>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(endpoint);
        _logger.LogDebug("Sending POST request to: {Url}", uri);

        try
        {
            var json = JsonSerializer.Serialize(data, requestTypeInfo);
            _logger.LogTrace("POST request body: {Body}", json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(uri, content, cancellationToken).ConfigureAwait(false);

            return await HandleResponseAsync(response, responseTypeInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not LegadoException and not OperationCanceledException)
        {
            _logger.LogError(ex, "POST request failed for endpoint: {Endpoint}", endpoint);
            throw new LegadoApiException($"Request failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 发送 POST 请求（无响应体）.
    /// </summary>
    /// <typeparam name="TRequest">请求数据类型.</typeparam>
    /// <param name="endpoint">API 端点.</param>
    /// <param name="data">请求数据.</param>
    /// <param name="requestTypeInfo">请求数据的 JSON 类型信息.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    public async Task PostAsync<TRequest>(
        string endpoint,
        TRequest data,
        JsonTypeInfo<TRequest> requestTypeInfo,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(endpoint);
        _logger.LogDebug("Sending POST request (no response) to: {Url}", uri);

        try
        {
            var json = JsonSerializer.Serialize(data, requestTypeInfo);
            _logger.LogTrace("POST request body: {Body}", json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(uri, content, cancellationToken).ConfigureAwait(false);

            await EnsureSuccessStatusCodeAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("POST request completed successfully for endpoint: {Endpoint}", endpoint);
        }
        catch (Exception ex) when (ex is not LegadoException and not OperationCanceledException)
        {
            _logger.LogError(ex, "POST request failed for endpoint: {Endpoint}", endpoint);
            throw new LegadoApiException($"Request failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        _disposed = true;
        _logger.LogDebug("LegadoDispatcher disposed");
    }

#pragma warning disable CA5400 // HttpClient 证书验证 - 用户配置选项
    private static HttpClient CreateHttpClient(LegadoClientOptions options)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

        if (options.IgnoreSslErrors)
        {
            handler.SslProtocols = SslProtocols.None;
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        return new HttpClient(handler)
        {
            Timeout = options.Timeout,
        };
    }
#pragma warning restore CA5400

    private void ConfigureHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
    }

    private Uri BuildUri(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        var url = ApiEndpoints.BuildUrl(
            _options.BaseUrl,
            endpoint,
            _options.ServerType,
            _options.AccessToken,
            queryParams);
        return new Uri(url);
    }

    private async Task<T> HandleResponseAsync<T>(
        HttpResponseMessage response,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
    {
        await EnsureSuccessStatusCodeAsync(response, cancellationToken).ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogTrace("Response content: {Content}", content);

        try
        {
            var result = JsonSerializer.Deserialize(content, typeInfo);
            if (result is null)
            {
                throw new LegadoApiException("Failed to deserialize response: result is null");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response: {Content}", content);
            throw new LegadoApiException($"Failed to deserialize response: {ex.Message}", ex);
        }
    }

    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogWarning("HTTP request failed with status {StatusCode}: {Content}", response.StatusCode, content);

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new LegadoAuthException($"Authentication failed: {response.StatusCode}");
        }

        throw new LegadoApiException(response.StatusCode, $"HTTP request failed: {response.StatusCode} - {content}");
    }
}
