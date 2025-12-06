// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions.Test.Unit;

/// <summary>
/// Guard 辅助类单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class GuardTests
{
    [TestMethod]
    public void NotNull_WithNonNullValue_DoesNotThrow()
    {
        // Arrange
        var value = "test";

        // Act & Assert
        Guard.NotNull(value);
    }

    [TestMethod]
    public void NotNull_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => Guard.NotNull(value));
    }

    [TestMethod]
    public void NotNullOrEmpty_WithValidString_DoesNotThrow()
    {
        // Arrange
        var value = "test";

        // Act & Assert
        Guard.NotNullOrEmpty(value);
    }

    [TestMethod]
    public void NotNullOrEmpty_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => Guard.NotNullOrEmpty(value));
    }

    [TestMethod]
    public void NotNullOrEmpty_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var value = string.Empty;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => Guard.NotNullOrEmpty(value));
    }

    [TestMethod]
    public void NotNullOrWhiteSpace_WithValidString_DoesNotThrow()
    {
        // Arrange
        var value = "test";

        // Act & Assert
        Guard.NotNullOrWhiteSpace(value);
    }

    [TestMethod]
    public void NotNullOrWhiteSpace_WithWhiteSpaceString_ThrowsArgumentException()
    {
        // Arrange
        var value = "   ";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => Guard.NotNullOrWhiteSpace(value));
    }

    [TestMethod]
    public void NonNegative_WithZero_DoesNotThrow()
    {
        // Arrange
        var value = 0;

        // Act & Assert
        Guard.NonNegative(value);
    }

    [TestMethod]
    public void NonNegative_WithPositiveValue_DoesNotThrow()
    {
        // Arrange
        var value = 10;

        // Act & Assert
        Guard.NonNegative(value);
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
    public void Positive_WithPositiveValue_DoesNotThrow()
    {
        // Arrange
        var value = 1;

        // Act & Assert
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
    public void NotEmpty_WithNonEmptyCollection_DoesNotThrow()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };

        // Act & Assert
        Guard.NotEmpty(collection);
    }

    [TestMethod]
    public void NotEmpty_WithEmptyCollection_ThrowsArgumentException()
    {
        // Arrange
        var collection = Array.Empty<int>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => Guard.NotEmpty(collection));
    }

    [TestMethod]
    public void NotEmpty_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        int[]? collection = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => Guard.NotEmpty(collection));
    }
}
