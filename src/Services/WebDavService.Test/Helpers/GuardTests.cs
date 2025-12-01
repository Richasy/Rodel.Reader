// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Helpers;

[TestClass]
public class GuardTests
{
    [TestMethod]
    public void NotNull_WithNonNullValue_DoesNotThrow()
    {
        // Arrange
        var value = new object();

        // Act & Assert - should not throw
        Guard.NotNull(value);
    }

    [TestMethod]
    public void NotNull_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        object? value = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => Guard.NotNull(value));
    }

    [TestMethod]
    public void NotNullOrEmpty_WithValidString_DoesNotThrow()
    {
        // Arrange
        var value = "test";

        // Act & Assert - should not throw
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
}
