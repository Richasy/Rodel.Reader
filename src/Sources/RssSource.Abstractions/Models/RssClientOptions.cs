// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 客户端基础配置选项.
/// </summary>
public class RssClientOptions
{
    /// <summary>
    /// 请求超时时间.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 最大并发请求数.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 10;
}

/// <summary>
/// 需要服务器地址的 RSS 客户端配置选项.
/// </summary>
public class ServerBasedRssClientOptions : RssClientOptions
{
    /// <summary>
    /// 服务器地址.
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// 用户名.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 访问令牌（用于已登录状态的恢复）.
    /// </summary>
    public string? AccessToken { get; set; }
}

/// <summary>
/// OAuth 认证的 RSS 客户端配置选项.
/// </summary>
public class OAuthRssClientOptions : RssClientOptions
{
    /// <summary>
    /// 访问令牌.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// 刷新令牌.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 令牌过期时间.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
