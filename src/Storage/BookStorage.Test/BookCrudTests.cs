// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// 书籍 CRUD 测试.
/// </summary>
[TestClass]
public class BookCrudTests
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
    public async Task UpsertBook_Insert_ShouldSucceed()
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        await _storage.UpsertBookAsync(book);
        var retrieved = await _storage.GetBookAsync(book.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(book.Id, retrieved.Id);
        Assert.AreEqual(book.Title, retrieved.Title);
        Assert.AreEqual(book.Format, retrieved.Format);
    }

    [TestMethod]
    public async Task UpsertBook_Update_ShouldSucceed()
    {
        // Arrange
        var book = CreateTestBook();
        await _storage.UpsertBookAsync(book);

        // Act
        book.Title = "Updated Title";
        await _storage.UpsertBookAsync(book);
        var retrieved = await _storage.GetBookAsync(book.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Updated Title", retrieved.Title);
    }

    [TestMethod]
    public async Task GetAllBooks_ShouldReturnAllBooks()
    {
        // Arrange
        var book1 = CreateTestBook("book1");
        var book2 = CreateTestBook("book2");
        await _storage.UpsertBookAsync(book1);
        await _storage.UpsertBookAsync(book2);

        // Act
        var books = await _storage.GetAllBooksAsync();

        // Assert
        Assert.AreEqual(2, books.Count);
    }

    [TestMethod]
    public async Task GetBookByPath_ShouldReturnCorrectBook()
    {
        // Arrange
        var book = CreateTestBook();
        book.LocalPath = "/path/to/book.epub";
        await _storage.UpsertBookAsync(book);

        // Act
        var retrieved = await _storage.GetBookByPathAsync("/path/to/book.epub");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(book.Id, retrieved.Id);
    }

    [TestMethod]
    public async Task GetBookByHash_ShouldReturnCorrectBook()
    {
        // Arrange
        var book = CreateTestBook();
        book.FileHash = "abc123hash";
        await _storage.UpsertBookAsync(book);

        // Act
        var retrieved = await _storage.GetBookByHashAsync("abc123hash");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(book.Id, retrieved.Id);
    }

    [TestMethod]
    public async Task SearchBooks_ShouldFindByTitle()
    {
        // Arrange
        var book1 = CreateTestBook("book1");
        book1.Title = "The Great Adventure";
        var book2 = CreateTestBook("book2");
        book2.Title = "Small Story";
        await _storage.UpsertBookAsync(book1);
        await _storage.UpsertBookAsync(book2);

        // Act
        var results = await _storage.SearchBooksAsync("Great");

        // Assert
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("The Great Adventure", results[0].Title);
    }

    [TestMethod]
    public async Task GetBooksByFormat_ShouldFilterCorrectly()
    {
        // Arrange
        var epubBook = CreateTestBook("epub1");
        epubBook.Format = BookFormat.Epub;
        var pdfBook = CreateTestBook("pdf1");
        pdfBook.Format = BookFormat.Pdf;
        await _storage.UpsertBookAsync(epubBook);
        await _storage.UpsertBookAsync(pdfBook);

        // Act
        var epubBooks = await _storage.GetBooksByFormatAsync(BookFormat.Epub);

        // Assert
        Assert.AreEqual(1, epubBooks.Count);
        Assert.AreEqual(BookFormat.Epub, epubBooks[0].Format);
    }

    [TestMethod]
    public async Task GetBooksByTrackStatus_ShouldFilterCorrectly()
    {
        // Arrange
        var readingBook = CreateTestBook("reading1");
        readingBook.TrackStatus = BookTrackStatus.Reading;
        var finishedBook = CreateTestBook("finished1");
        finishedBook.TrackStatus = BookTrackStatus.Finished;
        await _storage.UpsertBookAsync(readingBook);
        await _storage.UpsertBookAsync(finishedBook);

        // Act
        var readingBooks = await _storage.GetBooksByTrackStatusAsync(BookTrackStatus.Reading);

        // Assert
        Assert.AreEqual(1, readingBooks.Count);
        Assert.AreEqual(BookTrackStatus.Reading, readingBooks[0].TrackStatus);
    }

    [TestMethod]
    public async Task GetBooksBySourceType_ShouldFilterCorrectly()
    {
        // Arrange
        var localBook = CreateTestBook("local1");
        localBook.SourceType = BookSourceType.Local;
        var fanqieBook = CreateTestBook("fanqie1");
        fanqieBook.SourceType = BookSourceType.FanQie;
        await _storage.UpsertBookAsync(localBook);
        await _storage.UpsertBookAsync(fanqieBook);

        // Act
        var localBooks = await _storage.GetBooksBySourceTypeAsync(BookSourceType.Local);

        // Assert
        Assert.AreEqual(1, localBooks.Count);
        Assert.AreEqual(BookSourceType.Local, localBooks[0].SourceType);
    }

    [TestMethod]
    public async Task DeleteBook_ShouldRemoveBook()
    {
        // Arrange
        var book = CreateTestBook();
        await _storage.UpsertBookAsync(book);

        // Act
        var deleted = await _storage.DeleteBookAsync(book.Id);
        var retrieved = await _storage.GetBookAsync(book.Id);

        // Assert
        Assert.IsTrue(deleted);
        Assert.IsNull(retrieved);
    }

    [TestMethod]
    public async Task UpsertBooks_Batch_ShouldSucceed()
    {
        // Arrange
        var books = new List<Book>
        {
            CreateTestBook("batch1"),
            CreateTestBook("batch2"),
            CreateTestBook("batch3"),
        };

        // Act
        await _storage.UpsertBooksAsync(books);
        var allBooks = await _storage.GetAllBooksAsync();

        // Assert
        Assert.AreEqual(3, allBooks.Count);
    }

    [TestMethod]
    public async Task Book_WithUseComicReader_ShouldPersist()
    {
        // Arrange
        var book = CreateTestBook();
        book.UseComicReader = true;

        // Act
        await _storage.UpsertBookAsync(book);
        var retrieved = await _storage.GetBookAsync(book.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.IsTrue(retrieved.UseComicReader);
    }

    private static Book CreateTestBook(string? id = null)
    {
        return new Book
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            Title = "Test Book",
            Format = BookFormat.Epub,
            SourceType = BookSourceType.Local,
            TrackStatus = BookTrackStatus.None,
            AddedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
    }
}
