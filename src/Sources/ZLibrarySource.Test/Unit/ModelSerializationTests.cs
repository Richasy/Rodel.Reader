// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Test.Unit;

/// <summary>
/// 模型序列化测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ModelSerializationTests
{
    [TestMethod]
    public void BookItem_CanSerializeAndDeserialize()
    {
        // Arrange
        var book = new BookItem
        {
            Id = "12345",
            Name = "Clean Code",
            Year = "2008",
            CoverUrl = "https://example.com/cover.jpg",
            Language = "English",
            Publisher = "Prentice Hall",
            Extension = "pdf",
            FileSize = "2.5 MB",
            Rating = "4.5",
            Url = "https://zh.z-lib.fm/book/12345"
        };

        // Act
        var json = JsonSerializer.Serialize(book);
        var deserialized = JsonSerializer.Deserialize<BookItem>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(book.Id, deserialized.Id);
        Assert.AreEqual(book.Name, deserialized.Name);
        Assert.AreEqual(book.Year, deserialized.Year);
        Assert.AreEqual(book.Rating, deserialized.Rating);
    }

    [TestMethod]
    public void BookDetail_CanSerializeAndDeserialize()
    {
        // Arrange
        var detail = new BookDetail
        {
            Id = "12345",
            Name = "Design Patterns",
            Year = "1994",
            CoverUrl = "https://example.com/cover.jpg",
            Language = "English",
            Publisher = "Addison-Wesley",
            Extension = "epub",
            FileSize = "5.2 MB",
            Rating = "4.8",
            Isbn10 = "0201633612",
            Isbn13 = "978-0201633610",
            Description = "Classic book on software design patterns",
            DownloadUrl = "https://zh.z-lib.fm/dl/12345",
            Authors = [new BookAuthor { Name = "Gang of Four", Url = "/author/123" }]
        };

        // Act
        var json = JsonSerializer.Serialize(detail);
        var deserialized = JsonSerializer.Deserialize<BookDetail>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(detail.Id, deserialized.Id);
        Assert.AreEqual(detail.Isbn10, deserialized.Isbn10);
        Assert.AreEqual(detail.DownloadUrl, deserialized.DownloadUrl);
        Assert.AreEqual(1, deserialized.Authors?.Count);
    }

    [TestMethod]
    public void Booklist_CanSerializeAndDeserialize()
    {
        // Arrange
        var booklist = new Booklist
        {
            Name = "My Reading List",
            Description = "Books to read",
            Url = "https://zh.z-lib.fm/booklist/list123",
            BookCount = "10",
        };

        // Act
        var json = JsonSerializer.Serialize(booklist);
        var deserialized = JsonSerializer.Deserialize<Booklist>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(booklist.Name, deserialized.Name);
        Assert.AreEqual(booklist.BookCount, deserialized.BookCount);
    }

    [TestMethod]
    public void DownloadLimits_CanSerializeAndDeserialize()
    {
        // Arrange
        var limits = new DownloadLimits
        {
            DailyAllowed = 10,
            DailyUsed = 3
        };

        // Act
        var json = JsonSerializer.Serialize(limits);
        var deserialized = JsonSerializer.Deserialize<DownloadLimits>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(10, deserialized.DailyAllowed);
        Assert.AreEqual(3, deserialized.DailyUsed);
        Assert.AreEqual(7, deserialized.DailyRemaining);
    }

    [TestMethod]
    public void PagedResult_CanSerializeAndDeserialize()
    {
        // Arrange
        var pagedResult = new PagedResult<BookItem>
        {
            Items = [new BookItem { Id = "1", Name = "Book 1" }],
            CurrentPage = 1,
            TotalPages = 10
        };

        // Act
        var json = JsonSerializer.Serialize(pagedResult);
        var deserialized = JsonSerializer.Deserialize<PagedResult<BookItem>>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(1, deserialized.Items.Count);
        Assert.AreEqual(1, deserialized.CurrentPage);
        Assert.AreEqual(10, deserialized.TotalPages);
    }

    [TestMethod]
    public void BookSearchOptions_DefaultValues()
    {
        // Arrange & Act
        var options = new BookSearchOptions();

        // Assert
        Assert.IsFalse(options.Exact);
        Assert.IsNull(options.FromYear);
        Assert.IsNull(options.ToYear);
        Assert.IsNull(options.Languages);
        Assert.IsNull(options.Extensions);
    }

    [TestMethod]
    public void FullTextSearchOptions_DefaultValues()
    {
        // Arrange & Act
        var options = new FullTextSearchOptions();

        // Assert
        Assert.IsFalse(options.MatchPhrase);
        Assert.IsFalse(options.MatchWords);
    }
}
