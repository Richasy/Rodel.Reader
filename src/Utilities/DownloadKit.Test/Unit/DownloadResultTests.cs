// Copyright (c) Richasy. All rights reserved.

namespace DownloadKit.Test.Unit;

/// <summary>
/// DownloadResult 单元测试.
/// </summary>
[TestClass]
public sealed class DownloadResultTests
{
    /// <summary>
    /// 测试成功结果创建.
    /// </summary>
    [TestMethod]
    public void Success_ShouldCreateSuccessResult()
    {
        // Arrange
        var filePath = @"C:\test\file.epub";
        var totalBytes = 1024L;
        var elapsed = TimeSpan.FromSeconds(10);

        // Act
        var result = DownloadResult.Success(filePath, totalBytes, elapsed);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(filePath, result.FilePath);
        Assert.AreEqual(totalBytes, result.TotalBytes);
        Assert.AreEqual(elapsed, result.ElapsedTime);
        Assert.AreEqual(DownloadState.Completed, result.State);
        Assert.IsNull(result.Error);
    }

    /// <summary>
    /// 测试失败结果创建.
    /// </summary>
    [TestMethod]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var filePath = @"C:\test\file.epub";
        var error = new InvalidOperationException("Test error");
        var elapsed = TimeSpan.FromSeconds(5);

        // Act
        var result = DownloadResult.Failure(filePath, error, elapsed, 512);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(filePath, result.FilePath);
        Assert.AreEqual(512, result.TotalBytes);
        Assert.AreEqual(elapsed, result.ElapsedTime);
        Assert.AreEqual(DownloadState.Failed, result.State);
        Assert.AreEqual(error, result.Error);
    }

    /// <summary>
    /// 测试取消结果创建.
    /// </summary>
    [TestMethod]
    public void Canceled_ShouldCreateCanceledResult()
    {
        // Arrange
        var filePath = @"C:\test\file.epub";
        var elapsed = TimeSpan.FromSeconds(3);

        // Act
        var result = DownloadResult.Canceled(filePath, elapsed, 256);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(filePath, result.FilePath);
        Assert.AreEqual(256, result.TotalBytes);
        Assert.AreEqual(elapsed, result.ElapsedTime);
        Assert.AreEqual(DownloadState.Canceled, result.State);
        Assert.IsNull(result.Error);
    }

    /// <summary>
    /// 测试平均速度计算.
    /// </summary>
    [TestMethod]
    public void AverageSpeed_ShouldCalculateCorrectly()
    {
        // Arrange
        var result = DownloadResult.Success(@"C:\test\file.epub", 1000, TimeSpan.FromSeconds(10));

        // Assert
        Assert.AreEqual(100, result.AverageSpeed); // 1000 bytes / 10 seconds = 100 B/s
    }

    /// <summary>
    /// 测试平均速度计算 - 零耗时.
    /// </summary>
    [TestMethod]
    public void AverageSpeed_WithZeroTime_ShouldReturnZero()
    {
        // Arrange
        var result = DownloadResult.Success(@"C:\test\file.epub", 1000, TimeSpan.Zero);

        // Assert
        Assert.AreEqual(0, result.AverageSpeed);
    }
}
