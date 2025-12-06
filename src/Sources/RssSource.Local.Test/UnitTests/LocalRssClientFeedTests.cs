// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.UnitTests;

/// <summary>
/// LocalRssClient 订阅源管理单元测试.
/// </summary>
[TestClass]
public sealed class LocalRssClientFeedTests
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
    public async Task AddFeedAsync_WithEmptyId_ShouldGenerateId()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "IT之家",
            Url = "https://www.ithome.com/rss",
        };

        _mockStorage.Setup(s => s.UpsertFeedAsync(It.IsAny<RssFeed>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.AddFeedAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(!string.IsNullOrEmpty(result.Id));
        Assert.AreEqual("feed/https://www.ithome.com/rss", result.Id);
        _mockStorage.Verify(s => s.UpsertFeedAsync(It.IsAny<RssFeed>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithExistingId_ShouldKeepId()
    {
        // Arrange
        var feed = new RssFeed
        {
            Id = "custom-feed-id",
            Name = "IT之家",
            Url = "https://www.ithome.com/rss",
        };

        _mockStorage.Setup(s => s.UpsertFeedAsync(It.IsAny<RssFeed>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.AddFeedAsync(feed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("custom-feed-id", result.Id);
    }

    [TestMethod]
    public async Task UpdateFeedAsync_ShouldCallStorageUpsert()
    {
        // Arrange
        var newFeed = new RssFeed
        {
            Id = "feed1",
            Name = "更新后的订阅源",
            Url = "https://example.com/rss",
        };
        var oldFeed = new RssFeed
        {
            Id = "feed1",
            Name = "旧订阅源",
            Url = "https://example.com/rss",
        };

        _mockStorage.Setup(s => s.UpsertFeedAsync(It.IsAny<RssFeed>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.UpdateFeedAsync(newFeed, oldFeed);

        // Assert
        Assert.IsTrue(result);
        _mockStorage.Verify(s => s.UpsertFeedAsync(newFeed, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        var feed = new RssFeed { Id = "feed1", Name = "待删除订阅源" };

        _mockStorage.Setup(s => s.DeleteFeedAsync("feed1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _client.DeleteFeedAsync(feed);

        // Assert
        Assert.IsTrue(result);
        _mockStorage.Verify(s => s.DeleteFeedAsync("feed1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        var feed = new RssFeed { Id = "feed1", Name = "待删除订阅源" };

        _mockStorage.Setup(s => s.DeleteFeedAsync("feed1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _client.DeleteFeedAsync(feed);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AddFeedAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.AddFeedAsync(null!));
    }

    [TestMethod]
    public async Task UpdateFeedAsync_WithNullNewFeed_ShouldThrowArgumentNullException()
    {
        // Arrange
        var oldFeed = new RssFeed { Id = "feed1" };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.UpdateFeedAsync(null!, oldFeed));
    }

    [TestMethod]
    public async Task DeleteFeedAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.DeleteFeedAsync(null!));
    }
}
