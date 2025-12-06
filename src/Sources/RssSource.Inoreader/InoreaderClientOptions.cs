// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Inoreader;

/// <summary>
/// Inoreader 数据源服务器.
/// </summary>
public enum InoreaderDataSource
{
    /// <summary>
    /// 默认服务器 (www.inoreader.com).
    /// </summary>
    Default,

    /// <summary>
    /// 镜像服务器 (www.innoreader.com).
    /// </summary>
    Mirror,

    /// <summary>
    /// 日本服务器 (jp.inoreader.com).
    /// </summary>
    Japan,
}

/// <summary>
/// Inoreader 客户端配置选项.
/// </summary>
public sealed class InoreaderClientOptions
{
    /// <summary>
    /// 默认客户端 ID.
    /// </summary>
    public const string DefaultClientId = "999999903";

    /// <summary>
    /// 默认客户端密钥.
    /// </summary>
    public const string DefaultClientSecret = "Zu7l4W4QYOgznj1n7D1hUpGiM_NZgFt0";

    /// <summary>
    /// 默认重定向 URI.
    /// </summary>
    public const string DefaultRedirectUri = "readercop://inoreader";

    /// <summary>
    /// 获取或设置客户端 ID.
    /// </summary>
    public string ClientId { get; set; } = DefaultClientId;

    /// <summary>
    /// 获取或设置客户端密钥.
    /// </summary>
    public string ClientSecret { get; set; } = DefaultClientSecret;

    /// <summary>
    /// 获取或设置重定向 URI.
    /// </summary>
    public string RedirectUri { get; set; } = DefaultRedirectUri;

    /// <summary>
    /// 获取或设置访问令牌.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// 获取或设置刷新令牌.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 获取或设置令牌过期时间.
    /// </summary>
    public DateTimeOffset? ExpireTime { get; set; }

    /// <summary>
    /// 获取或设置数据源服务器.
    /// </summary>
    public InoreaderDataSource DataSource { get; set; } = InoreaderDataSource.Default;

    /// <summary>
    /// 获取或设置请求超时时间.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 获取或设置最大并发请求数.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 10;

    /// <summary>
    /// 获取或设置每次获取文章的数量.
    /// </summary>
    public int ArticlesPerRequest { get; set; } = 100;

    /// <summary>
    /// 获取或设置 Token 更新回调.
    /// 当 Token 刷新后会调用此回调，应用层可以保存新的 Token.
    /// </summary>
    public Action<TokenUpdateEventArgs>? OnTokenUpdated { get; set; }

    /// <summary>
    /// 获取基础 URL.
    /// </summary>
    /// <returns>基础 URL.</returns>
    public Uri GetBaseUrl()
    {
        var url = DataSource switch
        {
            InoreaderDataSource.Default => "https://www.inoreader.com",
            InoreaderDataSource.Mirror => "https://www.innoreader.com",
            InoreaderDataSource.Japan => "https://jp.inoreader.com",
            _ => "https://www.inoreader.com",
        };
        return new Uri(url);
    }

    /// <summary>
    /// 获取 API 基础 URL.
    /// </summary>
    /// <returns>API 基础 URL.</returns>
    public Uri GetApiBaseUrl()
        => new(GetBaseUrl(), "/reader/api/0");

    /// <summary>
    /// 克隆当前选项.
    /// </summary>
    /// <returns>新的选项实例.</returns>
    public InoreaderClientOptions Clone()
        => new()
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            RedirectUri = RedirectUri,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            ExpireTime = ExpireTime,
            DataSource = DataSource,
            Timeout = Timeout,
            MaxConcurrentRequests = MaxConcurrentRequests,
            ArticlesPerRequest = ArticlesPerRequest,
            OnTokenUpdated = OnTokenUpdated,
        };
}

/// <summary>
/// Token 更新事件参数.
/// </summary>
public sealed class TokenUpdateEventArgs
{
    /// <summary>
    /// 获取新的访问令牌.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// 获取新的刷新令牌.
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// 获取新的过期时间.
    /// </summary>
    public required DateTimeOffset ExpireTime { get; init; }
}
