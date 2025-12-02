// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// ZLibrary 客户端配置选项.
/// </summary>
public sealed class ZLibraryClientOptions
{
    /// <summary>
    /// 默认域名.
    /// </summary>
    public const string DefaultDomain = "https://zh.zlib.by";

    /// <summary>
    /// 默认登录路径.
    /// </summary>
    public const string LoginPath = "/rpc.php";

    /// <summary>
    /// 获取或设置自定义镜像地址.
    /// </summary>
    public string? CustomMirror { get; set; }

    /// <summary>
    /// 获取或设置请求超时时间.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(180);

    /// <summary>
    /// 获取或设置最大并发请求数.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 64;

    /// <summary>
    /// 获取或设置自定义 User-Agent.
    /// </summary>
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0";

    /// <summary>
    /// 获取或设置自定义请求头.
    /// 这些请求头将被添加到所有 HTTP 请求中.
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }

    /// <summary>
    /// 获取或设置初始 Cookies.
    /// 这些 Cookies 将在客户端初始化时设置.
    /// </summary>
    public Dictionary<string, string>? InitialCookies { get; set; }

    /// <summary>
    /// 获取有效的域名.
    /// </summary>
    /// <returns>域名地址.</returns>
    internal string GetEffectiveDomain()
    {
        if (!string.IsNullOrWhiteSpace(CustomMirror))
        {
            var mirror = CustomMirror;
            if (!mirror.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                mirror = "https://" + mirror;
            }

            return mirror.TrimEnd('/');
        }

        return DefaultDomain.TrimEnd('/');
    }

    /// <summary>
    /// 获取登录地址.
    /// </summary>
    /// <returns>登录 URL.</returns>
    internal string GetLoginUrl()
    {
        return GetEffectiveDomain() + LoginPath;
    }
}
