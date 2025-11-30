// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Models;

/// <summary>
/// MobiBook 测试。
/// </summary>
[TestClass]
public sealed class MobiBookTests
{
    /// <summary>
    /// 测试 Dispose 方法。
    /// </summary>
    [TestMethod]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        using var stream = TestDataFactory.CreateMinimalMobiStream();
        var book = MobiReader.Read(stream);

        // Act & Assert - 不应抛出异常
        book.Dispose();
    }

    /// <summary>
    /// 测试多次 Dispose 不会抛出异常。
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        using var stream = TestDataFactory.CreateMinimalMobiStream();
        var book = MobiReader.Read(stream);

        // Act & Assert - 多次 Dispose 不应抛出异常
        book.Dispose();
        book.Dispose();
        book.Dispose();
    }

    /// <summary>
    /// 测试 Dispose 后读取图片应抛出异常。
    /// </summary>
    [TestMethod]
    public async Task ReadImageContentAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        using var stream = TestDataFactory.CreateMobiWithImageStream();
        var book = await MobiReader.ReadAsync(stream);
        book.Dispose();

        // Act & Assert
        if (book.Images.Count > 0)
        {
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(
                async () => await book.ReadImageContentAsync(book.Images[0]));
        }
    }

    /// <summary>
    /// 测试 FindImageByIndex 方法。
    /// </summary>
    [TestMethod]
    public void FindImageByIndex_ExistingIndex_ShouldReturnImage()
    {
        // Arrange
        using var stream = TestDataFactory.CreateMobiWithImageStream();
        using var book = MobiReader.Read(stream);

        if (book.Images.Count == 0)
        {
            Assert.Inconclusive("测试数据中没有图片");
            return;
        }

        var expectedIndex = book.Images[0].Index;

        // Act
        var image = book.FindImageByIndex(expectedIndex);

        // Assert
        Assert.IsNotNull(image);
        Assert.AreEqual(expectedIndex, image.Index);
    }

    /// <summary>
    /// 测试 FindImageByIndex 方法 - 不存在的索引。
    /// </summary>
    [TestMethod]
    public void FindImageByIndex_NonExistingIndex_ShouldReturnNull()
    {
        // Arrange
        using var stream = TestDataFactory.CreateMinimalMobiStream();
        using var book = MobiReader.Read(stream);

        // Act
        var image = book.FindImageByIndex(99999);

        // Assert
        Assert.IsNull(image);
    }
}
