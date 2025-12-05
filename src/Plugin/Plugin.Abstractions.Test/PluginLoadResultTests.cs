// Copyright (c) Richasy. All rights reserved.

namespace Plugin.Abstractions.Test;

[TestClass]
public class PluginLoadResultTests
{
    [TestMethod]
    public void PluginLoadResult_Succeeded_ShouldHaveCorrectProperties()
    {
        // Arrange
        var mockPlugin = new Mock<IPlugin>(MockBehavior.Loose).Object;

        // Act
        var result = PluginLoadResult.Succeeded(mockPlugin);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreSame(mockPlugin, result.Plugin);
        Assert.IsNull(result.ErrorMessage);
        Assert.IsNull(result.Exception);
    }

    [TestMethod]
    public void PluginLoadResult_Failed_ShouldHaveCorrectProperties()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = PluginLoadResult.Failed(errorMessage);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Plugin);
        Assert.AreEqual(errorMessage, result.ErrorMessage);
        Assert.IsNull(result.Exception);
    }

    [TestMethod]
    public void PluginLoadResult_FailedWithException_ShouldHaveCorrectProperties()
    {
        // Arrange
        var errorMessage = "Test error message";
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = PluginLoadResult.Failed(errorMessage, exception);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNull(result.Plugin);
        Assert.AreEqual(errorMessage, result.ErrorMessage);
        Assert.AreSame(exception, result.Exception);
    }
}
