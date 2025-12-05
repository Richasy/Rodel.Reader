// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.Legado.Internal;
using Richasy.RodelReader.Sources.Legado.Models;

namespace Richasy.RodelReader.Components.Legado.Test.UnitTests;

/// <summary>
/// 目录哈希计算器测试.
/// </summary>
[TestClass]
public class TocHashCalculatorTests
{
    [TestMethod]
    public void Calculate_ReturnsConsistentHash()
    {
        // Arrange
        var chapters = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/1", Title = "第一章" },
            new() { Index = 1, Url = "https://example.com/chapter/2", Title = "第二章" },
        };

        // Act
        var hash1 = TocHashCalculator.Calculate(chapters);
        var hash2 = TocHashCalculator.Calculate(chapters);

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void Calculate_DifferentOrder_DifferentHash()
    {
        // Arrange
        var chapters1 = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/1", Title = "第一章" },
            new() { Index = 1, Url = "https://example.com/chapter/2", Title = "第二章" },
        };

        // 注意：Calculate 会按 Index 排序，所以这里我们交换顺序但保持不同的 URL
        var chapters2 = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/2", Title = "第二章" },
            new() { Index = 1, Url = "https://example.com/chapter/1", Title = "第一章" },
        };

        // Act
        var hash1 = TocHashCalculator.Calculate(chapters1);
        var hash2 = TocHashCalculator.Calculate(chapters2);

        // Assert
        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void Calculate_EmptyList_ReturnsHash()
    {
        // Arrange
        var chapters = new List<Chapter>();

        // Act
        var hash = TocHashCalculator.Calculate(chapters);

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(hash));
    }

    [TestMethod]
    public void Calculate_SingleChapter_ReturnsHash()
    {
        // Arrange
        var chapters = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/1", Title = "第一章" },
        };

        // Act
        var hash = TocHashCalculator.Calculate(chapters);

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(hash));
    }

    [TestMethod]
    public void Calculate_DifferentUrls_DifferentHash()
    {
        // Arrange
        var chapters1 = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/1", Title = "第一章" },
        };

        var chapters2 = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/999", Title = "第一章" },
        };

        // Act
        var hash1 = TocHashCalculator.Calculate(chapters1);
        var hash2 = TocHashCalculator.Calculate(chapters2);

        // Assert
        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void Calculate_LargeChapterList_ReturnsHash()
    {
        // Arrange
        var chapters = new List<Chapter>();
        for (var i = 0; i < 1000; i++)
        {
            chapters.Add(new Chapter
            {
                Index = i,
                Url = $"https://example.com/chapter/{i}",
                Title = $"第{i + 1}章",
            });
        }

        // Act
        var hash = TocHashCalculator.Calculate(chapters);

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(hash));
    }

    [TestMethod]
    public void Calculate_IsVolumeDoesNotAffectHash()
    {
        // 卷标记不应影响哈希计算（哈希仅基于URL）
        var chapters1 = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/1", Title = "卷一", IsVolume = true },
        };

        var chapters2 = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/1", Title = "卷一", IsVolume = false },
        };

        // Act
        var hash1 = TocHashCalculator.Calculate(chapters1);
        var hash2 = TocHashCalculator.Calculate(chapters2);

        // Assert - Hash 只基于 URL，所以应该相同
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void Calculate_HashLengthIs16()
    {
        // Arrange
        var chapters = new List<Chapter>
        {
            new() { Index = 0, Url = "https://example.com/chapter/1", Title = "第一章" },
        };

        // Act
        var hash = TocHashCalculator.Calculate(chapters);

        // Assert - 哈希长度应该是 16 个十六进制字符
        Assert.AreEqual(16, hash.Length);
    }

    [TestMethod]
    public void Calculate_WithEnumerable_ReturnsConsistentHash()
    {
        // Arrange
        var urls = new[] { "url1", "url2", "url3" };

        // Act
        var hash1 = TocHashCalculator.Calculate(urls);
        var hash2 = TocHashCalculator.Calculate(urls);

        // Assert
        Assert.AreEqual(hash1, hash2);
    }
}
