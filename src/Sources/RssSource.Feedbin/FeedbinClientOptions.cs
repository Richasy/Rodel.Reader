// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Feedbin;

/// <summary>
/// Feedbin 客户端配置选项.
/// </summary>
public sealed class FeedbinClientOptions
{
    /// <summary>
    /// 默认 Feedbin API 服务器地址.
    /// </summary>
    public const string DefaultServer = "https://api.feedbin.com/v2";

    /// <summary>
    /// 获取或设置服务器地址.
    /// </summary>
    /// <remarks>
    /// 默认为 https://api.feedbin.com/v2.
    /// </remarks>
    public string Server { get; set; } = DefaultServer;

    /// <summary>
    /// 获取或设置用户名.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 获取或设置密码.
    /// </summary>
    public string? Password { get; set; }

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
    /// 获取服务器基础 URL（移除尾部斜杠）.
    /// </summary>
    /// <returns>服务器基础 URL.</returns>
    public Uri GetServerBaseUrl()
        => new(Server.TrimEnd('/'));

    /// <summary>
    /// 克隆当前选项.
    /// </summary>
    /// <returns>新的选项实例.</returns>
    public FeedbinClientOptions Clone()
        => new()
        {
            Server = Server,
            UserName = UserName,
            Password = Password,
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
        var bytes = System.Text.Encoding.UTF8.GetBytes(credentials);
        return Convert.ToBase64String(bytes);
    }
}
