// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Models;

/// <summary>
/// Fb2Binary 单元测试。
/// </summary>
[TestClass]
public sealed class Fb2BinaryTests
{
    [TestMethod]
    public void Fb2Binary_DefaultValues_AreCorrect()
    {
        // Act
        var binary = new Fb2Binary();

        // Assert
        Assert.AreEqual(string.Empty, binary.Id);
        Assert.AreEqual(string.Empty, binary.MediaType);
        Assert.AreEqual(string.Empty, binary.Base64Data);
        Assert.IsFalse(binary.IsImage);
        Assert.AreEqual(0, binary.Size);
    }

    [TestMethod]
    public void Fb2Binary_IsImage_WithImageMediaType_ReturnsTrue()
    {
        // Arrange
        var binary = new Fb2Binary
        {
            MediaType = "image/jpeg",
        };

        // Act & Assert
        Assert.IsTrue(binary.IsImage);
    }

    [TestMethod]
    public void Fb2Binary_IsImage_WithNonImageMediaType_ReturnsFalse()
    {
        // Arrange
        var binary = new Fb2Binary
        {
            MediaType = "application/pdf",
        };

        // Act & Assert
        Assert.IsFalse(binary.IsImage);
    }

    [TestMethod]
    public void Fb2Binary_GetBytes_WithValidBase64_ReturnsBytes()
    {
        // Arrange
        var originalBytes = new byte[] { 1, 2, 3, 4, 5 };
        var binary = new Fb2Binary
        {
            Base64Data = Convert.ToBase64String(originalBytes),
        };

        // Act
        var result = binary.GetBytes();

        // Assert
        CollectionAssert.AreEqual(originalBytes, result);
    }

    [TestMethod]
    public void Fb2Binary_GetBytes_WithEmptyData_ReturnsEmptyArray()
    {
        // Arrange
        var binary = new Fb2Binary
        {
            Base64Data = string.Empty,
        };

        // Act
        var result = binary.GetBytes();

        // Assert
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void Fb2Binary_GetBytes_WithInvalidBase64_ReturnsEmptyArray()
    {
        // Arrange
        var binary = new Fb2Binary
        {
            Base64Data = "not-valid-base64!!!",
        };

        // Act
        var result = binary.GetBytes();

        // Assert
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void Fb2Binary_Size_ReturnsApproximateSize()
    {
        // Arrange
        var originalBytes = new byte[100];
        var binary = new Fb2Binary
        {
            Base64Data = Convert.ToBase64String(originalBytes),
        };

        // Act
        var size = binary.Size;

        // Assert
        // Base64 编码后大小约为原始大小的 4/3，所以反向计算会有误差
        Assert.IsTrue(size >= 90 && size <= 110, $"Size {size} is not in expected range");
    }

    [TestMethod]
    public void Fb2Binary_ToString_ReturnsFormattedString()
    {
        // Arrange
        var binary = new Fb2Binary
        {
            Id = "cover.jpg",
            MediaType = "image/jpeg",
            Base64Data = Convert.ToBase64String(new byte[1000]),
        };

        // Act
        var result = binary.ToString();

        // Assert
        Assert.IsTrue(result.Contains("cover.jpg"));
        Assert.IsTrue(result.Contains("image/jpeg"));
        Assert.IsTrue(result.Contains("bytes"));
    }
}
