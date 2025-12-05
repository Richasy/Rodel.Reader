// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Abstractions.Test;

[TestClass]
public class ScrapedBookTests
{
    [TestMethod]
    public void ScrapedBook_RequiredPropertiesSet_ShouldBeValid()
    {
        // Arrange & Act
        var book = new ScrapedBook
        {
            Id = "book123",
            Title = "Test Book",
            ScraperId = "test.scraper",
        };

        // Assert
        Assert.AreEqual("book123", book.Id);
        Assert.AreEqual("Test Book", book.Title);
        Assert.AreEqual("test.scraper", book.ScraperId);
    }

    [TestMethod]
    public void ScrapedBook_AllPropertiesSet_ShouldBeValid()
    {
        // Arrange & Act
        var book = new ScrapedBook
        {
            Id = "book123",
            Title = "Test Book",
            ScraperId = "test.scraper",
            Rating = 5,
            Subtitle = "A Test Subtitle",
            Description = "Test description",
            Cover = "https://example.com/cover.jpg",
            WebLink = "https://example.com/book/123",
            Author = "Test Author",
            Translator = "Test Translator",
            Publisher = "Test Publisher",
            PageCount = 300,
            PublishDate = "2024-01-01",
            ISBN = "978-0-123456-78-9",
            Category = "Fiction",
            ExtendedData = new Dictionary<string, string>
            {
                ["customKey"] = "customValue",
            },
        };

        // Assert
        Assert.AreEqual(5, book.Rating);
        Assert.AreEqual("A Test Subtitle", book.Subtitle);
        Assert.AreEqual("Test description", book.Description);
        Assert.AreEqual("https://example.com/cover.jpg", book.Cover);
        Assert.AreEqual("https://example.com/book/123", book.WebLink);
        Assert.AreEqual("Test Author", book.Author);
        Assert.AreEqual("Test Translator", book.Translator);
        Assert.AreEqual("Test Publisher", book.Publisher);
        Assert.AreEqual(300, book.PageCount);
        Assert.AreEqual("2024-01-01", book.PublishDate);
        Assert.AreEqual("978-0-123456-78-9", book.ISBN);
        Assert.AreEqual("Fiction", book.Category);
        Assert.AreEqual("customValue", book.ExtendedData!["customKey"]);
    }

    [TestMethod]
    public void ScrapedBook_Equals_SameIdAndScraperId_ShouldBeEqual()
    {
        // Arrange
        var book1 = new ScrapedBook
        {
            Id = "book123",
            Title = "Title 1",
            ScraperId = "test.scraper",
        };

        var book2 = new ScrapedBook
        {
            Id = "book123",
            Title = "Title 2", // Different title
            ScraperId = "test.scraper",
        };

        // Act & Assert
        Assert.IsTrue(book1.Equals(book2));
    }

    [TestMethod]
    public void ScrapedBook_Equals_DifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var book1 = new ScrapedBook
        {
            Id = "book123",
            Title = "Title",
            ScraperId = "test.scraper",
        };

        var book2 = new ScrapedBook
        {
            Id = "book456",
            Title = "Title",
            ScraperId = "test.scraper",
        };

        // Act & Assert
        Assert.IsFalse(book1.Equals(book2));
    }

    [TestMethod]
    public void ScrapedBook_Equals_DifferentScraperId_ShouldNotBeEqual()
    {
        // Arrange
        var book1 = new ScrapedBook
        {
            Id = "book123",
            Title = "Title",
            ScraperId = "scraper1",
        };

        var book2 = new ScrapedBook
        {
            Id = "book123",
            Title = "Title",
            ScraperId = "scraper2",
        };

        // Act & Assert
        Assert.IsFalse(book1.Equals(book2));
    }

    [TestMethod]
    public void ScrapedBook_GetHashCode_SameIdAndScraperId_ShouldBeSame()
    {
        // Arrange
        var book1 = new ScrapedBook
        {
            Id = "book123",
            Title = "Title 1",
            ScraperId = "test.scraper",
        };

        var book2 = new ScrapedBook
        {
            Id = "book123",
            Title = "Title 2",
            ScraperId = "test.scraper",
        };

        // Act & Assert
        Assert.AreEqual(book1.GetHashCode(), book2.GetHashCode());
    }
}
