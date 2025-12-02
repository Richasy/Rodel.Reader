// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Test.Unit;

/// <summary>
/// 模型测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ModelTests
{
    #region BookItem Tests

    [TestMethod]
    public void BookItem_RequiredProperties_AreSet()
    {
        // Arrange & Act
        var book = new BookItem
        {
            BookId = "12345",
            Title = "测试书籍",
        };

        // Assert
        Assert.AreEqual("12345", book.BookId);
        Assert.AreEqual("测试书籍", book.Title);
    }

    [TestMethod]
    public void BookItem_OptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var book = new BookItem
        {
            BookId = "12345",
            Title = "测试书籍",
        };

        // Assert
        Assert.IsNull(book.Author);
        Assert.IsNull(book.Abstract);
        Assert.IsNull(book.CoverUrl);
        Assert.IsNull(book.Category);
        Assert.IsNull(book.Score);
    }

    [TestMethod]
    public void BookItem_CreationStatus_DefaultsToOngoing()
    {
        // Arrange & Act
        var book = new BookItem
        {
            BookId = "12345",
            Title = "测试书籍",
        };

        // Assert
        Assert.AreEqual(BookCreationStatus.Ongoing, book.CreationStatus);
    }

    #endregion

    #region BookDetail Tests

    [TestMethod]
    public void BookDetail_RequiredProperties_AreSet()
    {
        // Arrange & Act
        var detail = new BookDetail
        {
            BookId = "12345",
            Title = "测试书籍详情",
        };

        // Assert
        Assert.AreEqual("12345", detail.BookId);
        Assert.AreEqual("测试书籍详情", detail.Title);
    }

    [TestMethod]
    public void BookDetail_AllProperties_CanBeSet()
    {
        // Arrange
        var tags = new List<string> { "玄幻", "都市" };
        var lastUpdate = DateTimeOffset.UtcNow;

        // Act
        var detail = new BookDetail
        {
            BookId = "12345",
            Title = "完整测试书籍",
            Author = "测试作者",
            AuthorId = "author_001",
            Abstract = "这是一本测试书籍",
            CoverUrl = "https://example.com/cover.jpg",
            Category = "玄幻",
            Tags = tags,
            WordCount = 100000,
            ChapterCount = 100,
            CreationStatus = BookCreationStatus.Completed,
            Gender = BookGender.Male,
            LastUpdateTime = lastUpdate,
            Score = "9.5",
        };

        // Assert
        Assert.AreEqual("测试作者", detail.Author);
        Assert.AreEqual("author_001", detail.AuthorId);
        Assert.AreEqual(100000, detail.WordCount);
        Assert.AreEqual(100, detail.ChapterCount);
        Assert.AreEqual(BookCreationStatus.Completed, detail.CreationStatus);
        Assert.AreEqual(BookGender.Male, detail.Gender);
        Assert.AreEqual(2, detail.Tags?.Count);
    }

    #endregion

    #region ChapterItem Tests

    [TestMethod]
    public void ChapterItem_RequiredProperties_AreSet()
    {
        // Arrange & Act
        var chapter = new ChapterItem
        {
            ItemId = "item_001",
            Title = "第一章 开始",
            Order = 1,
        };

        // Assert
        Assert.AreEqual("item_001", chapter.ItemId);
        Assert.AreEqual("第一章 开始", chapter.Title);
        Assert.AreEqual(1, chapter.Order);
    }

    [TestMethod]
    public void ChapterItem_LockProperties_DefaultToFalse()
    {
        // Arrange & Act
        var chapter = new ChapterItem
        {
            ItemId = "item_001",
            Title = "第一章",
            Order = 1,
        };

        // Assert
        Assert.IsFalse(chapter.IsLocked);
        Assert.IsFalse(chapter.NeedPay);
    }

    #endregion

    #region ChapterContent Tests

    [TestMethod]
    public void ChapterContent_RequiredProperties_AreSet()
    {
        // Arrange & Act
        var content = new ChapterContent
        {
            ItemId = "item_001",
            BookId = "book_001",
            BookTitle = "测试书籍",
            Title = "第一章",
            TextContent = "这是章节内容",
            HtmlContent = "<p>这是章节内容</p>",
        };

        // Assert
        Assert.AreEqual("item_001", content.ItemId);
        Assert.AreEqual("book_001", content.BookId);
        Assert.AreEqual("测试书籍", content.BookTitle);
        Assert.AreEqual("第一章", content.Title);
        Assert.AreEqual("这是章节内容", content.TextContent);
        Assert.AreEqual("<p>这是章节内容</p>", content.HtmlContent);
    }

    [TestMethod]
    public void ChapterContent_Images_CanBeSet()
    {
        // Arrange
        var images = new List<ChapterImage>
        {
            new() { Url = "https://example.com/img1.jpg", Offset = 100 },
            new() { Url = "https://example.com/img2.jpg", Offset = 500 },
        };

        // Act
        var content = new ChapterContent
        {
            ItemId = "item_001",
            BookId = "book_001",
            BookTitle = "测试书籍",
            Title = "第一章",
            TextContent = "内容",
            HtmlContent = "<p>内容</p>",
            Images = images,
        };

        // Assert
        Assert.AreEqual(2, content.Images?.Count);
        Assert.AreEqual("https://example.com/img1.jpg", content.Images?[0].Url);
        Assert.AreEqual(100, content.Images?[0].Offset);
    }

    #endregion

    #region BookVolume Tests

    [TestMethod]
    public void BookVolume_RequiredProperties_AreSet()
    {
        // Arrange
        var chapters = new List<ChapterItem>
        {
            new() { ItemId = "1", Title = "第一章", Order = 1 },
            new() { ItemId = "2", Title = "第二章", Order = 2 },
        };

        // Act
        var volume = new BookVolume
        {
            Index = 0,
            Name = "第一卷",
            Chapters = chapters,
        };

        // Assert
        Assert.AreEqual(0, volume.Index);
        Assert.AreEqual("第一卷", volume.Name);
        Assert.AreEqual(2, volume.Chapters.Count);
    }

    #endregion

    #region SearchResult Tests

    [TestMethod]
    public void SearchResult_RequiredProperties_AreSet()
    {
        // Arrange
        var items = new List<BookItem>
        {
            new() { BookId = "1", Title = "书籍1" },
            new() { BookId = "2", Title = "书籍2" },
        };

        // Act
        var result = new SearchResult<BookItem>
        {
            Items = items,
            HasMore = true,
            NextOffset = 20,
            SearchId = "search_001",
        };

        // Assert
        Assert.AreEqual(2, result.Items.Count);
        Assert.IsTrue(result.HasMore);
        Assert.AreEqual(20, result.NextOffset);
        Assert.AreEqual("search_001", result.SearchId);
    }

    #endregion

    #region FanQieClientOptions Tests

    [TestMethod]
    public void FanQieClientOptions_DefaultValues_AreSet()
    {
        // Arrange & Act
        var options = new FanQieClientOptions();

        // Assert
        Assert.AreEqual(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.AreEqual(FanQieClientOptions.DefaultUserAgent, options.UserAgent);
        Assert.AreEqual(FanQieClientOptions.DefaultAid, options.Aid);
        Assert.AreEqual(3, options.MaxConcurrentRequests);
        Assert.AreEqual(25, options.BatchSize);
        Assert.AreEqual(500, options.RequestDelayMs);
    }

    [TestMethod]
    public void FanQieClientOptions_CanBeCustomized()
    {
        // Arrange & Act
        var options = new FanQieClientOptions
        {
            Timeout = TimeSpan.FromMinutes(1),
            MaxConcurrentRequests = 5,
            BatchSize = 50,
            RequestDelayMs = 1000,
            CustomHeaders = new Dictionary<string, string>
            {
                ["X-Custom-Header"] = "value",
            },
        };

        // Assert
        Assert.AreEqual(TimeSpan.FromMinutes(1), options.Timeout);
        Assert.AreEqual(5, options.MaxConcurrentRequests);
        Assert.AreEqual(50, options.BatchSize);
        Assert.AreEqual(1000, options.RequestDelayMs);
        Assert.IsNotNull(options.CustomHeaders);
        Assert.AreEqual("value", options.CustomHeaders["X-Custom-Header"]);
    }

    #endregion

    #region Enum Tests

    [TestMethod]
    public void BookCreationStatus_HasCorrectValues()
    {
        // Assert
        Assert.AreEqual(0, (int)BookCreationStatus.Ongoing);
        Assert.AreEqual(1, (int)BookCreationStatus.Completed);
    }

    [TestMethod]
    public void BookGender_HasCorrectValues()
    {
        // Assert
        Assert.AreEqual(0, (int)BookGender.Unknown);
        Assert.AreEqual(1, (int)BookGender.Male);
        Assert.AreEqual(2, (int)BookGender.Female);
    }

    #endregion
}
