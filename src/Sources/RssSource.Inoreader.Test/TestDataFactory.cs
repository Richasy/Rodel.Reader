// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Inoreader.Test;

/// <summary>
/// 测试数据工厂.
/// 提供测试用的模拟数据.
/// </summary>
public static class TestDataFactory
{
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
    /// 创建默认测试配置.
    /// </summary>
    /// <returns>客户端配置.</returns>
    public static InoreaderClientOptions CreateDefaultOptions()
    {
        return new InoreaderClientOptions
        {
            AccessToken = "test_access_token",
            RefreshToken = "test_refresh_token",
            ExpireTime = DateTimeOffset.Now.AddHours(1),
            DataSource = InoreaderDataSource.Default,
            Timeout = TimeSpan.FromSeconds(30),
            MaxConcurrentRequests = 5,
        };
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
    /// 创建标签列表响应 JSON.
    /// </summary>
    public static string CreateTagListJson()
    {
        return """
        {
            "tags": [
                {
                    "id": "user/-/label/科技",
                    "sortid": "00000001",
                    "type": "folder",
                    "unread_count": 10,
                    "unseen_count": 5
                },
                {
                    "id": "user/-/label/开发",
                    "sortid": "00000002",
                    "type": "folder",
                    "unread_count": 20,
                    "unseen_count": 10
                },
                {
                    "id": "user/-/state/com.google/starred",
                    "sortid": "00000000",
                    "type": "tag"
                }
            ]
        }
        """;
    }

    /// <summary>
    /// 创建流偏好响应 JSON.
    /// </summary>
    public static string CreatePreferenceJson()
    {
        return """
        {
            "streamprefs": {
                "user/-/state/com.google/root": [
                    {
                        "id": "subscription-ordering",
                        "value": "0000000100000002"
                    }
                ]
            }
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
    /// 创建 OAuth Token 响应 JSON.
    /// </summary>
    public static string CreateAuthTokenJson()
    {
        return """
        {
            "access_token": "new_access_token",
            "refresh_token": "new_refresh_token",
            "expires_in": 3600,
            "token_type": "Bearer",
            "scope": "read write"
        }
        """;
    }
}
