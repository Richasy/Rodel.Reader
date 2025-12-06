// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.UnitTests;

/// <summary>
/// LocalRssClient 基本功能单元测试.
/// </summary>
[TestClass]
public sealed class LocalRssClientBasicTests
{
    private Mock<IRssStorage> _mockStorage = null!;
    private LocalRssClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockStorage = new Mock<IRssStorage>();
        _client = new LocalRssClient(_mockStorage.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
    }

    [TestMethod]
    public void Capabilities_ShouldReturnLocalCapabilities()
    {
        // Act
        var capabilities = _client.Capabilities;

        // Assert
        Assert.IsNotNull(capabilities);
        Assert.AreEqual("local", capabilities.SourceId);
        Assert.AreEqual("本地订阅", capabilities.DisplayName);
        Assert.IsFalse(capabilities.RequiresAuthentication);
        Assert.AreEqual(RssAuthType.None, capabilities.AuthType);
        Assert.IsTrue(capabilities.CanManageFeeds);
        Assert.IsTrue(capabilities.CanManageGroups);
        Assert.IsTrue(capabilities.CanMarkAsRead);
        Assert.IsTrue(capabilities.CanImportOpml);
        Assert.IsTrue(capabilities.CanExportOpml);
    }

    [TestMethod]
    public void IsAuthenticated_ShouldAlwaysReturnTrue()
    {
        // Act & Assert
        Assert.IsTrue(_client.IsAuthenticated);
    }

    [TestMethod]
    public async Task SignInAsync_ShouldAlwaysReturnTrue()
    {
        // Act
        var result = await _client.SignInAsync();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task SignOutAsync_ShouldAlwaysReturnTrue()
    {
        // Act
        var result = await _client.SignOutAsync();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Constructor_WithNullStorage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        _ = Assert.ThrowsExactly<ArgumentNullException>(() => new LocalRssClient(null!));
    }

    [TestMethod]
    public void Constructor_WithOptions_ShouldApplyOptions()
    {
        // Arrange
        var options = new LocalRssClientOptions
        {
            Timeout = TimeSpan.FromSeconds(60),
            MaxConcurrentRequests = 5,
            UserAgent = "TestAgent/1.0",
        };

        // Act
        using var client = new LocalRssClient(_mockStorage.Object, options);

        // Assert
        Assert.IsNotNull(client);
    }
}
