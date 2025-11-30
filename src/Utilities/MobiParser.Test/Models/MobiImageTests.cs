// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Models;

/// <summary>
/// MobiImage 测试。
/// </summary>
[TestClass]
public sealed class MobiImageTests
{
    /// <summary>
    /// 测试默认值。
    /// </summary>
    [TestMethod]
    public void DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var image = new MobiImage();

        // Assert
        Assert.AreEqual(0, image.Index);
        Assert.AreEqual(string.Empty, image.MediaType);
        Assert.AreEqual(0, image.Size);
        Assert.IsFalse(image.IsValid);
    }

    /// <summary>
    /// 测试 IsValid 属性 - 有效图片。
    /// </summary>
    [TestMethod]
    public void IsValid_ValidImage_ShouldReturnTrue()
    {
        // Arrange
        var image = new MobiImage
        {
            Index = 1,
            MediaType = "image/jpeg",
            Size = 1024,
        };

        // Assert
        Assert.IsTrue(image.IsValid);
    }

    /// <summary>
    /// 测试 IsValid 属性 - 无 MediaType。
    /// </summary>
    [TestMethod]
    public void IsValid_NoMediaType_ShouldReturnFalse()
    {
        // Arrange
        var image = new MobiImage
        {
            Index = 1,
            MediaType = string.Empty,
            Size = 1024,
        };

        // Assert
        Assert.IsFalse(image.IsValid);
    }

    /// <summary>
    /// 测试 IsValid 属性 - 无大小。
    /// </summary>
    [TestMethod]
    public void IsValid_ZeroSize_ShouldReturnFalse()
    {
        // Arrange
        var image = new MobiImage
        {
            Index = 1,
            MediaType = "image/jpeg",
            Size = 0,
        };

        // Assert
        Assert.IsFalse(image.IsValid);
    }

    /// <summary>
    /// 测试 ToString 方法。
    /// </summary>
    [TestMethod]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var image = new MobiImage
        {
            Index = 5,
            MediaType = "image/png",
            Size = 2048,
        };

        // Act
        var result = image.ToString();

        // Assert
        Assert.AreEqual("Image 5 (image/png, 2048 bytes)", result);
    }
}
