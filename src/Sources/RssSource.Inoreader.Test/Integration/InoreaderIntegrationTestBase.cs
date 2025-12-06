// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test.Integration;

/// <summary>
/// Inoreader 集成测试基类.
/// 使用 Mock HTTP 处理器模拟完整的 API 交互.
/// </summary>
public abstract class InoreaderIntegrationTestBase : IDisposable
{
    protected MockHttpMessageHandler MockHandler { get; private set; } = null!;
    protected HttpClient HttpClient { get; private set; } = null!;
    protected InoreaderClientOptions Options { get; set; } = null!;
    protected InoreaderClient Client { get; set; } = null!;

    private bool _disposed;

    /// <summary>
    /// 初始化测试环境.
    /// </summary>
    protected void Initialize()
    {
        MockHandler = new MockHttpMessageHandler();
        HttpClient = new HttpClient(MockHandler);
        Options = TestDataFactory.CreateDefaultOptions();

        // 设置标准 API 响应
        SetupStandardResponses();

        Client = new InoreaderClient(Options, HttpClient);
    }

    /// <summary>
    /// 设置标准 API 响应.
    /// </summary>
    protected virtual void SetupStandardResponses()
    {
        // 订阅列表
        MockHandler.SetupTextResponse("/subscription/list", TestDataFactory.CreateSubscriptionListJson());

        // 标签列表
        MockHandler.SetupTextResponse("/tag/list", TestDataFactory.CreateTagListJson());

        // 偏好设置
        MockHandler.SetupTextResponse("/preference/stream/list", TestDataFactory.CreatePreferenceJson());

        // 订阅操作
        MockHandler.SetupTextResponse("/subscription/edit", "OK");

        // 标签操作
        MockHandler.SetupTextResponse("/rename-tag", "OK");
        MockHandler.SetupTextResponse("/disable-tag", "OK");

        // 已读标记
        MockHandler.SetupTextResponse("/edit-tag", "OK");
        MockHandler.SetupTextResponse("/mark-all-as-read", "OK");

        // OPML 导入
        MockHandler.SetupTextResponse("/subscription/import", "OK", HttpStatusCode.OK);

        // OAuth
        MockHandler.SetupTextResponse("/oauth2/token", TestDataFactory.CreateAuthTokenJson());
    }

    /// <summary>
    /// 设置特定订阅源的文章流响应.
    /// </summary>
    protected void SetupStreamContentResponse(string feedId)
    {
        // URL 编码的 feed ID
        var encodedFeedId = Uri.EscapeDataString(feedId);
        MockHandler.SetupTextResponse(encodedFeedId, TestDataFactory.CreateStreamContentJson(feedId));
    }

    /// <summary>
    /// 设置所有测试订阅源的文章流响应.
    /// </summary>
    protected void SetupAllStreamContentResponses()
    {
        SetupStreamContentResponse(TestDataFactory.IthomeFeed.Id);
        SetupStreamContentResponse(TestDataFactory.GeekparkFeed.Id);
        SetupStreamContentResponse(TestDataFactory.DotNetBlogFeed.Id);
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
            HttpClient?.Dispose();
            MockHandler?.Dispose();
        }

        _disposed = true;
    }
}
