// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.NewsBlur;

/// <summary>
/// NewsBlur 客户端配置选项.
/// </summary>
public sealed class NewsBlurClientOptions
{
    /// <summary>
    /// NewsBlur API 基础地址.
    /// </summary>
    public const string BaseUrl = "https://newsblur.com";

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
    /// 获取或设置每次获取文章的页数.
    /// </summary>
    /// <remarks>
    /// NewsBlur 每页返回约 6-10 篇文章，默认获取 4 页.
    /// </remarks>
    public int PagesToFetch { get; set; } = 4;

    /// <summary>
    /// 获取是否有有效的凭据.
    /// </summary>
    public bool HasValidCredentials =>
        !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

    /// <summary>
    /// 克隆当前配置.
    /// </summary>
    /// <returns>新的配置实例.</returns>
    public NewsBlurClientOptions Clone()
        => new()
        {
            UserName = UserName,
            Password = Password,
            Timeout = Timeout,
            MaxConcurrentRequests = MaxConcurrentRequests,
            PagesToFetch = PagesToFetch,
        };
}
