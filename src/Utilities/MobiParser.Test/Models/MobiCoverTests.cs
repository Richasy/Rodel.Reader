// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Models;

/// <summary>
/// MobiCover 测试。
/// </summary>
[TestClass]
public sealed class MobiCoverTests
{
    /// <summary>
    /// 测试构造函数和属性。
    /// </summary>
    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var mediaType = "image/jpeg";
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        var cover = new MobiCover(mediaType, () => Task.FromResult(expectedData));

        // Assert
        Assert.AreEqual(mediaType, cover.MediaType);
    }

    /// <summary>
    /// 测试 ReadContentAsync 方法。
    /// </summary>
    [TestMethod]
    public async Task ReadContentAsync_ShouldReturnData()
    {
        // Arrange
        var expectedData = TestDataFactory.CreateMinimalJpeg();
        var cover = new MobiCover("image/jpeg", () => Task.FromResult(expectedData));

        // Act
        var data = await cover.ReadContentAsync();

        // Assert
        CollectionAssert.AreEqual(expectedData, data);
    }

    /// <summary>
    /// 测试 ReadContent 同步方法。
    /// </summary>
    [TestMethod]
    public void ReadContent_ShouldReturnData()
    {
        // Arrange
        var expectedData = TestDataFactory.CreateMinimalPng();
        var cover = new MobiCover("image/png", () => Task.FromResult(expectedData));

        // Act
        var data = cover.ReadContent();

        // Assert
        CollectionAssert.AreEqual(expectedData, data);
    }
}
