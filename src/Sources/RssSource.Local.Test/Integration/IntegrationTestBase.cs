// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.Integration;

/// <summary>
/// LocalRssClient 集成测试基类.
/// 提供通用的测试基础设施.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected string TestDbPath { get; private set; } = null!;
    protected IRssStorage Storage { get; private set; } = null!;
    protected LocalRssClient Client { get; private set; } = null!;
    private bool _disposed;

    /// <summary>
    /// IT之家 RSS 订阅源.
    /// </summary>
    protected static RssFeed IthomeFeed => new()
    {
        Name = "IT之家",
        Url = "https://www.ithome.com/rss",
        Website = "https://www.ithome.com",
    };

    /// <summary>
    /// 极客公园 RSS 订阅源.
    /// </summary>
    protected static RssFeed GeekparkFeed => new()
    {
        Name = "极客公园",
        Url = "https://www.geekpark.net/rss",
        Website = "https://www.geekpark.net",
    };

    /// <summary>
    /// .NET Blog RSS 订阅源.
    /// </summary>
    protected static RssFeed DotNetBlogFeed => new()
    {
        Name = ".NET Blog",
        Url = "https://devblogs.microsoft.com/dotnet/feed/",
        Website = "https://devblogs.microsoft.com/dotnet",
    };

    protected async Task InitializeAsync()
    {
        // 创建临时数据库路径
        TestDbPath = Path.Combine(Path.GetTempPath(), $"rss_test_{Guid.NewGuid():N}.db");

        var options = new RssStorageOptions
        {
            DatabasePath = TestDbPath,
            CreateTablesOnInit = true,
        };

        Storage = new RssStorage(options);
        await Storage.InitializeAsync();

        var clientOptions = new LocalRssClientOptions
        {
            Timeout = TimeSpan.FromSeconds(30),
            MaxConcurrentRequests = 3,
        };

        Client = new LocalRssClient(Storage, clientOptions);
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
            Storage?.Dispose();

            // 清理测试数据库文件
            try
            {
                if (File.Exists(TestDbPath))
                {
                    File.Delete(TestDbPath);
                }
            }
            catch
            {
                // 忽略清理错误
            }
        }

        _disposed = true;
    }
}
