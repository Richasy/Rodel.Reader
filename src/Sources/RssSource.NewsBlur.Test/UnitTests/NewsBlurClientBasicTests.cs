// Copyright (c) Richasy. All rights reserved.

namespace RssSource.NewsBlur.Test.UnitTests;

/// <summary>
/// NewsBlurClient 基本功能单元测试.
/// </summary>
[TestClass]
public sealed class NewsBlurClientBasicTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private NewsBlurClientOptions _options = null!;
    private NewsBlurClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _options = TestDataFactory.CreateDefaultOptions();
        _client = new NewsBlurClient(_options, _httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [TestMethod]
    public void Capabilities_ShouldReturnNewsBlurCapabilities()
    {
        // Act
        var capabilities = _client.Capabilities;

        // Assert
        Assert.IsNotNull(capabilities);
        Assert.AreEqual("newsblur", capabilities.SourceId);
        Assert.AreEqual("NewsBlur", capabilities.DisplayName);
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
    public async Task SignInAsync_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        _mockHandler.SetupResponse("/api/login", TestDataFactory.CreateLoginSuccessResponse());

        // Act
        var result = await _client.SignInAsync();

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(_client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithInvalidCredentials_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetupResponse("/api/login", TestDataFactory.CreateLoginFailedResponse());

        // Act
        var result = await _client.SignInAsync();

        // Assert
        Assert.IsFalse(result);
        Assert.IsFalse(_client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_WithNoCredentials_ShouldReturnFalse()
    {
        // Arrange
        var options = TestDataFactory.CreateUnauthenticatedOptions();
        using var client = new NewsBlurClient(options, _httpClient);

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
        _mockHandler.SetupErrorResponse("/api/login", HttpStatusCode.InternalServerError, "Server error");

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
        _mockHandler.SetupResponse("/api/login", TestDataFactory.CreateLoginSuccessResponse());
        _mockHandler.SetupResponse("/api/logout", HttpStatusCode.OK);
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
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new NewsBlurClient(null!));
    }

    [TestMethod]
    public void Options_Clone_ShouldCreateIndependentCopy()
    {
        // Arrange
        var original = TestDataFactory.CreateDefaultOptions();

        // Act
        var clone = original.Clone();
        clone.UserName = "different-user";
        clone.Password = "different-password";

        // Assert
        Assert.AreNotEqual(original.UserName, clone.UserName);
        Assert.AreNotEqual(original.Password, clone.Password);
    }

    [TestMethod]
    public void Options_HasValidCredentials_ShouldReturnCorrectValue()
    {
        // Arrange & Act & Assert
        var withCredentials = TestDataFactory.CreateDefaultOptions();
        Assert.IsTrue(withCredentials.HasValidCredentials);

        var withoutCredentials = TestDataFactory.CreateUnauthenticatedOptions();
        Assert.IsFalse(withoutCredentials.HasValidCredentials);

        var withUsernameOnly = new NewsBlurClientOptions { UserName = "user" };
        Assert.IsFalse(withUsernameOnly.HasValidCredentials);

        var withPasswordOnly = new NewsBlurClientOptions { Password = "pass" };
        Assert.IsFalse(withPasswordOnly.HasValidCredentials);
    }

    [TestMethod]
    public async Task AnyOperation_WhenNotAuthenticated_ShouldThrowInvalidOperationException()
    {
        // Arrange - 不调用 SignIn

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _client.GetFeedListAsync());
    }
}
