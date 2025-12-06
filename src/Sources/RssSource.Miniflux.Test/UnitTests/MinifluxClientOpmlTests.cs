// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Miniflux.Test.UnitTests;

/// <summary>
/// MinifluxClient OPML 操作单元测试.
/// </summary>
[TestClass]
public sealed class MinifluxClientOpmlTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private MinifluxClientOptions _options = null!;
    private MinifluxClient _client = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new MinifluxClient(_options, _httpClient);

        // 模拟登录
        _mockHandler.SetupResponse("/v1/me", TestDataFactory.CreateUserResponse());
        await _client.SignInAsync();
        _mockHandler.Clear();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public async Task ExportOpmlAsync_ShouldReturnOpmlContent()
    {
        // Arrange
        _mockHandler.SetupResponse("/v1/export", TestDataFactory.CreateOpmlExportResponse(), "application/xml");

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains("<?xml") || result.Contains("<opml"));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithInvalidResponse_ShouldReturnEmpty()
    {
        // Arrange
        _mockHandler.SetupResponse("/v1/export", "Invalid content");

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_ShouldReturnTrue()
    {
        // Arrange
        var opmlContent = TestDataFactory.CreateOpmlExportResponse();
        _mockHandler.SetupResponse("/v1/import", HttpStatusCode.Created, TestDataFactory.CreateOpmlImportResponse());

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithEmptyContent_ShouldReturnFalse()
    {
        // Arrange
        var opmlContent = string.Empty;

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var opmlContent = "Invalid OPML";
        _mockHandler.SetupErrorResponse("/v1/import", HttpStatusCode.BadRequest, "Invalid OPML format");

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsFalse(result);
    }
}
