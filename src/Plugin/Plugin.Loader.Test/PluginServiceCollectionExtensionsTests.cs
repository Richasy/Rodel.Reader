// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Loader.Test;

[TestClass]
public class PluginServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddPluginSystem_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddPluginSystem();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(provider.GetService<PluginLoaderOptions>());
        Assert.IsNotNull(provider.GetService<IPluginLoader>());
        Assert.IsNotNull(provider.GetService<PluginRegistry>());
    }

    [TestMethod]
    public void AddPluginSystem_WithConfiguration_ShouldApplyOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddPluginSystem(options =>
        {
            options.PluginDirectory = "/custom/plugins";
            options.HostVersion = new Version(2, 0, 0);
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<PluginLoaderOptions>();

        // Assert
        Assert.AreEqual("/custom/plugins", options.PluginDirectory);
        Assert.AreEqual(new Version(2, 0, 0), options.HostVersion);
    }

    [TestMethod]
    public void AddPluginSystem_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddPluginSystem();

        // Assert
        Assert.AreSame(services, result);
    }
}
