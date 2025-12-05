// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Loader.Test;

[TestClass]
public class PluginLoaderTests
{
    private string _testPluginDir = null!;
    private string _testDataDir = null!;
    private IServiceProvider _serviceProvider = null!;
    private ILoggerFactory _loggerFactory = null!;

    [TestInitialize]
    public void Setup()
    {
        _testPluginDir = Path.Combine(Path.GetTempPath(), "PluginLoaderTests", Guid.NewGuid().ToString());
        _testDataDir = Path.Combine(Path.GetTempPath(), "PluginLoaderTestsData", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testPluginDir);
        Directory.CreateDirectory(_testDataDir);

        var services = new ServiceCollection()
            .AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace))
            .BuildServiceProvider();

        _serviceProvider = services;
        _loggerFactory = services.GetRequiredService<ILoggerFactory>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testPluginDir))
        {
            try
            {
                Directory.Delete(_testPluginDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        if (Directory.Exists(_testDataDir))
        {
            try
            {
                Directory.Delete(_testDataDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [TestMethod]
    public async Task LoadPluginAsync_NonExistentFile_ShouldReturnFailed()
    {
        // Arrange
        var options = CreateOptions();
        var loader = new PluginLoader(_serviceProvider, options, _loggerFactory);
        var nonExistentPath = Path.Combine(_testPluginDir, "nonexistent.dll");

        // Act
        var result = await loader.LoadPluginAsync(nonExistentPath);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.IsTrue(result.ErrorMessage.Contains("不存在"));
    }

    [TestMethod]
    public async Task LoadPluginsFromDirectoryAsync_EmptyDirectory_ShouldReturnEmptyList()
    {
        // Arrange
        var options = CreateOptions();
        var loader = new PluginLoader(_serviceProvider, options, _loggerFactory);

        // Act
        var results = await loader.LoadPluginsFromDirectoryAsync(_testPluginDir);

        // Assert
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public async Task LoadPluginsFromDirectoryAsync_NonExistentDirectory_ShouldReturnEmptyList()
    {
        // Arrange
        var options = CreateOptions();
        var loader = new PluginLoader(_serviceProvider, options, _loggerFactory);
        var nonExistentDir = Path.Combine(_testPluginDir, "nonexistent");

        // Act
        var results = await loader.LoadPluginsFromDirectoryAsync(nonExistentDir);

        // Assert
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void GetAllLoadedPlugins_NoPluginsLoaded_ShouldReturnEmptyList()
    {
        // Arrange
        var options = CreateOptions();
        var loader = new PluginLoader(_serviceProvider, options, _loggerFactory);

        // Act
        var plugins = loader.GetAllLoadedPlugins();

        // Assert
        Assert.AreEqual(0, plugins.Count);
    }

    [TestMethod]
    public void GetLoadedPlugin_NonExistentPlugin_ShouldReturnNull()
    {
        // Arrange
        var options = CreateOptions();
        var loader = new PluginLoader(_serviceProvider, options, _loggerFactory);

        // Act
        var plugin = loader.GetLoadedPlugin("nonexistent.plugin");

        // Assert
        Assert.IsNull(plugin);
    }

    [TestMethod]
    public void UnloadPlugin_NonExistentPlugin_ShouldReturnFalse()
    {
        // Arrange
        var options = CreateOptions();
        var loader = new PluginLoader(_serviceProvider, options, _loggerFactory);

        // Act
        var result = loader.UnloadPlugin("nonexistent.plugin");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetPluginsByCapability_NoPlugins_ShouldReturnEmptyList()
    {
        // Arrange
        var options = CreateOptions();
        var loader = new PluginLoader(_serviceProvider, options, _loggerFactory);

        // Act
        var plugins = loader.GetPluginsByCapability(PluginCapability.BookScraper);

        // Assert
        Assert.AreEqual(0, plugins.Count);
    }

    [TestMethod]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var options = CreateOptions();
        var loader = new PluginLoader(_serviceProvider, options, _loggerFactory);

        // Act & Assert - should not throw
        loader.Dispose();
        loader.Dispose(); // Double dispose should be safe
    }

    private PluginLoaderOptions CreateOptions()
    {
        return new PluginLoaderOptions
        {
            PluginDirectory = _testPluginDir,
            PluginDataDirectory = _testDataDir,
            HostVersion = new Version(1, 0, 0),
        };
    }
}
