// Copyright (c) Richasy. All rights reserved.

namespace RssSource.NewsBlur.Test.UnitTests;

/// <summary>
/// NewsBlurClient OPML 操作单元测试.
/// </summary>
[TestClass]
public sealed class NewsBlurClientOpmlTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private NewsBlurClientOptions _options = null!;
    private NewsBlurClient _client = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new NewsBlurClient(_options, _httpClient);

        // 模拟登录成功
        _mockHandler.SetupResponse("/api/login", TestDataFactory.CreateLoginSuccessResponse());
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
    public async Task ExportOpmlAsync_ShouldReturnOpmlContent()
    {
        // Arrange
        _mockHandler.SetupResponse("/import/opml_export", TestDataFactory.CreateOpmlExportContent(), "application/xml");

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.StartsWith("<?xml", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithNonXmlResponse_ShouldReturnEmpty()
    {
        // Arrange
        _mockHandler.SetupResponse("/import/opml_export", "Not an XML response");

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithServerError_ShouldReturnEmpty()
    {
        // Arrange
        _mockHandler.SetupErrorResponse("/import/opml_export", HttpStatusCode.InternalServerError, "Server error");

        // Act
        var result = await _client.ExportOpmlAsync();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_ShouldSucceed()
    {
        // Arrange
        var opmlContent = TestDataFactory.CreateOpmlExportContent();
        _mockHandler.SetupResponse("/import/opml_upload", TestDataFactory.CreateOperationSuccessResponse());

        // Act
        var result = await _client.ImportOpmlAsync(opmlContent);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var opmlContent = TestDataFactory.CreateOpmlExportContent();
        _mockHandler.SetupErrorResponse("/import/opml_upload", HttpStatusCode.InternalServerError, "Server error");

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
    public async Task ImportOpmlAsync_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _client.ImportOpmlAsync(null!));
    }
}
