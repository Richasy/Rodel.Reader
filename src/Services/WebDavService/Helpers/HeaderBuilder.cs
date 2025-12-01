// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// HTTP 请求头构建器.
/// </summary>
internal sealed class HeaderBuilder
{
    private readonly Dictionary<string, string> _headers = [];

    /// <summary>
    /// 添加请求头.
    /// </summary>
    /// <param name="name">请求头名称.</param>
    /// <param name="value">请求头值.</param>
    /// <returns>当前构建器实例.</returns>
    public HeaderBuilder Add(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    /// <summary>
    /// 添加请求头（如果不存在）.
    /// </summary>
    /// <param name="name">请求头名称.</param>
    /// <param name="value">请求头值.</param>
    /// <returns>当前构建器实例.</returns>
    public HeaderBuilder AddIfNotExists(string name, string value)
    {
        _headers.TryAdd(name, value);
        return this;
    }

    /// <summary>
    /// 添加请求头并允许覆盖.
    /// </summary>
    /// <param name="headers">要添加的请求头.</param>
    /// <returns>当前构建器实例.</returns>
    public HeaderBuilder AddWithOverwrite(IDictionary<string, string>? headers)
    {
        if (headers == null)
        {
            return this;
        }

        foreach (var header in headers)
        {
            _headers[header.Key] = header.Value;
        }

        return this;
    }

    /// <summary>
    /// 构建请求头字典.
    /// </summary>
    /// <returns>请求头字典.</returns>
    public IDictionary<string, string> Build() => _headers;
}
