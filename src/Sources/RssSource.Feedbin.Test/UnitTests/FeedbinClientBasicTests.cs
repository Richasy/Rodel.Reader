// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Feedbin.Test.UnitTests;

/// <summary>
/// FeedbinClient 基本功能单元测试.
/// </summary>
[TestClass]
public sealed class FeedbinClientBasicTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private FeedbinClientOptions _options = null!;
    private FeedbinClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _mockHandler.SetupTextResponse("/authentication.json", "{}", HttpStatusCode.OK);
        _client = new FeedbinClient(_options, _httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public void Capabilities_ShouldReturnFeedbinCapabilities()
    {
        // Act
        var capabilities = _client.Capabilities;

        // Assert
        Assert.IsNotNull(capabilities);
        Assert.AreEqual("feedbin", capabilities.SourceId);
        Assert.AreEqual("Feedbin", capabilities.DisplayName);
        Assert.IsTrue(capabilities.RequiresAuthentication);
        Assert.AreEqual(RssAuthType.Basic, capabilities.AuthType);
        Assert.IsTrue(capabilities.CanManageFeeds);
        Assert.IsFalse(capabilities.CanManageGroups); // Feedbin 不支持分组管理
        Assert.IsTrue(capabilities.CanMarkAsRead);
        Assert.IsTrue(capabilities.CanImportOpml);
        Assert.IsTrue(capabilities.CanExportOpml);
    }

    [TestMethod]
    public void IsAuthenticated_BeforeSignIn_ShouldReturnFalse()
    {
        // Assert
        Assert.IsFalse(_client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithValidCredentials_ShouldReturnTrueAndSetAuthenticated()
    {
        // Act
        var result = await _client.SignInAsync();

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(_client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithEmptyCredentials_ShouldReturnFalse()
    {
        // Arrange
        var options = TestDataFactory.CreateEmptyCredentialsOptions();
        using var client = new FeedbinClient(options, _httpClient);

        // Act
        var result = await client.SignInAsync();

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithServerError_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.Clear();
        _mockHandler.SetupErrorResponse("/authentication.json", HttpStatusCode.Unauthorized, "Invalid credentials");
        using var client = new FeedbinClient(_options, _httpClient);

        // Act
        var result = await client.SignInAsync();

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignOutAsync_ShouldClearAuthenticationAndReturnTrue()
    {
        // Arrange
        await _client.SignInAsync();
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
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new FeedbinClient(null!));
    }

    [TestMethod]
    public void Constructor_WithCustomOptions_ShouldApplyOptions()
    {
        // Arrange
        var options = new FeedbinClientOptions
        {
            Server = "https://custom.feedbin.com/v2",
            UserName = "customuser",
            Password = "custompassword",
            Timeout = TimeSpan.FromSeconds(60),
            MaxConcurrentRequests = 20,
            ArticlesPerRequest = 50,
        };

        // Act
        using var client = new FeedbinClient(options, _httpClient);

        // Assert
        Assert.IsNotNull(client);
        Assert.AreEqual("feedbin", client.Capabilities.SourceId);
    }

    [TestMethod]
    public void GetServerBaseUrl_ShouldRemoveTrailingSlash()
    {
        // Arrange
        var options = new FeedbinClientOptions
        {
            Server = "https://api.feedbin.com/v2/",
        };

        // Act
        var baseUrl = options.GetServerBaseUrl();

        // Assert
        Assert.AreEqual(new Uri("https://api.feedbin.com/v2"), baseUrl);
    }

    [TestMethod]
    public void Clone_ShouldCreateIndependentCopy()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var cloned = options.Clone();
        cloned.Server = "https://other.server.com";
        cloned.UserName = "other_user";

        // Assert
        Assert.AreNotEqual(options.Server, cloned.Server);
        Assert.AreNotEqual(options.UserName, cloned.UserName);
        Assert.AreEqual(TestDataFactory.TestServer, options.Server);
    }

    [TestMethod]
    public void GenerateBasicAuthToken_WithValidCredentials_ShouldReturnBase64Token()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var token = options.GenerateBasicAuthToken();

        // Assert
        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrEmpty(token));

        // Verify it's valid Base64
        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
        Assert.AreEqual($"{TestDataFactory.TestUserName}:{TestDataFactory.TestPassword}", decoded);
    }

    [TestMethod]
    public void GenerateBasicAuthToken_WithEmptyCredentials_ShouldThrowException()
    {
        // Arrange
        var options = TestDataFactory.CreateEmptyCredentialsOptions();

        // Act & Assert
        _ = Assert.ThrowsExactly<InvalidOperationException>(() => options.GenerateBasicAuthToken());
    }
}
