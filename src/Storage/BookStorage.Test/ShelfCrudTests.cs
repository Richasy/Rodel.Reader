// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// 书架 CRUD 测试.
/// </summary>
[TestClass]
public class ShelfCrudTests
{
    private string _testDbPath = null!;
    private BookStorage _storage = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"book_test_{Guid.NewGuid()}.db");
        var options = new BookStorageOptions
        {
            DatabasePath = _testDbPath,
            CreateTablesOnInit = true,
        };
        _storage = new BookStorage(options);
        await _storage.InitializeAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _storage.DisposeAsync();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [TestMethod]
    public async Task UpsertShelf_Insert_ShouldSucceed()
    {
        // Arrange
        var shelf = CreateTestShelf();

        // Act
        await _storage.UpsertShelfAsync(shelf);
        var retrieved = await _storage.GetShelfAsync(shelf.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(shelf.Id, retrieved.Id);
        Assert.AreEqual(shelf.Name, retrieved.Name);
    }

    [TestMethod]
    public async Task UpsertShelf_Update_ShouldSucceed()
    {
        // Arrange
        var shelf = CreateTestShelf();
        await _storage.UpsertShelfAsync(shelf);

        // Act
        shelf.Name = "Updated Shelf Name";
        await _storage.UpsertShelfAsync(shelf);
        var retrieved = await _storage.GetShelfAsync(shelf.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Updated Shelf Name", retrieved.Name);
    }

    [TestMethod]
    public async Task GetAllShelves_ShouldReturnAllShelves()
    {
        // Arrange
        var shelf1 = CreateTestShelf("shelf1");
        var shelf2 = CreateTestShelf("shelf2");
        await _storage.UpsertShelfAsync(shelf1);
        await _storage.UpsertShelfAsync(shelf2);

        // Act
        var shelves = await _storage.GetAllShelvesAsync();

        // Assert
        Assert.AreEqual(2, shelves.Count);
    }

    [TestMethod]
    public async Task IsShelfNameExists_ShouldReturnTrueForExistingName()
    {
        // Arrange
        var shelf = CreateTestShelf();
        shelf.Name = "My Favorite Books";
        await _storage.UpsertShelfAsync(shelf);

        // Act
        var exists = await _storage.IsShelfNameExistsAsync("My Favorite Books");

        // Assert
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task IsShelfNameExists_WithExcludeId_ShouldExcludeSelf()
    {
        // Arrange
        var shelf = CreateTestShelf();
        shelf.Name = "My Favorite Books";
        await _storage.UpsertShelfAsync(shelf);

        // Act - 排除自己
        var exists = await _storage.IsShelfNameExistsAsync("My Favorite Books", shelf.Id);

        // Assert
        Assert.IsFalse(exists);
    }

    [TestMethod]
    public async Task DeleteShelf_ShouldRemoveShelf()
    {
        // Arrange
        var shelf = CreateTestShelf();
        await _storage.UpsertShelfAsync(shelf);

        // Act
        var deleted = await _storage.DeleteShelfAsync(shelf.Id);
        var retrieved = await _storage.GetShelfAsync(shelf.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsNull(retrieved);
    }

    [TestMethod]
    public async Task DeleteShelf_ShouldRemoveRelatedGroupsAndLinks()
    {
        // Arrange
        var shelf = CreateTestShelf();
        await _storage.UpsertShelfAsync(shelf);

        var group = new BookGroup
        {
            Id = Guid.NewGuid().ToString("N"),
            ShelfId = shelf.Id,
            Name = "Test Group",
            SortIndex = 0,
        };
        await _storage.UpsertGroupAsync(group);

        var book = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Test Book",
            Format = BookFormat.Epub,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow,
        };
        await _storage.UpsertBookAsync(book);
        await _storage.AddBookToShelfAsync(book.Id, shelf.Id);

        // Act
        await _storage.DeleteShelfAsync(shelf.Id);

        // Assert - 分组应被删除
        var groups = await _storage.GetGroupsByShelfAsync(shelf.Id);
        Assert.AreEqual(0, groups.Count);

        // Assert - 书架关联应被删除，但书籍本身应该还在
        var retrievedBook = await _storage.GetBookAsync(book.Id);
        Assert.IsNotNull(retrievedBook);
    }

    [TestMethod]
    public async Task Shelf_WithEmoji_ShouldPersist()
    {
        // Arrange
        var shelf = CreateTestShelf();
        shelf.IconEmoji = "📚";

        // Act
        await _storage.UpsertShelfAsync(shelf);
        var retrieved = await _storage.GetShelfAsync(shelf.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("📚", retrieved.IconEmoji);
    }

    private static Shelf CreateTestShelf(string? id = null)
    {
        return new Shelf
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            Name = "Test Shelf",
            SortIndex = 0,
            IsDefault = false,
        };
    }
}
