// Copyright (c) Richasy. All rights reserved.

namespace RssSource.GoogleReader.Test.Integration;

/// <summary>
/// Google Reader 集成测试基类.
/// 使用真实 API 进行测试.
/// </summary>
public abstract class GoogleReaderIntegrationTestBase : IDisposable
{
    /// <summary>
    /// 测试服务器地址 (FreshRSS).
    /// </summary>
    protected const string TestServer = "https://freshrss.richasy.net/api/greader.php";

    /// <summary>
    /// 测试用户名.
    /// </summary>
    protected const string TestUserName = "Richasy";

    /// <summary>
    /// 测试密码.
    /// </summary>
    protected const string TestPassword = "Shar6501209!";

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

    protected GoogleReaderClientOptions Options { get; private set; } = null!;
    protected GoogleReaderClient Client { get; private set; } = null!;

    private bool _disposed;

    /// <summary>
    /// 初始化测试环境.
    /// </summary>
    protected async Task InitializeAsync()
    {
        Options = new GoogleReaderClientOptions
        {
            Server = TestServer,
            UserName = TestUserName,
            Password = TestPassword,
            Timeout = TimeSpan.FromSeconds(60),
            MaxConcurrentRequests = 5,
            ArticlesPerRequest = 20,
        };

        Client = new GoogleReaderClient(Options);

        // 登录
        var loginResult = await Client.SignInAsync();
        if (!loginResult)
        {
            throw new InvalidOperationException("无法登录到测试服务器，请检查凭据是否正确。");
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
