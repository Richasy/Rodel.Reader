// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.Legado.Models;

namespace Richasy.RodelReader.Components.Legado.Test.UnitTests;

/// <summary>
/// 同步进度测试.
/// </summary>
[TestClass]
public class SyncProgressTests
{
    [TestMethod]
    public void Analyzing_ReturnsCorrectPhase()
    {
        // Act
        var progress = SyncProgress.Analyzing();

        // Assert
        Assert.AreEqual(SyncPhase.Analyzing, progress.Phase);
        Assert.IsNotNull(progress.Message);
    }

    [TestMethod]
    public void FetchingToc_ReturnsCorrectPhase()
    {
        // Act
        var progress = SyncProgress.FetchingToc();

        // Assert
        Assert.AreEqual(SyncPhase.FetchingToc, progress.Phase);
        Assert.IsNotNull(progress.Message);
    }

    [TestMethod]
    public void DownloadProgressDetail_CalculatesPercentageCorrectly()
    {
        // Arrange
        var detail = new DownloadProgressDetail
        {
            Completed = 50,
            Total = 100,
            Failed = 5,
            Skipped = 10,
        };

        // Assert
        Assert.AreEqual(60, detail.Percentage); // (50 + 10) / 100 * 100 = 60%
    }

    [TestMethod]
    public void DownloadProgressDetail_ZeroTotal_ReturnsZeroPercentage()
    {
        // Arrange
        var detail = new DownloadProgressDetail
        {
            Completed = 0,
            Total = 0,
        };

        // Assert
        Assert.AreEqual(0, detail.Percentage);
    }

    [TestMethod]
    public void GenerateProgressDetail_CalculatesPercentageCorrectly()
    {
        // Arrange
        var detail = new GenerateProgressDetail
        {
            ProcessedChapters = 75,
            TotalChapters = 100,
        };

        // Assert
        Assert.AreEqual(75, detail.Percentage);
    }

    [TestMethod]
    public void GenerateProgressDetail_ZeroTotal_ReturnsZeroPercentage()
    {
        // Arrange
        var detail = new GenerateProgressDetail
        {
            ProcessedChapters = 0,
            TotalChapters = 0,
        };

        // Assert
        Assert.AreEqual(0, detail.Percentage);
    }

    [TestMethod]
    public void SyncProgress_CanIncludeDownloadDetail()
    {
        // Arrange
        var detail = new DownloadProgressDetail
        {
            Completed = 50,
            Total = 100,
            CurrentChapter = "第51章",
        };

        var progress = new SyncProgress
        {
            Phase = SyncPhase.DownloadingChapters,
            DownloadDetail = detail,
        };

        // Assert
        Assert.IsNotNull(progress.DownloadDetail);
        Assert.AreEqual("第51章", progress.DownloadDetail.CurrentChapter);
    }

    [TestMethod]
    public void SyncProgress_CanIncludeGenerateDetail()
    {
        // Arrange
        var detail = new GenerateProgressDetail
        {
            ProcessedChapters = 75,
            TotalChapters = 100,
            Step = "正在生成目录...",
        };

        var progress = new SyncProgress
        {
            Phase = SyncPhase.GeneratingEpub,
            GenerateDetail = detail,
        };

        // Assert
        Assert.IsNotNull(progress.GenerateDetail);
        Assert.AreEqual("正在生成目录...", progress.GenerateDetail.Step);
    }

    [TestMethod]
    public void AllSyncPhases_AreValid()
    {
        // Assert - 确保所有阶段都可用
        Assert.AreEqual(SyncPhase.Analyzing, SyncProgress.Analyzing().Phase);
        Assert.AreEqual(SyncPhase.FetchingToc, SyncProgress.FetchingToc().Phase);

        // 验证枚举值存在
        Assert.IsTrue(Enum.IsDefined(SyncPhase.DownloadingChapters));
        Assert.IsTrue(Enum.IsDefined(SyncPhase.DownloadingImages));
        Assert.IsTrue(Enum.IsDefined(SyncPhase.GeneratingEpub));
        Assert.IsTrue(Enum.IsDefined(SyncPhase.CleaningUp));
        Assert.IsTrue(Enum.IsDefined(SyncPhase.Completed));
        Assert.IsTrue(Enum.IsDefined(SyncPhase.Failed));
    }
}
