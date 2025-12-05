// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Abstractions.Test;

[TestClass]
public class PluginBaseTests
{
    [TestMethod]
    public async Task PluginBase_InitializeAsync_ShouldSetIsInitialized()
    {
        // Arrange
        var plugin = new TestPlugin();
        var context = CreateTestContext();

        // Act
        await plugin.InitializeAsync(context);

        // Assert
        Assert.IsTrue(plugin.IsInitialized);
    }

    [TestMethod]
    public async Task PluginBase_InitializeAsync_CalledTwice_ShouldOnlyInitializeOnce()
    {
        // Arrange
        var plugin = new TestPlugin();
        var context = CreateTestContext();

        // Act
        await plugin.InitializeAsync(context);
        await plugin.InitializeAsync(context);

        // Assert
        Assert.AreEqual(1, plugin.InitializeCallCount);
    }

    [TestMethod]
    public async Task PluginBase_RegisterFeature_ShouldAddToFeatureList()
    {
        // Arrange
        var plugin = new TestPlugin();
        var context = CreateTestContext();

        // Act
        await plugin.InitializeAsync(context);
        var features = plugin.GetAllFeatures();

        // Assert
        Assert.AreEqual(1, features.Count);
        Assert.IsInstanceOfType<TestScraperFeature>(features[0]);
    }

    [TestMethod]
    public async Task PluginBase_GetFeature_ShouldReturnRegisteredFeature()
    {
        // Arrange
        var plugin = new TestPlugin();
        var context = CreateTestContext();
        await plugin.InitializeAsync(context);

        // Act
        var feature = plugin.GetFeature<TestScraperFeature>();

        // Assert
        Assert.IsNotNull(feature);
        Assert.AreEqual("test.scraper", feature.FeatureId);
    }

    [TestMethod]
    public async Task PluginBase_GetFeature_UnregisteredType_ShouldReturnNull()
    {
        // Arrange
        var plugin = new TestPlugin();
        var context = CreateTestContext();
        await plugin.InitializeAsync(context);

        // Act
        var feature = plugin.GetFeature<IBookScraperFeature>();

        // Assert
        Assert.IsNull(feature);
    }

    [TestMethod]
    public async Task PluginBase_Dispose_ShouldDisposeFeatures()
    {
        // Arrange
        var plugin = new TestPlugin();
        var context = CreateTestContext();
        await plugin.InitializeAsync(context);
        var feature = plugin.GetFeature<TestScraperFeature>();

        // Act
        plugin.Dispose();

        // Assert
        Assert.IsTrue(feature!.IsDisposed);
    }

    [TestMethod]
    public void PluginBase_Metadata_ShouldReturnCorrectValues()
    {
        // Arrange
        var plugin = new TestPlugin();

        // Act & Assert
        Assert.AreEqual("test.plugin", plugin.Metadata.Id);
        Assert.AreEqual("Test Plugin", plugin.Metadata.Name);
        Assert.AreEqual(new Version(1, 0, 0), plugin.Metadata.Version);
    }

    [TestMethod]
    public void PluginBase_Capabilities_ShouldReturnCorrectValue()
    {
        // Arrange
        var plugin = new TestPlugin();

        // Act & Assert
        Assert.IsTrue(plugin.Capabilities.HasFlag(PluginCapability.BookScraper));
    }

    private static PluginInitializationContext CreateTestContext()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Test");

        return new PluginInitializationContext(
            services,
            logger,
            Environment.CurrentDirectory,
            Path.GetTempPath(),
            new Version(1, 0, 0));
    }

    /// <summary>
    /// 测试用插件实现.
    /// </summary>
    private sealed class TestPlugin : PluginBase
    {
        private readonly PluginMetadata _metadata = new()
        {
            Id = "test.plugin",
            Name = "Test Plugin",
            Version = new Version(1, 0, 0),
        };

        public int InitializeCallCount { get; private set; }

        public override PluginMetadata Metadata => _metadata;

        public override PluginCapability Capabilities => PluginCapability.BookScraper;

        protected override Task OnInitializeAsync(PluginInitializationContext context)
        {
            InitializeCallCount++;
            RegisterFeature(new TestScraperFeature());
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 测试用功能特性实现.
    /// </summary>
    private sealed class TestScraperFeature : IPluginFeature, IDisposable
    {
        public string FeatureId => "test.scraper";

        public string FeatureName => "Test Scraper";

        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
