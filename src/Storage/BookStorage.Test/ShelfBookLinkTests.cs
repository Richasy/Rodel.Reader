// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// 书架书籍关联测试.
/// </summary>
[TestClass]
public class ShelfBookLinkTests
{
    private string _testDbPath = null!;
    private BookStorage _storage = null!;
    private Shelf _testShelf = null!;
    private Book _testBook = null!;

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

        // 创建测试书架
        _testShelf = new Shelf
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = "Test Shelf",
            SortIndex = 0,
        };
        await _storage.UpsertShelfAsync(_testShelf);

        // 创建测试书籍
        _testBook = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Test Book",
            Format = BookFormat.Epub,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertBookAsync(_testBook);
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
    public async Task AddBookToShelf_ShouldSucceed()
    {
        // Act
        await _storage.AddBookToShelfAsync(_testBook.Id, _testShelf.Id);
        var shelfBooks = await _storage.GetBooksInShelfAsync(_testShelf.Id);

        // Assert
        Assert.AreEqual(1, shelfBooks.Count);
        Assert.AreEqual(_testBook.Id, shelfBooks[0].Book.Id);
        Assert.IsNull(shelfBooks[0].Link.GroupId);
    }

    [TestMethod]
    public async Task AddBookToShelf_WithGroup_ShouldSucceed()
    {
        // Arrange
        var group = new BookGroup
        {
            Id = Guid.NewGuid().ToString("N"),
            ShelfId = _testShelf.Id,
            Name = "Test Group",
            SortIndex = 0,
        };
        await _storage.UpsertGroupAsync(group);

        // Act
        await _storage.AddBookToShelfAsync(_testBook.Id, _testShelf.Id, group.Id);
        var shelfBooks = await _storage.GetBooksInShelfAsync(_testShelf.Id);

        // Assert
        Assert.AreEqual(1, shelfBooks.Count);
        Assert.AreEqual(_testBook.Id, shelfBooks[0].Book.Id);
        Assert.AreEqual(group.Id, shelfBooks[0].Link.GroupId);
    }

    [TestMethod]
    public async Task GetBooksInGroup_ShouldReturnOnlyGroupBooks()
    {
        // Arrange
        var group = new BookGroup
        {
            Id = Guid.NewGuid().ToString("N"),
            ShelfId = _testShelf.Id,
            Name = "Test Group",
            SortIndex = 0,
        };
        await _storage.UpsertGroupAsync(group);

        var book2 = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Book 2",
            Format = BookFormat.Pdf,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertBookAsync(book2);

        // 一本书加到分组，一本书只加到书架
        await _storage.AddBookToShelfAsync(_testBook.Id, _testShelf.Id, group.Id);
        await _storage.AddBookToShelfAsync(book2.Id, _testShelf.Id);

        // Act
        var groupBooks = await _storage.GetBooksInGroupAsync(group.Id);

        // Assert
        Assert.AreEqual(1, groupBooks.Count);
        Assert.AreEqual(_testBook.Id, groupBooks[0].Id);
    }

    [TestMethod]
    public async Task RemoveBookFromShelf_ShouldSucceed()
    {
        // Arrange
        await _storage.AddBookToShelfAsync(_testBook.Id, _testShelf.Id);

        // Act
        var removed = await _storage.RemoveBookFromShelfAsync(_testBook.Id, _testShelf.Id);
        var shelfBooks = await _storage.GetBooksInShelfAsync(_testShelf.Id);

        // Assert
        Assert.IsTrue(removed);
        Assert.AreEqual(0, shelfBooks.Count);
    }

    [TestMethod]
    public async Task UpdateBookSortIndex_ShouldSucceed()
    {
        // Arrange
        await _storage.AddBookToShelfAsync(_testBook.Id, _testShelf.Id);

        // Act
        await _storage.UpdateBookSortIndexAsync(_testBook.Id, _testShelf.Id, 99);
        var shelfBooks = await _storage.GetBooksInShelfAsync(_testShelf.Id);

        // Assert
        Assert.AreEqual(99, shelfBooks[0].Link.SortIndex);
    }

    [TestMethod]
    public async Task MoveBookToGroup_ShouldUpdateGroupId()
    {
        // Arrange
        var group1 = new BookGroup
        {
            Id = Guid.NewGuid().ToString("N"),
            ShelfId = _testShelf.Id,
            Name = "Group 1",
            SortIndex = 0,
        };
        var group2 = new BookGroup
        {
            Id = Guid.NewGuid().ToString("N"),
            ShelfId = _testShelf.Id,
            Name = "Group 2",
            SortIndex = 1,
        };
        await _storage.UpsertGroupAsync(group1);
        await _storage.UpsertGroupAsync(group2);

        await _storage.AddBookToShelfAsync(_testBook.Id, _testShelf.Id, group1.Id);

        // Act - 移动到另一个分组
        await _storage.MoveBookToGroupAsync(_testBook.Id, _testShelf.Id, group2.Id);

        // Assert
        var group1Books = await _storage.GetBooksInGroupAsync(group1.Id);
        var group2Books = await _storage.GetBooksInGroupAsync(group2.Id);
        Assert.AreEqual(0, group1Books.Count);
        Assert.AreEqual(1, group2Books.Count);
    }

    [TestMethod]
    public async Task MoveBookToGroup_WithNullGroupId_ShouldMoveOutOfGroup()
    {
        // Arrange
        var group = new BookGroup
        {
            Id = Guid.NewGuid().ToString("N"),
            ShelfId = _testShelf.Id,
            Name = "Test Group",
            SortIndex = 0,
        };
        await _storage.UpsertGroupAsync(group);
        await _storage.AddBookToShelfAsync(_testBook.Id, _testShelf.Id, group.Id);

        // Act - 移出分组
        await _storage.MoveBookToGroupAsync(_testBook.Id, _testShelf.Id, null);

        // Assert
        var groupBooks = await _storage.GetBooksInGroupAsync(group.Id);
        var shelfBooks = await _storage.GetBooksInShelfAsync(_testShelf.Id);
        Assert.AreEqual(0, groupBooks.Count);
        Assert.AreEqual(1, shelfBooks.Count);
        Assert.IsNull(shelfBooks[0].Link.GroupId);
    }
}
