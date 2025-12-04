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
            new() { Id = "img_001_0", Url = "https://example.com/img1.jpg" },
            new() { Id = "img_001_1", Url = "https://example.com/img2.jpg" },
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
        Assert.AreEqual("img_001_0", content.Images?[0].Id);
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
        Assert.AreEqual(10, options.BatchSize);
        Assert.AreEqual(500, options.RequestDelayMs);
        Assert.IsNull(options.SelfHostApiBaseUrl);
        Assert.AreEqual("https://fqnovel.richasy.net", FanQieClientOptions.BuiltInApiBaseUrl);
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

    #region Comment Tests

    [TestMethod]
    public void Comment_RequiredProperties_AreSet()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Id = "comment_123",
            Content = "这是一条测试评论",
        };

        // Assert
        Assert.AreEqual("comment_123", comment.Id);
        Assert.AreEqual("这是一条测试评论", comment.Content);
    }

    [TestMethod]
    public void Comment_OptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Id = "comment_123",
            Content = "测试评论",
        };

        // Assert
        Assert.IsNull(comment.UserId);
        Assert.IsNull(comment.UserName);
        Assert.IsNull(comment.Avatar);
        Assert.IsNull(comment.Pictures);
    }

    [TestMethod]
    public void Comment_AllProperties_CanBeSet()
    {
        // Arrange
        var pictures = new List<Uri>
        {
            new("https://example.com/pic1.jpg"),
            new("https://example.com/pic2.jpg"),
        };
        var publishTime = DateTime.Now;

        // Act
        var comment = new Comment
        {
            Id = "comment_123",
            Content = "完整测试评论",
            UserId = "user_001",
            UserName = "测试用户",
            Avatar = new Uri("https://example.com/avatar.jpg"),
            IsAuthor = true,
            PublishTime = publishTime,
            LikeCount = 100,
            ReplyCount = 10,
            Pictures = pictures,
        };

        // Assert
        Assert.AreEqual("comment_123", comment.Id);
        Assert.AreEqual("完整测试评论", comment.Content);
        Assert.AreEqual("user_001", comment.UserId);
        Assert.AreEqual("测试用户", comment.UserName);
        Assert.IsNotNull(comment.Avatar);
        Assert.AreEqual("https://example.com/avatar.jpg", comment.Avatar.ToString());
        Assert.IsTrue(comment.IsAuthor);
        Assert.AreEqual(publishTime, comment.PublishTime);
        Assert.AreEqual(100, comment.LikeCount);
        Assert.AreEqual(10, comment.ReplyCount);
        Assert.IsNotNull(comment.Pictures);
        Assert.AreEqual(2, comment.Pictures.Count);
    }

    [TestMethod]
    public void Comment_IsAuthor_DefaultsToFalse()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Id = "comment_123",
            Content = "测试评论",
        };

        // Assert
        Assert.IsFalse(comment.IsAuthor);
    }

    [TestMethod]
    public void Comment_Counts_DefaultToZero()
    {
        // Arrange & Act
        var comment = new Comment
        {
            Id = "comment_123",
            Content = "测试评论",
        };

        // Assert
        Assert.AreEqual(0, comment.LikeCount);
        Assert.AreEqual(0, comment.ReplyCount);
    }

    #endregion

    #region CommentListResult Tests

    [TestMethod]
    public void CommentListResult_RequiredProperties_AreSet()
    {
        // Arrange & Act
        var result = new CommentListResult
        {
            Comments = new List<Comment>(),
        };

        // Assert
        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(0, result.Comments.Count);
    }

    [TestMethod]
    public void CommentListResult_AllProperties_CanBeSet()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new() { Id = "1", Content = "评论1" },
            new() { Id = "2", Content = "评论2" },
        };

        // Act
        var result = new CommentListResult
        {
            Comments = comments,
            ParagraphIndex = 5,
            HasMore = true,
            NextOffset = "100",
            ParagraphContent = "这是段落原文内容",
        };

        // Assert
        Assert.AreEqual(2, result.Comments.Count);
        Assert.AreEqual(5, result.ParagraphIndex);
        Assert.IsTrue(result.HasMore);
        Assert.AreEqual("100", result.NextOffset);
        Assert.AreEqual("这是段落原文内容", result.ParagraphContent);
    }

    [TestMethod]
    public void CommentListResult_OptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var result = new CommentListResult
        {
            Comments = new List<Comment>(),
        };

        // Assert
        Assert.IsNull(result.NextOffset);
        Assert.IsNull(result.ParagraphContent);
    }

    [TestMethod]
    public void CommentListResult_HasMore_DefaultsToFalse()
    {
        // Arrange & Act
        var result = new CommentListResult
        {
            Comments = new List<Comment>(),
        };

        // Assert
        Assert.IsFalse(result.HasMore);
    }

    [TestMethod]
    public void CommentListResult_ParagraphIndex_DefaultsToZero()
    {
        // Arrange & Act
        var result = new CommentListResult
        {
            Comments = new List<Comment>(),
        };

        // Assert
        Assert.AreEqual(0, result.ParagraphIndex);
    }

    #endregion
}
