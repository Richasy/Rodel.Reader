// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Test.UnitTests;

/// <summary>
/// 同步结果模型测试.
/// </summary>
[TestClass]
public class SyncResultTests
{
    [TestMethod]
    public void CreateSuccess_SetsPropertiesCorrectly()
    {
        // Arrange
        var epubPath = @"C:\Books\test.epub";
        var bookInfo = new FanQieBookInfo
        {
            BookId = "12345",
            Title = "测试书籍",
        };
        var stats = new SyncStatistics
        {
            TotalChapters = 100,
            NewlyDownloaded = 50,
        };

        // Act
        var result = SyncResult.CreateSuccess(epubPath, bookInfo, stats);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.IsCancelled);
        Assert.AreEqual(epubPath, result.EpubPath);
        Assert.AreEqual(bookInfo, result.BookInfo);
        Assert.AreEqual(stats, result.Statistics);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void CreateFailure_SetsPropertiesCorrectly()
    {
        // Arrange
        var errorMessage = "网络错误";
        var bookInfo = new FanQieBookInfo
        {
            BookId = "12345",
            Title = "测试书籍",
        };

        // Act
        var result = SyncResult.CreateFailure(errorMessage, bookInfo);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.IsCancelled);
        Assert.AreEqual(errorMessage, result.ErrorMessage);
        Assert.AreEqual(bookInfo, result.BookInfo);
        Assert.IsNull(result.EpubPath);
    }

    [TestMethod]
    public void CreateCancelled_SetsPropertiesCorrectly()
    {
        // Arrange
        var bookInfo = new FanQieBookInfo
        {
            BookId = "12345",
            Title = "测试书籍",
        };

        // Act
        var result = SyncResult.CreateCancelled(bookInfo);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.IsCancelled);
        Assert.AreEqual(bookInfo, result.BookInfo);
    }
}
