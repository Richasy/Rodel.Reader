// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Abstractions.Test;

[TestClass]
public class PluginMetadataTests
{
    [TestMethod]
    public void PluginMetadata_RequiredPropertiesSet_ShouldBeValid()
    {
        // Arrange & Act
        var metadata = new PluginMetadata
        {
            Id = "com.example.testplugin",
            Name = "Test Plugin",
            Version = new Version(1, 0, 0),
        };

        // Assert
        Assert.AreEqual("com.example.testplugin", metadata.Id);
        Assert.AreEqual("Test Plugin", metadata.Name);
        Assert.AreEqual(new Version(1, 0, 0), metadata.Version);
    }

    [TestMethod]
    public void PluginMetadata_OptionalPropertiesSet_ShouldBeValid()
    {
        // Arrange & Act
        var metadata = new PluginMetadata
        {
            Id = "com.example.testplugin",
            Name = "Test Plugin",
            Version = new Version(1, 2, 3),
            Author = "Test Author",
            Description = "Test Description",
            Homepage = "https://example.com",
            MinHostVersion = new Version(1, 0, 0),
            IconUri = "https://example.com/icon.png",
            SupportedCultures = ["zh-CN", "en-US"],
        };

        // Assert
        Assert.AreEqual("Test Author", metadata.Author);
        Assert.AreEqual("Test Description", metadata.Description);
        Assert.AreEqual("https://example.com", metadata.Homepage);
        Assert.AreEqual(new Version(1, 0, 0), metadata.MinHostVersion);
        Assert.AreEqual("https://example.com/icon.png", metadata.IconUri);
        Assert.AreEqual(2, metadata.SupportedCultures!.Count);
    }

    [TestMethod]
    public void PluginMetadata_Equals_SameId_ShouldBeEqual()
    {
        // Arrange
        var metadata1 = new PluginMetadata
        {
            Id = "com.example.plugin",
            Name = "Plugin 1",
            Version = new Version(1, 0, 0),
        };

        var metadata2 = new PluginMetadata
        {
            Id = "com.example.plugin",
            Name = "Plugin 2",
            Version = new Version(2, 0, 0),
        };

        // Act & Assert
        Assert.IsTrue(metadata1.Equals(metadata2));
    }

    [TestMethod]
    public void PluginMetadata_Equals_DifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var metadata1 = new PluginMetadata
        {
            Id = "com.example.plugin1",
            Name = "Plugin",
            Version = new Version(1, 0, 0),
        };

        var metadata2 = new PluginMetadata
        {
            Id = "com.example.plugin2",
            Name = "Plugin",
            Version = new Version(1, 0, 0),
        };

        // Act & Assert
        Assert.IsFalse(metadata1.Equals(metadata2));
    }

    [TestMethod]
    public void PluginMetadata_GetHashCode_SameId_ShouldBeSame()
    {
        // Arrange
        var metadata1 = new PluginMetadata
        {
            Id = "com.example.plugin",
            Name = "Plugin 1",
            Version = new Version(1, 0, 0),
        };

        var metadata2 = new PluginMetadata
        {
            Id = "com.example.plugin",
            Name = "Plugin 2",
            Version = new Version(2, 0, 0),
        };

        // Act & Assert
        Assert.AreEqual(metadata1.GetHashCode(), metadata2.GetHashCode());
    }
}
