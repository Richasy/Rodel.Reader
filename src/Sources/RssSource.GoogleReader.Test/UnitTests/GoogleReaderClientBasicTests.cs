// Copyright (c) Richasy. All rights reserved.

namespace RssSource.GoogleReader.Test.UnitTests;

/// <summary>
/// GoogleReaderClient 基本功能单元测试.
/// </summary>
[TestClass]
public sealed class GoogleReaderClientBasicTests
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
    public void Capabilities_ShouldReturnGoogleReaderCapabilities()
    {
        // Act
        var capabilities = _client.Capabilities;

        // Assert
        Assert.IsNotNull(capabilities);
        Assert.AreEqual("google-reader", capabilities.SourceId);
        Assert.AreEqual("Google Reader API", capabilities.DisplayName);
        Assert.IsTrue(capabilities.RequiresAuthentication);
        Assert.AreEqual(RssAuthType.Basic, capabilities.AuthType);
        Assert.IsTrue(capabilities.CanManageFeeds);
        Assert.IsTrue(capabilities.CanManageGroups);
        Assert.IsTrue(capabilities.CanMarkAsRead);
        Assert.IsFalse(capabilities.CanImportOpml);
        Assert.IsTrue(capabilities.CanExportOpml);
    }

    [TestMethod]
    public void IsAuthenticated_WithToken_ShouldReturnTrue()
    {
        // Arrange - options already has auth token

        // Act & Assert
        Assert.IsTrue(_client.IsAuthenticated);
    }

    [TestMethod]
    public void IsAuthenticated_WithoutToken_ShouldReturnFalse()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();
        using var client = new GoogleReaderClient(options, _httpClient);

        // Act & Assert
        Assert.IsFalse(client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithJsonResponse_ShouldReturnTrueAndSetToken()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();
        _mockHandler.SetupTextResponse("/accounts/ClientLogin", TestDataFactory.CreateAuthResponseJson());
        using var client = new GoogleReaderClient(options, _httpClient);

        // Act
        var result = await client.SignInAsync();

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithTextResponse_ShouldReturnTrueAndSetToken()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();
        _mockHandler.SetupTextResponse("/accounts/ClientLogin", TestDataFactory.CreateAuthResponseText());
        using var client = new GoogleReaderClient(options, _httpClient);

        // Act
        var result = await client.SignInAsync();

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithEmptyCredentials_ShouldReturnFalse()
    {
        // Arrange
        var options = new GoogleReaderClientOptions { Server = TestDataFactory.TestServer };
        using var client = new GoogleReaderClient(options, _httpClient);

        // Act
        var result = await client.SignInAsync();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task SignInAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();
        _mockHandler.SetupErrorResponse("/accounts/ClientLogin", HttpStatusCode.Unauthorized, "Invalid credentials");
        using var client = new GoogleReaderClient(options, _httpClient);

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
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new GoogleReaderClient(null!));
    }

    [TestMethod]
    public void Constructor_WithCustomOptions_ShouldApplyOptions()
    {
        // Arrange
        var options = new GoogleReaderClientOptions
        {
            Server = "https://custom.server.com/api/greader.php",
            AuthToken = "custom_token",
            Timeout = TimeSpan.FromSeconds(60),
            MaxConcurrentRequests = 20,
            ArticlesPerRequest = 50,
        };

        // Act
        using var client = new GoogleReaderClient(options, _httpClient);

        // Assert
        Assert.IsNotNull(client);
        Assert.IsTrue(client.IsAuthenticated);
    }

    [TestMethod]
    public void GetServerBaseUrl_ShouldRemoveTrailingSlash()
    {
        // Arrange
        var options = new GoogleReaderClientOptions
        {
            Server = "https://example.com/api/greader.php/",
        };

        // Act
        var baseUrl = options.GetServerBaseUrl();

        // Assert
        Assert.AreEqual(new Uri("https://example.com/api/greader.php"), baseUrl);
    }

    [TestMethod]
    public void Clone_ShouldCreateIndependentCopy()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var cloned = options.Clone();
        cloned.Server = "https://other.server.com";
        cloned.AuthToken = "other_token";

        // Assert
        Assert.AreNotEqual(options.Server, cloned.Server);
        Assert.AreNotEqual(options.AuthToken, cloned.AuthToken);
    }
}
