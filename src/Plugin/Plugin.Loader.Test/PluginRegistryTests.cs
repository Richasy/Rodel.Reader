// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Loader.Test;

[TestClass]
public class PluginRegistryTests
{
    [TestMethod]
    public void GetBookScrapers_NoPlugins_ShouldReturnEmptyList()
    {
        // Arrange
        var mockLoader = new Mock<IPluginLoader>();
        mockLoader.Setup(l => l.GetPluginsByCapability(PluginCapability.BookScraper))
            .Returns(new List<LoadedPlugin>());

        var logger = Mock.Of<ILogger<PluginRegistry>>();
        var registry = new PluginRegistry(mockLoader.Object, logger);

        // Act
        var scrapers = registry.GetBookScrapers();

        // Assert
        Assert.AreEqual(0, scrapers.Count);
    }

    [TestMethod]
    public void GetBookScrapers_WithPlugins_ShouldReturnScrapers()
    {
        // Arrange
        var mockFeature = new Mock<IBookScraperFeature>();
        mockFeature.Setup(f => f.FeatureId).Returns("test.scraper");
        mockFeature.Setup(f => f.FeatureName).Returns("Test Scraper");

        var mockPlugin = new Mock<IPlugin>();
        mockPlugin.Setup(p => p.Metadata).Returns(new PluginMetadata
        {
            Id = "test.plugin",
            Name = "Test Plugin",
            Version = new Version(1, 0, 0),
        });
        mockPlugin.Setup(p => p.GetAllFeatures())
            .Returns(new List<IPluginFeature> { mockFeature.Object });

        var loadedPlugin = CreateLoadedPlugin(mockPlugin.Object);

        var mockLoader = new Mock<IPluginLoader>();
        mockLoader.Setup(l => l.GetPluginsByCapability(PluginCapability.BookScraper))
            .Returns(new List<LoadedPlugin> { loadedPlugin });

        var logger = Mock.Of<ILogger<PluginRegistry>>();
        var registry = new PluginRegistry(mockLoader.Object, logger);

        // Act
        var scrapers = registry.GetBookScrapers();

        // Assert
        Assert.AreEqual(1, scrapers.Count);
        Assert.AreEqual("test.scraper", scrapers[0].FeatureId);
    }

    [TestMethod]
    public void GetBookScraper_ExistingScraper_ShouldReturn()
    {
        // Arrange
        var mockFeature = new Mock<IBookScraperFeature>();
        mockFeature.Setup(f => f.FeatureId).Returns("test.scraper");
        mockFeature.Setup(f => f.FeatureName).Returns("Test Scraper");

        var mockPlugin = new Mock<IPlugin>();
        mockPlugin.Setup(p => p.Metadata).Returns(new PluginMetadata
        {
            Id = "test.plugin",
            Name = "Test Plugin",
            Version = new Version(1, 0, 0),
        });
        mockPlugin.Setup(p => p.GetAllFeatures())
            .Returns(new List<IPluginFeature> { mockFeature.Object });

        var loadedPlugin = CreateLoadedPlugin(mockPlugin.Object);

        var mockLoader = new Mock<IPluginLoader>();
        mockLoader.Setup(l => l.GetPluginsByCapability(PluginCapability.BookScraper))
            .Returns(new List<LoadedPlugin> { loadedPlugin });

        var logger = Mock.Of<ILogger<PluginRegistry>>();
        var registry = new PluginRegistry(mockLoader.Object, logger);

        // Act
        var scraper = registry.GetBookScraper("test.scraper");

        // Assert
        Assert.IsNotNull(scraper);
        Assert.AreEqual("test.scraper", scraper.FeatureId);
    }

    [TestMethod]
    public void GetBookScraper_NonExistentScraper_ShouldReturnNull()
    {
        // Arrange
        var mockLoader = new Mock<IPluginLoader>();
        mockLoader.Setup(l => l.GetPluginsByCapability(PluginCapability.BookScraper))
            .Returns(new List<LoadedPlugin>());

        var logger = Mock.Of<ILogger<PluginRegistry>>();
        var registry = new PluginRegistry(mockLoader.Object, logger);

        // Act
        var scraper = registry.GetBookScraper("nonexistent");

        // Assert
        Assert.IsNull(scraper);
    }

    [TestMethod]
    public void GetPluginFeatures_NonExistentPlugin_ShouldReturnEmptyList()
    {
        // Arrange
        var mockLoader = new Mock<IPluginLoader>();
        mockLoader.Setup(l => l.GetLoadedPlugin(It.IsAny<string>()))
            .Returns((LoadedPlugin?)null);

        var logger = Mock.Of<ILogger<PluginRegistry>>();
        var registry = new PluginRegistry(mockLoader.Object, logger);

        // Act
        var features = registry.GetPluginFeatures("nonexistent");

        // Assert
        Assert.AreEqual(0, features.Count);
    }

    // Helper to create LoadedPlugin with internal constructor
    private static LoadedPlugin CreateLoadedPlugin(IPlugin plugin)
    {
        // Use reflection to access internal constructor
        var constructor = typeof(LoadedPlugin).GetConstructors(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .First();

        return (LoadedPlugin)constructor.Invoke([plugin, "/test/path.dll", null]);
    }
}
