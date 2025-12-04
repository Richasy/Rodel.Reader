// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Test.Unit;

/// <summary>
/// 数据模型单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ModelTests
{
    [TestMethod]
    public void LegadoClientOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new LegadoClientOptions { BaseUrl = "http://localhost" };

        // Assert
        Assert.AreEqual(ServerType.Legado, options.ServerType);
        Assert.AreEqual(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.IsTrue(options.IgnoreSslErrors);
        Assert.IsNull(options.AccessToken);
        Assert.AreEqual("RodelReader/1.0", options.UserAgent);
    }

    [TestMethod]
    public void Book_Serialization_RoundTrip()
    {
        // Arrange
        var book = new Book
        {
            BookUrl = "https://example.com/book/1",
            Name = "测试书籍",
            Author = "测试作者",
            CoverUrl = "https://example.com/cover.jpg",
            Intro = "这是一本测试书籍",
            TotalChapterNum = 100,
            DurChapterIndex = 10,
        };

        // Act
        var json = JsonSerializer.Serialize(book);
        var deserialized = JsonSerializer.Deserialize<Book>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(book.BookUrl, deserialized.BookUrl);
        Assert.AreEqual(book.Name, deserialized.Name);
        Assert.AreEqual(book.Author, deserialized.Author);
        Assert.AreEqual(book.CoverUrl, deserialized.CoverUrl);
        Assert.AreEqual(book.Intro, deserialized.Intro);
        Assert.AreEqual(book.TotalChapterNum, deserialized.TotalChapterNum);
        Assert.AreEqual(book.DurChapterIndex, deserialized.DurChapterIndex);
    }

    [TestMethod]
    public void Chapter_Serialization_RoundTrip()
    {
        // Arrange
        var chapter = new Chapter
        {
            Url = "https://example.com/chapter/1",
            Title = "第一章",
            Index = 0,
            IsVolume = false,
            BookUrl = "https://example.com/book/1",
        };

        // Act
        var json = JsonSerializer.Serialize(chapter);
        var deserialized = JsonSerializer.Deserialize<Chapter>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(chapter.Url, deserialized.Url);
        Assert.AreEqual(chapter.Title, deserialized.Title);
        Assert.AreEqual(chapter.Index, deserialized.Index);
        Assert.AreEqual(chapter.IsVolume, deserialized.IsVolume);
    }

    [TestMethod]
    public void BookProgress_Serialization_RoundTrip()
    {
        // Arrange
        var progress = new BookProgress
        {
            Name = "测试书籍",
            Author = "测试作者",
            DurChapterIndex = 5,
            DurChapterTitle = "第五章",
            DurChapterPos = 100,
            DurChapterTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
        };

        // Act
        var json = JsonSerializer.Serialize(progress);
        var deserialized = JsonSerializer.Deserialize<BookProgress>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(progress.Name, deserialized.Name);
        Assert.AreEqual(progress.Author, deserialized.Author);
        Assert.AreEqual(progress.DurChapterIndex, deserialized.DurChapterIndex);
        Assert.AreEqual(progress.DurChapterTitle, deserialized.DurChapterTitle);
    }

    [TestMethod]
    public void BookSource_Serialization_RoundTrip()
    {
        // Arrange
        var source = new BookSource
        {
            BookSourceUrl = "https://example.com",
            BookSourceName = "测试书源",
            BookSourceGroup = "测试",
            BookSourceType = 0,
            Enabled = true,
        };

        // Act
        var json = JsonSerializer.Serialize(source);
        var deserialized = JsonSerializer.Deserialize<BookSource>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(source.BookSourceUrl, deserialized.BookSourceUrl);
        Assert.AreEqual(source.BookSourceName, deserialized.BookSourceName);
        Assert.AreEqual(source.BookSourceGroup, deserialized.BookSourceGroup);
        Assert.AreEqual(source.Enabled, deserialized.Enabled);
    }

    [TestMethod]
    public void ChapterContent_Properties_AreSet()
    {
        // Arrange & Act
        var content = new ChapterContent
        {
            BookUrl = "https://example.com/book/1",
            ChapterIndex = 5,
            Title = "第五章",
            Content = "<p>章节内容</p>",
        };

        // Assert
        Assert.AreEqual("https://example.com/book/1", content.BookUrl);
        Assert.AreEqual(5, content.ChapterIndex);
        Assert.AreEqual("第五章", content.Title);
        Assert.AreEqual("<p>章节内容</p>", content.Content);
    }

    [TestMethod]
    public void ServerType_HasExpectedValues()
    {
        // Assert
        Assert.AreEqual(0, (int)ServerType.Legado);
        Assert.AreEqual(1, (int)ServerType.HectorqinReader);
    }
}
