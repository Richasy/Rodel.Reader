// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelReader.Utilities.FeedParser.Readers;

namespace FeedParser.Test.Readers;

/// <summary>
/// AtomFeedReader 单元测试.
/// </summary>
[TestClass]
public sealed class AtomFeedReaderTests
{
    #region 基本功能测试

    [TestMethod]
    public async Task ReadChannelAsync_MinimalAtom_ShouldReturnChannel()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalAtom();
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new AtomFeedReader(stream);

        // Act
        var channel = await reader.ReadChannelAsync();

        // Assert
        Assert.IsNotNull(channel);
        Assert.AreEqual("测试 Feed", channel.Title);
        Assert.AreEqual(FeedType.Atom, reader.FeedType);
    }

    [TestMethod]
    public async Task ReadItemsAsync_AtomWithEntries_ShouldReturnAllEntries()
    {
        // Arrange
        var xml = TestDataFactory.CreateAtomWithEntries(4);
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new AtomFeedReader(stream);

        // Act
        await reader.ReadChannelAsync();
        var items = new List<FeedItem>();
        await foreach (var item in reader.ReadItemsAsync())
        {
            items.Add(item);
        }

        // Assert
        Assert.AreEqual(4, items.Count);
        for (var i = 0; i < 4; i++)
        {
            Assert.AreEqual($"条目 {i + 1}", items[i].Title);
        }
    }

    [TestMethod]
    public async Task ReadAllItemsAsync_AtomWithEntries_ShouldReturnList()
    {
        // Arrange
        var xml = TestDataFactory.CreateAtomWithEntries(3);
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new AtomFeedReader(stream);

        // Act
        await reader.ReadChannelAsync();
        var items = await reader.ReadAllItemsAsync();

        // Assert
        Assert.AreEqual(3, items.Count);
    }

    #endregion

    #region 完整 Feed 测试

    [TestMethod]
    public async Task ReadChannelAsync_FullAtom_ShouldParseAllFields()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>完整的 Atom Feed</title>
              <id>urn:uuid:full-feed</id>
              <updated>2024-01-15T12:00:00Z</updated>
              <subtitle>这是副标题</subtitle>
              <link href="https://example.com" rel="alternate"/>
              <link href="https://example.com/feed.xml" rel="self"/>
              <author>
                <name>主作者</name>
                <email>author@example.com</email>
              </author>
              <contributor>
                <name>贡献者</name>
              </contributor>
              <category term="blog" label="博客"/>
              <logo>https://example.com/logo.png</logo>
              <icon>https://example.com/icon.ico</icon>
              <rights>© 2024 Example</rights>
              <generator uri="https://generator.com" version="1.0">Generator Name</generator>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new AtomFeedReader(stream);

        // Act
        var channel = await reader.ReadChannelAsync();

        // Assert
        Assert.AreEqual("完整的 Atom Feed", channel.Title);
        Assert.AreEqual("这是副标题", channel.Description);
        Assert.AreEqual("© 2024 Example", channel.Copyright);
        Assert.AreEqual(2, channel.Links.Count);
        Assert.AreEqual(2, channel.Contributors.Count);
        Assert.AreEqual(1, channel.Categories.Count);
        Assert.AreEqual(2, channel.Images.Count);
    }

    #endregion

    #region 边界情况测试

    [TestMethod]
    public async Task ReadChannelAsync_EmptyFeed_ShouldNotThrow()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title></title>
              <id>urn:uuid:empty</id>
              <updated>2024-01-01T00:00:00Z</updated>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new AtomFeedReader(stream);

        // Act
        var channel = await reader.ReadChannelAsync();

        // Assert
        Assert.IsNotNull(channel);
    }

    [TestMethod]
    public async Task ReadItemsAsync_NoEntries_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalAtom();
        using var stream = TestDataFactory.ToStream(xml);
        using var reader = new AtomFeedReader(stream);

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
        var xml = TestDataFactory.CreateMinimalAtom();
        var stream = TestDataFactory.ToStream(xml);
        var reader = new AtomFeedReader(stream);

        // Act & Assert
        reader.Dispose();
        // 再次调用应该也不会抛出异常
        reader.Dispose();
    }

    #endregion
}
