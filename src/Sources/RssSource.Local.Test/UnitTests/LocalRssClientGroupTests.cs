// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.UnitTests;

/// <summary>
/// LocalRssClient 分组管理单元测试.
/// </summary>
[TestClass]
public sealed class LocalRssClientGroupTests
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
    public async Task GetFeedListAsync_ShouldReturnGroupsAndFeeds()
    {
        // Arrange
        var expectedGroups = new List<RssFeedGroup>
        {
            new() { Id = "group1", Name = "科技" },
            new() { Id = "group2", Name = "新闻" },
        };

        var expectedFeeds = new List<RssFeed>
        {
            new() { Id = "feed1", Name = "IT之家", Url = "https://www.ithome.com/rss", GroupIds = "group1" },
            new() { Id = "feed2", Name = "极客公园", Url = "https://www.geekpark.net/rss", GroupIds = "group1" },
        };

        _mockStorage.Setup(s => s.GetAllGroupsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGroups);
        _mockStorage.Setup(s => s.GetAllFeedsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFeeds);

        // Act
        var (groups, feeds) = await _client.GetFeedListAsync();

        // Assert
        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual(2, feeds.Count);
        Assert.AreEqual("科技", groups[0].Name);
        Assert.AreEqual("IT之家", feeds[0].Name);
    }

    [TestMethod]
    public async Task AddGroupAsync_WithEmptyId_ShouldGenerateId()
    {
        // Arrange
        var group = new RssFeedGroup { Name = "新分组" };

        _mockStorage.Setup(s => s.UpsertGroupAsync(It.IsAny<RssFeedGroup>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.AddGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(!string.IsNullOrEmpty(result.Id));
        Assert.IsTrue(result.Id.StartsWith("folder/", StringComparison.OrdinalIgnoreCase));
        _mockStorage.Verify(s => s.UpsertGroupAsync(It.IsAny<RssFeedGroup>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task AddGroupAsync_WithExistingId_ShouldKeepId()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "custom-id", Name = "自定义分组" };

        _mockStorage.Setup(s => s.UpsertGroupAsync(It.IsAny<RssFeedGroup>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.AddGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("custom-id", result.Id);
    }

    [TestMethod]
    public async Task UpdateGroupAsync_ShouldCallStorageUpsert()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "group1", Name = "更新后的分组" };

        _mockStorage.Setup(s => s.UpsertGroupAsync(It.IsAny<RssFeedGroup>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.UpdateGroupAsync(group);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("更新后的分组", result.Name);
        _mockStorage.Verify(s => s.UpsertGroupAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "group1", Name = "待删除分组" };

        _mockStorage.Setup(s => s.DeleteGroupAsync("group1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _client.DeleteGroupAsync(group);

        // Assert
        Assert.IsTrue(result);
        _mockStorage.Verify(s => s.DeleteGroupAsync("group1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        var group = new RssFeedGroup { Id = "group1", Name = "待删除分组" };

        _mockStorage.Setup(s => s.DeleteGroupAsync("group1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _client.DeleteGroupAsync(group);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AddGroupAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.AddGroupAsync(null!));
    }

    [TestMethod]
    public async Task UpdateGroupAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.UpdateGroupAsync(null!));
    }

    [TestMethod]
    public async Task DeleteGroupAsync_WithNullGroup_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            () => _client.DeleteGroupAsync(null!));
    }
}
