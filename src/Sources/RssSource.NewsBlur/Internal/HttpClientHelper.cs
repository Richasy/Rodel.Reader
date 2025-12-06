// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.NewsBlur.Internal;

/// <summary>
/// HTTP 客户端辅助类.
/// </summary>
internal static class HttpClientHelper
{
    /// <summary>
    /// 创建配置好的 HttpClient.
    /// </summary>
    /// <param name="cookieContainer">Cookie 容器.</param>
    /// <param name="timeout">超时时间.</param>
    /// <returns>HttpClient 实例.</returns>
    public static HttpClient CreateHttpClient(CookieContainer cookieContainer, TimeSpan timeout)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            CookieContainer = cookieContainer,
            UseCookies = true,
        };

        var client = new HttpClient(handler)
        {
            Timeout = timeout,
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("RodelReader/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

        return client;
    }
}
