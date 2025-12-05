// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Abstractions.Test;

[TestClass]
public class PluginCapabilityTests
{
    [TestMethod]
    public void PluginCapability_None_ShouldBeZero()
    {
        // Arrange & Act
        var capability = PluginCapability.None;

        // Assert
        Assert.AreEqual(0, (int)capability);
    }

    [TestMethod]
    public void PluginCapability_BookScraper_ShouldBePowerOfTwo()
    {
        // Arrange & Act
        var capability = PluginCapability.BookScraper;

        // Assert
        Assert.AreEqual(1, (int)capability);
    }

    [TestMethod]
    public void PluginCapability_MultipleCapabilities_ShouldCombine()
    {
        // Arrange
        var combined = PluginCapability.BookScraper | PluginCapability.BookSource;

        // Act & Assert
        Assert.IsTrue(combined.HasFlag(PluginCapability.BookScraper));
        Assert.IsTrue(combined.HasFlag(PluginCapability.BookSource));
        Assert.IsFalse(combined.HasFlag(PluginCapability.BookParser));
    }

    [TestMethod]
    public void PluginCapability_HasFlag_ShouldWork()
    {
        // Arrange
        var allCapabilities = PluginCapability.BookScraper
            | PluginCapability.BookSource
            | PluginCapability.BookParser
            | PluginCapability.BookExporter;

        // Act & Assert
        Assert.IsTrue(allCapabilities.HasFlag(PluginCapability.BookScraper));
        Assert.IsTrue(allCapabilities.HasFlag(PluginCapability.BookSource));
        Assert.IsTrue(allCapabilities.HasFlag(PluginCapability.BookParser));
        Assert.IsTrue(allCapabilities.HasFlag(PluginCapability.BookExporter));
    }

    [TestMethod]
    public void PluginCapability_BitwiseOperations_ShouldWork()
    {
        // Arrange
        var initial = PluginCapability.BookScraper | PluginCapability.BookSource;

        // Act - Remove BookSource
        var removed = initial & ~PluginCapability.BookSource;

        // Assert
        Assert.IsTrue(removed.HasFlag(PluginCapability.BookScraper));
        Assert.IsFalse(removed.HasFlag(PluginCapability.BookSource));
    }
}
