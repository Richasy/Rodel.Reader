// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Models;

/// <summary>
/// Fb2Cover 单元测试。
/// </summary>
[TestClass]
public sealed class Fb2CoverTests
{
    [TestMethod]
    public void Fb2Cover_Constructor_SetsProperties()
    {
        // Arrange
        var imageId = "cover.jpg";
        var mediaType = "image/jpeg";
        var expectedBytes = new byte[] { 1, 2, 3 };

        // Act
        var cover = new Fb2Cover(imageId, mediaType, () => Task.FromResult(expectedBytes));

        // Assert
        Assert.AreEqual(imageId, cover.ImageId);
        Assert.AreEqual(mediaType, cover.MediaType);
    }

    [TestMethod]
    public async Task Fb2Cover_ReadContentAsync_ReturnsBytes()
    {
        // Arrange
        var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
        var cover = new Fb2Cover("cover.jpg", "image/jpeg", () => Task.FromResult(expectedBytes));

        // Act
        var result = await cover.ReadContentAsync();

        // Assert
        CollectionAssert.AreEqual(expectedBytes, result);
    }

    [TestMethod]
    public void Fb2Cover_ReadContent_ReturnsBytes()
    {
        // Arrange
        var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
        var cover = new Fb2Cover("cover.jpg", "image/jpeg", () => Task.FromResult(expectedBytes));

        // Act
        var result = cover.ReadContent();

        // Assert
        CollectionAssert.AreEqual(expectedBytes, result);
    }
}
