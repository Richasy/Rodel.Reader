// Copyright (c) Richasy. All rights reserved.

using RichardSzalay.MockHttp;

namespace Richasy.RodelReader.Sources.Podcast.Apple.Test;

/// <summary>
/// 测试数据工厂.
/// </summary>
internal static class TestDataFactory
{
    public const string BaseAddress = "https://itunes.apple.com";

    /// <summary>
    /// 热门播客响应（简化版）.
    /// </summary>
    public static string TopPodcastsResponse => """
        {
            "feed": {
                "entry": [
                    {
                        "im:name": { "label": "Test Podcast 1" },
                        "im:image": [
                            { "label": "https://example.com/cover-small.jpg", "attributes": { "height": "55" } },
                            { "label": "https://example.com/cover-large.jpg", "attributes": { "height": "170" } }
                        ],
                        "summary": { "label": "A great podcast about testing" },
                        "id": { "label": "https://podcasts.apple.com/podcast/id12345", "attributes": { "im:id": "12345" } },
                        "im:artist": { "label": "Test Artist" }
                    },
                    {
                        "im:name": { "label": "Test Podcast 2" },
                        "im:image": [
                            { "label": "https://example.com/cover2.jpg", "attributes": { "height": "170" } }
                        ],
                        "id": { "label": "https://podcasts.apple.com/podcast/id67890", "attributes": { "im:id": "67890" } },
                        "im:artist": { "label": "Another Artist" }
                    }
                ]
            }
        }
        """;

    /// <summary>
    /// 搜索响应.
    /// </summary>
    public static string SearchResponse => """
        {
            "resultCount": 2,
            "results": [
                {
                    "wrapperType": "track",
                    "kind": "podcast",
                    "trackId": 111111,
                    "trackName": "Search Result 1",
                    "artistName": "Artist One",
                    "feedUrl": "https://example.com/feed1.xml",
                    "trackViewUrl": "https://podcasts.apple.com/podcast/id111111",
                    "artworkUrl600": "https://example.com/artwork1.jpg"
                },
                {
                    "wrapperType": "track",
                    "kind": "podcast",
                    "trackId": 222222,
                    "trackName": "Search Result 2",
                    "artistName": "Artist Two",
                    "feedUrl": "https://example.com/feed2.xml",
                    "trackViewUrl": "https://podcasts.apple.com/podcast/id222222",
                    "artworkUrl100": "https://example.com/artwork2-100.jpg"
                }
            ]
        }
        """;

    /// <summary>
    /// 查询响应.
    /// </summary>
    public static string LookupResponse => """
        {
            "resultCount": 1,
            "results": [
                {
                    "wrapperType": "track",
                    "kind": "podcast",
                    "trackId": 12345,
                    "trackName": "Lookup Podcast",
                    "artistName": "Lookup Artist",
                    "feedUrl": "https://example.com/lookup-feed.xml",
                    "trackViewUrl": "https://podcasts.apple.com/podcast/id12345",
                    "artworkUrl600": "https://example.com/lookup-artwork.jpg"
                }
            ]
        }
        """;

    /// <summary>
    /// 简单的 RSS Feed.
    /// </summary>
    public static string SimpleFeed => """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0" xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd" xmlns:content="http://purl.org/rss/1.0/modules/content/">
            <channel>
                <title>Test Podcast</title>
                <link>https://example.com</link>
                <description>A test podcast for unit testing</description>
                <itunes:author>Test Author</itunes:author>
                <itunes:image href="https://example.com/cover.jpg"/>
                <itunes:category text="Technology"/>
                <item>
                    <title>Episode 1</title>
                    <guid>ep-001</guid>
                    <description>First episode description</description>
                    <enclosure url="https://example.com/ep1.mp3" type="audio/mpeg" length="12345678"/>
                    <itunes:duration>1:30:45</itunes:duration>
                    <pubDate>Mon, 01 Jan 2024 12:00:00 GMT</pubDate>
                    <itunes:season>1</itunes:season>
                    <itunes:episode>1</itunes:episode>
                    <itunes:episodeType>full</itunes:episodeType>
                </item>
                <item>
                    <title>Episode 2</title>
                    <guid>ep-002</guid>
                    <description>Second episode</description>
                    <enclosure url="https://example.com/ep2.mp3" type="audio/mpeg" length="9876543"/>
                    <itunes:duration>45:30</itunes:duration>
                    <pubDate>Mon, 08 Jan 2024 12:00:00 GMT</pubDate>
                </item>
            </channel>
        </rss>
        """;

    /// <summary>
    /// 空响应.
    /// </summary>
    public static string EmptyTopPodcastsResponse => """
        {
            "feed": {
                "entry": []
            }
        }
        """;

    /// <summary>
    /// 空搜索响应.
    /// </summary>
    public static string EmptySearchResponse => """
        {
            "resultCount": 0,
            "results": []
        }
        """;

    /// <summary>
    /// 创建配置好的 MockHttpMessageHandler.
    /// </summary>
    public static MockHttpMessageHandler CreateMockHandler()
    {
        var mockHandler = new MockHttpMessageHandler();

        // 热门播客
        mockHandler.When($"{BaseAddress}/us/rss/toppodcasts/limit=100/genre=0/json")
            .Respond("application/json", TopPodcastsResponse);

        // 搜索
        mockHandler.When($"{BaseAddress}/search*")
            .Respond("application/json", SearchResponse);

        // 查询
        mockHandler.When($"{BaseAddress}/lookup*")
            .Respond("application/json", LookupResponse);

        return mockHandler;
    }
}
