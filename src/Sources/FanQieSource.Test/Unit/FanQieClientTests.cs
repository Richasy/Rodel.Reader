// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Test.Unit;

/// <summary>
/// FanQieClient 单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class FanQieClientTests
{
    [TestMethod]
    public void Constructor_WithDefaultOptions_CreatesClient()
    {
        // Arrange & Act
        using var client = new FanQieClient();

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void Constructor_WithCustomOptions_CreatesClient()
    {
        // Arrange
        var options = new FanQieClientOptions
        {
            Timeout = TimeSpan.FromMinutes(1),
            MaxConcurrentRequests = 5,
        };

        // Act
        using var client = new FanQieClient(options);

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void Constructor_WithHttpClient_CreatesClient()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        using var client = new FanQieClient(httpClient: httpClient);

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public async Task SearchBooksAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.SearchBooksAsync(null!));
    }

    [TestMethod]
    public async Task SearchBooksAsync_WithEmptyQuery_ThrowsArgumentException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await client.SearchBooksAsync(string.Empty));
    }

    [TestMethod]
    public async Task SearchBooksAsync_WithNegativeOffset_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(
            async () => await client.SearchBooksAsync("test", -1));
    }

    [TestMethod]
    public async Task GetBookDetailAsync_WithNullBookId_ThrowsArgumentNullException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetBookDetailAsync(null!));
    }

    [TestMethod]
    public async Task GetBookDetailAsync_WithEmptyBookId_ThrowsArgumentException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await client.GetBookDetailAsync(string.Empty));
    }

    [TestMethod]
    public async Task GetBookTocAsync_WithNullBookId_ThrowsArgumentNullException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetBookTocAsync(null!));
    }

    [TestMethod]
    public async Task GetChapterContentAsync_WithNullBookId_ThrowsArgumentNullException()
    {
        // Arrange
        using var client = new FanQieClient();
        var chapter = new ChapterItem { ItemId = "1", Title = "Test", Order = 1 };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetChapterContentAsync(null!, "Title", chapter));
    }

    [TestMethod]
    public async Task GetChapterContentAsync_WithNullBookTitle_ThrowsArgumentNullException()
    {
        // Arrange
        using var client = new FanQieClient();
        var chapter = new ChapterItem { ItemId = "1", Title = "Test", Order = 1 };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetChapterContentAsync("12345", null!, chapter));
    }

    [TestMethod]
    public async Task GetChapterContentAsync_WithNullChapter_ThrowsArgumentNullException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetChapterContentAsync("12345", "Title", null!));
    }

    [TestMethod]
    public async Task GetChapterContentsAsync_WithNullChapters_ThrowsArgumentNullException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.GetChapterContentsAsync("12345", "Title", null!));
    }

    [TestMethod]
    public async Task GetChapterContentsAsync_WithEmptyChapters_ReturnsEmptyList()
    {
        // Arrange
        using var client = new FanQieClient();
        var chapters = Array.Empty<ChapterItem>();

        // Act
        var result = await client.GetChapterContentsAsync("12345", "Title", chapters);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task DownloadBookAsync_WithNullBookId_ThrowsArgumentNullException()
    {
        // Arrange
        using var client = new FanQieClient();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await client.DownloadBookAsync(null!));
    }

    [TestMethod]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var client = new FanQieClient();

        // Act & Assert - should not throw
        client.Dispose();
        client.Dispose();
        client.Dispose();
    }
}
