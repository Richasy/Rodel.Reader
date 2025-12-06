// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// BookStorage 基础测试.
/// </summary>
[TestClass]
public class BookStorageBasicTests
{
    private string _testDbPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"book_test_{Guid.NewGuid()}.db");
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
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new BookStorage(null!));
    }

    [TestMethod]
    public async Task InitializeAsync_ShouldCreateDatabaseFile()
    {
        // Arrange
        var options = new BookStorageOptions
        {
            DatabasePath = _testDbPath,
            CreateTablesOnInit = true,
        };
        await using var storage = new BookStorage(options);

        // Act
        await storage.InitializeAsync();

        // Assert
        Assert.IsTrue(File.Exists(_testDbPath));
    }

    [TestMethod]
    public async Task InitializeAsync_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var options = new BookStorageOptions { DatabasePath = _testDbPath };
        await using var storage = new BookStorage(options);

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
        var options = new BookStorageOptions { DatabasePath = _testDbPath };
        var storage = new BookStorage(options);
        await storage.InitializeAsync();

        // Act & Assert
        await storage.DisposeAsync();
    }
}
