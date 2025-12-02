// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.ZLibrary.Helpers;

namespace Richasy.RodelReader.Sources.ZLibrary.Test.Unit;

/// <summary>
/// Guard 辅助类单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class GuardTests
{
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
    public void NotNullOrWhiteSpace_WithValidString_ReturnsString()
    {
        // Arrange
        var str = "test";

        // Act
        var result = Guard.NotNullOrWhiteSpace(str);

        // Assert
        Assert.AreEqual(str, result);
    }

    [TestMethod]
    public void NotNullOrWhiteSpace_WithNull_ThrowsArgumentException()
    {
        // Arrange
        string? str = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => Guard.NotNullOrWhiteSpace(str));
    }

    [TestMethod]
    public void NotNullOrWhiteSpace_WithEmpty_ThrowsArgumentException()
    {
        // Arrange
        var str = string.Empty;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => Guard.NotNullOrWhiteSpace(str));
    }

    [TestMethod]
    public void NotNullOrWhiteSpace_WithWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var str = "   ";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => Guard.NotNullOrWhiteSpace(str));
    }

    [TestMethod]
    public void IsAuthenticated_WhenTrue_DoesNotThrow()
    {
        // Act & Assert - should not throw
        Guard.IsAuthenticated(true);
    }

    [TestMethod]
    public void IsAuthenticated_WhenFalse_ThrowsNotAuthenticatedException()
    {
        // Act & Assert
        Assert.ThrowsExactly<NotAuthenticatedException>(() => Guard.IsAuthenticated(false));
    }
}
