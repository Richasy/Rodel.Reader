// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Internal;

/// <summary>
/// ImageDetector 测试。
/// </summary>
[TestClass]
public sealed class ImageDetectorTests
{
    /// <summary>
    /// 测试 JPEG 检测。
    /// </summary>
    [TestMethod]
    public void DetectMediaType_Jpeg_ShouldReturnImageJpeg()
    {
        // Arrange
        var jpegData = TestDataFactory.CreateMinimalJpeg();

        // Act
        var mediaType = ImageDetector.DetectMediaType(jpegData);

        // Assert
        Assert.AreEqual("image/jpeg", mediaType);
    }

    /// <summary>
    /// 测试 PNG 检测。
    /// </summary>
    [TestMethod]
    public void DetectMediaType_Png_ShouldReturnImagePng()
    {
        // Arrange
        var pngData = TestDataFactory.CreateMinimalPng();

        // Act
        var mediaType = ImageDetector.DetectMediaType(pngData);

        // Assert
        Assert.AreEqual("image/png", mediaType);
    }

    /// <summary>
    /// 测试 GIF 检测。
    /// </summary>
    [TestMethod]
    public void DetectMediaType_Gif87a_ShouldReturnImageGif()
    {
        // Arrange
        var gifData = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61, 0x00, 0x00 }; // GIF87a + padding

        // Act
        var mediaType = ImageDetector.DetectMediaType(gifData);

        // Assert
        Assert.AreEqual("image/gif", mediaType);
    }

    /// <summary>
    /// 测试 GIF89a 检测。
    /// </summary>
    [TestMethod]
    public void DetectMediaType_Gif89a_ShouldReturnImageGif()
    {
        // Arrange
        var gifData = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x00, 0x00 }; // GIF89a + padding

        // Act
        var mediaType = ImageDetector.DetectMediaType(gifData);

        // Assert
        Assert.AreEqual("image/gif", mediaType);
    }

    /// <summary>
    /// 测试 BMP 检测。
    /// </summary>
    [TestMethod]
    public void DetectMediaType_Bmp_ShouldReturnImageBmp()
    {
        // Arrange
        var bmpData = new byte[] { 0x42, 0x4D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        // Act
        var mediaType = ImageDetector.DetectMediaType(bmpData);

        // Assert
        Assert.AreEqual("image/bmp", mediaType);
    }

    /// <summary>
    /// 测试无效数据。
    /// </summary>
    [TestMethod]
    public void DetectMediaType_InvalidData_ShouldReturnNull()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

        // Act
        var mediaType = ImageDetector.DetectMediaType(invalidData);

        // Assert
        Assert.IsNull(mediaType);
    }

    /// <summary>
    /// 测试 null 数据。
    /// </summary>
    [TestMethod]
    public void DetectMediaType_NullData_ShouldReturnNull()
    {
        // Act
        var mediaType = ImageDetector.DetectMediaType(null!);

        // Assert
        Assert.IsNull(mediaType);
    }

    /// <summary>
    /// 测试数据太短。
    /// </summary>
    [TestMethod]
    public void DetectMediaType_TooShort_ShouldReturnNull()
    {
        // Arrange
        var shortData = new byte[] { 0xFF, 0xD8 };

        // Act
        var mediaType = ImageDetector.DetectMediaType(shortData);

        // Assert
        Assert.IsNull(mediaType);
    }

    /// <summary>
    /// 测试 IsImage 方法。
    /// </summary>
    [TestMethod]
    public void IsImage_ValidJpeg_ShouldReturnTrue()
    {
        // Arrange
        var jpegData = TestDataFactory.CreateMinimalJpeg();

        // Act
        var isImage = ImageDetector.IsImage(jpegData);

        // Assert
        Assert.IsTrue(isImage);
    }

    /// <summary>
    /// 测试 IsImage 方法对无效数据。
    /// </summary>
    [TestMethod]
    public void IsImage_InvalidData_ShouldReturnFalse()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

        // Act
        var isImage = ImageDetector.IsImage(invalidData);

        // Assert
        Assert.IsFalse(isImage);
    }
}
