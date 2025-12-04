// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Test.Unit;

/// <summary>
/// LegadoClient 单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class LegadoClientTests
{
    private static LegadoClientOptions CreateOptions(ServerType serverType = ServerType.Legado)
    {
        return new LegadoClientOptions
        {
            BaseUrl = "http://localhost:1234",
            ServerType = serverType,
        };
    }

    [TestMethod]
    public void Constructor_WithOptions_CreatesClient()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        using var client = new LegadoClient(options);

        // Assert
        Assert.IsNotNull(client);
        Assert.AreEqual(options.BaseUrl, client.Options.BaseUrl);
    }

    [TestMethod]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new LegadoClient(null!));
    }

    [TestMethod]
    public void Constructor_WithLogger_CreatesClient()
    {
        // Arrange
        var options = CreateOptions();
        var logger = NullLogger<LegadoClient>.Instance;

        // Act
        using var client = new LegadoClient(options, logger);

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void Constructor_WithHttpClient_CreatesClient()
    {
        // Arrange
        var options = CreateOptions();
        using var httpClient = new HttpClient();

        // Act
        using var client = new LegadoClient(options, httpClient, null);

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var options = CreateOptions();
        var client = new LegadoClient(options);

        // Act & Assert - should not throw
        client.Dispose();
        client.Dispose();
    }

    [TestMethod]
    public async Task GetBookshelfAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var options = CreateOptions();
        var client = new LegadoClient(options);
        client.Dispose();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(
            async () => await client.GetBookshelfAsync());
    }

    [TestMethod]
    public async Task GetChapterListAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetChapterListAsync(null!));
    }

    [TestMethod]
    public async Task GetChapterListAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await client.GetChapterListAsync(string.Empty));
    }

    [TestMethod]
    public async Task GetChapterContentAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetChapterContentAsync(null!, 0));
    }

    [TestMethod]
    public async Task GetChapterContentAsync_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(
            async () => await client.GetChapterContentAsync("http://test.com", -1));
    }

    [TestMethod]
    public async Task SaveBookAsync_WithNullBook_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.SaveBookAsync(null!));
    }

    [TestMethod]
    public async Task DeleteBookAsync_WithNullBook_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.DeleteBookAsync(null!));
    }

    [TestMethod]
    public async Task SaveProgressAsync_WithNullProgress_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.SaveProgressAsync(null!));
    }

    [TestMethod]
    public async Task GetBookSourceAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetBookSourceAsync(null!));
    }

    [TestMethod]
    public async Task SaveBookSourceAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.SaveBookSourceAsync(null!));
    }

    [TestMethod]
    public async Task SaveBookSourcesAsync_WithNullSources_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.SaveBookSourcesAsync(null!));
    }

    [TestMethod]
    public async Task DeleteBookSourcesAsync_WithNullSources_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.DeleteBookSourcesAsync(null!));
    }

    [TestMethod]
    public async Task GetCoverAsync_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetCoverAsync(null!));
    }

    [TestMethod]
    public void GetCoverUri_WithValidPath_ReturnsUri()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act
        var uri = client.GetCoverUri("/cover/test.jpg");

        // Assert
        Assert.IsNotNull(uri);
        Assert.IsTrue(uri.ToString().Contains("cover", StringComparison.Ordinal));
        Assert.IsTrue(uri.ToString().Contains("path", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GetCoverUri_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions();
        using var client = new LegadoClient(options);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => client.GetCoverUri(null!));
    }

    [TestMethod]
    public void Options_ReturnsProvidedOptions()
    {
        // Arrange
        var options = CreateOptions(ServerType.HectorqinReader);
        options.AccessToken = "test-token";

        // Act
        using var client = new LegadoClient(options);

        // Assert
        Assert.AreEqual(options.BaseUrl, client.Options.BaseUrl);
        Assert.AreEqual(options.ServerType, client.Options.ServerType);
        Assert.AreEqual(options.AccessToken, client.Options.AccessToken);
    }
}
