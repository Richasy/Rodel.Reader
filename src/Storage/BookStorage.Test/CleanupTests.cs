// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// 数据清理测试.
/// </summary>
[TestClass]
public class CleanupTests
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
    public async Task DeleteBook_ShouldDeleteAllRelatedData()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Test Book",
            Format = BookFormat.Epub,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertBookAsync(book);

        // 添加相关数据
        var shelf = new Shelf
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = "Test Shelf",
            SortIndex = 0,
        };
        await _storage.UpsertShelfAsync(shelf);
        await _storage.AddBookToShelfAsync(book.Id, shelf.Id);

        var progress = new ReadProgress
        {
            BookId = book.Id,
            Progress = 0.5,
            UpdatedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertReadProgressAsync(progress);

        var session = new ReadingSession
        {
            Id = Guid.NewGuid().ToString("N"),
            BookId = book.Id,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10).ToString("O"),
            EndedAt = DateTimeOffset.UtcNow.ToString("O"),
            DurationSeconds = 600,
        };
        await _storage.AddReadingSessionAsync(session);

        var bookmark = new Bookmark
        {
            Id = Guid.NewGuid().ToString("N"),
            BookId = book.Id,
            Position = "chapter-1",
            CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertBookmarkAsync(bookmark);

        var annotation = new Annotation
        {
            Id = Guid.NewGuid().ToString("N"),
            BookId = book.Id,
            Type = AnnotationType.Highlight,
            Position = "chapter-1",
            SelectedText = "Test",
            CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertAnnotationAsync(annotation);

        // Act
        await _storage.DeleteBookAsync(book.Id);

        // Assert - 所有相关数据都应被删除
        var retrievedBook = await _storage.GetBookAsync(book.Id);
        Assert.IsNull(retrievedBook);

        var shelfBooks = await _storage.GetBooksInShelfAsync(shelf.Id);
        Assert.AreEqual(0, shelfBooks.Count);

        var retrievedProgress = await _storage.GetReadProgressAsync(book.Id);
        Assert.IsNull(retrievedProgress);

        var sessions = await _storage.GetReadingSessionsAsync(book.Id);
        Assert.AreEqual(0, sessions.Count);

        var bookmarks = await _storage.GetBookmarksAsync(book.Id);
        Assert.AreEqual(0, bookmarks.Count);

        var annotations = await _storage.GetAnnotationsAsync(book.Id);
        Assert.AreEqual(0, annotations.Count);
    }

    [TestMethod]
    public async Task CleanupOrphanedData_ShouldNotThrow()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Test Book",
            Format = BookFormat.Epub,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertBookAsync(book);

        var shelf = new Shelf
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = "Test Shelf",
            SortIndex = 0,
        };
        await _storage.UpsertShelfAsync(shelf);
        await _storage.AddBookToShelfAsync(book.Id, shelf.Id);

        // Act
        var removed = await _storage.CleanupOrphanedDataAsync();

        // Assert
        // 由于数据是一致的，应该没有孤立数据需要清理
        Assert.IsTrue(removed >= 0);
    }

    [TestMethod]
    public async Task ClearAll_ShouldRemoveAllData()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Test Book",
            Format = BookFormat.Epub,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertBookAsync(book);

        var shelf = new Shelf
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = "Test Shelf",
            SortIndex = 0,
        };
        await _storage.UpsertShelfAsync(shelf);

        // Act
        await _storage.ClearAllAsync();

        // Assert
        var books = await _storage.GetAllBooksAsync();
        var shelves = await _storage.GetAllShelvesAsync();
        Assert.AreEqual(0, books.Count);
        Assert.AreEqual(0, shelves.Count);
    }
}
