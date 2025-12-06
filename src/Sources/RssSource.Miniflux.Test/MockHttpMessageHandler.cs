// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace RssSource.Miniflux.Test;

/// <summary>
/// Mock HTTP 消息处理器.
/// </summary>
public sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, (HttpStatusCode StatusCode, string Content, string ContentType)> _responses = [];
    private readonly List<HttpRequestMessage> _requests = [];

    /// <summary>
    /// 获取所有发送的请求.
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

    /// <summary>
    /// 获取最后一个请求.
    /// </summary>
    public HttpRequestMessage? LastRequest => _requests.Count > 0 ? _requests[^1] : null;

    /// <summary>
    /// 设置指定路径的成功响应.
    /// </summary>
    /// <param name="pathContains">路径包含的字符串.</param>
    /// <param name="content">响应内容.</param>
    /// <param name="contentType">内容类型.</param>
    public void SetupResponse(string pathContains, string content, string contentType = "application/json")
    {
        _responses[pathContains] = (HttpStatusCode.OK, content, contentType);
    }

    /// <summary>
    /// 设置指定路径的响应.
    /// </summary>
    /// <param name="pathContains">路径包含的字符串.</param>
    /// <param name="statusCode">状态码.</param>
    /// <param name="content">响应内容.</param>
    /// <param name="contentType">内容类型.</param>
    public void SetupResponse(string pathContains, HttpStatusCode statusCode, string content = "", string contentType = "application/json")
    {
        _responses[pathContains] = (statusCode, content, contentType);
    }

    /// <summary>
    /// 设置错误响应.
    /// </summary>
    /// <param name="pathContains">路径包含的字符串.</param>
    /// <param name="statusCode">状态码.</param>
    /// <param name="errorMessage">错误消息.</param>
    public void SetupErrorResponse(string pathContains, HttpStatusCode statusCode, string errorMessage)
    {
        var content = $"{{\"error_message\": \"{errorMessage}\"}}";
        _responses[pathContains] = (statusCode, content, "application/json");
    }

    /// <summary>
    /// 清除所有响应设置.
    /// </summary>
    public void Clear()
    {
        _responses.Clear();
        _requests.Clear();
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requests.Add(request);

        var path = request.RequestUri?.PathAndQuery ?? string.Empty;

        // 按路径长度降序排列，优先匹配更精确的路径
        var orderedResponses = _responses
            .Where(kvp => path.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(kvp => kvp.Key.Length)
            .ToList();

        if (orderedResponses.Count > 0)
        {
            var (key, value) = orderedResponses[0];
            var response = new HttpResponseMessage(value.StatusCode)
            {
                Content = new StringContent(value.Content, Encoding.UTF8, value.ContentType),
                RequestMessage = request,
            };
            return Task.FromResult(response);
        }

        // 默认返回 404
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{\"error_message\": \"Not Found\"}", Encoding.UTF8, "application/json"),
            RequestMessage = request,
        });
    }
}
