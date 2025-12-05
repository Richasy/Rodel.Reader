// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelReader.Utilities.FeedParser.Readers;

namespace FeedParser.Test.Readers;

/// <summary>
/// RssFeedReader 单元测试.
/// </summary>
[TestClass]
public sealed class RssFeedReaderTests
{
    #region 基本功能测试

    [TestMethod]
    public async Task ReadChannelAsync_MinimalRss_ShouldReturnChannel()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalRss();
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new RssFeedReader(stream);

        // Act
        var channel = await reader.ReadChannelAsync();

        // Assert
        Assert.IsNotNull(channel);
        Assert.AreEqual("测试频道", channel.Title);
        Assert.AreEqual(FeedType.Rss, reader.FeedType);
    }

    [TestMethod]
    public async Task ReadItemsAsync_RssWithItems_ShouldReturnAllItems()
    {
        // Arrange
        var xml = TestDataFactory.CreateRssWithItems(5);
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new RssFeedReader(stream);

        // Act
        await reader.ReadChannelAsync();
        var items = new List<FeedItem>();
        await foreach (var item in reader.ReadItemsAsync())
        {
            items.Add(item);
        }

        // Assert
        Assert.AreEqual(5, items.Count);
        for (var i = 0; i < 5; i++)
        {
            Assert.AreEqual($"文章 {i + 1}", items[i].Title);
        }
    }

    [TestMethod]
    public async Task ReadAllItemsAsync_RssWithItems_ShouldReturnList()
    {
        // Arrange
        var xml = TestDataFactory.CreateRssWithItems(3);
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new RssFeedReader(stream);

        // Act
        await reader.ReadChannelAsync();
        var items = await reader.ReadAllItemsAsync();

        // Assert
        Assert.AreEqual(3, items.Count);
    }

    #endregion

    #region 播客测试

    [TestMethod]
    public async Task ReadChannelAsync_Podcast_ShouldParseITunesFields()
    {
        // Arrange
        var xml = TestDataFactory.CreatePodcastRss();
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new RssFeedReader(stream);

        // Act
        var channel = await reader.ReadChannelAsync();

        // Assert
        Assert.AreEqual("测试播客", channel.Title);
        AssertExtensions.IsNotEmpty(channel.Contributors);
        AssertExtensions.IsNotEmpty(channel.Images);
        AssertExtensions.IsNotEmpty(channel.Categories);
    }

    [TestMethod]
    public async Task ReadItemsAsync_Podcast_ShouldParseEnclosure()
    {
        // Arrange
        var xml = TestDataFactory.CreatePodcastRss();
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new RssFeedReader(stream);

        // Act
        await reader.ReadChannelAsync();
        var items = await reader.ReadAllItemsAsync();

        // Assert
        Assert.AreEqual(1, items.Count);
        var episode = items[0];
        Assert.IsTrue(episode.Links.Any(l => l.LinkType == FeedLinkType.Enclosure));
    }

    #endregion

    #region 边界情况测试

    [TestMethod]
    public async Task ReadChannelAsync_EmptyChannel_ShouldNotThrow()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title></title>
                <link></link>
                <description></description>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new RssFeedReader(stream);

        // Act
        var channel = await reader.ReadChannelAsync();

        // Assert
        Assert.IsNotNull(channel);
    }

    [TestMethod]
    public async Task ReadItemsAsync_NoItems_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalRss();
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new RssFeedReader(stream);

        // Act
        await reader.ReadChannelAsync();
        var items = await reader.ReadAllItemsAsync();

        // Assert
        Assert.AreEqual(0, items.Count);
    }

    #endregion

    #region Dispose 测试

    [TestMethod]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalRss();
        var stream = TestDataFactory.ToStream(xml);
        var reader = new RssFeedReader(stream);

        // Act & Assert
        reader.Dispose();
        // 再次调用应该也不会抛出异常
        reader.Dispose();
    }

    #endregion
}
