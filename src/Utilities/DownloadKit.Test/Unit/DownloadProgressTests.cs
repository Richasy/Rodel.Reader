// Copyright (c) Richasy. All rights reserved.

namespace DownloadKit.Test.Unit;

/// <summary>
/// DownloadProgress 单元测试.
/// </summary>
[TestClass]
public sealed class DownloadProgressTests
{
    /// <summary>
    /// 测试百分比计算 - 已知总大小.
    /// </summary>
    [TestMethod]
    public void Percentage_WithTotalBytes_ShouldCalculateCorrectly()
    {
        // Arrange
        var progress = new DownloadProgress(500, 1000, 100, DownloadState.Downloading);

        // Assert
        Assert.AreEqual(50.0, progress.Percentage);
    }

    /// <summary>
    /// 测试百分比计算 - 未知总大小.
    /// </summary>
    [TestMethod]
    public void Percentage_WithoutTotalBytes_ShouldReturnNull()
    {
        // Arrange
        var progress = new DownloadProgress(500, null, 100, DownloadState.Downloading);

        // Assert
        Assert.IsNull(progress.Percentage);
    }

    /// <summary>
    /// 测试剩余时间估算.
    /// </summary>
    [TestMethod]
    public void EstimatedRemaining_ShouldCalculateCorrectly()
    {
        // Arrange
        var progress = new DownloadProgress(500, 1000, 100, DownloadState.Downloading);

        // Act
        var remaining = progress.EstimatedRemaining;

        // Assert
        Assert.IsNotNull(remaining);
        Assert.AreEqual(5, remaining.Value.TotalSeconds); // 500 bytes / 100 bytes/s = 5s
    }

    /// <summary>
    /// 测试剩余时间估算 - 速度为零.
    /// </summary>
    [TestMethod]
    public void EstimatedRemaining_WithZeroSpeed_ShouldReturnNull()
    {
        // Arrange
        var progress = new DownloadProgress(500, 1000, 0, DownloadState.Downloading);

        // Assert
        Assert.IsNull(progress.EstimatedRemaining);
    }

    /// <summary>
    /// 测试剩余时间估算 - 下载完成.
    /// </summary>
    [TestMethod]
    public void EstimatedRemaining_WhenCompleted_ShouldReturnZero()
    {
        // Arrange
        var progress = new DownloadProgress(1000, 1000, 100, DownloadState.Completed);

        // Assert
        Assert.AreEqual(TimeSpan.Zero, progress.EstimatedRemaining);
    }

    /// <summary>
    /// 测试格式化速度 - B/s.
    /// </summary>
    [TestMethod]
    public void GetFormattedSpeed_Bytes_ShouldFormatCorrectly()
    {
        // Arrange
        var progress = new DownloadProgress(0, null, 500, DownloadState.Downloading);

        // Assert
        Assert.AreEqual("500 B/s", progress.GetFormattedSpeed());
    }

    /// <summary>
    /// 测试格式化速度 - KB/s.
    /// </summary>
    [TestMethod]
    public void GetFormattedSpeed_KiloBytes_ShouldFormatCorrectly()
    {
        // Arrange
        var progress = new DownloadProgress(0, null, 1536, DownloadState.Downloading); // 1.5 KB

        // Assert
        Assert.AreEqual("1.50 KB/s", progress.GetFormattedSpeed());
    }

    /// <summary>
    /// 测试格式化速度 - MB/s.
    /// </summary>
    [TestMethod]
    public void GetFormattedSpeed_MegaBytes_ShouldFormatCorrectly()
    {
        // Arrange
        var progress = new DownloadProgress(0, null, 1.5 * 1024 * 1024, DownloadState.Downloading);

        // Assert
        Assert.AreEqual("1.50 MB/s", progress.GetFormattedSpeed());
    }

    /// <summary>
    /// 测试格式化已下载大小.
    /// </summary>
    [TestMethod]
    public void GetFormattedBytesReceived_ShouldFormatCorrectly()
    {
        // Arrange
        var progress = new DownloadProgress(1536, null, 0, DownloadState.Downloading);

        // Assert
        Assert.AreEqual("1.50 KB", progress.GetFormattedBytesReceived());
    }

    /// <summary>
    /// 测试格式化总大小 - 未知.
    /// </summary>
    [TestMethod]
    public void GetFormattedTotalBytes_Unknown_ShouldReturnUnknown()
    {
        // Arrange
        var progress = new DownloadProgress(500, null, 0, DownloadState.Downloading);

        // Assert
        Assert.AreEqual("未知", progress.GetFormattedTotalBytes());
    }
}
