// Copyright (c) Reader Copilot. All rights reserved.

namespace FeedParser.Test;

/// <summary>
/// 测试数据工厂.
/// </summary>
internal static class TestDataFactory
{
    /// <summary>
    /// 测试数据目录.
    /// </summary>
    public static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");

    /// <summary>
    /// RSS 测试数据目录.
    /// </summary>
    public static readonly string RssDir = Path.Combine(TestDataDir, "Rss");

    /// <summary>
    /// Atom 测试数据目录.
    /// </summary>
    public static readonly string AtomDir = Path.Combine(TestDataDir, "Atom");

    /// <summary>
    /// 播客测试数据目录.
    /// </summary>
    public static readonly string PodcastDir = Path.Combine(TestDataDir, "Podcast");

    /// <summary>
    /// 创建最小的 RSS 2.0 Feed XML.
    /// </summary>
    public static string CreateMinimalRss(string title = "测试频道", string link = "https://example.com")
    {
        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>{title}</title>
                <link>{link}</link>
                <description>测试描述</description>
              </channel>
            </rss>
            """;
    }

    /// <summary>
    /// 创建包含订阅项的 RSS 2.0 Feed XML.
    /// </summary>
    public static string CreateRssWithItems(int itemCount = 3)
    {
        var items = string.Join("\n", Enumerable.Range(1, itemCount).Select(i => $"""
                <item>
                  <title>文章 {i}</title>
                  <link>https://example.com/article-{i}</link>
                  <description>这是文章 {i} 的描述</description>
                  <pubDate>Sat, 0{i} Jan 2024 12:00:00 GMT</pubDate>
                  <guid>https://example.com/article-{i}</guid>
                </item>
            """));

        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>测试频道</title>
                <link>https://example.com</link>
                <description>包含多个订阅项的测试频道</description>
                <language>zh-cn</language>
                <lastBuildDate>Sat, 01 Jan 2024 12:00:00 GMT</lastBuildDate>
                {items}
              </channel>
            </rss>
            """;
    }

    /// <summary>
    /// 创建包含 iTunes 播客扩展的 RSS XML.
    /// </summary>
    public static string CreatePodcastRss()
    {
        return """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd">
              <channel>
                <title>测试播客</title>
                <link>https://example.com/podcast</link>
                <description>这是一个测试播客</description>
                <language>zh-cn</language>
                <itunes:author>播客作者</itunes:author>
                <itunes:image href="https://example.com/cover.jpg"/>
                <itunes:category text="Technology"/>
                <item>
                  <title>第一集</title>
                  <link>https://example.com/ep1</link>
                  <description>第一集的描述</description>
                  <enclosure url="https://example.com/ep1.mp3" length="12345678" type="audio/mpeg"/>
                  <pubDate>Mon, 15 Jan 2024 10:00:00 GMT</pubDate>
                  <itunes:duration>01:23:45</itunes:duration>
                  <itunes:image href="https://example.com/ep1-cover.jpg"/>
                </item>
              </channel>
            </rss>
            """;
    }

    /// <summary>
    /// 创建最小的 Atom 1.0 Feed XML.
    /// </summary>
    public static string CreateMinimalAtom(string title = "测试 Feed", string id = "urn:uuid:test-feed")
    {
        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>{title}</title>
              <id>{id}</id>
              <updated>2024-01-01T12:00:00Z</updated>
            </feed>
            """;
    }

    /// <summary>
    /// 创建包含条目的 Atom 1.0 Feed XML.
    /// </summary>
    public static string CreateAtomWithEntries(int entryCount = 3)
    {
        var entries = string.Join("\n", Enumerable.Range(1, entryCount).Select(i => $"""
                <entry>
                  <title>条目 {i}</title>
                  <id>urn:uuid:entry-{i}</id>
                  <updated>2024-01-0{i}T12:00:00Z</updated>
                  <link href="https://example.com/entry-{i}"/>
                  <summary>这是条目 {i} 的摘要</summary>
                  <author>
                    <name>作者 {i}</name>
                  </author>
                </entry>
            """));

        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>测试 Feed</title>
              <id>urn:uuid:test-feed</id>
              <updated>2024-01-01T12:00:00Z</updated>
              <link href="https://example.com" rel="alternate"/>
              <author>
                <name>Feed 作者</name>
                <email>author@example.com</email>
              </author>
              {entries}
            </feed>
            """;
    }

    /// <summary>
    /// 创建包含 content 模块的 RSS XML.
    /// </summary>
    public static string CreateRssWithContentEncoded()
    {
        return """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:content="http://purl.org/rss/1.0/modules/content/">
              <channel>
                <title>包含 content:encoded 的频道</title>
                <link>https://example.com</link>
                <description>测试 content:encoded</description>
                <item>
                  <title>富文本文章</title>
                  <link>https://example.com/rich-content</link>
                  <description>简短描述</description>
                  <content:encoded><![CDATA[
                    <h1>富文本内容</h1>
                    <p>这是带有 <strong>HTML</strong> 格式的内容。</p>
                    <img src="https://example.com/image.jpg" alt="示例图片"/>
                  ]]></content:encoded>
                </item>
              </channel>
            </rss>
            """;
    }

    /// <summary>
    /// 创建无效的 XML.
    /// </summary>
    public static string CreateInvalidXml()
    {
        return """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>未闭合的标签
              </channel>
            """;
    }

    /// <summary>
    /// 创建非 Feed 的有效 XML.
    /// </summary>
    public static string CreateNonFeedXml()
    {
        return """
            <?xml version="1.0" encoding="UTF-8"?>
            <html>
              <head><title>网页</title></head>
              <body><p>这不是 Feed</p></body>
            </html>
            """;
    }

    /// <summary>
    /// 将字符串转换为 Stream.
    /// </summary>
    public static Stream ToStream(string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    /// <summary>
    /// 从测试数据文件读取 Stream.
    /// </summary>
    public static Stream? ReadTestFile(string relativePath)
    {
        var fullPath = Path.Combine(TestDataDir, relativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        return File.OpenRead(fullPath);
    }

    /// <summary>
    /// 获取测试数据目录中的所有 XML 文件.
    /// </summary>
    public static IEnumerable<string> GetTestFiles(string subDir)
    {
        var dir = Path.Combine(TestDataDir, subDir);
        if (!Directory.Exists(dir))
        {
            return [];
        }

        return Directory.GetFiles(dir, "*.xml");
    }
}
