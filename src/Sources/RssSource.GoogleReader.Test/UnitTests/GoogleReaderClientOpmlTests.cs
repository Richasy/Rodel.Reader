// Copyright (c) Richasy. All rights reserved.

namespace RssSource.GoogleReader.Test.UnitTests;

/// <summary>
/// GoogleReaderClient OPML 功能单元测试.
/// </summary>
[TestClass]
public sealed class GoogleReaderClientOpmlTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private GoogleReaderClientOptions _options = null!;
    private GoogleReaderClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new GoogleReaderClient(_options, _httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public async Task ImportOpmlAsync_ShouldReturnTrue()
    {
        // Arrange
        _mockHandler.SetupTextResponse("/subscription/import", "OK", HttpStatusCode.OK);
        var opmlContent = TestDataFactory.CreateOpmlContent();

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithFailure_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetupErrorResponse("/subscription/import", HttpStatusCode.BadRequest, "Invalid OPML");
        var opmlContent = TestDataFactory.CreateOpmlContent();

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => _client.ImportOpmlAsync(string.Empty));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithServerSupport_ShouldReturnServerOpml()
    {
        // Arrange
        var serverOpml = TestDataFactory.CreateOpmlContent();
        _mockHandler.SetupTextResponse("/subscription/export", serverOpml);

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<?xml"));
        Assert.IsTrue(result.Contains("<opml"));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithoutServerSupport_ShouldGenerateLocally()
    {
        // Arrange
        _mockHandler.SetupErrorResponse("/subscription/export", HttpStatusCode.NotFound);
        _mockHandler.SetupTextResponse("/subscription/list", TestDataFactory.CreateSubscriptionListJson());

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("<?xml"));
        Assert.IsTrue(result.Contains("<opml"));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithoutAuth_ShouldThrowException()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();
        using var client = new GoogleReaderClient(options, _httpClient);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.ExportOpmlAsync());
    }
}
