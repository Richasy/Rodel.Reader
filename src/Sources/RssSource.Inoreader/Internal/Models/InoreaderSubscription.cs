// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Inoreader.Internal;

/// <summary>
/// 订阅列表响应.
/// </summary>
internal sealed class InoreaderSubscriptionResponse
{
    /// <summary>
    /// 订阅列表.
    /// </summary>
    [JsonPropertyName("subscriptions")]
    public List<InoreaderSubscription> Subscriptions { get; set; } = [];
}

/// <summary>
/// 订阅项.
/// </summary>
internal sealed class InoreaderSubscription
{
    /// <summary>
    /// 订阅 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 订阅标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 订阅分类.
    /// </summary>
    [JsonPropertyName("categories")]
    public List<InoreaderCategory>? Categories { get; set; }

    /// <summary>
    /// 订阅源 URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 网站 URL.
    /// </summary>
    [JsonPropertyName("htmlUrl")]
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// 图标 URL.
    /// </summary>
    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// 排序 ID.
    /// </summary>
    [JsonPropertyName("sortid")]
    public string? SortId { get; set; }
}

/// <summary>
/// 分类/分组.
/// </summary>
internal sealed class InoreaderCategory
{
    /// <summary>
    /// 分类 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 分类标签.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
