// Copyright (c) Richasy. All rights reserved.

using RichardSzalay.MockHttp;

namespace Richasy.RodelReader.Services.OpdsService.Test;

/// <summary>
/// 测试数据工厂.
/// </summary>
internal static class TestDataFactory
{
    public const string BaseAddress = "https://opds.example.com";

    /// <summary>
    /// 简单的 OPDS 根目录 Feed.
    /// </summary>
    public static string RootFeed => """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom"
              xmlns:opds="http://opds-spec.org/2010/catalog"
              xmlns:dc="http://purl.org/dc/elements/1.1/">
            <id>urn:uuid:root-catalog</id>
            <title>My Library</title>
            <subtitle>A test OPDS catalog</subtitle>
            <updated>2024-01-15T12:00:00Z</updated>
            <icon>/favicon.ico</icon>
            <link rel="self" href="/opds/v1.2/catalog" type="application/atom+xml;profile=opds-catalog"/>
            <link rel="start" href="/opds/v1.2/catalog" type="application/atom+xml;profile=opds-catalog"/>
            <link rel="search" href="/opds/v1.2/opensearch" type="application/opensearchdescription+xml"/>
            <entry>
                <id>urn:uuid:new-books</id>
                <title>New Books</title>
                <updated>2024-01-15T12:00:00Z</updated>
                <link rel="subsection" href="/opds/v1.2/new" type="application/atom+xml;profile=opds-catalog"/>
                <content type="text">Recently added books</content>
            </entry>
            <entry>
                <id>urn:uuid:all-books</id>
                <title>All Books</title>
                <updated>2024-01-14T12:00:00Z</updated>
                <link rel="subsection" href="/opds/v1.2/all" type="application/atom+xml;profile=opds-catalog"/>
                <content type="text">Browse all books</content>
            </entry>
        </feed>
        """;

    /// <summary>
    /// 包含书籍的 Feed.
    /// </summary>
    public static string BooksFeed => """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom"
              xmlns:opds="http://opds-spec.org/2010/catalog"
              xmlns:dc="http://purl.org/dc/elements/1.1/"
              xmlns:dcterms="http://purl.org/dc/terms/"
              xmlns:thr="http://purl.org/syndication/thread/1.0">
            <id>urn:uuid:books-catalog</id>
            <title>All Books</title>
            <updated>2024-01-15T12:00:00Z</updated>
            <link rel="self" href="/opds/v1.2/all" type="application/atom+xml;profile=opds-catalog"/>
            <link rel="start" href="/opds/v1.2/catalog" type="application/atom+xml;profile=opds-catalog"/>
            <link rel="next" href="/opds/v1.2/all?page=2" type="application/atom+xml;profile=opds-catalog"/>
            <link rel="http://opds-spec.org/facet" href="/opds/v1.2/all?author=john" title="John Doe" opds:facetGroup="Authors" opds:activeFacet="false" thr:count="5"/>
            <link rel="http://opds-spec.org/facet" href="/opds/v1.2/all?genre=fiction" title="Fiction" opds:facetGroup="Genres" opds:activeFacet="true" thr:count="10"/>
            <entry>
                <id>urn:uuid:book-1</id>
                <title>The Great Adventure</title>
                <summary>An exciting journey through uncharted territories.</summary>
                <content type="html">&lt;p&gt;Full description of the book...&lt;/p&gt;</content>
                <updated>2024-01-10T12:00:00Z</updated>
                <published>2023-06-15T00:00:00Z</published>
                <author>
                    <name>John Doe</name>
                    <uri>https://example.com/authors/john</uri>
                </author>
                <author>
                    <name>Jane Smith</name>
                </author>
                <category term="fiction" label="Fiction"/>
                <category term="adventure" label="Adventure"/>
                <dc:language>en</dc:language>
                <dc:publisher>Example Publisher</dc:publisher>
                <dc:identifier>isbn:978-0-123456-78-9</dc:identifier>
                <link rel="http://opds-spec.org/image" href="/covers/book-1.jpg" type="image/jpeg"/>
                <link rel="http://opds-spec.org/image/thumbnail" href="/covers/book-1-thumb.jpg" type="image/jpeg"/>
                <link rel="http://opds-spec.org/acquisition/open-access" href="/download/book-1.epub" type="application/epub+zip"/>
                <link rel="http://opds-spec.org/acquisition/open-access" href="/download/book-1.pdf" type="application/pdf"/>
            </entry>
            <entry>
                <id>urn:uuid:book-2</id>
                <title>Mystery at Midnight</title>
                <summary>A thrilling mystery novel.</summary>
                <updated>2024-01-08T12:00:00Z</updated>
                <author>
                    <name>Alice Wonder</name>
                </author>
                <category term="mystery" label="Mystery"/>
                <link rel="http://opds-spec.org/image" href="/covers/book-2.jpg" type="image/jpeg"/>
                <link rel="http://opds-spec.org/acquisition" href="/download/book-2.epub" type="application/epub+zip"/>
            </entry>
        </feed>
        """;

