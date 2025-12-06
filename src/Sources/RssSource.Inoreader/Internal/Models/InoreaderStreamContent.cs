// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Inoreader.Internal;

/// <summary>
/// 文章流响应.
/// </summary>
internal sealed class InoreaderStreamContentResponse
{
    /// <summary>
    /// 流 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 更新时间戳.
    /// </summary>
    [JsonPropertyName("updated")]
    public long Updated { get; set; }

    /// <summary>
    /// 文章列表.
    /// </summary>
    [JsonPropertyName("items")]
    public List<InoreaderArticleItem> Items { get; set; } = [];

    /// <summary>
    /// 继续加载的标识.
    /// </summary>
    [JsonPropertyName("continuation")]
    public string? Continuation { get; set; }
}

/// <summary>
/// 文章项.
/// </summary>
internal sealed class InoreaderArticleItem
{
    /// <summary>
    /// 文章 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 抓取时间（毫秒）.
    /// </summary>
    [JsonPropertyName("crawlTimeMsec")]
    public string? CrawlTimeMsec { get; set; }

    /// <summary>
    /// 时间戳（微秒）.
    /// </summary>
    [JsonPropertyName("timestampUsec")]
    public string? TimestampUsec { get; set; }

    /// <summary>
    /// 发布时间戳（秒）.
    /// </summary>
    [JsonPropertyName("published")]
    public long Published { get; set; }

    /// <summary>
    /// 文章标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 规范链接.
    /// </summary>
    [JsonPropertyName("canonical")]
    public List<InoreaderLink>? Canonical { get; set; }

    /// <summary>
    /// 备用链接.
    /// </summary>
    [JsonPropertyName("alternate")]
    public List<InoreaderLink>? Alternate { get; set; }

    /// <summary>
    /// 分类/标签.
    /// </summary>
    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    /// <summary>
    /// 来源信息.
    /// </summary>
    [JsonPropertyName("origin")]
    public InoreaderOrigin? Origin { get; set; }

    /// <summary>
    /// 摘要.
    /// </summary>
    [JsonPropertyName("summary")]
    public InoreaderContent? Summary { get; set; }

    /// <summary>
    /// 正文内容.
    /// </summary>
    [JsonPropertyName("content")]
    public InoreaderContent? Content { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }
}

/// <summary>
/// 链接.
/// </summary>
internal sealed class InoreaderLink
{
    /// <summary>
    /// 链接地址.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }
}

/// <summary>
/// 来源信息.
/// </summary>
internal sealed class InoreaderOrigin
{
    /// <summary>
    /// 流 ID.
    /// </summary>
    [JsonPropertyName("streamId")]
    public string? StreamId { get; set; }

    /// <summary>
    /// 网站 URL.
    /// </summary>
    [JsonPropertyName("htmlUrl")]
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

/// <summary>
/// 内容.
/// </summary>
internal sealed class InoreaderContent
{
    /// <summary>
    /// 内容文本.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
