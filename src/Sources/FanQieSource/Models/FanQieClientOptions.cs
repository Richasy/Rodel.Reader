// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 番茄小说客户端配置.
/// </summary>
public sealed class FanQieClientOptions
{
    /// <summary>
    /// 默认 User-Agent.
    /// </summary>
    public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";

    /// <summary>
    /// 默认 Aid.
    /// </summary>
    public const string DefaultAid = "1967";

    /// <summary>
    /// 默认 Update Version Code.
    /// </summary>
    public const string DefaultUpdateVersionCode = "60900";

    /// <summary>
    /// 请求超时时间.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// User-Agent.
    /// </summary>
    public string UserAgent { get; set; } = DefaultUserAgent;

    /// <summary>
    /// 应用 ID.
    /// </summary>
    public string Aid { get; set; } = DefaultAid;

    /// <summary>
    /// 更新版本码.
    /// </summary>
    public string UpdateVersionCode { get; set; } = DefaultUpdateVersionCode;

    /// <summary>
    /// 安装 ID（用于 Cookie）.
    /// </summary>
    public string? InstallId { get; set; }

    /// <summary>
    /// 服务器设备 ID（用于加密）.
    /// </summary>
    public string? ServerDeviceId { get; set; }

    /// <summary>
    /// 最大并发请求数.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 3;

    /// <summary>
    /// 批量下载每组章节数.
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// 请求间隔（毫秒）.
    /// </summary>
    public int RequestDelayMs { get; set; } = 500;

    /// <summary>
    /// 自定义请求头.
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }

    /// <summary>
    /// 后备 API 服务地址.
    /// </summary>
    public string FallbackApiBaseUrl { get; set; } = "https://fqnovel.richasy.net";

    /// <summary>
    /// 是否启用后备 API.
    /// </summary>
    public bool EnableFallback { get; set; } = true;
}
