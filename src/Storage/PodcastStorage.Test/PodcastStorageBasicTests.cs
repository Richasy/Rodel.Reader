// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Test;

/// <summary>
/// PodcastStorage 基础测试.
/// </summary>
[TestClass]
public class PodcastStorageBasicTests
{
    private string _testDbPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"podcast_test_{Guid.NewGuid()}.db");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [TestMethod]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new PodcastStorage(null!));
    }

    [TestMethod]
    public async Task InitializeAsync_ShouldCreateDatabaseFile()
    {
        // Arrange
        var options = new PodcastStorageOptions
        {
            DatabasePath = _testDbPath,
            CreateTablesOnInit = true,
        };
        await using var storage = new PodcastStorage(options);

        // Act
        await storage.InitializeAsync();

        // Assert
        Assert.IsTrue(File.Exists(_testDbPath));
    }

    [TestMethod]
    public async Task InitializeAsync_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var options = new PodcastStorageOptions { DatabasePath = _testDbPath };
        await using var storage = new PodcastStorage(options);

        // Act
        await storage.InitializeAsync();
        await storage.InitializeAsync();

        // Assert - 没有抛出异常即为通过
        Assert.IsTrue(File.Exists(_testDbPath));
    }

    [TestMethod]
    public async Task DisposeAsync_ShouldNotThrow()
    {
        // Arrange
        var options = new PodcastStorageOptions { DatabasePath = _testDbPath };
        var storage = new PodcastStorage(options);
        await storage.InitializeAsync();

        // Act & Assert
        await storage.DisposeAsync();
    }

    [TestMethod]
    public async Task Operations_BeforeInitialize_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new PodcastStorageOptions { DatabasePath = _testDbPath };
        await using var storage = new PodcastStorage(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => storage.GetAllPodcastsAsync());
    }

    [TestMethod]
    public async Task Operations_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var options = new PodcastStorageOptions { DatabasePath = _testDbPath };
        var storage = new PodcastStorage(options);
        await storage.InitializeAsync();
        await storage.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(
            () => storage.GetAllPodcastsAsync());
    }
}
