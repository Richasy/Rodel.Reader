// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;

namespace BookScraper.Test.Integration;

/// <summary>
/// 刮削器集成测试基类.
/// </summary>
public abstract class ScraperIntegrationTestBase
{
    private ServiceProvider? _serviceProvider;

    protected IServiceProvider ServiceProvider => _serviceProvider!;

    protected IBrowsingContextFactory BrowsingContextFactory => ServiceProvider.GetRequiredService<IBrowsingContextFactory>();

    protected IHttpClientFactory HttpClientFactory => ServiceProvider.GetRequiredService<IHttpClientFactory>();

    [TestInitialize]
    public virtual void TestSetup()
    {
        var services = new ServiceCollection();

        // 添加日志
        services.AddLogging(builder => builder
            .AddDebug()
            .SetMinimumLevel(LogLevel.Debug));

        // 添加基础设施
        services.AddSingleton<IBrowsingContextFactory, BrowsingContextFactory>();

        // 配置 HttpClient
        services.AddHttpClient(HttpClientNames.Scraper, client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient(HttpClientNames.DouBan, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient(HttpClientNames.QiDian, client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
    }

    /// <summary>
    /// 子类可以在此添加额外的服务配置.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected T GetService<T>()
        where T : notnull
        => ServiceProvider.GetRequiredService<T>();

    protected T? GetOptionalService<T>()
        => ServiceProvider.GetService<T>();
}
