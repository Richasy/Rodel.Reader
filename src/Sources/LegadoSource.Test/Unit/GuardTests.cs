// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.Legado.Helpers;

namespace Richasy.RodelReader.Sources.Legado.Test.Unit;

/// <summary>
/// Guard 辅助类单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class GuardTests
{
    [TestMethod]
    public void NotNullOrEmpty_WithValidString_ReturnsValue()
    {
        // Arrange
        var str = "test";

        // Act
        var result = Guard.NotNullOrEmpty(str);

        // Assert
        Assert.AreEqual(str, result);
    }

    [TestMethod]
    public void NotNullOrEmpty_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? str = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => Guard.NotNullOrEmpty(str));
    }

    [TestMethod]
    public void NotNullOrEmpty_WithEmpty_ThrowsArgumentException()
    {
        // Arrange
        var str = string.Empty;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => Guard.NotNullOrEmpty(str));
    }

    [TestMethod]
    public void NotNullOrEmpty_WithWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var str = "   ";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => Guard.NotNullOrEmpty(str));
    }

    [TestMethod]
    public void NotNull_WithValue_ReturnsValue()
    {
        // Arrange
        var obj = new object();

        // Act
        var result = Guard.NotNull(obj);

        // Assert
        Assert.AreSame(obj, result);
    }

    [TestMethod]
    public void NotNull_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        object? obj = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => Guard.NotNull(obj));
    }

    [TestMethod]
    public void NonNegative_WithPositiveValue_ReturnsValue()
    {
        // Arrange
        var value = 5;

        // Act
        var result = Guard.NonNegative(value);

        // Assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void NonNegative_WithZero_ReturnsValue()
    {
        // Arrange
        var value = 0;

        // Act
        var result = Guard.NonNegative(value);

        // Assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void NonNegative_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = -1;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.NonNegative(value));
    }

    [TestMethod]
    public void Positive_WithPositiveValue_ReturnsValue()
    {
        // Arrange
        var value = 5;

        // Act
        var result = Guard.Positive(value);

        // Assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void Positive_WithZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = 0;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.Positive(value));
    }

    [TestMethod]
    public void Positive_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = -1;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.Positive(value));
    }
}
