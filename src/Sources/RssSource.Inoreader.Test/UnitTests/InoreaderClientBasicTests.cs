// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test.UnitTests;

/// <summary>
/// InoreaderClient 基本功能单元测试.
/// </summary>
[TestClass]
public sealed class InoreaderClientBasicTests
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
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public void Capabilities_ShouldReturnInoreaderCapabilities()
    {
        // Act
        var capabilities = _client.Capabilities;

        // Assert
        Assert.IsNotNull(capabilities);
        Assert.AreEqual("inoreader", capabilities.SourceId);
        Assert.AreEqual("Inoreader", capabilities.DisplayName);
        Assert.IsTrue(capabilities.RequiresAuthentication);
        Assert.AreEqual(RssAuthType.OAuth, capabilities.AuthType);
        Assert.IsTrue(capabilities.CanManageFeeds);
        Assert.IsTrue(capabilities.CanManageGroups);
        Assert.IsTrue(capabilities.CanMarkAsRead);
        Assert.IsTrue(capabilities.CanImportOpml);
        Assert.IsTrue(capabilities.CanExportOpml);
    }

    [TestMethod]
    public void IsAuthenticated_WithToken_ShouldReturnTrue()
    {
        // Arrange - options already has access token

        // Act & Assert
        Assert.IsTrue(_client.IsAuthenticated);
    }

    [TestMethod]
    public void IsAuthenticated_WithoutToken_ShouldReturnFalse()
    {
        // Arrange
        var options = new InoreaderClientOptions();
        using var client = new InoreaderClient(options, _httpClient);

        // Act & Assert
        Assert.IsFalse(client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithValidToken_ShouldReturnTrue()
    {
        // Act
        var result = await _client.SignInAsync();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task SignInAsync_WithoutToken_ShouldReturnFalse()
    {
        // Arrange
        var options = new InoreaderClientOptions();
        using var client = new InoreaderClient(options, _httpClient);

        // Act
        var result = await client.SignInAsync();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task SignOutAsync_ShouldClearTokenAndReturnTrue()
    {
        // Arrange - client has token initially
        Assert.IsTrue(_client.IsAuthenticated);

        // Act
        var result = await _client.SignOutAsync();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(_client.IsAuthenticated);
    }

    [TestMethod]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new InoreaderClient(null!));
    }

    [TestMethod]
    public void Constructor_WithCustomOptions_ShouldApplyOptions()
    {
        // Arrange
        var options = new InoreaderClientOptions
        {
            AccessToken = "custom_token",
            DataSource = InoreaderDataSource.Japan,
            Timeout = TimeSpan.FromSeconds(60),
            MaxConcurrentRequests = 20,
            ArticlesPerRequest = 50,
        };

        // Act
        using var client = new InoreaderClient(options, _httpClient);

        // Assert
        Assert.IsNotNull(client);
        Assert.IsTrue(client.IsAuthenticated);
    }
}
