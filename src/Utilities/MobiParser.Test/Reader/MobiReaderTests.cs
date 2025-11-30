// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Reader;

/// <summary>
/// MobiReader 测试。
/// </summary>
[TestClass]
public sealed class MobiReaderTests
{
    /// <summary>
    /// 获取或设置测试上下文。
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// 测试从流解析最小 Mobi 文件。
    /// </summary>
    [TestMethod]
    public async Task ReadAsync_MinimalMobi_ShouldParseSuccessfully()
    {
        // Arrange
        using var stream = TestDataFactory.CreateMinimalMobiStream();

        // Act
        using var book = await MobiReader.ReadAsync(stream);

        // Assert
        Assert.IsNotNull(book);
        Assert.IsNotNull(book.Metadata);
        Assert.IsNull(book.FilePath, "从流加载时 FilePath 应为 null");

        TestContext.WriteLine($"标题: {book.Metadata.Title}");
        TestContext.WriteLine($"语言: {book.Metadata.Language}");
    }

    /// <summary>
    /// 测试从流解析包含图片的 Mobi 文件。
    /// </summary>
    [TestMethod]
    public async Task ReadAsync_MobiWithImage_ShouldParseImages()
    {
        // Arrange
        using var stream = TestDataFactory.CreateMobiWithImageStream();

        // Act
        using var book = await MobiReader.ReadAsync(stream);

        // Assert
        Assert.IsNotNull(book);
        Assert.IsNotNull(book.Metadata);

        TestContext.WriteLine($"标题: {book.Metadata.Title}");
        TestContext.WriteLine($"图片数量: {book.Images.Count}");

        foreach (var image in book.Images)
        {
            TestContext.WriteLine($"  图片 {image.Index}: {image.MediaType} ({image.Size} bytes)");
        }
    }

    /// <summary>
    /// 测试同步读取方法。
    /// </summary>
    [TestMethod]
    public void Read_MinimalMobi_ShouldParseSuccessfully()
    {
        // Arrange
        using var stream = TestDataFactory.CreateMinimalMobiStream();

        // Act
        using var book = MobiReader.Read(stream);

        // Assert
        Assert.IsNotNull(book);
        Assert.IsNotNull(book.Metadata);
    }

    /// <summary>
    /// 测试文件不存在时抛出异常。
    /// </summary>
    [TestMethod]
    public async Task ReadAsync_FileNotFound_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "C:\\NonExistent\\Book.mobi";

        // Act & Assert
        await Assert.ThrowsExactlyAsync<FileNotFoundException>(
            async () => await MobiReader.ReadAsync(nonExistentPath));
    }
}
