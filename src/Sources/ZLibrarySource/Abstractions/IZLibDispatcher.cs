// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary;

/// <summary>
/// HTTP 请求分发器接口.
/// </summary>
internal interface IZLibDispatcher
{
    /// <summary>
    /// 获取或设置 Cookies.
    /// </summary>
    Dictionary<string, string>? Cookies { get; set; }

    /// <summary>
    /// 发送 GET 请求.
    /// </summary>
    /// <param name="url">请求 URL.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应内容.</returns>
    Task<string> GetAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送 POST 请求.
    /// </summary>
    /// <param name="url">请求 URL.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>响应内容和 Set-Cookie 头.</returns>
    Task<(string Content, IEnumerable<string> SetCookies)> PostAsync(
        string url,
        Dictionary<string, string> formData,
        CancellationToken cancellationToken = default);
}
