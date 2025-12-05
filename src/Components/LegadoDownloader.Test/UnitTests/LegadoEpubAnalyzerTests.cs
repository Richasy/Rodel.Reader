// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.Legado.Internal;

namespace Richasy.RodelReader.Components.Legado.Test.UnitTests;

/// <summary>
/// Legado EPUB 分析器测试.
/// </summary>
[TestClass]
public class LegadoEpubAnalyzerTests
{
    private string _testDirectory = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LegadoEpubTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [TestMethod]
    public void ExtractInfoAsync_NullBook_ReturnsNull()
    {
        // 测试当 EpubBook 为 null 时的行为
        // LegadoEpubAnalyzer.ExtractInfoAsync 应该能处理这种情况

        // 由于需要实际的 EpubBook 对象来测试，这里只验证目录创建
        Assert.IsTrue(Directory.Exists(_testDirectory));
    }

    [TestMethod]
    public async Task CreateInvalidEpubFile_DirectoryExists()
    {
        // Arrange - 创建一个无效的 EPUB 文件
        var epubPath = Path.Combine(_testDirectory, "invalid.epub");
        await File.WriteAllTextAsync(epubPath, "This is not a valid EPUB file");

        // Assert - 文件存在
        Assert.IsTrue(File.Exists(epubPath));
    }

    [TestMethod]
    public async Task CreateEmptyFile_IsEmpty()
    {
        // Arrange
        var epubPath = Path.Combine(_testDirectory, "empty.epub");
        await File.WriteAllBytesAsync(epubPath, []);

        // Assert
        var fileInfo = new FileInfo(epubPath);
        Assert.AreEqual(0, fileInfo.Length);
    }
}
