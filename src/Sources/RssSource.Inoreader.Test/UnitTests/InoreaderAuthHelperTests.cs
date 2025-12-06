// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test.UnitTests;

/// <summary>
/// InoreaderAuthHelper 单元测试.
/// </summary>
[TestClass]
public sealed class InoreaderAuthHelperTests
{
    [TestMethod]
    public void GetAuthorizationUrl_ShouldReturnValidUrl()
    {
        // Arrange
        var options = new InoreaderClientOptions
        {
            ClientId = "test_client_id",
            RedirectUri = "test://callback",
            DataSource = InoreaderDataSource.Default,
        };

        // Act
        var url = InoreaderAuthHelper.GetAuthorizationUrl(options, "test_state");

        // Assert
        Assert.IsNotNull(url);
        var urlString = url.AbsoluteUri;
        Assert.IsTrue(urlString.StartsWith("https://www.inoreader.com/oauth2/auth", StringComparison.Ordinal));
        Assert.IsTrue(urlString.Contains("client_id=test_client_id"));
        Assert.IsTrue(urlString.Contains("redirect_uri=test%3A%2F%2Fcallback"));
        Assert.IsTrue(urlString.Contains("response_type=code"));
        Assert.IsTrue(urlString.Contains("scope=read%20write"));
        Assert.IsTrue(urlString.Contains("state=test_state"));
    }

    [TestMethod]
    public void GetAuthorizationUrl_WithJapanDataSource_ShouldUseJapanUrl()
    {
        // Arrange
        var options = new InoreaderClientOptions
        {
            DataSource = InoreaderDataSource.Japan,
        };

        // Act
        var url = InoreaderAuthHelper.GetAuthorizationUrl(options);

        // Assert
        Assert.IsTrue(url.AbsoluteUri.StartsWith("https://jp.inoreader.com/oauth2/auth", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GetAuthorizationUrl_WithMirrorDataSource_ShouldUseMirrorUrl()
    {
        // Arrange
        var options = new InoreaderClientOptions
        {
            DataSource = InoreaderDataSource.Mirror,
        };

        // Act
        var url = InoreaderAuthHelper.GetAuthorizationUrl(options);

        // Assert
        Assert.IsTrue(url.AbsoluteUri.StartsWith("https://www.innoreader.com/oauth2/auth", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GetAuthorizationUrl_WithoutState_ShouldGenerateRandomState()
    {
        // Arrange
        var options = new InoreaderClientOptions();

        // Act
        var url1 = InoreaderAuthHelper.GetAuthorizationUrl(options);
        var url2 = InoreaderAuthHelper.GetAuthorizationUrl(options);

        // Assert
        Assert.IsTrue(url1.AbsoluteUri.Contains("state="));
        Assert.IsTrue(url2.AbsoluteUri.Contains("state="));
        // 两次生成的 state 应该不同
        Assert.AreNotEqual(url1.AbsoluteUri, url2.AbsoluteUri);
    }

    [TestMethod]
    public void GetAuthorizationUrl_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => InoreaderAuthHelper.GetAuthorizationUrl(null!));
    }

    [TestMethod]
    public async Task ExchangeCodeForTokenAsync_ShouldReturnTokenInfo()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.SetupTextResponse("/oauth2/token", TestDataFactory.CreateAuthTokenJson());

        using var httpClient = new HttpClient(mockHandler);
        var options = new InoreaderClientOptions();

        // Act
        var result = await InoreaderAuthHelper.ExchangeCodeForTokenAsync("test_code", options, httpClient);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("new_access_token", result.AccessToken);
        Assert.AreEqual("new_refresh_token", result.RefreshToken);
        Assert.IsTrue(result.ExpireTime > DateTimeOffset.Now);
    }

    [TestMethod]
    public async Task ExchangeCodeForTokenAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.SetupTextResponse("/oauth2/token", TestDataFactory.CreateAuthTokenJson());

        using var httpClient = new HttpClient(mockHandler);
        var options = new InoreaderClientOptions
        {
            ClientId = "my_client_id",
            ClientSecret = "my_client_secret",
            RedirectUri = "my://redirect",
        };

        // Act
        await InoreaderAuthHelper.ExchangeCodeForTokenAsync("auth_code", options, httpClient);

        // Assert
        var request = mockHandler.Requests[0];
        Assert.AreEqual(HttpMethod.Post, request.Method);

        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("code=auth_code"));
        Assert.IsTrue(content.Contains("client_id=my_client_id"));
        Assert.IsTrue(content.Contains("client_secret=my_client_secret"));
        Assert.IsTrue(content.Contains("grant_type=authorization_code"));
    }

    [TestMethod]
    public async Task RefreshTokenAsync_ShouldReturnNewTokenInfo()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.SetupTextResponse("/oauth2/token", TestDataFactory.CreateAuthTokenJson());

        using var httpClient = new HttpClient(mockHandler);
        var options = new InoreaderClientOptions();

        // Act
        var result = await InoreaderAuthHelper.RefreshTokenAsync("old_refresh_token", options, httpClient);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("new_access_token", result.AccessToken);
        Assert.AreEqual("new_refresh_token", result.RefreshToken);
    }

    [TestMethod]
    public async Task RefreshTokenAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.SetupTextResponse("/oauth2/token", TestDataFactory.CreateAuthTokenJson());

        using var httpClient = new HttpClient(mockHandler);
        var options = new InoreaderClientOptions
        {
            ClientId = "my_client_id",
            ClientSecret = "my_client_secret",
        };

        // Act
        await InoreaderAuthHelper.RefreshTokenAsync("refresh_token_value", options, httpClient);

        // Assert
        var request = mockHandler.Requests[0];
        var content = await request.Content!.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("refresh_token=refresh_token_value"));
        Assert.IsTrue(content.Contains("grant_type=refresh_token"));
    }

    [TestMethod]
    public async Task ExchangeCodeForTokenAsync_WithNullCode_ShouldThrowArgumentNullException()
    {
        // Arrange
        var options = new InoreaderClientOptions();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => InoreaderAuthHelper.ExchangeCodeForTokenAsync(null!, options));
    }

    [TestMethod]
    public async Task ExchangeCodeForTokenAsync_WithEmptyCode_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new InoreaderClientOptions();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => InoreaderAuthHelper.ExchangeCodeForTokenAsync(string.Empty, options));
    }

    [TestMethod]
    public async Task RefreshTokenAsync_WithNullRefreshToken_ShouldThrowArgumentNullException()
    {
        // Arrange
        var options = new InoreaderClientOptions();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => InoreaderAuthHelper.RefreshTokenAsync(null!, options));
    }

    [TestMethod]
    public async Task RefreshTokenAsync_WithEmptyRefreshToken_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new InoreaderClientOptions();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => InoreaderAuthHelper.RefreshTokenAsync(string.Empty, options));
    }
}
