// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Test.UnitTests;

/// <summary>
/// 目录哈希计算器测试.
/// </summary>
[TestClass]
public class TocHashCalculatorTests
{
    [TestMethod]
    public void Calculate_WithSameChapters_ReturnsSameHash()
    {
        // Arrange
        var volumes1 = CreateTestVolumes(["ch1", "ch2", "ch3"]);
        var volumes2 = CreateTestVolumes(["ch1", "ch2", "ch3"]);

        // Act
        var hash1 = TocHashCalculator.Calculate(volumes1);
        var hash2 = TocHashCalculator.Calculate(volumes2);

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void Calculate_WithDifferentChapters_ReturnsDifferentHash()
    {
        // Arrange
        var volumes1 = CreateTestVolumes(["ch1", "ch2", "ch3"]);
        var volumes2 = CreateTestVolumes(["ch1", "ch2", "ch4"]);

        // Act
        var hash1 = TocHashCalculator.Calculate(volumes1);
        var hash2 = TocHashCalculator.Calculate(volumes2);

        // Assert
        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void Calculate_WithDifferentOrder_ReturnsSameHash_BecauseSortedByOrder()
    {
        // Arrange - 即使输入顺序不同，因为按 Order 排序，所以应该相同
        var volumes1 = new List<BookVolume>
        {
            new()
            {
                Index = 0,
                Name = "卷一",
                Chapters =
                [
                    new() { ItemId = "ch2", Title = "第二章", Order = 2 },
                    new() { ItemId = "ch1", Title = "第一章", Order = 1 },
                ],
            },
        };

        var volumes2 = new List<BookVolume>
        {
            new()
            {
                Index = 0,
                Name = "卷一",
                Chapters =
                [
                    new() { ItemId = "ch1", Title = "第一章", Order = 1 },
                    new() { ItemId = "ch2", Title = "第二章", Order = 2 },
                ],
            },
        };

        // Act
        var hash1 = TocHashCalculator.Calculate(volumes1);
        var hash2 = TocHashCalculator.Calculate(volumes2);

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void Calculate_WithEmptyVolumes_ReturnsConsistentHash()
    {
        // Arrange
        var volumes1 = new List<BookVolume>();
        var volumes2 = new List<BookVolume>();

        // Act
        var hash1 = TocHashCalculator.Calculate(volumes1);
        var hash2 = TocHashCalculator.Calculate(volumes2);

        // Assert
        Assert.AreEqual(hash1, hash2);
        Assert.AreEqual(16, hash1.Length); // 应该是 16 位十六进制
    }

    [TestMethod]
    public void Calculate_FromChapterIds_MatchesVolumeCalculation()
    {
        // Arrange
        var chapterIds = new[] { "ch1", "ch2", "ch3" };
        var volumes = CreateTestVolumes(chapterIds);

        // Act
        var hash1 = TocHashCalculator.Calculate(chapterIds);
        var hash2 = TocHashCalculator.Calculate(volumes);

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void Calculate_Returns16CharHex()
    {
        // Arrange
        var volumes = CreateTestVolumes(["ch1", "ch2"]);

        // Act
        var hash = TocHashCalculator.Calculate(volumes);

        // Assert
        Assert.AreEqual(16, hash.Length);
        Assert.IsTrue(hash.All(c => "0123456789ABCDEF".Contains(c, StringComparison.Ordinal)));
    }

    private static List<BookVolume> CreateTestVolumes(string[] chapterIds)
    {
        return
        [
            new()
            {
                Index = 0,
                Name = "卷一",
                Chapters = chapterIds.Select((id, i) => new ChapterItem
                {
                    ItemId = id,
                    Title = $"第{i + 1}章",
                    Order = i + 1,
                }).ToList(),
            },
        ];
    }
}
