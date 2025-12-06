// Copyright (c) Richasy. All rights reserved.

namespace RssSource.GoogleReader.Test;

/// <summary>
/// 测试数据工厂.
/// 提供测试用的模拟数据.
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// 测试服务器地址.
    /// </summary>
    public const string TestServer = "https://freshrss.example.com/api/greader.php";

    /// <summary>
    /// IT之家订阅源.
    /// </summary>
    public static RssFeed IthomeFeed => new()
    {
        Id = "feed/https://www.ithome.com/rss",
        Name = "IT之家",
        Url = "https://www.ithome.com/rss",
        Website = "https://www.ithome.com",
    };

    /// <summary>
    /// 极客公园订阅源.
    /// </summary>
    public static RssFeed GeekparkFeed => new()
    {
        Id = "feed/https://www.geekpark.net/rss",
        Name = "极客公园",
        Url = "https://www.geekpark.net/rss",
        Website = "https://www.geekpark.net",
    };

    /// <summary>
    /// .NET Blog 订阅源.
    /// </summary>
    public static RssFeed DotNetBlogFeed => new()
    {
        Id = "feed/https://devblogs.microsoft.com/dotnet/feed/",
        Name = ".NET Blog",
        Url = "https://devblogs.microsoft.com/dotnet/feed/",
        Website = "https://devblogs.microsoft.com/dotnet",
    };

    /// <summary>
    /// 科技分组.
    /// </summary>
    public static RssFeedGroup TechGroup => new()
    {
        Id = "user/-/label/科技",
        Name = "科技",
    };

    /// <summary>
    /// 开发分组.
    /// </summary>
    public static RssFeedGroup DevGroup => new()
    {
        Id = "user/-/label/开发",
        Name = "开发",
    };

    /// <summary>
    /// 创建默认测试配置（已认证）.
    /// </summary>
    /// <returns>客户端配置.</returns>
    public static GoogleReaderClientOptions CreateDefaultOptions()
    {
        return new GoogleReaderClientOptions
        {
            Server = TestServer,
            UserName = "testuser",
            Password = "testpassword",
            AuthToken = "test_auth_token",
            Timeout = TimeSpan.FromSeconds(30),
            MaxConcurrentRequests = 5,
            ArticlesPerRequest = 100,
        };
    }

    /// <summary>
    /// 创建未认证的测试配置.
    /// </summary>
    /// <returns>客户端配置.</returns>
    public static GoogleReaderClientOptions CreateUnauthenticatedOptions()
    {
        return new GoogleReaderClientOptions
        {
            Server = TestServer,
            UserName = "testuser",
            Password = "testpassword",
            Timeout = TimeSpan.FromSeconds(30),
        };
    }

    /// <summary>
    /// 创建登录响应 JSON.
    /// </summary>
    public static string CreateAuthResponseJson()
    {
        return """
        {
            "SID": "test_sid",
            "LSID": "test_lsid",
            "Auth": "test_auth_token_new"
        }
        """;
    }

    /// <summary>
    /// 创建登录响应（文本格式）.
    /// </summary>
    public static string CreateAuthResponseText()
    {
        return """
        SID=test_sid
        LSID=test_lsid
        Auth=test_auth_token_new
        """;
    }

    /// <summary>
    /// 创建订阅列表响应 JSON.
    /// </summary>
    public static string CreateSubscriptionListJson()
    {
        return """
        {
            "subscriptions": [
                {
                    "id": "feed/https://www.ithome.com/rss",
                    "title": "IT之家",
                    "url": "https://www.ithome.com/rss",
                    "htmlUrl": "https://www.ithome.com",
                    "iconUrl": "https://www.ithome.com/favicon.ico",
                    "categories": [
                        { "id": "user/-/label/科技", "label": "科技" }
                    ]
                },
                {
                    "id": "feed/https://www.geekpark.net/rss",
                    "title": "极客公园",
                    "url": "https://www.geekpark.net/rss",
                    "htmlUrl": "https://www.geekpark.net",
                    "categories": [
                        { "id": "user/-/label/科技", "label": "科技" }
                    ]
                },
                {
                    "id": "feed/https://devblogs.microsoft.com/dotnet/feed/",
                    "title": ".NET Blog",
                    "url": "https://devblogs.microsoft.com/dotnet/feed/",
                    "htmlUrl": "https://devblogs.microsoft.com/dotnet",
                    "categories": [
                        { "id": "user/-/label/开发", "label": "开发" }
                    ]
                }
            ]
        }
        """;
    }

    /// <summary>
    /// 创建文章流响应 JSON.
    /// </summary>
    /// <param name="feedId">订阅源 ID.</param>
    public static string CreateStreamContentJson(string feedId)
    {
        var timestamp = DateTimeOffset.Now.AddHours(-1).ToUnixTimeSeconds();
        return $$"""
        {
            "id": "{{feedId}}",
            "updated": {{DateTimeOffset.Now.ToUnixTimeSeconds()}},
            "items": [
                {
                    "id": "tag:google.com,2005:reader/item/000000001",
                    "published": {{timestamp}},
                    "title": "测试文章标题 1",
                    "canonical": [{ "href": "https://example.com/article1" }],
                    "categories": [
                        "user/-/state/com.google/reading-list",
                        "user/-/label/科技"
                    ],
                    "summary": { "content": "<p>这是测试文章 1 的摘要内容。</p>" },
                    "author": "测试作者"
                },
                {
                    "id": "tag:google.com,2005:reader/item/000000002",
                    "published": {{timestamp - 3600}},
                    "title": "测试文章标题 2",
                    "alternate": [{ "href": "https://example.com/article2" }],
                    "categories": [
                        "user/-/state/com.google/reading-list",
                        "user/-/state/com.google/read"
                    ],
                    "content": { "content": "<p>这是测试文章 2 的完整内容。</p><img src=\"https://example.com/image.jpg\"/>" },
                    "author": "另一位作者"
                }
            ],
            "continuation": "next_page_token"
        }
        """;
    }

    /// <summary>
    /// 创建 OPML 内容.
    /// </summary>
    public static string CreateOpmlContent()
    {
        return """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="2.0">
            <head><title>Test OPML</title></head>
            <body>
                <outline text="科技" title="科技">
                    <outline text="IT之家" type="rss" xmlUrl="https://www.ithome.com/rss" htmlUrl="https://www.ithome.com"/>
                    <outline text="极客公园" type="rss" xmlUrl="https://www.geekpark.net/rss" htmlUrl="https://www.geekpark.net"/>
                </outline>
                <outline text="开发" title="开发">
                    <outline text=".NET Blog" type="rss" xmlUrl="https://devblogs.microsoft.com/dotnet/feed/" htmlUrl="https://devblogs.microsoft.com/dotnet"/>
                </outline>
            </body>
        </opml>
        """;
    }
}
