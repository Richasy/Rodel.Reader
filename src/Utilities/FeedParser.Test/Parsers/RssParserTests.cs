// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;
using Richasy.RodelPlayer.Utilities.FeedParser.Parsers;

namespace FeedParser.Test.Parsers;

/// <summary>
/// RssParser 单元测试.
/// </summary>
[TestClass]
public sealed class RssParserTests
{
    private RssParser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new RssParser();
    }

    #region ParseChannel 测试

    [TestMethod]
    public void ParseChannel_MinimalRss_ShouldParseBasicFields()
    {
        // Arrange
        var xml = TestDataFactory.CreateMinimalRss("我的频道", "https://myblog.com");
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.IsNotNull(channel);
        Assert.AreEqual("我的频道", channel.Title);
        Assert.AreEqual("测试描述", channel.Description);
        AssertExtensions.IsNotEmpty(channel.Links);
        Assert.AreEqual("https://myblog.com/", channel.Links[0].Uri.ToString());
    }

    [TestMethod]
    public void ParseChannel_RssWithLanguage_ShouldParseLanguage()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>中文频道</title>
                <link>https://example.com</link>
                <description>描述</description>
                <language>zh-cn</language>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.AreEqual("zh-cn", channel.Language);
    }

    [TestMethod]
    public void ParseChannel_RssWithCopyright_ShouldParseCopyright()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>频道</title>
                <link>https://example.com</link>
                <description>描述</description>
                <copyright>© 2024 Example Inc.</copyright>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.AreEqual("© 2024 Example Inc.", channel.Copyright);
    }

    [TestMethod]
    public void ParseChannel_RssWithImage_ShouldParseImage()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>频道</title>
                <link>https://example.com</link>
                <description>描述</description>
                <image>
                  <url>https://example.com/logo.png</url>
                  <title>Logo</title>
                  <link>https://example.com</link>
                </image>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        AssertExtensions.IsNotEmpty(channel.Images);
        Assert.AreEqual("https://example.com/logo.png", channel.Images[0].Url.ToString());
    }

    [TestMethod]
    public void ParseChannel_RssWithITunesImage_ShouldParseITunesImage()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd">
              <channel>
                <title>播客</title>
                <link>https://example.com</link>
                <description>描述</description>
                <itunes:image href="https://example.com/podcast-cover.jpg"/>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        AssertExtensions.IsNotEmpty(channel.Images);
        Assert.AreEqual("https://example.com/podcast-cover.jpg", channel.Images[0].Url.ToString());
    }

    [TestMethod]
    public void ParseChannel_RssWithCategories_ShouldParseCategories()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>频道</title>
                <link>https://example.com</link>
                <description>描述</description>
                <category>技术</category>
                <category>编程</category>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.AreEqual(2, channel.Categories.Count);
        Assert.IsTrue(channel.Categories.Any(c => c.Name == "技术"));
        Assert.IsTrue(channel.Categories.Any(c => c.Name == "编程"));
    }

    #endregion

    #region ParseItem 测试

    [TestMethod]
    public void ParseItem_BasicItem_ShouldParseAllFields()
    {
        // Arrange
        var content = new FeedContent(
            "item",
            null,
            null,
            null,
            [
                new FeedContent("title", null, "文章标题", null, null),
                new FeedContent("link", null, "https://example.com/article", null, null),
                new FeedContent("description", null, "文章描述", null, null),
                new FeedContent("pubDate", null, "Mon, 15 Jan 2024 10:00:00 GMT", null, null),
                new FeedContent("guid", null, "https://example.com/article", null, null),
            ]);

        // Act
        var item = _parser.ParseItem(content);

        // Assert
        Assert.AreEqual("文章标题", item.Title);
        Assert.AreEqual("文章描述", item.Description);
        AssertExtensions.IsNotEmpty(item.Links);
        AssertExtensions.IsReasonableDateTime(item.PublishedAt);
    }

    [TestMethod]
    public void ParseItem_ItemWithAuthor_ShouldParseAuthor()
    {
        // Arrange
        var content = new FeedContent(
            "item",
            null,
            null,
            null,
            [
                new FeedContent("title", null, "文章", null, null),
                new FeedContent("author", null, "author@example.com (张三)", null, null),
            ]);

        // Act
        var item = _parser.ParseItem(content);

        // Assert
        AssertExtensions.IsNotEmpty(item.Contributors);
    }

    [TestMethod]
    public void ParseItem_ItemWithEnclosure_ShouldParseEnclosure()
    {
        // Arrange
        var content = new FeedContent(
            "item",
            null,
            null,
            null,
            [
                new FeedContent("title", null, "播客节目", null, null),
                new FeedContent(
                    "enclosure",
                    null,
                    null,
                    [
                        new FeedAttribute("url", "https://example.com/episode.mp3"),
                        new FeedAttribute("type", "audio/mpeg"),
                        new FeedAttribute("length", "12345678"),
                    ],
                    null),
            ]);

        // Act
        var item = _parser.ParseItem(content);

        // Assert
        Assert.IsTrue(item.Links.Any(l => l.LinkType == FeedLinkType.Enclosure));
        var enclosure = item.Links.First(l => l.LinkType == FeedLinkType.Enclosure);
        Assert.AreEqual("audio/mpeg", enclosure.MediaType);
    }

    #endregion

    #region ParseLink 测试

    [TestMethod]
    public void ParseLink_SimpleLink_ShouldParse()
    {
        // Arrange
        var content = new FeedContent("link", null, "https://example.com", null, null);

        // Act
        var link = _parser.ParseLink(content);

        // Assert
        Assert.AreEqual("https://example.com/", link.Uri.ToString());
        Assert.AreEqual(FeedLinkType.Alternate, link.LinkType);
    }

    [TestMethod]
    public void ParseLink_EnclosureLink_ShouldParseWithAttributes()
    {
        // Arrange
        var content = new FeedContent(
            "enclosure",
            null,
            null,
            [
                new FeedAttribute("url", "https://example.com/file.mp3"),
                new FeedAttribute("type", "audio/mpeg"),
                new FeedAttribute("length", "12345678"),
            ],
            null);

        // Act
        var link = _parser.ParseLink(content);

        // Assert
        Assert.AreEqual(FeedLinkType.Enclosure, link.LinkType);
        Assert.AreEqual("audio/mpeg", link.MediaType);
        Assert.AreEqual(12345678L, link.Length);
    }

    #endregion

    #region ParseCategory 测试

    [TestMethod]
    public void ParseCategory_SimpleCategory_ShouldParse()
    {
        // Arrange
        var content = new FeedContent("category", null, "技术", null, null);

        // Act
        var category = _parser.ParseCategory(content);

        // Assert - 简单分类的文本内容存储在 Name 中
        Assert.AreEqual("技术", category.Name);
    }

    [TestMethod]
    public void ParseCategory_ITunesCategory_ShouldParseFromAttribute()
    {
        // Arrange
        var content = new FeedContent(
            "category",
            "http://www.itunes.com/dtds/podcast-1.0.dtd",
            null,
            [new FeedAttribute("text", "Technology")],
            null);

        // Act
        var category = _parser.ParseCategory(content);

        // Assert - iTunes 分类的 text 属性存储在 Name 中
        Assert.AreEqual("Technology", category.Name);
    }

    #endregion

    #region ParsePerson 测试

    [TestMethod]
    public void ParsePerson_EmailFormat_ShouldParseNameAndEmail()
    {
        // Arrange
        var content = new FeedContent(
            "author",
            null,
            "john@example.com (John Doe)",
            null,
            null);

        // Act
        var person = _parser.ParsePerson(content);

        // Assert
        Assert.AreEqual("John Doe", person.Name);
        Assert.AreEqual("john@example.com", person.Email);
    }

    [TestMethod]
    public void ParsePerson_NameOnly_ShouldParseName()
    {
        // Arrange
        var content = new FeedContent("author", null, "张三", null, null);

        // Act
        var person = _parser.ParsePerson(content);

        // Assert
        Assert.AreEqual("张三", person.Name);
        Assert.IsNull(person.Email);
    }

    #endregion

    #region RFC 5005 分页链接测试

    [TestMethod]
    public void ParseChannel_WithAtomPagingLinks_ShouldExtractPagingInfo()
    {
        // Arrange - RSS 使用 atom:link 命名空间支持分页
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
              <channel>
                <title>分页测试</title>
                <link>https://example.com</link>
                <description>RSS 分页测试</description>
                <atom:link href="https://example.com/rss" rel="self" type="application/rss+xml"/>
                <atom:link href="https://example.com/rss?page=1" rel="first"/>
                <atom:link href="https://example.com/rss?page=2" rel="previous"/>
                <atom:link href="https://example.com/rss?page=4" rel="next"/>
                <atom:link href="https://example.com/rss?page=10" rel="last"/>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.IsNotNull(channel.PagingLinks);
        Assert.IsTrue(channel.PagingLinks.HasPaging);
        Assert.IsTrue(channel.PagingLinks.HasNext);
        Assert.IsTrue(channel.PagingLinks.HasPrevious);
        Assert.AreEqual("https://example.com/rss?page=1", channel.PagingLinks.First?.ToString());
        Assert.AreEqual("https://example.com/rss?page=2", channel.PagingLinks.Previous?.ToString());
        Assert.AreEqual("https://example.com/rss?page=4", channel.PagingLinks.Next?.ToString());
        Assert.AreEqual("https://example.com/rss?page=10", channel.PagingLinks.Last?.ToString());
        Assert.AreEqual("https://example.com/rss", channel.PagingLinks.Current?.ToString());
    }

    [TestMethod]
    public void ParseChannel_WithNextLinkOnly_ShouldExtractPagingInfo()
    {
        // Arrange - 只有 next 链接
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
              <channel>
                <title>部分分页</title>
                <link>https://example.com</link>
                <description>测试</description>
                <atom:link href="https://example.com/rss?page=2" rel="next"/>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.IsNotNull(channel.PagingLinks);
        Assert.IsTrue(channel.PagingLinks.HasPaging);
        Assert.IsTrue(channel.PagingLinks.HasNext);
        Assert.IsFalse(channel.PagingLinks.HasPrevious);
    }

    [TestMethod]
    public void ParseChannel_WithoutPagingLinks_ShouldReturnNull()
    {
        // Arrange - 没有分页链接
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>普通Feed</title>
                <link>https://example.com</link>
                <description>测试</description>
              </channel>
            </rss>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.IsNull(channel.PagingLinks);
    }

    #endregion
}

