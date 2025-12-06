// Copyright (c) Richasy. All rights reserved.

namespace RssSource.NewsBlur.Test.Integration;

/// <summary>
/// NewsBlur 集成测试基类.
/// 使用真实 API 进行测试.
/// </summary>
public abstract class NewsBlurIntegrationTestBase : IDisposable
{
    /// <summary>
    /// 测试用户名.
    /// </summary>
    protected const string TestUserName = "richasy";

    /// <summary>
    /// 测试密码.
    /// </summary>
    protected const string TestPassword = "shar6501209";

    /// <summary>
    /// IT之家 RSS 地址.
    /// </summary>
    protected const string IthomeRssUrl = "https://www.ithome.com/rss";

    /// <summary>
    /// 极客公园 RSS 地址.
    /// </summary>
    protected const string GeekparkRssUrl = "https://www.geekpark.net/rss";

    /// <summary>
    /// .NET Blog RSS 地址.
    /// </summary>
    protected const string DotNetBlogRssUrl = "https://devblogs.microsoft.com/dotnet/feed/";

    protected NewsBlurClientOptions Options { get; private set; } = null!;
    protected NewsBlurClient Client { get; private set; } = null!;

    private bool _disposed;

    /// <summary>
    /// 初始化测试环境.
    /// </summary>
    protected async Task InitializeAsync()
    {
        Options = new NewsBlurClientOptions
        {
            UserName = TestUserName,
            Password = TestPassword,
            Timeout = TimeSpan.FromSeconds(60),
            MaxConcurrentRequests = 5,
            PagesToFetch = 2,
        };

        Client = new NewsBlurClient(Options);

        // 登录
        var loginResult = await Client.SignInAsync();
        if (!loginResult)
        {
            throw new InvalidOperationException("无法登录到 NewsBlur，请检查凭据是否正确。");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Client?.Dispose();
        }

        _disposed = true;
    }
}
