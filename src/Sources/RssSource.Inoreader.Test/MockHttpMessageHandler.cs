// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test;

/// <summary>
/// Mock HTTP 消息处理器.
/// 用于模拟 HTTP 请求和响应.
/// </summary>
public sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _handlers = [];
    private readonly List<HttpRequestMessage> _requests = [];

    /// <summary>
    /// 获取所有已发送的请求.
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> Requests => _requests;

    /// <summary>
    /// 配置特定路径的响应.
    /// </summary>
    /// <param name="pathContains">路径包含的字符串.</param>
    /// <param name="responseFactory">响应工厂函数.</param>
    public void SetupResponse(string pathContains, Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        _handlers[pathContains] = responseFactory;
    }

    /// <summary>
    /// 配置特定路径返回 JSON 响应.
    /// </summary>
    /// <typeparam name="T">响应数据类型.</typeparam>
    /// <param name="pathContains">路径包含的字符串.</param>
    /// <param name="data">响应数据.</param>
    /// <param name="statusCode">HTTP 状态码.</param>
    public void SetupJsonResponse<T>(string pathContains, T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _handlers[pathContains] = _ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(data),
                System.Text.Encoding.UTF8,
                "application/json"),
        };
    }

    /// <summary>
    /// 配置特定路径返回纯文本响应.
    /// </summary>
    /// <param name="pathContains">路径包含的字符串.</param>
    /// <param name="text">响应文本.</param>
    /// <param name="statusCode">HTTP 状态码.</param>
    public void SetupTextResponse(string pathContains, string text, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _handlers[pathContains] = _ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(text, System.Text.Encoding.UTF8, "text/plain"),
        };
    }

    /// <summary>
    /// 配置特定路径返回错误响应.
    /// </summary>
    /// <param name="pathContains">路径包含的字符串.</param>
    /// <param name="statusCode">HTTP 状态码.</param>
    /// <param name="content">错误内容.</param>
    public void SetupErrorResponse(string pathContains, HttpStatusCode statusCode, string? content = null)
    {
        _handlers[pathContains] = _ => new HttpResponseMessage(statusCode)
        {
            Content = content != null
                ? new StringContent(content, System.Text.Encoding.UTF8, "text/plain")
                : null,
        };
    }

    /// <summary>
    /// 清除所有配置的处理器.
    /// </summary>
    public void Clear()
    {
        _handlers.Clear();
        _requests.Clear();
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _requests.Add(request);

        var path = request.RequestUri?.PathAndQuery ?? string.Empty;

        foreach (var (key, handler) in _handlers)
        {
            if (path.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(handler(request));
            }
        }

        // 默认返回 404
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"No handler configured for path: {path}"),
        });
    }
}
