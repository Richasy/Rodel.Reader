// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.UnitTests;

/// <summary>
/// LocalRssClient 文章标记单元测试.
/// </summary>
[TestClass]
public sealed class LocalRssClientMarkTests
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
    public async Task MarkArticlesAsReadAsync_ShouldCallStorageMarkAsRead()
    {
        // Arrange
        var articleIds = new List<string> { "article1", "article2", "article3" };

        _mockStorage.Setup(s => s.MarkAsReadAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsTrue(result);
        _mockStorage.Verify(s => s.MarkAsReadAsync(
            It.Is<IEnumerable<string>>(ids => ids.Count() == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_ShouldCallStorageMarkFeedAsRead()
    {
        // Arrange
        var feed = new RssFeed { Id = "feed1", Name = "测试订阅源" };

        _mockStorage.Setup(s => s.MarkFeedAsReadAsync("feed1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.MarkFeedAsReadAsync(feed);

        // Assert
        Assert.IsTrue(result);
        _mockStorage.Verify(s => s.MarkFeedAsReadAsync("feed1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_ShouldMarkAllFeedsInGroup()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "group1", Name = "测试分组" };

        var feeds = new List<RssFeed>
        {
            new() { Id = "feed1", Name = "订阅源1", GroupIds = "group1" },
            new() { Id = "feed2", Name = "订阅源2", GroupIds = "group1,group2" },
            new() { Id = "feed3", Name = "订阅源3", GroupIds = "group2" }, // 不在 group1 中
        };

        _mockStorage.Setup(s => s.GetAllFeedsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(feeds);
        _mockStorage.Setup(s => s.MarkFeedAsReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.MarkGroupAsReadAsync(group);

        // Assert
        Assert.IsTrue(result);
        _mockStorage.Verify(s => s.MarkFeedAsReadAsync("feed1", It.IsAny<CancellationToken>()), Times.Once);
        _mockStorage.Verify(s => s.MarkFeedAsReadAsync("feed2", It.IsAny<CancellationToken>()), Times.Once);
        _mockStorage.Verify(s => s.MarkFeedAsReadAsync("feed3", It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task MarkFeedAsReadAsync_WithNullFeed_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.MarkFeedAsReadAsync(null!));
    }

    [TestMethod]
    public async Task MarkGroupAsReadAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.MarkGroupAsReadAsync(null!));
    }
}
