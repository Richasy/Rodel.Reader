// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Miniflux.Internal;

/// <summary>
/// 创建订阅源请求.
/// </summary>
internal sealed class MinifluxCreateFeedRequest
{
    /// <summary>
    /// Feed URL.
    /// </summary>
    [JsonPropertyName("feed_url")]
    public string FeedUrl { get; set; } = string.Empty;

    /// <summary>
    /// 分类 ID（可选，从 2.0.49 开始）.
    /// </summary>
    [JsonPropertyName("category_id")]
    public long? CategoryId { get; set; }

    /// <summary>
    /// Feed 用户名.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Feed 密码.
    /// </summary>
    [JsonPropertyName("password")]
    public string? Password { get; set; }

    /// <summary>
    /// 是否启用爬虫.
    /// </summary>
    [JsonPropertyName("crawler")]
    public bool? Crawler { get; set; }

    /// <summary>
    /// 自定义 User-Agent.
    /// </summary>
    [JsonPropertyName("user_agent")]
    public string? UserAgent { get; set; }
}

/// <summary>
/// 创建订阅源响应.
/// </summary>
internal sealed class MinifluxCreateFeedResponse
{
    /// <summary>
    /// 新创建的 Feed ID.
    /// </summary>
    [JsonPropertyName("feed_id")]
    public long FeedId { get; set; }
}

/// <summary>
/// 更新订阅源请求.
/// </summary>
internal sealed class MinifluxUpdateFeedRequest
{
    /// <summary>
    /// 新标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 新分类 ID.
    /// </summary>
    [JsonPropertyName("category_id")]
    public long? CategoryId { get; set; }

    /// <summary>
    /// Feed URL.
    /// </summary>
    [JsonPropertyName("feed_url")]
    public string? FeedUrl { get; set; }

    /// <summary>
    /// 网站 URL.
    /// </summary>
    [JsonPropertyName("site_url")]
    public string? SiteUrl { get; set; }

    /// <summary>
    /// 是否启用爬虫.
    /// </summary>
    [JsonPropertyName("crawler")]
    public bool? Crawler { get; set; }

    /// <summary>
    /// 是否禁用.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }
}

/// <summary>
/// 创建分类请求.
/// </summary>
internal sealed class MinifluxCreateCategoryRequest
{
    /// <summary>
    /// 分类标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 是否全局隐藏.
    /// </summary>
    [JsonPropertyName("hide_globally")]
    public bool? HideGlobally { get; set; }
}

/// <summary>
/// 更新分类请求.
/// </summary>
internal sealed class MinifluxUpdateCategoryRequest
{
    /// <summary>
    /// 新标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 是否全局隐藏.
    /// </summary>
    [JsonPropertyName("hide_globally")]
    public bool? HideGlobally { get; set; }
}

/// <summary>
/// 更新文章状态请求.
/// </summary>
internal sealed class MinifluxUpdateEntriesRequest
{
    /// <summary>
    /// 文章 ID 列表.
    /// </summary>
    [JsonPropertyName("entry_ids")]
    public List<long> EntryIds { get; set; } = [];

    /// <summary>
    /// 新状态（read, unread）.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "read";
}

/// <summary>
/// 发现订阅请求.
/// </summary>
internal sealed class MinifluxDiscoverRequest
{
    /// <summary>
    /// 网站 URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Feed 用户名.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Feed 密码.
    /// </summary>
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

/// <summary>
/// 发现的订阅源.
/// </summary>
internal sealed class MinifluxDiscoveredFeed
{
    /// <summary>
    /// Feed URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Feed 标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Feed 类型（atom, rss）.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

/// <summary>
/// 错误响应.
/// </summary>
internal sealed class MinifluxErrorResponse
{
    /// <summary>
    /// 错误消息.
    /// </summary>
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 导入响应.
/// </summary>
internal sealed class MinifluxImportResponse
{
    /// <summary>
    /// 消息.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
