// Copyright (c) Richasy. All rights reserved.

using RichardSzalay.MockHttp;

namespace DownloadKit.Test;

/// <summary>
/// 测试数据工厂.
/// </summary>
internal static class TestDataFactory
{
    /// <summary>
    /// 创建模拟的 HTTP 消息处理器.
    /// </summary>
    /// <param name="url">请求 URL.</param>
    /// <param name="content">响应内容.</param>
    /// <param name="contentType">内容类型.</param>
    /// <returns>模拟的消息处理器.</returns>
    public static MockHttpMessageHandler CreateMockHandler(
        string url,
        byte[] content,
        string contentType = "application/octet-stream")
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When(url)
            .Respond(contentType, new MemoryStream(content));
        return mockHttp;
    }

    /// <summary>
    /// 创建模拟的 HTTP 消息处理器（带 HEAD 支持）.
    /// </summary>
    /// <param name="url">请求 URL.</param>
    /// <param name="content">响应内容.</param>
    /// <param name="contentType">内容类型.</param>
    /// <returns>模拟的消息处理器.</returns>
    public static MockHttpMessageHandler CreateMockHandlerWithHead(
        string url,
        byte[] content,
        string contentType = "application/octet-stream")
    {
        var mockHttp = new MockHttpMessageHandler();

        // HEAD 请求
        mockHttp
            .When(HttpMethod.Head, url)
            .Respond(req =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content.Headers.ContentLength = content.Length;
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                return response;
            });

        // GET 请求
        mockHttp
            .When(HttpMethod.Get, url)
            .Respond(contentType, new MemoryStream(content));

        return mockHttp;
    }

    /// <summary>
    /// 创建测试用的随机字节数组.
    /// </summary>
    /// <param name="size">大小.</param>
    /// <returns>随机字节数组.</returns>
    public static byte[] CreateRandomBytes(int size)
    {
        var bytes = new byte[size];
        Random.Shared.NextBytes(bytes);
        return bytes;
    }

    /// <summary>
    /// 获取临时文件路径.
    /// </summary>
    /// <param name="extension">文件扩展名.</param>
    /// <returns>临时文件路径.</returns>
    public static string GetTempFilePath(string extension = ".tmp")
    {
        return Path.Combine(Path.GetTempPath(), $"downloadkit_test_{Guid.NewGuid()}{extension}");
    }

    /// <summary>
    /// 清理测试文件.
    /// </summary>
    /// <param name="filePath">文件路径.</param>
    public static void CleanupFile(string? filePath)
    {
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // 忽略清理错误
            }
        }
    }
}
