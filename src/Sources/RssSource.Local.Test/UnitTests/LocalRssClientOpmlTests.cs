// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.UnitTests;

/// <summary>
/// LocalRssClient OPML 导入导出单元测试.
/// </summary>
[TestClass]
public sealed class LocalRssClientOpmlTests
{
    private Mock<IRssStorage> _mockStorage = null!;
    private LocalRssClient _client = null!;

    private const string ValidOpml = """
        <?xml version="1.0" encoding="UTF-8" ?>
        <opml version="2.0">
            <head>
                <title>Test Subscriptions</title>
            </head>
            <body>
                <outline text="科技" title="科技">
                    <outline text="IT之家" title="IT之家" type="rss" xmlUrl="https://www.ithome.com/rss" htmlUrl="https://www.ithome.com" />
                    <outline text="极客公园" title="极客公园" type="rss" xmlUrl="https://www.geekpark.net/rss" htmlUrl="https://www.geekpark.net" />
                </outline>
                <outline text=".NET Blog" title=".NET Blog" type="rss" xmlUrl="https://devblogs.microsoft.com/dotnet/feed/" htmlUrl="https://devblogs.microsoft.com/dotnet" />
            </body>
        </opml>
        """;

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
    public async Task ImportOpmlAsync_WithValidOpml_ShouldImportGroupsAndFeeds()
    {
        // Arrange
        _mockStorage.Setup(s => s.UpsertGroupsAsync(It.IsAny<IEnumerable<RssFeedGroup>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockStorage.Setup(s => s.UpsertFeedsAsync(It.IsAny<IEnumerable<RssFeed>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _client.ImportOpmlAsync(ValidOpml);

        // Assert
        Assert.IsTrue(result);
        _mockStorage.Verify(s => s.UpsertGroupsAsync(
            It.Is<IEnumerable<RssFeedGroup>>(g => g.Count() == 1),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockStorage.Verify(s => s.UpsertFeedsAsync(
            It.Is<IEnumerable<RssFeed>>(f => f.Count() == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ImportOpmlAsync_WithCompletelyInvalidXml_ShouldReturnFalse()
    {
        // Arrange - this is not valid XML at all
        var invalidOpml = "this is not xml at all <>";

        // Act
        var result = await _client.ImportOpmlAsync(invalidOpml);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ExportOpmlAsync_ShouldGenerateValidOpml()
    {
        // Arrange
        var groups = new List<RssFeedGroup>
        {
            new() { Id = "group1", Name = "科技" },
        };

        var feeds = new List<RssFeed>
        {
            new() { Id = "feed1", Name = "IT之家", Url = "https://www.ithome.com/rss", Website = "https://www.ithome.com", GroupIds = "group1" },
            new() { Id = "feed2", Name = "极客公园", Url = "https://www.geekpark.net/rss", Website = "https://www.geekpark.net", GroupIds = "group1" },
            new() { Id = "feed3", Name = ".NET Blog", Url = "https://devblogs.microsoft.com/dotnet/feed/", Website = "https://devblogs.microsoft.com/dotnet" },
        };

        _mockStorage.Setup(s => s.GetAllGroupsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);
        _mockStorage.Setup(s => s.GetAllFeedsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(feeds);

        // Act
        var opml = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsTrue(!string.IsNullOrEmpty(opml));
        Assert.IsTrue(opml.Contains("<?xml version=\"1.0\""));
        Assert.IsTrue(opml.Contains("<opml"));
        Assert.IsTrue(opml.Contains("IT之家"));
        Assert.IsTrue(opml.Contains("极客公园"));
        Assert.IsTrue(opml.Contains(".NET Blog"));
        Assert.IsTrue(opml.Contains("https://www.ithome.com/rss"));
    }

    [TestMethod]
    public async Task ExportOpmlAsync_WithEmptyData_ShouldReturnValidEmptyOpml()
    {
        // Arrange
        _mockStorage.Setup(s => s.GetAllGroupsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RssFeedGroup>());
        _mockStorage.Setup(s => s.GetAllFeedsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RssFeed>());

        // Act
        var opml = await _client.ExportOpmlAsync();

        // Assert
        Assert.IsTrue(!string.IsNullOrEmpty(opml));
        Assert.IsTrue(opml.Contains("<?xml version=\"1.0\""));
        Assert.IsTrue(opml.Contains("<opml"));
        Assert.IsTrue(opml.Contains("</opml>"));
    }
}
