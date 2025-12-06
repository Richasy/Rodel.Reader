// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Feedbin.Internal;

/// <summary>
/// 创建订阅请求.
/// </summary>
internal sealed class FeedbinCreateSubscriptionRequest
{
    /// <summary>
    /// Feed URL.
    /// </summary>
    [JsonPropertyName("feed_url")]
    public string FeedUrl { get; set; } = string.Empty;
}

/// <summary>
/// 更新订阅请求.
/// </summary>
internal sealed class FeedbinUpdateSubscriptionRequest
{
    /// <summary>
    /// 新标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// 创建标签关联请求.
/// </summary>
internal sealed class FeedbinCreateTaggingRequest
{
    /// <summary>
    /// Feed ID.
    /// </summary>
    [JsonPropertyName("feed_id")]
    public int FeedId { get; set; }

    /// <summary>
    /// 标签名称.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 未读文章请求.
/// </summary>
internal sealed class FeedbinUnreadEntriesRequest
{
    /// <summary>
    /// 文章 ID 列表.
    /// </summary>
    [JsonPropertyName("unread_entries")]
    public List<long> UnreadEntries { get; set; } = [];
}

/// <summary>
/// 导入响应.
/// </summary>
internal sealed class FeedbinImportResponse
{
    /// <summary>
    /// 导入 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 是否完成.
    /// </summary>
    [JsonPropertyName("complete")]
    public bool Complete { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 导入项列表.
    /// </summary>
    [JsonPropertyName("import_items")]
    public List<FeedbinImportItem>? ImportItems { get; set; }
}

/// <summary>
/// 导入项.
/// </summary>
internal sealed class FeedbinImportItem
{
    /// <summary>
    /// 标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Feed URL.
    /// </summary>
    [JsonPropertyName("feed_url")]
    public string? FeedUrl { get; set; }

    /// <summary>
    /// 状态: pending, complete, failed.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// 发现的多个 Feed 响应项.
/// </summary>
internal sealed class FeedbinDiscoveredFeed
{
    /// <summary>
    /// Feed URL.
    /// </summary>
    [JsonPropertyName("feed_url")]
    public string? FeedUrl { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
