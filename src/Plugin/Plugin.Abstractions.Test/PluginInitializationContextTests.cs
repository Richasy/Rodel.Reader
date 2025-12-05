// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Abstractions.Test;

[TestClass]
public class PluginInitializationContextTests
{
    [TestMethod]
    public void PluginInitializationContext_Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();
        var logger = new Mock<ILogger>(MockBehavior.Loose).Object;
        var pluginDirectory = "/plugins/test";
        var dataDirectory = "/data/test";
        var hostVersion = new Version(2, 0, 0);

        // Act
        var context = new PluginInitializationContext(
            services,
            logger,
            pluginDirectory,
            dataDirectory,
            hostVersion);

        // Assert
        Assert.AreSame(services, context.Services);
        Assert.AreSame(logger, context.Logger);
        Assert.AreEqual(pluginDirectory, context.PluginDirectory);
        Assert.AreEqual(dataDirectory, context.DataDirectory);
        Assert.AreEqual(hostVersion, context.HostVersion);
    }

    [TestMethod]
    public void PluginInitializationContext_NullServices_ShouldThrow()
    {
        // Arrange & Act & Assert
        try
        {
            _ = new PluginInitializationContext(
                null!,
                new Mock<ILogger>(MockBehavior.Loose).Object,
                "/plugins",
                "/data",
                new Version(1, 0, 0));
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void PluginInitializationContext_NullLogger_ShouldThrow()
    {
        // Arrange & Act & Assert
        try
        {
            _ = new PluginInitializationContext(
                new ServiceCollection().BuildServiceProvider(),
                null!,
                "/plugins",
                "/data",
                new Version(1, 0, 0));
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void PluginInitializationContext_NullPluginDirectory_ShouldThrow()
    {
        // Arrange & Act & Assert
        try
        {
            _ = new PluginInitializationContext(
                new ServiceCollection().BuildServiceProvider(),
                new Mock<ILogger>(MockBehavior.Loose).Object,
                null!,
                "/data",
                new Version(1, 0, 0));
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void PluginInitializationContext_NullDataDirectory_ShouldThrow()
    {
        // Arrange & Act & Assert
        try
        {
            _ = new PluginInitializationContext(
                new ServiceCollection().BuildServiceProvider(),
                new Mock<ILogger>(MockBehavior.Loose).Object,
                "/plugins",
                null!,
                new Version(1, 0, 0));
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void PluginInitializationContext_NullHostVersion_ShouldThrow()
    {
        // Arrange & Act & Assert
        try
        {
            _ = new PluginInitializationContext(
                new ServiceCollection().BuildServiceProvider(),
                new Mock<ILogger>(MockBehavior.Loose).Object,
                "/plugins",
                "/data",
                null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }
}
