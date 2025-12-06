// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Feedbin.Test.UnitTests;

/// <summary>
/// FeedbinClient OPML 导入导出单元测试.
/// </summary>
[TestClass]
public sealed class FeedbinClientOpmlTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private FeedbinClientOptions _options = null!;
    private FeedbinClient _client = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _mockHandler.SetupTextResponse("/authentication.json", "{}", HttpStatusCode.OK);
        _client = new FeedbinClient(_options, _httpClient);
        await _client.SignInAsync();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public async Task ImportOpmlAsync_ShouldReturnTrueOnSuccess()
    {
        // Arrange
        var opmlContent = """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="2.0">
            <head>
                <title>Test Subscriptions</title>
            </head>
            <body>
                <outline text="IT之家" type="rss" xmlUrl="https://www.ithome.com/rss" />
            </body>
        </opml>
        """;

        _mockHandler.SetupTextResponse("/imports.json", TestDataFactory.CreateImportResponseJson(), HttpStatusCode.OK);

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var opmlContent = "<opml><body></body></opml>";
        _mockHandler.SetupErrorResponse("/imports.json", HttpStatusCode.BadRequest, "Invalid OPML");

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
    }

    [TestMethod]
    public async Task ExportOpmlAsync_ShouldGenerateValidOpml()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscriptions.json", TestDataFactory.CreateSubscriptionListJson());
        _mockHandler.SetupTextResponse("/taggings.json", TestDataFactory.CreateTaggingsListJson());

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<opml", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(result.Contains("IT之家", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(result.Contains("极客公园", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(result.Contains(".NET Blog", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithEmptyFeeds_ShouldGenerateEmptyOpml()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscriptions.json", "[]");
        _mockHandler.SetupTextResponse("/taggings.json", "[]");

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<opml", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithoutAuth_ShouldThrowException()
    {
        // Arrange
        await _client.SignOutAsync();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _client.ExportOpmlAsync());
    }
}