    /// <summary>
    /// 包含购买链接的 Feed.
    /// </summary>
    public static string PaidBooksFeed => """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom"
              xmlns:opds="http://opds-spec.org/2010/catalog"
              xmlns:dc="http://purl.org/dc/elements/1.1/">
            <id>urn:uuid:paid-books</id>
            <title>Paid Books</title>
            <updated>2024-01-15T12:00:00Z</updated>
            <entry>
                <id>urn:uuid:paid-book-1</id>
                <title>Premium Content</title>
                <updated>2024-01-15T12:00:00Z</updated>
                <link rel="http://opds-spec.org/acquisition/buy" href="/buy/book-1" type="application/epub+zip">
                    <opds:price currencycode="USD">9.99</opds:price>
                    <opds:indirectAcquisition type="application/vnd.adobe.adept+xml">
                        <opds:indirectAcquisition type="application/epub+zip"/>
                    </opds:indirectAcquisition>
                </link>
                <link rel="http://opds-spec.org/acquisition/sample" href="/sample/book-1.epub" type="application/epub+zip"/>
                <link rel="http://opds-spec.org/acquisition/borrow" href="/borrow/book-1" type="application/epub+zip"/>
            </entry>
        </feed>
        """;

    /// <summary>
    /// 带分页的 Feed（第二页）.
    /// </summary>
    public static string PagedFeed => """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
            <id>urn:uuid:paged-catalog</id>
            <title>All Books - Page 2</title>
            <updated>2024-01-15T12:00:00Z</updated>
            <link rel="self" href="/opds/v1.2/all?page=2" type="application/atom+xml"/>
            <link rel="start" href="/opds/v1.2/catalog" type="application/atom+xml"/>
            <link rel="first" href="/opds/v1.2/all" type="application/atom+xml"/>
            <link rel="previous" href="/opds/v1.2/all?page=1" type="application/atom+xml"/>
            <link rel="next" href="/opds/v1.2/all?page=3" type="application/atom+xml"/>
            <link rel="last" href="/opds/v1.2/all?page=5" type="application/atom+xml"/>
            <entry>
                <id>urn:uuid:book-3</id>
                <title>Book on Page 2</title>
                <updated>2024-01-15T12:00:00Z</updated>
                <link rel="http://opds-spec.org/acquisition/open-access" href="/download/book-3.epub" type="application/epub+zip"/>
            </entry>
        </feed>
        """;

    /// <summary>
    /// OpenSearch 描述文档.
    /// </summary>
    public static string OpenSearchDescription => """
        <?xml version="1.0" encoding="UTF-8"?>
        <OpenSearchDescription xmlns="http://a9.com/-/spec/opensearch/1.1/">
            <ShortName>My Library</ShortName>
            <Description>Search the library catalog</Description>
            <Url type="application/atom+xml;profile=opds-catalog" template="https://opds.example.com/opds/v1.2/search?q={searchTerms}"/>
            <Url type="text/html" template="https://opds.example.com/search?q={searchTerms}"/>
        </OpenSearchDescription>
        """;

    /// <summary>
    /// 搜索结果 Feed.
    /// </summary>
    public static string SearchResultsFeed => """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom"
              xmlns:opds="http://opds-spec.org/2010/catalog">
            <id>urn:uuid:search-results</id>
            <title>Search Results</title>
            <updated>2024-01-15T12:00:00Z</updated>
            <link rel="self" href="/opds/v1.2/search?q=adventure" type="application/atom+xml"/>
            <entry>
                <id>urn:uuid:book-1</id>
                <title>The Great Adventure</title>
                <updated>2024-01-10T12:00:00Z</updated>
                <link rel="http://opds-spec.org/acquisition/open-access" href="/download/book-1.epub" type="application/epub+zip"/>
            </entry>
        </feed>
        """;

    /// <summary>
    /// 空的 Feed.
    /// </summary>
    public static string EmptyFeed => """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
            <id>urn:uuid:empty</id>
            <title>Empty Catalog</title>
            <updated>2024-01-15T12:00:00Z</updated>
        </feed>
        """;

    /// <summary>
    /// 最小化的有效 Feed.
    /// </summary>
    public static string MinimalFeed => """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
            <title>Minimal</title>
        </feed>
        """;

    /// <summary>
    /// 创建 Mock HTTP 客户端.
    /// </summary>
    public static HttpClient CreateMockHttpClient(MockHttpMessageHandler handler)
    {
        var client = handler.ToHttpClient();
        client.BaseAddress = new Uri(BaseAddress);
        return client;
    }

    /// <summary>
    /// 创建用于测试的日志器.
    /// </summary>
    public static ILogger<T> CreateLogger<T>() => NullLogger<T>.Instance;
}
