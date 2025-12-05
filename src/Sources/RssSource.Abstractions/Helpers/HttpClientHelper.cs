// Copyright (c) Richasy. All rights reserved.

using System.Net;
using System.Net.Http.Headers;

namespace Richasy.RodelReader.Sources.Rss.Abstractions.Helpers;

/// <summary>
/// HTTP 客户端辅助类.
/// </summary>
public static class HttpClientHelper
{
    private const string DefaultAcceptString = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
    private const string DefaultUserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    /// <summary>
    /// 创建配置好的 HttpClient 实例.
    /// </summary>
    /// <param name="timeout">超时时间.</param>
    /// <returns>HttpClient 实例.</returns>
    public static HttpClient CreateHttpClient(TimeSpan? timeout = null)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };

        var client = new HttpClient(handler);

        if (timeout.HasValue)
        {
            client.Timeout = timeout.Value;
        }

        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
        };
        client.DefaultRequestHeaders.Add("Accept", DefaultAcceptString);
        client.DefaultRequestHeaders.Add("User-Agent", DefaultUserAgentString);

        return client;
    }

    /// <summary>
    /// 创建支持 Cookie 的 HttpClient 实例.
    /// </summary>
    /// <param name="cookieContainer">Cookie 容器.</param>
    /// <param name="timeout">超时时间.</param>
    /// <returns>HttpClient 实例.</returns>
    public static HttpClient CreateHttpClientWithCookies(
        CookieContainer cookieContainer,
        TimeSpan? timeout = null)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            CookieContainer = cookieContainer,
            UseCookies = true,
        };

        var client = new HttpClient(handler);

        if (timeout.HasValue)
        {
            client.Timeout = timeout.Value;
        }

        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
        };
        client.DefaultRequestHeaders.Add("Accept", DefaultAcceptString);
        client.DefaultRequestHeaders.Add("User-Agent", DefaultUserAgentString);

        return client;
    }
}
