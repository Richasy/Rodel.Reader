// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Internal;

/// <summary>
/// HTTP 请求分发器实现.
/// </summary>
internal sealed class ZLibDispatcher : IZLibDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly Dictionary<string, string>? _customHeaders;
    private readonly string? _baseUrl;

    /// <summary>
    /// 初始化 <see cref="ZLibDispatcher"/> 类的新实例.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="maxConcurrent">最大并发数.</param>
    /// <param name="logger">日志器.</param>
    /// <param name="customHeaders">自定义请求头.</param>
    /// <param name="baseUrl">基础 URL，用于设置 Origin 和 Referer.</param>
    public ZLibDispatcher(HttpClient httpClient, int maxConcurrent, ILogger logger, Dictionary<string, string>? customHeaders = null, string? baseUrl = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        _customHeaders = customHeaders;
        _baseUrl = baseUrl;
    }

    /// <inheritdoc/>
    public Dictionary<string, string>? Cookies { get; set; }

    /// <inheritdoc/>
    public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _logger.LogDebug("GET {Url}", url);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddRequestHeaders(request, isPost: false);

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<(string Content, IEnumerable<string> SetCookies)> PostAsync(
        string url,
        Dictionary<string, string> formData,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _logger.LogDebug("POST {Url}", url);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new FormUrlEncodedContent(formData);
            AddRequestHeaders(request, isPost: true);

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
                ? cookies
                : [];

            return (content, setCookies);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void AddRequestHeaders(HttpRequestMessage request, bool isPost)
    {
        // 添加 Cookies
        if (Cookies != null && Cookies.Count > 0)
        {
            var cookieString = string.Join("; ", Cookies.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            request.Headers.Add("Cookie", cookieString);
        }

        // 为 POST 请求添加浏览器特定请求头
        if (isPost && !string.IsNullOrEmpty(_baseUrl))
        {
            request.Headers.Add("Origin", _baseUrl);
            request.Headers.Add("Referer", _baseUrl + "/");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            // 添加 sec-fetch 请求头
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Dest", "empty");

            // 添加 Client Hints
            request.Headers.Add("Sec-Ch-Ua", "\"Chromium\";v=\"142\", \"Microsoft Edge\";v=\"142\", \"Not_A Brand\";v=\"99\"");
            request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
        }

        // 添加自定义请求头
        if (_customHeaders != null)
        {
            foreach (var header in _customHeaders)
            {
                // 跳过已经设置的请求头
                if (!request.Headers.Contains(header.Key))
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }
    }
}
