// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.Legado.Models;

namespace Richasy.RodelReader.Components.Legado.Test.UnitTests;

/// <summary>
/// 同步结果测试.
/// </summary>
[TestClass]
public class SyncResultTests
{
    [TestMethod]
    public void CreateSuccess_CreatesSuccessResult()
    {
        // Arrange
        var epubPath = "/path/to/book.epub";
        var bookInfo = new LegadoBookInfo
        {
            Title = "测试书籍",
            BookUrl = "https://example.com/book/123",
            BookSource = "https://source.com",
            ServerUrl = "http://192.168.1.1:1234",
        };
        var stats = new SyncStatistics
        {
            TotalChapters = 100,
            NewlyDownloaded = 95,
            Failed = 5,
            Duration = TimeSpan.FromMinutes(5),
        };

        // Act
        var result = SyncResult.CreateSuccess(epubPath, bookInfo, stats);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(epubPath, result.EpubPath);
        Assert.IsNotNull(result.Statistics);
        Assert.AreEqual(100, result.Statistics.TotalChapters);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    public void CreateFailure_CreatesFailureResult()
    {
        // Arrange
        var error = "下载失败：网络超时";

        // Act
        var result = SyncResult.CreateFailure(error);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(error, result.ErrorMessage);
        Assert.IsNull(result.EpubPath);
    }

    [TestMethod]
    public void CreateCancelled_CreatesCancelledResult()
    {
        // Act
        var result = SyncResult.CreateCancelled();

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.IsCancelled);
        Assert.IsNull(result.EpubPath);
    }

    [TestMethod]
    public void CreateFailure_WithBookInfo_IncludesBookInfo()
    {
        // Arrange
        var bookInfo = new LegadoBookInfo
        {
            Title = "测试书籍",
            BookUrl = "https://example.com/book/123",
            BookSource = "https://source.com",
            ServerUrl = "http://192.168.1.1:1234",
        };

        // Act
        var result = SyncResult.CreateFailure("错误", bookInfo);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.BookInfo);
        Assert.AreEqual("测试书籍", result.BookInfo.Title);
    }

    [TestMethod]
    public void Statistics_TracksAllMetrics()
    {
        // Arrange
        var stats = new SyncStatistics
        {
            TotalChapters = 200,
            NewlyDownloaded = 180,
            Failed = 10,
            RestoredFromCache = 5,
            Reused = 5,
            ImagesDownloaded = 100,
            VolumeChapters = 10,
            Duration = TimeSpan.FromMinutes(10),
        };

        // Assert
        Assert.AreEqual(200, stats.TotalChapters);
        Assert.AreEqual(180, stats.NewlyDownloaded);
        Assert.AreEqual(10, stats.Failed);
        Assert.AreEqual(5, stats.RestoredFromCache);
        Assert.AreEqual(5, stats.Reused);
        Assert.AreEqual(100, stats.ImagesDownloaded);
        Assert.AreEqual(10, stats.VolumeChapters);
        Assert.AreEqual(TimeSpan.FromMinutes(10), stats.Duration);
    }

    [TestMethod]
    public void CreateFailure_WithEmptyError_StillCreatesFailureResult()
    {
        // Act
        var result = SyncResult.CreateFailure(string.Empty);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(string.Empty, result.ErrorMessage);
    }

    [TestMethod]
    public void Statistics_DefaultValues_AreZero()
    {
        // Arrange
        var stats = new SyncStatistics();

        // Assert
        Assert.AreEqual(0, stats.TotalChapters);
        Assert.AreEqual(0, stats.NewlyDownloaded);
        Assert.AreEqual(0, stats.Failed);
        Assert.AreEqual(0, stats.RestoredFromCache);
        Assert.AreEqual(0, stats.Reused);
        Assert.AreEqual(0, stats.ImagesDownloaded);
        Assert.AreEqual(0, stats.VolumeChapters);
        Assert.AreEqual(TimeSpan.Zero, stats.Duration);
    }
}
