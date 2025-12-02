// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Test.Unit;

/// <summary>
/// ZLibraryClientOptions 单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ZLibraryClientOptionsTests
{
    [TestMethod]
    public void GetEffectiveDomain_NoCustomMirror_ReturnsDefaultDomain()
    {
        // Arrange
        var options = new ZLibraryClientOptions();

        // Act
        var domain = options.GetEffectiveDomain();

        // Assert
        Assert.AreEqual("https://zh.zlib.by", domain);
    }

    [TestMethod]
    public void GetEffectiveDomain_WithCustomMirror_ReturnsCustomMirror()
    {
        // Arrange
        var options = new ZLibraryClientOptions
        {
            CustomMirror = "https://zh.z-lib.fm"
        };

        // Act
        var domain = options.GetEffectiveDomain();

        // Assert
        Assert.AreEqual("https://zh.z-lib.fm", domain);
    }

    [TestMethod]
    public void GetEffectiveDomain_WithCustomMirrorNoProtocol_AddsHttps()
    {
        // Arrange
        var options = new ZLibraryClientOptions
        {
            CustomMirror = "zh.z-lib.fm"
        };

        // Act
        var domain = options.GetEffectiveDomain();

        // Assert
        Assert.AreEqual("https://zh.z-lib.fm", domain);
    }

    [TestMethod]
    public void GetEffectiveDomain_WithTrailingSlash_RemovesSlash()
    {
        // Arrange
        var options = new ZLibraryClientOptions
        {
            CustomMirror = "https://zh.z-lib.fm/"
        };

        // Act
        var domain = options.GetEffectiveDomain();

        // Assert
        Assert.AreEqual("https://zh.z-lib.fm", domain);
    }

    [TestMethod]
    public void GetLoginUrl_NoCustomMirror_ReturnsDefaultLoginUrl()
    {
        // Arrange
        var options = new ZLibraryClientOptions();

        // Act
        var loginUrl = options.GetLoginUrl();

        // Assert
        Assert.AreEqual("https://zh.zlib.by/rpc.php", loginUrl);
    }

    [TestMethod]
    public void GetLoginUrl_WithCustomMirror_ReturnsCustomLoginUrl()
    {
        // Arrange
        var options = new ZLibraryClientOptions
        {
            CustomMirror = "https://zh.z-lib.fm"
        };

        // Act
        var loginUrl = options.GetLoginUrl();

        // Assert
        Assert.AreEqual("https://zh.z-lib.fm/rpc.php", loginUrl);
    }

    [TestMethod]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new ZLibraryClientOptions();

        // Assert
        Assert.AreEqual(TimeSpan.FromSeconds(180), options.Timeout);
        Assert.AreEqual(64, options.MaxConcurrentRequests);
        Assert.IsNotNull(options.UserAgent);
        Assert.IsTrue(options.UserAgent.Contains("Mozilla", StringComparison.Ordinal));
    }
}
