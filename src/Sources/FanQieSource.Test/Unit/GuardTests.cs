// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.FanQie.Helpers;

namespace Richasy.RodelReader.Sources.FanQie.Test.Unit;

/// <summary>
/// Guard 辅助类单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class GuardTests
{
    [TestMethod]
    public void NotNullOrEmpty_WithValidString_DoesNotThrow()
    {
        // Arrange
        var str = "test";

        // Act & Assert - should not throw
        Guard.NotNullOrEmpty(str);
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
    public void NotNull_WithValue_DoesNotThrow()
    {
        // Arrange
        var obj = new object();

        // Act & Assert - should not throw
        Guard.NotNull(obj);
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
    public void Positive_WithPositiveValue_DoesNotThrow()
    {
        // Arrange
        var value = 5;

        // Act & Assert - should not throw
        Guard.Positive(value);
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
    public void Positive_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = -1;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.Positive(value));
    }

    [TestMethod]
    public void NonNegative_WithPositiveValue_DoesNotThrow()
    {
        // Arrange
        var value = 5;

        // Act & Assert - should not throw
        Guard.NonNegative(value);
    }

    [TestMethod]
    public void NonNegative_WithZero_DoesNotThrow()
    {
        // Arrange
        var value = 0;

        // Act & Assert - should not throw
        Guard.NonNegative(value);
    }

    [TestMethod]
    public void NonNegative_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = -1;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.NonNegative(value));
    }
}
