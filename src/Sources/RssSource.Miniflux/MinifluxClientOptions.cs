// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace Richasy.RodelReader.Sources.Rss.Miniflux;

/// <summary>
/// Miniflux 客户端配置选项.
/// </summary>
public sealed class MinifluxClientOptions
{
    /// <summary>
    /// 获取或设置服务器地址.
    /// </summary>
    /// <remarks>
    /// 例如: https://miniflux.example.com
    /// </remarks>
    public required string Server { get; set; }

    /// <summary>
    /// 获取或设置用户名（HTTP Basic Auth）.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 获取或设置密码（HTTP Basic Auth）.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 获取或设置 API Token（推荐使用）.
    /// </summary>
    /// <remarks>
    /// API Token 可在 Miniflux 设置 -> API Keys 中生成.
    /// 如果同时设置了 API Token 和用户名/密码，将优先使用 API Token.
    /// </remarks>
    public string? ApiToken { get; set; }

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
    /// 获取是否配置了 API Token.
    /// </summary>
    public bool HasApiToken => !string.IsNullOrEmpty(ApiToken);

    /// <summary>
    /// 获取是否配置了用户名密码.
    /// </summary>
    public bool HasBasicAuth => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);

    /// <summary>
    /// 获取是否有有效的认证信息.
    /// </summary>
    public bool HasValidCredentials => HasApiToken || HasBasicAuth;

    /// <summary>
    /// 获取服务器基础 URL（移除尾部斜杠）.
    /// </summary>
    /// <returns>服务器基础 URL.</returns>
    public Uri GetServerBaseUrl()
        => new(Server.TrimEnd('/'));

    /// <summary>
    /// 克隆当前选项.
    /// </summary>
    /// <returns>新的选项实例.</returns>
    public MinifluxClientOptions Clone()
        => new()
        {
            Server = Server,
            UserName = UserName,
            Password = Password,
            ApiToken = ApiToken,
            Timeout = Timeout,
            MaxConcurrentRequests = MaxConcurrentRequests,
            ArticlesPerRequest = ArticlesPerRequest,
        };

    /// <summary>
    /// 生成 HTTP Basic Auth 令牌.
    /// </summary>
    /// <returns>Base64 编码的认证令牌.</returns>
    /// <exception cref="InvalidOperationException">用户名或密码为空时抛出.</exception>
    public string GenerateBasicAuthToken()
    {
        if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
        {
            throw new InvalidOperationException("用户名和密码不能为空。");
        }

        var credentials = $"{UserName}:{Password}";
        var bytes = Encoding.UTF8.GetBytes(credentials);
        return Convert.ToBase64String(bytes);
    }
}
