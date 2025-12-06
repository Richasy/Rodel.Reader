// Copyright (c) Richasy. All rights reserved.

namespace RssSource.NewsBlur.Test;

/// <summary>
/// 测试数据工厂.
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// 测试用户名.
    /// </summary>
    public const string TestUserName = "testuser";

    /// <summary>
    /// 测试密码.
    /// </summary>
    public const string TestPassword = "testpassword";

    /// <summary>
    /// 创建默认配置选项.
    /// </summary>
    public static NewsBlurClientOptions CreateDefaultOptions()
        => new()
        {
            UserName = TestUserName,
            Password = TestPassword,
            Timeout = TimeSpan.FromSeconds(30),
            MaxConcurrentRequests = 5,
            PagesToFetch = 2,
        };

    /// <summary>
    /// 创建未认证的配置选项.
    /// </summary>
    public static NewsBlurClientOptions CreateUnauthenticatedOptions()
        => new()
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

    /// <summary>
    /// 创建登录成功响应.
    /// </summary>
    public static string CreateLoginSuccessResponse()
        => """
        {
            "authenticated": true,
            "user_id": 12345
        }
        """;

    /// <summary>
    /// 创建登录失败响应.
    /// </summary>
    public static string CreateLoginFailedResponse()
        => """
        {
            "authenticated": false,
            "errors": {
                "username": ["Invalid username or password"]
            }
        }
        """;

    /// <summary>
    /// 创建订阅源列表响应.
    /// </summary>
    public static string CreateFeedsResponse()
        => """
        {
            "authenticated": true,
            "feeds": {
                "123": {
                    "id": 123,
                    "feed_title": "Test Feed 1",
                    "feed_address": "https://example.com/feed1.xml",
                    "feed_link": "https://example.com",
                    "active": true,
                    "favicon_url": "https://example.com/favicon.ico",
                    "ps": 5,
                    "nt": 10,
                    "ng": 0
                },
                "456": {
                    "id": 456,
                    "feed_title": "Test Feed 2",
                    "feed_address": "https://example.org/feed2.xml",
                    "feed_link": "https://example.org",
                    "active": true,
                    "ps": 3,
                    "nt": 7,
                    "ng": 1
                }
            },
            "folders": [
                {
                    "Tech": [123]
                },
                {
                    "News": [456]
                }
            ]
        }
        """;

    /// <summary>
    /// 创建空订阅源列表响应.
    /// </summary>
    public static string CreateEmptyFeedsResponse()
        => """
        {
            "authenticated": true,
            "feeds": {},
            "folders": []
        }
        """;

    /// <summary>
    /// 创建故事列表响应.
    /// </summary>
    public static string CreateStoriesResponse()
        => """
        {
            "authenticated": true,
            "stories": [
                {
                    "story_hash": "123:abc",
                    "story_title": "Test Article 1",
                    "story_content": "<p>This is the content of article 1.</p>",
                    "story_permalink": "https://example.com/article1",
                    "story_date": "2024-01-01T12:00:00Z",
                    "story_timestamp": "1704110400",
                    "story_authors": "John Doe",
                    "story_feed_id": 123,
                    "image_urls": ["https://example.com/image1.jpg"],
                    "read_status": 0,
                    "score": 1,
                    "id": "article1"
                },
                {
                    "story_hash": "123:def",
                    "story_title": "Test Article 2",
                    "story_content": "<p>This is the content of article 2.</p>",
                    "story_permalink": "https://example.com/article2",
                    "story_date": "2024-01-02T12:00:00Z",
                    "story_timestamp": "1704196800",
                    "story_authors": "Jane Smith",
                    "story_feed_id": 123,
                    "read_status": 1,
                    "score": 0,
                    "id": "article2"
                }
            ]
        }
        """;

    /// <summary>
    /// 创建空故事列表响应.
    /// </summary>
    public static string CreateEmptyStoriesResponse()
        => """
        {
            "authenticated": true,
            "stories": []
        }
        """;

    /// <summary>
    /// 创建添加订阅源成功响应.
    /// </summary>
    public static string CreateAddFeedSuccessResponse()
        => """
        {
            "result": 1,
            "feed": {
                "id": 789,
                "feed_title": "New Feed",
                "feed_address": "https://newsite.com/feed.xml",
                "feed_link": "https://newsite.com",
                "active": true
            }
        }
        """;

    /// <summary>
    /// 创建添加订阅源失败响应.
    /// </summary>
    public static string CreateAddFeedFailedResponse()
        => """
        {
            "result": 0,
            "message": "Could not find RSS feed"
        }
        """;

    /// <summary>
    /// 创建操作成功响应.
    /// </summary>
    public static string CreateOperationSuccessResponse()
        => """
        {
            "result": "ok",
            "authenticated": true
        }
        """;

    /// <summary>
    /// 创建 OPML 导出内容.
    /// </summary>
    public static string CreateOpmlExportContent()
        => """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="1.0">
            <head>
                <title>NewsBlur Subscriptions</title>
            </head>
            <body>
                <outline text="Tech" title="Tech">
                    <outline text="Test Feed 1" title="Test Feed 1" type="rss" xmlUrl="https://example.com/feed1.xml" htmlUrl="https://example.com"/>
                </outline>
            </body>
        </opml>
        """;

    /// <summary>
    /// 创建测试订阅源.
    /// </summary>
    public static RssFeed CreateTestFeed(string id = "123", string name = "Test Feed")
        => new()
        {
            Id = id,
            Name = name,
            Url = $"https://example.com/feed{id}.xml",
            Website = "https://example.com",
        };

    /// <summary>
    /// 创建测试分组.
    /// </summary>
    public static RssFeedGroup CreateTestGroup(string id = "TestGroup", string name = "Test Group")
        => new()
        {
            Id = id,
            Name = name,
        };
}
