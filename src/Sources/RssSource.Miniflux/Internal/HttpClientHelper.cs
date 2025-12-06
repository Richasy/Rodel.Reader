// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Miniflux.Internal;

/// <summary>
/// HTTP 客户端辅助类.
/// </summary>
internal static class HttpClientHelper
{
    /// <summary>
    /// 创建配置好的 HttpClient.
    /// </summary>
    /// <param name="timeout">超时时间.</param>
    /// <returns>HttpClient 实例.</returns>
    public static HttpClient CreateHttpClient(TimeSpan timeout)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All,
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
