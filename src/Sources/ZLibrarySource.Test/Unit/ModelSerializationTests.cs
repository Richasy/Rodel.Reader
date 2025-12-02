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
            Url = "https://zh.z-lib.fm/book/12345",
            DownloadUrl = "https://zh.z-lib.fm/dl/12345",
            Description = "A book about clean code",
            Hash = "abc123"
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
        Assert.AreEqual(book.DownloadUrl, deserialized.DownloadUrl);
        Assert.AreEqual(book.Description, deserialized.Description);
        Assert.AreEqual(book.Hash, deserialized.Hash);
    }

    [TestMethod]
    public void UserProfile_CanSerializeAndDeserialize()
    {
        // Arrange
        var profile = new UserProfile
        {
            Id = 12345,
            Email = "test@example.com",
            Name = "Test User",
            KindleEmail = "kindle@example.com",
            DownloadsToday = 3,
            DownloadsLimit = 10,
            IsConfirmed = true,
            IsPremium = false
        };

        // Act
        var json = JsonSerializer.Serialize(profile);
        var deserialized = JsonSerializer.Deserialize<UserProfile>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(12345, deserialized.Id);
        Assert.AreEqual("test@example.com", deserialized.Email);
        Assert.AreEqual(10, deserialized.DownloadsLimit);
        Assert.AreEqual(3, deserialized.DownloadsToday);
        Assert.AreEqual(7, deserialized.DownloadsRemaining);
        Assert.IsTrue(deserialized.IsConfirmed);
        Assert.IsFalse(deserialized.IsPremium);
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
}
