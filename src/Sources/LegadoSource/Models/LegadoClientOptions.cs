// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.Legado.Models.Enums;

namespace Richasy.RodelReader.Sources.Legado.Models;

/// <summary>
/// Legado 客户端配置选项.
/// </summary>
public sealed class LegadoClientOptions
{
    /// <summary>
    /// 服务器基础 URL.
    /// </summary>
    /// <remarks>
    /// 例如: http://192.168.1.100:1234 或 http://your-server.com:8080
    /// </remarks>
    public required string BaseUrl { get; set; }

    /// <summary>
    /// 服务器类型.
    /// </summary>
    /// <remarks>
    /// <see cref="Enums.ServerType.Legado"/> 为 Legado 原版 API，
    /// <see cref="Enums.ServerType.HectorqinReader"/> 为 hectorqin/reader 服务器 API.
    /// </remarks>
    public ServerType ServerType { get; set; } = ServerType.Legado;

    /// <summary>
    /// 访问令牌.
    /// </summary>
    /// <remarks>
    /// 用于 hectorqin/reader 多用户模式下的身份认证.
    /// </remarks>
    public string? AccessToken { get; set; }

    /// <summary>
    /// 请求超时时间.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 是否忽略 SSL 证书验证错误.
    /// </summary>
    /// <remarks>
    /// 对于自签名证书的本地服务器，可能需要设置为 <c>true</c>.
    /// </remarks>
    public bool IgnoreSslErrors { get; set; } = true;

    /// <summary>
    /// User-Agent 字符串.
    /// </summary>
    public string UserAgent { get; set; } = "RodelReader/1.0";
}
