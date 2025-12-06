// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// 书签和批注测试.
/// </summary>
[TestClass]
public class BookmarkAndAnnotationTests
{
    private string _testDbPath = null!;
    private BookStorage _storage = null!;
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

    #region Bookmark Tests

    [TestMethod]
    public async Task UpsertBookmark_Insert_ShouldSucceed()
    {
        // Arrange
        var bookmark = CreateTestBookmark();

        // Act
        await _storage.UpsertBookmarkAsync(bookmark);
        var bookmarks = await _storage.GetBookmarksAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(1, bookmarks.Count);
        Assert.AreEqual(bookmark.Id, bookmarks[0].Id);
        Assert.AreEqual("chapter-1", bookmarks[0].Position);
    }

    [TestMethod]
    public async Task UpsertBookmark_Update_ShouldSucceed()
    {
        // Arrange
        var bookmark = CreateTestBookmark();
        await _storage.UpsertBookmarkAsync(bookmark);

        // Act
        bookmark.Title = "Updated Bookmark Title";
        await _storage.UpsertBookmarkAsync(bookmark);
        var bookmarks = await _storage.GetBookmarksAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(1, bookmarks.Count);
        Assert.AreEqual("Updated Bookmark Title", bookmarks[0].Title);
    }

    [TestMethod]
    public async Task GetBookmarks_ShouldReturnOnlyBookBookmarks()
    {
        // Arrange
        var book2 = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Book 2",
            Format = BookFormat.Pdf,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _storage.UpsertBookAsync(book2);

        var bookmark1 = CreateTestBookmark("bm1");
        var bookmark2 = CreateTestBookmark("bm2");
        var bookmark3 = new Bookmark
        {
            Id = "bm3",
            BookId = book2.Id,
            Position = "page-1",
            CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
        };

        await _storage.UpsertBookmarkAsync(bookmark1);
        await _storage.UpsertBookmarkAsync(bookmark2);
        await _storage.UpsertBookmarkAsync(bookmark3);

        // Act
        var bookmarks = await _storage.GetBookmarksAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(2, bookmarks.Count);
        Assert.IsTrue(bookmarks.All(b => b.BookId == _testBook.Id));
    }

    [TestMethod]
    public async Task DeleteBookmark_ShouldRemoveBookmark()
    {
        // Arrange
        var bookmark = CreateTestBookmark();
        await _storage.UpsertBookmarkAsync(bookmark);

        // Act
        var deleted = await _storage.DeleteBookmarkAsync(bookmark.Id);
        var bookmarks = await _storage.GetBookmarksAsync(_testBook.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.AreEqual(0, bookmarks.Count);
    }

    #endregion

    #region Annotation Tests

    [TestMethod]
    public async Task UpsertAnnotation_Insert_ShouldSucceed()
    {
        // Arrange
        var annotation = CreateTestAnnotation();

        // Act
        await _storage.UpsertAnnotationAsync(annotation);
        var annotations = await _storage.GetAnnotationsAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(1, annotations.Count);
        Assert.AreEqual(annotation.Id, annotations[0].Id);
        Assert.AreEqual("This is a highlighted text.", annotations[0].SelectedText);
    }

    [TestMethod]
    public async Task UpsertAnnotation_Update_ShouldSucceed()
    {
        // Arrange
        var annotation = CreateTestAnnotation();
        await _storage.UpsertAnnotationAsync(annotation);

        // Act
        annotation.Note = "Updated note content";
        await _storage.UpsertAnnotationAsync(annotation);
        var annotations = await _storage.GetAnnotationsAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(1, annotations.Count);
        Assert.AreEqual("Updated note content", annotations[0].Note);
    }

    [TestMethod]
    public async Task DeleteAnnotation_ShouldRemoveAnnotation()
    {
        // Arrange
        var annotation = CreateTestAnnotation();
        await _storage.UpsertAnnotationAsync(annotation);

        // Act
        var deleted = await _storage.DeleteAnnotationAsync(annotation.Id);
        var annotations = await _storage.GetAnnotationsAsync(_testBook.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.AreEqual(0, annotations.Count);
    }

    [TestMethod]
    public async Task Annotation_WithColor_ShouldPersist()
    {
        // Arrange
        var annotation = CreateTestAnnotation();
        annotation.Color = "#FFFF00"; // Yellow

        // Act
        await _storage.UpsertAnnotationAsync(annotation);
        var annotations = await _storage.GetAnnotationsAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(1, annotations.Count);
        Assert.AreEqual("#FFFF00", annotations[0].Color);
    }

    #endregion

    private Bookmark CreateTestBookmark(string? id = null)
    {
        return new Bookmark
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            BookId = _testBook.Id,
            Position = "chapter-1",
            Title = "Test Bookmark",
            CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
    }

    private Annotation CreateTestAnnotation(string? id = null)
    {
        return new Annotation
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            BookId = _testBook.Id,
            Type = AnnotationType.Highlight,
            Position = "chapter-1",
            SelectedText = "This is a highlighted text.",
            CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
    }
}
