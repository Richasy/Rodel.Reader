// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Test;

/// <summary>
/// RssStorage 基础测试.
/// </summary>
[TestClass]
public class RssStorageBasicTests
{
    private string _testDbPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"rss_test_{Guid.NewGuid()}.db");
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
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new RssStorage(null!));
    }

    [TestMethod]
    public async Task InitializeAsync_ShouldCreateDatabaseFile()
    {
        // Arrange
        var options = new RssStorageOptions
        {
            DatabasePath = _testDbPath,
            CreateTablesOnInit = true,
        };
        await using var storage = new RssStorage(options);

        // Act
        await storage.InitializeAsync();

        // Assert
        Assert.IsTrue(File.Exists(_testDbPath));
    }

    [TestMethod]
    public async Task InitializeAsync_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var options = new RssStorageOptions { DatabasePath = _testDbPath };
        await using var storage = new RssStorage(options);

        // Act
        await storage.InitializeAsync();
        await storage.InitializeAsync();

        // Assert - no exception thrown
        Assert.IsTrue(File.Exists(_testDbPath));
    }

    [TestMethod]
    public async Task Operations_BeforeInitialize_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new RssStorageOptions { DatabasePath = _testDbPath };
        await using var storage = new RssStorage(options);

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => storage.GetAllFeedsAsync());
    }

    [TestMethod]
    public async Task Operations_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var options = new RssStorageOptions { DatabasePath = _testDbPath };
        var storage = new RssStorage(options);
        await storage.InitializeAsync();
        await storage.DisposeAsync();

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => storage.GetAllFeedsAsync());
    }
}
