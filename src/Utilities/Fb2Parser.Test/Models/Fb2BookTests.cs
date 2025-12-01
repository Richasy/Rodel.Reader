// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Models;

/// <summary>
/// Fb2Book 单元测试。
/// </summary>
[TestClass]
public sealed class Fb2BookTests
{
    [TestMethod]
    public async Task Fb2Book_FromMinimalContent_HasCorrectStructure()
    {
        // Arrange
        var content = TestDataFactory.CreateMinimalFb2();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsNotNull(book);
        Assert.IsNotNull(book.Metadata);
        Assert.IsNotNull(book.Navigation);
        Assert.IsNotNull(book.Sections);
        Assert.IsNotNull(book.Binaries);
        Assert.IsNotNull(book.Images);
    }

    [TestMethod]
    public async Task Fb2Book_FindBinaryById_WithValidId_ReturnsBinary()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Act
        var binary = book.FindBinaryById("cover.jpg");

        // Assert
        Assert.IsNotNull(binary);
        Assert.AreEqual("cover.jpg", binary.Id);
    }

    [TestMethod]
    public async Task Fb2Book_FindBinaryById_WithHashPrefix_ReturnsBinary()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Act
        var binary = book.FindBinaryById("#cover.jpg");

        // Assert
        Assert.IsNotNull(binary);
        Assert.AreEqual("cover.jpg", binary.Id);
    }

    [TestMethod]
    public async Task Fb2Book_FindBinaryById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Act
        var binary = book.FindBinaryById("nonexistent.jpg");

        // Assert
        Assert.IsNull(binary);
    }

    [TestMethod]
    public async Task Fb2Book_ReadBinaryContent_ReturnsBytes()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();
        using var book = await Fb2Reader.ReadFromStringAsync(content);
        var binary = book.Binaries.First();

        // Act
        var bytes = book.ReadBinaryContent(binary);

        // Assert
        Assert.IsNotNull(bytes);
        Assert.IsTrue(bytes.Length > 0);
    }

    [TestMethod]
    public async Task Fb2Book_ReadBinaryContentById_ReturnsBytes()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Act
        var bytes = await book.ReadBinaryContentAsync("cover.jpg");

        // Assert
        Assert.IsNotNull(bytes);
        Assert.IsTrue(bytes.Length > 0);
    }

    [TestMethod]
    public async Task Fb2Book_ReadBinaryContentById_WithInvalidId_ThrowsException()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<Fb2ParseException>(async () =>
        {
            await book.ReadBinaryContentAsync("nonexistent.jpg");
        });
    }

    [TestMethod]
    public async Task Fb2Book_GetAllSections_ReturnsFlattenedList()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithNestedSections();
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Act
        var allSections = book.GetAllSections();

        // Assert
        Assert.IsNotNull(allSections);
        // 应该包含所有嵌套的章节
        Assert.IsTrue(allSections.Count >= book.Sections.Count);
    }

    [TestMethod]
    public async Task Fb2Book_Dispose_PreventsSubsequentOperations()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();
        var book = await Fb2Reader.ReadFromStringAsync(content);

        // Act
        book.Dispose();

        // Assert
        _ = Assert.ThrowsExactly<ObjectDisposedException>(() => book.FindBinaryById("cover.jpg"));
    }

    [TestMethod]
    public async Task Fb2Book_Images_OnlyContainsImageBinaries()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Act & Assert
        foreach (var image in book.Images)
        {
            Assert.IsTrue(image.IsImage, $"Binary {image.Id} is not an image");
        }
    }
}
