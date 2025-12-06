// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test.UnitTests;

/// <summary>
/// InoreaderClient OPML 操作单元测试.
/// </summary>
[TestClass]
public sealed class InoreaderClientOpmlTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private InoreaderClientOptions _options = null!;
    private InoreaderClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new InoreaderClient(_options, _httpClient);

        // 设置默认响应
        _mockHandler.SetupTextResponse("/subscription/list", TestDataFactory.CreateSubscriptionListJson());
        _mockHandler.SetupTextResponse("/tag/list", TestDataFactory.CreateTagListJson());
        _mockHandler.SetupTextResponse("/preference/stream/list", TestDataFactory.CreatePreferenceJson());
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public async Task ImportOpmlAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/import", "OK", HttpStatusCode.OK);

        var opmlContent = """
            <?xml version="1.0" encoding="UTF-8"?>
            <opml version="2.0">
                <head><title>Test OPML</title></head>
                <body>
                    <outline text="Test Feed" type="rss" xmlUrl="https://example.com/rss" htmlUrl="https://example.com"/>
                </body>
            </opml>
            """;

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/import", "OK", HttpStatusCode.OK);

        var opmlContent = """
            <?xml version="1.0" encoding="UTF-8"?>
            <opml version="2.0">
                <body></body>
            </opml>
            """;

        // Act
        await _client.ImportOpmlAsync(opmlContent);

        // Assert
        var request = _mockHandler.Requests.First(r => r.RequestUri!.PathAndQuery.Contains("/subscription/import"));
        Assert.AreEqual(HttpMethod.Post, request.Method);

        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("opml"));
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetupErrorResponse("/subscription/import", HttpStatusCode.BadRequest);

        var opmlContent = "invalid opml";

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.ImportOpmlAsync(null!));
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => _client.ImportOpmlAsync(string.Empty));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => _client.ImportOpmlAsync("   "));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_ShouldReturnValidOpml()
    {
        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<?xml"));
        Assert.IsTrue(result.Contains("<opml"));
        Assert.IsTrue(result.Contains("</opml>"));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_ShouldContainFeeds()
    {
        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsTrue(result.Contains("IT之家") || result.Contains("IT%E4%B9%8B%E5%AE%B6"));
        Assert.IsTrue(result.Contains("ithome.com"));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_ShouldContainGroups()
    {
        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        // OPML 应该包含分组作为 outline 容器
        Assert.IsTrue(result.Contains("科技") || result.Contains("%E7%A7%91%E6%8A%80"));
        Assert.IsTrue(result.Contains("开发") || result.Contains("%E5%BC%80%E5%8F%91"));
    }
}
