// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Feedbin.Test;

/// <summary>
/// 测试数据工厂.
/// 提供测试用的模拟数据.
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// 测试服务器地址.
    /// </summary>
    public const string TestServer = "https://api.feedbin.com/v2";

    /// <summary>
    /// 测试用户名.
    /// </summary>
    public const string TestUserName = "testuser";

    /// <summary>
    /// 测试密码.
    /// </summary>
    public const string TestPassword = "testpassword";

    /// <summary>
    /// IT之家订阅源.
    /// </summary>
    public static RssFeed IthomeFeed => new()
    {
        Id = "123",
        Name = "IT之家",
        Url = "https://www.ithome.com/rss",
        Website = "https://www.ithome.com",
        Comment = "1001", // subscription_id
    };

    /// <summary>
    /// 极客公园订阅源.
    /// </summary>
    public static RssFeed GeekparkFeed => new()
    {
        Id = "124",
        Name = "极客公园",
        Url = "https://www.geekpark.net/rss",
        Website = "https://www.geekpark.net",
        Comment = "1002",
    };

    /// <summary>
    /// .NET Blog 订阅源.
    /// </summary>
    public static RssFeed DotNetBlogFeed => new()
    {
        Id = "125",
        Name = ".NET Blog",
        Url = "https://devblogs.microsoft.com/dotnet/feed/",
        Website = "https://devblogs.microsoft.com/dotnet",
        Comment = "1003",
    };

    /// <summary>
    /// 科技分组.
    /// </summary>
    public static RssFeedGroup TechGroup => new()
    {
        Id = "科技",
        Name = "科技",
    };

    /// <summary>
    /// 开发分组.
    /// </summary>
    public static RssFeedGroup DevGroup => new()
    {
        Id = "开发",
        Name = "开发",
    };

    /// <summary>
    /// 创建默认测试配置.
    /// </summary>
    /// <returns>客户端配置.</returns>
    public static FeedbinClientOptions CreateDefaultOptions()
    {
        return new FeedbinClientOptions
        {
            Server = TestServer,
            UserName = TestUserName,
            Password = TestPassword,
            Timeout = TimeSpan.FromSeconds(30),
            MaxConcurrentRequests = 5,
            ArticlesPerRequest = 100,
        };
    }

    /// <summary>
    /// 创建无凭据的测试配置.
    /// </summary>
    /// <returns>客户端配置.</returns>
    public static FeedbinClientOptions CreateEmptyCredentialsOptions()
    {
        return new FeedbinClientOptions
        {
            Server = TestServer,
            Timeout = TimeSpan.FromSeconds(30),
        };
    }

    /// <summary>
    /// 创建认证成功响应 JSON.
    /// </summary>
    public static string CreateAuthSuccessResponseJson()
    {
        return "{}"; // Feedbin 返回空对象表示成功
    }

    /// <summary>
    /// 创建订阅列表响应 JSON.
    /// </summary>
    public static string CreateSubscriptionListJson()
    {
        return """
        [
            {
                "id": 1001,
                "created_at": "2023-01-15T10:30:00.000000Z",
                "feed_id": 123,
                "title": "IT之家",
                "feed_url": "https://www.ithome.com/rss",
                "site_url": "https://www.ithome.com"
            },
            {
                "id": 1002,
                "created_at": "2023-01-16T11:00:00.000000Z",
                "feed_id": 124,
                "title": "极客公园",
                "feed_url": "https://www.geekpark.net/rss",
                "site_url": "https://www.geekpark.net"
            },
            {
                "id": 1003,
                "created_at": "2023-01-17T09:00:00.000000Z",
                "feed_id": 125,
                "title": ".NET Blog",
                "feed_url": "https://devblogs.microsoft.com/dotnet/feed/",
                "site_url": "https://devblogs.microsoft.com/dotnet"
            }
        ]
        """;
    }

    /// <summary>
    /// 创建标签列表响应 JSON.
    /// </summary>
    public static string CreateTaggingsListJson()
    {
        return """
        [
            {
                "id": 1,
                "feed_id": 123,
                "name": "科技"
            },
            {
                "id": 2,
                "feed_id": 124,
                "name": "科技"
            },
            {
                "id": 3,
                "feed_id": 125,
                "name": "开发"
            }
        ]
        """;
    }

    /// <summary>
    /// 创建空标签列表响应 JSON.
    /// </summary>
    public static string CreateEmptyTaggingsListJson()
    {
        return "[]";
    }

    /// <summary>
    /// 创建文章列表响应 JSON.
    /// </summary>
    public static string CreateEntriesListJson()
    {
        return """
        [
            {
                "id": 10001,
                "feed_id": 123,
                "title": "新款 iPhone 发布",
                "author": "IT之家",
                "summary": "苹果公司今日发布了新款 iPhone...",
                "content": "<p>苹果公司今日发布了新款 iPhone，带来多项创新功能。</p><img src=\"https://example.com/iphone.jpg\" />",
                "url": "https://www.ithome.com/article/12345.htm",
                "published": "2023-12-01T08:00:00.000000Z",
                "created_at": "2023-12-01T08:05:00.000000Z"
            },
            {
                "id": 10002,
                "feed_id": 123,
                "title": "Windows 12 预览版更新",
                "author": "IT之家",
                "summary": "微软发布了 Windows 12 最新预览版...",
                "content": "<p>微软发布了 Windows 12 最新预览版，修复了多个问题。</p>",
                "url": "https://www.ithome.com/article/12346.htm",
                "published": "2023-12-01T09:00:00.000000Z",
                "created_at": "2023-12-01T09:05:00.000000Z"
            }
        ]
        """;
    }

    /// <summary>
    /// 创建单个订阅响应 JSON.
    /// </summary>
    public static string CreateSubscriptionJson()
    {
        return """
        {
            "id": 1004,
            "created_at": "2023-12-01T10:00:00.000000Z",
            "feed_id": 126,
            "title": "New Feed",
            "feed_url": "https://example.com/feed.xml",
            "site_url": "https://example.com"
        }
        """;
    }

    /// <summary>
    /// 创建未读文章 ID 列表响应 JSON.
    /// </summary>
    public static string CreateUnreadEntriesJson()
    {
        return "[10001, 10002, 10003, 10004, 10005]";
    }

    /// <summary>
    /// 创建空未读文章列表 JSON.
    /// </summary>
    public static string CreateEmptyUnreadEntriesJson()
    {
        return "[]";
    }

    /// <summary>
    /// 创建导入响应 JSON.
    /// </summary>
    public static string CreateImportResponseJson()
    {
        return """
        {
            "id": 1,
            "complete": false,
            "created_at": "2023-12-01T10:00:00.000000Z",
            "import_items": [
                {
                    "title": "Test Feed 1",
                    "feed_url": "https://example1.com/feed.xml",
                    "status": "pending"
                },
                {
                    "title": "Test Feed 2",
                    "feed_url": "https://example2.com/feed.xml",
                    "status": "pending"
                }
            ]
        }
        """;
    }

    /// <summary>
    /// 创建发现多个 Feed 响应 JSON.
    /// </summary>
    public static string CreateMultipleFeedsDiscoveredJson()
    {
        return """
        [
            {
                "feed_url": "https://example.com/rss.xml",
                "title": "Example RSS"
            },
            {
                "feed_url": "https://example.com/atom.xml",
                "title": "Example Atom"
            }
        ]
        """;
    }

    /// <summary>
    /// 创建标签关联响应 JSON.
    /// </summary>
    public static string CreateTaggingJson()
    {
        return """
        {
            "id": 10,
            "feed_id": 126,
            "name": "科技"
        }
        """;
    }
}
