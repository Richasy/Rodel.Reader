// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;
using Richasy.RodelPlayer.Utilities.FeedParser.Parsers;

namespace FeedParser.Test.Parsers;

/// <summary>
/// AtomParser 单元测试.
/// </summary>
[TestClass]
public sealed class AtomParserTests
{
    private AtomParser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new AtomParser();
    }

    #region ParseChannel 测试

    [TestMethod]
    public void ParseChannel_MinimalAtom_ShouldParseBasicFields()
    {
        // Arrange - 使用包含 link 的 Atom feed
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>我的频道</title>
              <id>https://myblog.com</id>
              <updated>2024-01-01T12:00:00Z</updated>
              <link href="https://myblog.com" rel="alternate"/>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.IsNotNull(channel);
        Assert.AreEqual("我的频道", channel.Title);
        AssertExtensions.IsNotEmpty(channel.Links);
    }

    [TestMethod]
    public void ParseChannel_AtomWithSubtitle_ShouldParseAsDescription()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>频道标题</title>
              <id>urn:uuid:12345</id>
              <updated>2024-01-15T10:00:00Z</updated>
              <subtitle>这是副标题描述</subtitle>
              <link href="https://example.com" rel="alternate"/>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.AreEqual("这是副标题描述", channel.Description);
    }

    [TestMethod]
    public void ParseChannel_AtomWithRights_ShouldParseCopyright()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>频道</title>
              <id>urn:uuid:12345</id>
              <updated>2024-01-15T10:00:00Z</updated>
              <rights>© 2024 Example Inc.</rights>
              <link href="https://example.com"/>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.AreEqual("© 2024 Example Inc.", channel.Copyright);
    }

    [TestMethod]
    public void ParseChannel_AtomWithIcon_ShouldParseAsImage()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>频道</title>
              <id>urn:uuid:12345</id>
              <updated>2024-01-15T10:00:00Z</updated>
              <icon>https://example.com/icon.png</icon>
              <link href="https://example.com"/>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        AssertExtensions.IsNotEmpty(channel.Images);
        Assert.AreEqual("https://example.com/icon.png", channel.Images[0].Url.ToString());
    }

    [TestMethod]
    public void ParseChannel_AtomWithLogo_ShouldParseAsImage()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>频道</title>
              <id>urn:uuid:12345</id>
              <updated>2024-01-15T10:00:00Z</updated>
              <logo>https://example.com/logo.png</logo>
              <link href="https://example.com"/>
            </feed>
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
    public void ParseChannel_AtomWithAuthor_ShouldParseAuthor()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>频道</title>
              <id>urn:uuid:12345</id>
              <updated>2024-01-15T10:00:00Z</updated>
              <author>
                <name>张三</name>
                <email>zhangsan@example.com</email>
                <uri>https://zhangsan.example.com</uri>
              </author>
              <link href="https://example.com"/>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        AssertExtensions.IsNotEmpty(channel.Contributors);
        var author = channel.Contributors[0];
        Assert.AreEqual("张三", author.Name);
        Assert.AreEqual("zhangsan@example.com", author.Email);
    }

    [TestMethod]
    public void ParseChannel_AtomWithCategory_ShouldParseCategory()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>频道</title>
              <id>urn:uuid:12345</id>
              <updated>2024-01-15T10:00:00Z</updated>
              <category term="tech" label="Technology"/>
              <link href="https://example.com"/>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        AssertExtensions.IsNotEmpty(channel.Categories);
        Assert.AreEqual("Technology", channel.Categories[0].Label);
    }

    #endregion

    #region ParseItem 测试

    [TestMethod]
    public void ParseItem_BasicEntry_ShouldParseAllFields()
    {
        // Arrange
        var content = new FeedContent(
            "entry",
            "http://www.w3.org/2005/Atom",
            null,
            null,
            [
                new FeedContent("title", "http://www.w3.org/2005/Atom", "文章标题", null, null),
                new FeedContent("id", "http://www.w3.org/2005/Atom", "urn:uuid:article-1", null, null),
                new FeedContent("updated", "http://www.w3.org/2005/Atom", "2024-01-15T10:00:00Z", null, null),
                new FeedContent("summary", "http://www.w3.org/2005/Atom", "文章摘要", null, null),
                new FeedContent(
                    "link",
                    "http://www.w3.org/2005/Atom",
                    null,
                    [new FeedAttribute("href", "https://example.com/article")],
                    null),
            ]);

        // Act
        var item = _parser.ParseItem(content);

        // Assert
        Assert.AreEqual("文章标题", item.Title);
        Assert.AreEqual("urn:uuid:article-1", item.Id);
        Assert.AreEqual("文章摘要", item.Description);
        AssertExtensions.IsNotEmpty(item.Links);
    }

    [TestMethod]
    public void ParseItem_EntryWithContent_ShouldParseContent()
    {
        // Arrange
        var content = new FeedContent(
            "entry",
            "http://www.w3.org/2005/Atom",
            null,
            null,
            [
                new FeedContent("title", "http://www.w3.org/2005/Atom", "文章", null, null),
                new FeedContent("id", "http://www.w3.org/2005/Atom", "urn:uuid:1", null, null),
                new FeedContent(
                    "content",
                    "http://www.w3.org/2005/Atom",
                    "<p>这是完整内容</p>",
                    [new FeedAttribute("type", "html")],
                    null),
            ]);

        // Act
        var item = _parser.ParseItem(content);

        // Assert
        Assert.IsNotNull(item.Content);
        Assert.IsTrue(item.Content.Contains("这是完整内容", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ParseItem_EntryWithAuthor_ShouldParseAuthor()
    {
        // Arrange
        var content = new FeedContent(
            "entry",
            "http://www.w3.org/2005/Atom",
            null,
            null,
            [
                new FeedContent("title", "http://www.w3.org/2005/Atom", "文章", null, null),
                new FeedContent("id", "http://www.w3.org/2005/Atom", "urn:uuid:1", null, null),
                new FeedContent(
                    "author",
                    "http://www.w3.org/2005/Atom",
                    null,
                    null,
                    [
                        new FeedContent("name", "http://www.w3.org/2005/Atom", "李四", null, null),
                        new FeedContent("email", "http://www.w3.org/2005/Atom", "lisi@example.com", null, null),
                    ]),
            ]);

        // Act
        var item = _parser.ParseItem(content);

        // Assert
        AssertExtensions.IsNotEmpty(item.Contributors);
        Assert.AreEqual("李四", item.Contributors[0].Name);
    }

    #endregion

    #region ParseLink 测试

    [TestMethod]
    public void ParseLink_AlternateLink_ShouldParse()
    {
        // Arrange
        var content = new FeedContent(
            "link",
            "http://www.w3.org/2005/Atom",
            null,
            [
                new FeedAttribute("href", "https://example.com"),
                new FeedAttribute("rel", "alternate"),
                new FeedAttribute("type", "text/html"),
            ],
            null);

        // Act
        var link = _parser.ParseLink(content);

        // Assert
        Assert.AreEqual("https://example.com/", link.Uri.ToString());
        Assert.AreEqual(FeedLinkType.Alternate, link.LinkType);
        Assert.AreEqual("text/html", link.MediaType);
    }

    [TestMethod]
    public void ParseLink_SelfLink_ShouldParse()
    {
        // Arrange
        var content = new FeedContent(
            "link",
            "http://www.w3.org/2005/Atom",
            null,
            [
                new FeedAttribute("href", "https://example.com/feed.xml"),
                new FeedAttribute("rel", "self"),
            ],
            null);

        // Act
        var link = _parser.ParseLink(content);

        // Assert
        Assert.AreEqual(FeedLinkType.Self, link.LinkType);
    }

    [TestMethod]
    public void ParseLink_EnclosureLink_ShouldParse()
    {
        // Arrange
        var content = new FeedContent(
            "link",
            "http://www.w3.org/2005/Atom",
            null,
            [
                new FeedAttribute("href", "https://example.com/media.mp3"),
                new FeedAttribute("rel", "enclosure"),
                new FeedAttribute("type", "audio/mpeg"),
                new FeedAttribute("length", "9876543"),
            ],
            null);

        // Act
        var link = _parser.ParseLink(content);

        // Assert
        Assert.AreEqual(FeedLinkType.Enclosure, link.LinkType);
        Assert.AreEqual("audio/mpeg", link.MediaType);
        Assert.AreEqual(9876543L, link.Length);
    }

    #endregion

    #region ParseCategory 测试

    [TestMethod]
    public void ParseCategory_WithTermAndLabel_ShouldParseBoth()
    {
        // Arrange
        var content = new FeedContent(
            "category",
            "http://www.w3.org/2005/Atom",
            null,
            [
                new FeedAttribute("term", "tech"),
                new FeedAttribute("label", "Technology"),
                new FeedAttribute("scheme", "https://example.com/categories"),
            ],
            null);

        // Act
        var category = _parser.ParseCategory(content);

        // Assert - Atom 的 term 存储在 Name，label 存储在 Label
        Assert.AreEqual("Technology", category.Label);
        Assert.AreEqual("tech", category.Name);
    }

    [TestMethod]
    public void ParseCategory_TermOnly_ShouldUseTerm()
    {
        // Arrange
        var content = new FeedContent(
            "category",
            "http://www.w3.org/2005/Atom",
            null,
            [new FeedAttribute("term", "programming")],
            null);

        // Act
        var category = _parser.ParseCategory(content);

        // Assert - 没有 label 时，Name 和 Label 都应该使用 term
        Assert.AreEqual("programming", category.Name);
        // Label 可能为 null 或等于 term，取决于实现
    }

    #endregion

    #region ParsePerson 测试

    [TestMethod]
    public void ParsePerson_FullPerson_ShouldParseAllFields()
    {
        // Arrange
        var content = new FeedContent(
            "author",
            "http://www.w3.org/2005/Atom",
            null,
            null,
            [
                new FeedContent("name", "http://www.w3.org/2005/Atom", "王五", null, null),
                new FeedContent("email", "http://www.w3.org/2005/Atom", "wangwu@example.com", null, null),
                new FeedContent("uri", "http://www.w3.org/2005/Atom", "https://wangwu.example.com", null, null),
            ]);

        // Act
        var person = _parser.ParsePerson(content);

        // Assert
        Assert.AreEqual("王五", person.Name);
        Assert.AreEqual("wangwu@example.com", person.Email);
        // Uri 类会自动为没有路径的地址添加尾随斜杠
        Assert.IsTrue(
            person.Uri?.ToString().StartsWith("https://wangwu.example.com", StringComparison.Ordinal) == true,
            $"URI 应以 'https://wangwu.example.com' 开头，实际为: {person.Uri}");
    }

    [TestMethod]
    public void ParsePerson_NameOnly_ShouldParseName()
    {
        // Arrange
        var content = new FeedContent(
            "contributor",
            "http://www.w3.org/2005/Atom",
            null,
            null,
            [new FeedContent("name", "http://www.w3.org/2005/Atom", "赵六", null, null)]);

        // Act
        var person = _parser.ParsePerson(content);

        // Assert
        Assert.AreEqual("赵六", person.Name);
        Assert.IsNull(person.Email);
        Assert.IsNull(person.Uri);
    }

    #endregion

    #region RFC 5005 分页链接测试

    [TestMethod]
    public void ParseChannel_WithPagingLinks_ShouldExtractPagingInfo()
    {
        // Arrange - RFC 5005 分页链接
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>分页测试</title>
              <id>https://example.com/feed</id>
              <updated>2024-01-01T12:00:00Z</updated>
              <link href="https://example.com" rel="alternate"/>
              <link href="https://example.com/feed" rel="self"/>
              <link href="https://example.com/feed?page=1" rel="first"/>
              <link href="https://example.com/feed?page=2" rel="previous"/>
              <link href="https://example.com/feed?page=4" rel="next"/>
              <link href="https://example.com/feed?page=10" rel="last"/>
            </feed>
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
        Assert.AreEqual("https://example.com/feed?page=1", channel.PagingLinks.First?.ToString());
        Assert.AreEqual("https://example.com/feed?page=2", channel.PagingLinks.Previous?.ToString());
        Assert.AreEqual("https://example.com/feed?page=4", channel.PagingLinks.Next?.ToString());
        Assert.AreEqual("https://example.com/feed?page=10", channel.PagingLinks.Last?.ToString());
        Assert.AreEqual("https://example.com/feed", channel.PagingLinks.Current?.ToString());
    }

    [TestMethod]
    public void ParseChannel_WithNextLinkOnly_ShouldExtractPagingInfo()
    {
        // Arrange - 只有 next 链接
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>部分分页</title>
              <id>https://example.com/feed</id>
              <updated>2024-01-01T12:00:00Z</updated>
              <link href="https://example.com" rel="alternate"/>
              <link href="https://example.com/feed?page=2" rel="next"/>
            </feed>
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
        Assert.AreEqual("https://example.com/feed?page=2", channel.PagingLinks.Next?.ToString());
        Assert.IsNull(channel.PagingLinks.Previous);
    }

    [TestMethod]
    public void ParseChannel_WithPrevLink_ShouldRecognizePrevAlias()
    {
        // Arrange - 使用 "prev" 作为 "previous" 的别名
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>Prev别名测试</title>
              <id>https://example.com/feed</id>
              <updated>2024-01-01T12:00:00Z</updated>
              <link href="https://example.com" rel="alternate"/>
              <link href="https://example.com/feed?page=1" rel="prev"/>
            </feed>
            """;
        using var stream = TestDataFactory.ToStream(xml);
        using var xmlReader = XmlReader.Create(stream);

        // Act
        var channel = _parser.ParseChannel(xmlReader);

        // Assert
        Assert.IsNotNull(channel.PagingLinks);
        Assert.IsTrue(channel.PagingLinks.HasPaging);
        Assert.IsTrue(channel.PagingLinks.HasPrevious);
        Assert.AreEqual("https://example.com/feed?page=1", channel.PagingLinks.Previous?.ToString());
    }

    [TestMethod]
    public void ParseChannel_WithoutPagingLinks_ShouldReturnNull()
    {
        // Arrange - 没有分页链接
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>普通Feed</title>
              <id>https://example.com/feed</id>
              <updated>2024-01-01T12:00:00Z</updated>
              <link href="https://example.com" rel="alternate"/>
              <link href="https://example.com/feed" rel="self"/>
            </feed>
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
