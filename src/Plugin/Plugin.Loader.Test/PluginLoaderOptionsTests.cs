// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Loader.Test;

[TestClass]
public class PluginLoaderOptionsTests
{
    [TestMethod]
    public void PluginLoaderOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new PluginLoaderOptions();

        // Assert
        Assert.AreEqual("plugins", options.PluginDirectory);
        Assert.AreEqual("plugin-data", options.PluginDataDirectory);
        Assert.AreEqual(new Version(1, 0, 0), options.HostVersion);
        Assert.IsTrue(options.ContinueOnLoadError);
        Assert.AreEqual("*.dll", options.SearchPattern);
        Assert.IsTrue(options.SearchSubdirectories);
        Assert.AreEqual(30, options.LoadTimeoutSeconds);
    }

    [TestMethod]
    public void PluginLoaderOptions_CustomValues_ShouldBeSet()
    {
        // Arrange & Act
        var options = new PluginLoaderOptions
        {
            PluginDirectory = "/custom/plugins",
            PluginDataDirectory = "/custom/data",
            HostVersion = new Version(2, 0, 0),
            ContinueOnLoadError = false,
            SearchPattern = "*.plugin.dll",
            SearchSubdirectories = false,
            LoadTimeoutSeconds = 60,
        };

        // Assert
        Assert.AreEqual("/custom/plugins", options.PluginDirectory);
        Assert.AreEqual("/custom/data", options.PluginDataDirectory);
        Assert.AreEqual(new Version(2, 0, 0), options.HostVersion);
        Assert.IsFalse(options.ContinueOnLoadError);
        Assert.AreEqual("*.plugin.dll", options.SearchPattern);
        Assert.IsFalse(options.SearchSubdirectories);
        Assert.AreEqual(60, options.LoadTimeoutSeconds);
    }
}
