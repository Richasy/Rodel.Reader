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

    /// <summary>
    /// 初始化 <see cref="ZLibDispatcher"/> 类的新实例.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="maxConcurrent">最大并发数.</param>
    /// <param name="logger">日志器.</param>
    public ZLibDispatcher(HttpClient httpClient, int maxConcurrent, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
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
            AddCookies(request);

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
            AddCookies(request);

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

    private void AddCookies(HttpRequestMessage request)
    {
        if (Cookies != null && Cookies.Count > 0)
        {
            var cookieString = string.Join("; ", Cookies.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            request.Headers.Add("Cookie", cookieString);
        }
    }
}
