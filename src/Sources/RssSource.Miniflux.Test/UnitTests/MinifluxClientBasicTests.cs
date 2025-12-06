// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Miniflux.Test.UnitTests;

/// <summary>
/// MinifluxClient 基本功能单元测试.
/// </summary>
[TestClass]
public sealed class MinifluxClientBasicTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private MinifluxClientOptions _options = null!;
    private MinifluxClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new MinifluxClient(_options, _httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public void Capabilities_ShouldReturnMinifluxCapabilities()
    {
        // Act
        var capabilities = _client.Capabilities;

        // Assert
        Assert.IsNotNull(capabilities);
        Assert.AreEqual("miniflux", capabilities.SourceId);
        Assert.AreEqual("Miniflux", capabilities.DisplayName);
        Assert.IsTrue(capabilities.RequiresAuthentication);
        Assert.AreEqual(RssAuthType.Basic, capabilities.AuthType);
        Assert.IsTrue(capabilities.CanManageFeeds);
        Assert.IsTrue(capabilities.CanManageGroups);
        Assert.IsTrue(capabilities.CanMarkAsRead);
        Assert.IsTrue(capabilities.CanImportOpml);
        Assert.IsTrue(capabilities.CanExportOpml);
    }

    [TestMethod]
    public void IsAuthenticated_BeforeSignIn_ShouldReturnFalse()
    {
        // Act & Assert
        Assert.IsFalse(_client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithApiToken_ShouldSucceed()
    {
        // Arrange
        _mockHandler.SetupResponse("/v1/me", TestDataFactory.CreateUserResponse());

        // Act
        var result = await _client.SignInAsync();

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(_client.IsAuthenticated);

        // 验证使用了正确的认证头
        var request = _mockHandler.LastRequest;
        Assert.IsNotNull(request);
        Assert.IsTrue(request.Headers.Contains("X-Auth-Token"));
    }

    [TestMethod]
    public async Task SignInAsync_WithBasicAuth_ShouldSucceed()
    {
        // Arrange
        var options = TestDataFactory.CreateBasicAuthOptions();
        using var client = new MinifluxClient(options, _httpClient);
        _mockHandler.SetupResponse("/v1/me", TestDataFactory.CreateUserResponse());

        // Act
        var result = await client.SignInAsync();

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(client.IsAuthenticated);

        // 验证使用了 Basic Auth
        var request = _mockHandler.LastRequest;
        Assert.IsNotNull(request);
        Assert.IsNotNull(request.Headers.Authorization);
        Assert.AreEqual("Basic", request.Headers.Authorization.Scheme);
    }

    [TestMethod]
    public async Task SignInAsync_WithNoCredentials_ShouldReturnFalse()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();
        using var client = new MinifluxClient(options, _httpClient);

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
        _mockHandler.SetupErrorResponse("/v1/me", HttpStatusCode.Unauthorized, "Invalid credentials");

        // Act
        var result = await _client.SignInAsync();

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(_client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignOutAsync_ShouldClearAuthenticationStatus()
    {
        // Arrange
        _mockHandler.SetupResponse("/v1/me", TestDataFactory.CreateUserResponse());
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
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new MinifluxClient(null!));
    }

    [TestMethod]
    public void Options_Clone_ShouldCreateIndependentCopy()
    {
        // Arrange
        var original = TestDataFactory.CreateDefaultOptions();

        // Act
        var clone = original.Clone();
        clone.Server = "https://different.example.com";
        clone.ApiToken = "different-token";

        // Assert
        Assert.AreNotEqual(original.Server, clone.Server);
        Assert.AreNotEqual(original.ApiToken, clone.ApiToken);
    }

    [TestMethod]
    public void Options_GenerateBasicAuthToken_ShouldCreateValidToken()
    {
        // Arrange
        var options = TestDataFactory.CreateBasicAuthOptions();

        // Act
        var token = options.GenerateBasicAuthToken();

        // Assert
        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrEmpty(token));

        // 验证 token 可以解码
        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
        Assert.AreEqual($"{TestDataFactory.TestUserName}:{TestDataFactory.TestPassword}", decoded);
    }

    [TestMethod]
    public void Options_GenerateBasicAuthToken_WithNoCredentials_ShouldThrow()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();

        // Act & Assert
        _ = Assert.ThrowsExactly<InvalidOperationException>(() => options.GenerateBasicAuthToken());
    }

    [TestMethod]
    public void Options_HasValidCredentials_ShouldReturnCorrectValue()
    {
        // Arrange & Act & Assert
        var withApiToken = TestDataFactory.CreateDefaultOptions();
        Assert.IsTrue(withApiToken.HasValidCredentials);
        Assert.IsTrue(withApiToken.HasApiToken);
        Assert.IsFalse(withApiToken.HasBasicAuth);

        var withBasicAuth = TestDataFactory.CreateBasicAuthOptions();
        Assert.IsTrue(withBasicAuth.HasValidCredentials);
        Assert.IsFalse(withBasicAuth.HasApiToken);
        Assert.IsTrue(withBasicAuth.HasBasicAuth);

        var withNothing = TestDataFactory.CreateUnauthenticatedOptions();
        Assert.IsFalse(withNothing.HasValidCredentials);
        Assert.IsFalse(withNothing.HasApiToken);
        Assert.IsFalse(withNothing.HasBasicAuth);
    }

    [TestMethod]
    public void Options_GetServerBaseUrl_ShouldRemoveTrailingSlash()
    {
        // Arrange
        var options = new MinifluxClientOptions
        {
            Server = "https://miniflux.example.com/",
        };

        // Act
        var url = options.GetServerBaseUrl();

        // Assert
        Assert.AreEqual("https://miniflux.example.com/", url.ToString());
    }
}
