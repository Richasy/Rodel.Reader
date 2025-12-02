// Copyright (c) Richasy. All rights reserved.

namespace DownloadKit.Test.Unit;

/// <summary>
/// ProgressTracker 单元测试.
/// </summary>
[TestClass]
public sealed class ProgressTrackerTests
{
    /// <summary>
    /// 测试初始状态.
    /// </summary>
    [TestMethod]
    public void Initial_ShouldBePendingState()
    {
        // Arrange & Act
        var tracker = new ProgressTracker();

        // Assert
        Assert.AreEqual(DownloadState.Pending, tracker.State);
        Assert.AreEqual(0, tracker.BytesReceived);
        Assert.IsNull(tracker.TotalBytes);
    }

    /// <summary>
    /// 测试开始追踪.
    /// </summary>
    [TestMethod]
    public void Start_ShouldChangeStateToDownloading()
    {
        // Arrange
        var tracker = new ProgressTracker();

        // Act
        tracker.Start();

        // Assert
        Assert.AreEqual(DownloadState.Downloading, tracker.State);
    }

    /// <summary>
    /// 测试停止追踪.
    /// </summary>
    [TestMethod]
    public void Stop_ShouldChangeStateToSpecifiedState()
    {
        // Arrange
        var tracker = new ProgressTracker();
        tracker.Start();

        // Act
        tracker.Stop(DownloadState.Completed);

        // Assert
        Assert.AreEqual(DownloadState.Completed, tracker.State);
    }

    /// <summary>
    /// 测试更新已接收字节数.
    /// </summary>
    [TestMethod]
    public void UpdateBytesReceived_ShouldAccumulate()
    {
        // Arrange
        var tracker = new ProgressTracker();
        tracker.Start();

        // Act
        tracker.UpdateBytesReceived(100);
        tracker.UpdateBytesReceived(200);

        // Assert
        Assert.AreEqual(300, tracker.BytesReceived);
    }

    /// <summary>
    /// 测试设置总字节数.
    /// </summary>
    [TestMethod]
    public void SetTotalBytes_ShouldUpdateTotalBytes()
    {
        // Arrange
        var tracker = new ProgressTracker();

        // Act
        tracker.SetTotalBytes(1000);

        // Assert
        Assert.AreEqual(1000, tracker.TotalBytes);
    }

    /// <summary>
    /// 测试获取进度信息.
    /// </summary>
    [TestMethod]
    public void GetProgress_ShouldReturnCorrectProgress()
    {
        // Arrange
        var tracker = new ProgressTracker();
        tracker.SetTotalBytes(1000);
        tracker.Start();
        tracker.UpdateBytesReceived(500);

        // Act
        var progress = tracker.GetProgress();

        // Assert
        Assert.AreEqual(500, progress.BytesReceived);
        Assert.AreEqual(1000, progress.TotalBytes);
        Assert.AreEqual(DownloadState.Downloading, progress.State);
    }

    /// <summary>
    /// 测试进度报告节流.
    /// </summary>
    [TestMethod]
    public void ShouldReportProgress_WithThrottling_ShouldRespectThrottleTime()
    {
        // Arrange
        var tracker = new ProgressTracker(throttleMs: 100);
        tracker.SetTotalBytes(100000);
        tracker.Start();

        // Act - 第一次应该报告
        tracker.UpdateBytesReceived(10000);
        var firstReport = tracker.ShouldReportProgress();

        // 立即再次检查 - 应该被节流
        tracker.UpdateBytesReceived(1000);
        var secondReport = tracker.ShouldReportProgress();

        // Assert
        Assert.IsTrue(firstReport);
        Assert.IsFalse(secondReport);
    }

    /// <summary>
    /// 测试进度报告节流 - 等待后应该报告.
    /// </summary>
    [TestMethod]
    public async Task ShouldReportProgress_AfterThrottleTime_ShouldReport()
    {
        // Arrange
        var tracker = new ProgressTracker(throttleMs: 50);
        tracker.SetTotalBytes(100000);
        tracker.Start();

        // Act
        tracker.UpdateBytesReceived(10000);
        _ = tracker.ShouldReportProgress(); // 第一次报告

        await Task.Delay(60); // 等待超过节流时间

        tracker.UpdateBytesReceived(10000);
        var shouldReport = tracker.ShouldReportProgress();

        // Assert
        Assert.IsTrue(shouldReport);
    }
}
