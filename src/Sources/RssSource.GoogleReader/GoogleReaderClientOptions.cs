// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.GoogleReader;

/// <summary>
/// Google Reader 客户端配置选项.
/// </summary>
public sealed class GoogleReaderClientOptions
{
    /// <summary>
    /// 获取或设置服务器地址.
    /// </summary>
    /// <remarks>
    /// 例如: https://freshrss.example.com/api/greader.php
    /// </remarks>
    public required string Server { get; set; }

    /// <summary>
    /// 获取或设置用户名.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 获取或设置密码.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 获取或设置认证令牌.
    /// </summary>
    /// <remarks>
    /// 登录成功后由服务端返回，后续请求使用此令牌进行认证.
    /// </remarks>
    public string? AuthToken { get; set; }

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
    /// 当 Token 更新后会调用此回调，应用层可以保存新的 Token.
    /// </summary>
    public Action<string>? OnTokenUpdated { get; set; }

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
    public GoogleReaderClientOptions Clone()
        => new()
        {
            Server = Server,
            UserName = UserName,
            Password = Password,
            AuthToken = AuthToken,
            Timeout = Timeout,
            MaxConcurrentRequests = MaxConcurrentRequests,
            ArticlesPerRequest = ArticlesPerRequest,
            OnTokenUpdated = OnTokenUpdated,
        };
}
