// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 客户端配置选项.
/// </summary>
public sealed class OpdsClientOptions
{
    /// <summary>
    /// 获取或设置根目录 URI.
    /// </summary>
    public Uri RootUri { get; set; } = null!;

    /// <summary>
    /// 获取或设置认证凭据.
    /// </summary>
    public NetworkCredential? Credentials { get; set; }

    /// <summary>
    /// 获取或设置请求超时时间.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 获取或设置用户代理.
    /// </summary>
    public string? UserAgent { get; set; }
}
