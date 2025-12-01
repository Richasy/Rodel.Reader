// Copyright (c) Richasy. All rights reserved.

using System.Net.Http.Headers;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 请求参数基类.
/// </summary>
public abstract class RequestParameters
{
    /// <summary>
    /// 获取或设置自定义请求头.
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// 获取或设置取消令牌.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// 获取或设置内容类型.
    /// </summary>
    public MediaTypeHeaderValue? ContentType { get; set; }
}
