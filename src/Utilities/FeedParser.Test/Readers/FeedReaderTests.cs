// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelPlayer.Utilities.FeedParser.Readers;

namespace FeedParser.Test.Readers;

/// <summary>
/// FeedReader 静态门面测试.
/// </summary>
[TestClass]
public sealed class FeedReaderTests
{
    #region CreateAsync 测试

    [TestMethod]
    public async Task CreateAsync_RssFeed_ShouldReturnRssFeedReader()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalRss();
        using var stream = TestDataFactory.ToStream(xml);

        // Act
        using var reader = await FeedReader.CreateAsync(stream);

        // Assert
        Assert.AreEqual(FeedType.Rss, reader.FeedType);
    }

    [TestMethod]
    public async Task CreateAsync_AtomFeed_ShouldReturnAtomFeedReader()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalAtom();
        using var stream = TestDataFactory.ToStream(xml);

        // Act
        using var reader = await FeedReader.CreateAsync(stream);

        // Assert
        Assert.AreEqual(FeedType.Atom, reader.FeedType);
    }

    [TestMethod]
    public async Task CreateAsync_NonFeedXml_ShouldThrowUnsupportedFeedFormatException()
    {
        // Arrange
        var xml = TestDataFactory.CreateNonFeedXml();
        using var stream = TestDataFactory.ToStream(xml);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<UnsupportedFeedFormatException>(
            async () => await FeedReader.CreateAsync(stream));
    }

    #endregion

    #region ReadAsync 测试

    [TestMethod]
    public async Task ReadAsync_RssFeed_ShouldReturnChannelAndItems()
    {
        // Arrange
        var xml = TestDataFactory.CreateRssWithItems(3);
        using var stream = TestDataFactory.ToStream(xml);

        // Act
        var (channel, items) = await FeedReader.ReadAsync(stream);

        // Assert
        Assert.IsNotNull(channel);
        Assert.AreEqual("测试频道", channel.Title);
        Assert.AreEqual(3, items.Count);
    }

    [TestMethod]
    public async Task ReadAsync_AtomFeed_ShouldReturnChannelAndItems()
    {
        // Arrange
        var xml = TestDataFactory.CreateAtomWithEntries(2);
        using var stream = TestDataFactory.ToStream(xml);

        // Act
        var (channel, items) = await FeedReader.ReadAsync(stream);

        // Assert
        Assert.IsNotNull(channel);
        Assert.AreEqual("测试 Feed", channel.Title);
        Assert.AreEqual(2, items.Count);
    }

    [TestMethod]
    public async Task ReadAsync_EmptyFeed_ShouldReturnEmptyItems()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalRss();
        using var stream = TestDataFactory.ToStream(xml);

        // Act
        var (channel, items) = await FeedReader.ReadAsync(stream);

        // Assert
        Assert.IsNotNull(channel);
        Assert.AreEqual(0, items.Count);
    }

    #endregion

    #region DetectFeedType 测试

    [TestMethod]
    public async Task DetectFeedType_RssFeed_ShouldReturnRss()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalRss();
        using var stream = TestDataFactory.ToStream(xml);

        // Act
        var feedType = await FeedReader.DetectFeedTypeAsync(stream);

        // Assert
        Assert.AreEqual(FeedType.Rss, feedType);
    }

    [TestMethod]
    public async Task DetectFeedType_AtomFeed_ShouldReturnAtom()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalAtom();
        using var stream = TestDataFactory.ToStream(xml);

        // Act
        var feedType = await FeedReader.DetectFeedTypeAsync(stream);

        // Assert
        Assert.AreEqual(FeedType.Atom, feedType);
    }

    [TestMethod]
    public async Task DetectFeedType_NonFeedXml_ShouldReturnUnknown()
    {
        // Arrange
        var xml = TestDataFactory.CreateNonFeedXml();
        using var stream = TestDataFactory.ToStream(xml);

        // Act
        var feedType = await FeedReader.DetectFeedTypeAsync(stream);

        // Assert
        Assert.AreEqual(FeedType.Unknown, feedType);
    }

    #endregion
}
