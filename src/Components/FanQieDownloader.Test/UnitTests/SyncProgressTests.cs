// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Test.UnitTests;

/// <summary>
/// 同步进度模型测试.
/// </summary>
[TestClass]
public class SyncProgressTests
{
    [TestMethod]
    public void DownloadingChapters_CalculatesCorrectProgress()
    {
        // Arrange
        var detail = new DownloadProgressDetail
        {
            Completed = 50,
            Total = 100,
            Failed = 5,
            Skipped = 10,
        };

        // Act
        var progress = SyncProgress.DownloadingChapters(detail);

        // Assert
        Assert.AreEqual(SyncPhase.DownloadingChapters, progress.Phase);
        // TotalProgress = 10 + (60 * 0.5) = 40
        Assert.AreEqual(40, progress.TotalProgress);
        Assert.AreEqual(60, progress.PhaseProgress);
    }

    [TestMethod]
    public void GeneratingEpub_CalculatesCorrectProgress()
    {
        // Arrange
        var detail = new GenerateProgressDetail
        {
            ProcessedChapters = 50,
            TotalChapters = 100,
            Step = "处理章节",
        };

        // Act
        var progress = SyncProgress.GeneratingEpub(detail);

        // Assert
        Assert.AreEqual(SyncPhase.GeneratingEpub, progress.Phase);
        // TotalProgress = 75 + (50 * 0.2) = 85
        Assert.AreEqual(85, progress.TotalProgress);
        Assert.AreEqual(50, progress.PhaseProgress);
    }

    [TestMethod]
    public void Completed_Returns100Percent()
    {
        // Act
        var progress = SyncProgress.Completed();

        // Assert
        Assert.AreEqual(SyncPhase.Completed, progress.Phase);
        Assert.AreEqual(100, progress.TotalProgress);
        Assert.AreEqual(100, progress.PhaseProgress);
    }

    [TestMethod]
    public void DownloadProgressDetail_CalculatesPercentage()
    {
        // Arrange
        var detail = new DownloadProgressDetail
        {
            Completed = 30,
            Total = 100,
            Skipped = 20,
        };

        // Act & Assert
        Assert.AreEqual(50, detail.Percentage);
    }

    [TestMethod]
    public void DownloadProgressDetail_ZeroTotal_ReturnsZero()
    {
        // Arrange
        var detail = new DownloadProgressDetail
        {
            Completed = 0,
            Total = 0,
        };

        // Act & Assert
        Assert.AreEqual(0, detail.Percentage);
    }
}
