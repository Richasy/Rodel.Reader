// Copyright (c) Richasy. All rights reserved.

using System.Text.Json;

namespace RssSource.Miniflux.Test;

/// <summary>
/// 测试数据工厂.
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// 测试服务器地址.
    /// </summary>
    public const string TestServer = "https://miniflux.example.com";

    /// <summary>
    /// 测试用户名.
    /// </summary>
    public const string TestUserName = "testuser";

    /// <summary>
    /// 测试密码.
    /// </summary>
    public const string TestPassword = "testpassword";

    /// <summary>
    /// 测试 API Token.
    /// </summary>
    public const string TestApiToken = "test-api-token-12345";

    /// <summary>
    /// 创建默认配置选项（使用 API Token）.
    /// </summary>
    public static MinifluxClientOptions CreateDefaultOptions()
        => new()
        {
            Server = TestServer,
            ApiToken = TestApiToken,
            Timeout = TimeSpan.FromSeconds(30),
            MaxConcurrentRequests = 5,
            ArticlesPerRequest = 50,
        };

    /// <summary>
    /// 创建使用用户名密码的配置选项.
    /// </summary>
    public static MinifluxClientOptions CreateBasicAuthOptions()
        => new()
        {
            Server = TestServer,
            UserName = TestUserName,
            Password = TestPassword,
            Timeout = TimeSpan.FromSeconds(30),
            MaxConcurrentRequests = 5,
            ArticlesPerRequest = 50,
        };

    /// <summary>
    /// 创建未认证的配置选项.
    /// </summary>
    public static MinifluxClientOptions CreateUnauthenticatedOptions()
        => new()
        {
            Server = TestServer,
            Timeout = TimeSpan.FromSeconds(30),
        };

    /// <summary>
    /// 创建用户信息响应 JSON.
    /// </summary>
    public static string CreateUserResponse()
        => """
        {
            "id": 1,
            "username": "testuser",
            "is_admin": false,
            "theme": "system_serif",
            "language": "en_US",
            "timezone": "UTC",
            "last_login_at": "2024-01-01T00:00:00Z"
        }
        """;

    /// <summary>
    /// 创建订阅源列表响应 JSON.
    /// </summary>
    public static string CreateFeedsResponse()
        => """
        [
            {
                "id": 1,
                "user_id": 1,
                "title": "Test Feed 1",
                "feed_url": "https://example.com/feed1.xml",
                "site_url": "https://example.com",
                "checked_at": "2024-01-01T00:00:00Z",
                "parsing_error_count": 0,
                "category": {
                    "id": 1,
                    "title": "Tech",
                    "user_id": 1,
                    "hide_globally": false
                }
            },
            {
                "id": 2,
                "user_id": 1,
                "title": "Test Feed 2",
                "feed_url": "https://example.org/feed2.xml",
                "site_url": "https://example.org",
                "checked_at": "2024-01-01T00:00:00Z",
                "parsing_error_count": 0,
                "category": {
                    "id": 2,
                    "title": "News",
                    "user_id": 1,
                    "hide_globally": false
                }
            },
            {
                "id": 3,
                "user_id": 1,
                "title": "Test Feed 3 (No Category)",
                "feed_url": "https://example.net/feed3.xml",
                "site_url": "https://example.net",
                "checked_at": "2024-01-01T00:00:00Z",
                "parsing_error_count": 0,
                "category": null
            }
        ]
        """;

    /// <summary>
    /// 创建单个订阅源响应 JSON.
    /// </summary>
    public static string CreateFeedResponse(long id = 1, string title = "Test Feed")
        => $$"""
        {
            "id": {{id}},
            "user_id": 1,
            "title": "{{title}}",
            "feed_url": "https://example.com/feed.xml",
            "site_url": "https://example.com",
            "checked_at": "2024-01-01T00:00:00Z",
            "parsing_error_count": 0,
            "category": {
                "id": 1,
                "title": "Tech",
                "user_id": 1,
                "hide_globally": false
            }
        }
        """;

    /// <summary>
    /// 创建文章列表响应 JSON.
    /// </summary>
    public static string CreateEntriesResponse()
        => """
        {
            "total": 2,
            "entries": [
                {
                    "id": 100,
                    "user_id": 1,
                    "feed_id": 1,
                    "title": "Test Article 1",
                    "url": "https://example.com/article1",
                    "author": "Author 1",
                    "content": "<p>This is the content of article 1.</p><img src=\"https://example.com/image1.jpg\">",
                    "published_at": "2024-01-01T12:00:00Z",
                    "created_at": "2024-01-01T12:00:00Z",
                    "status": "unread",
                    "starred": false,
                    "reading_time": 2,
                    "tags": ["tech", "news"]
                },
                {
                    "id": 101,
                    "user_id": 1,
                    "feed_id": 1,
                    "title": "Test Article 2",
                    "url": "https://example.com/article2",
                    "author": "Author 2",
                    "content": "<p>This is the content of article 2.</p>",
                    "published_at": "2024-01-02T12:00:00Z",
                    "created_at": "2024-01-02T12:00:00Z",
                    "status": "read",
                    "starred": true,
                    "reading_time": 3,
                    "tags": []
                }
            ]
        }
        """;

    /// <summary>
    /// 创建分类列表响应 JSON.
    /// </summary>
    public static string CreateCategoriesResponse()
        => """
        [
            {"id": 1, "title": "Tech", "user_id": 1, "hide_globally": false},
            {"id": 2, "title": "News", "user_id": 1, "hide_globally": false}
        ]
        """;

    /// <summary>
    /// 创建单个分类响应 JSON.
    /// </summary>
    public static string CreateCategoryResponse(long id = 1, string title = "New Category")
        => $$"""
        {
            "id": {{id}},
            "title": "{{title}}",
            "user_id": 1,
            "hide_globally": false
        }
        """;

    /// <summary>
    /// 创建添加订阅源响应 JSON.
    /// </summary>
    public static string CreateAddFeedResponse(long feedId = 10)
        => $$"""
        {
            "feed_id": {{feedId}}
        }
        """;

    /// <summary>
    /// 创建 OPML 导出响应.
    /// </summary>
    public static string CreateOpmlExportResponse()
        => """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="2.0">
            <head>
                <title>Miniflux</title>
            </head>
            <body>
                <outline text="Tech" title="Tech">
                    <outline type="rss" text="Test Feed" title="Test Feed" xmlUrl="https://example.com/feed.xml" htmlUrl="https://example.com"/>
                </outline>
            </body>
        </opml>
        """;

    /// <summary>
    /// 创建 OPML 导入响应.
    /// </summary>
    public static string CreateOpmlImportResponse()
        => """
        {
            "message": "Feeds imported successfully"
        }
        """;

    /// <summary>
    /// 创建测试订阅源对象.
    /// </summary>
    public static RssFeed CreateTestFeed(string id = "1", string name = "Test Feed", string url = "https://example.com/feed.xml")
    {
        var feed = new RssFeed
        {
            Id = id,
            Name = name,
            Url = url,
            Website = "https://example.com",
        };
        feed.SetGroupIdList(["1"]);
        return feed;
    }

    /// <summary>
    /// 创建测试分组对象.
    /// </summary>
    public static RssFeedGroup CreateTestGroup(string id = "1", string name = "Test Group")
        => new()
        {
            Id = id,
            Name = name,
        };
}
