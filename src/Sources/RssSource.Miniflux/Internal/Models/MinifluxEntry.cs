// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Miniflux.Internal;

/// <summary>
/// Miniflux 文章条目.
/// </summary>
internal sealed class MinifluxEntry
{
    /// <summary>
    /// Entry ID.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 用户 ID.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    /// <summary>
    /// Feed ID.
    /// </summary>
    [JsonPropertyName("feed_id")]
    public long FeedId { get; set; }

    /// <summary>
    /// 文章标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 文章 URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 评论 URL.
    /// </summary>
    [JsonPropertyName("comments_url")]
    public string? CommentsUrl { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// 文章内容.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// 内容 Hash.
    /// </summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    [JsonPropertyName("published_at")]
    public DateTimeOffset PublishedAt { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [JsonPropertyName("changed_at")]
    public DateTimeOffset? ChangedAt { get; set; }

    /// <summary>
    /// 阅读状态（read, unread, removed）.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "unread";

    /// <summary>
    /// 分享码.
    /// </summary>
    [JsonPropertyName("share_code")]
    public string? ShareCode { get; set; }

    /// <summary>
    /// 是否已收藏.
    /// </summary>
    [JsonPropertyName("starred")]
    public bool Starred { get; set; }

    /// <summary>
    /// 预估阅读时间（分钟）.
    /// </summary>
    [JsonPropertyName("reading_time")]
    public int ReadingTime { get; set; }

    /// <summary>
    /// 附件列表.
    /// </summary>
    [JsonPropertyName("enclosures")]
    public List<MinifluxEnclosure>? Enclosures { get; set; }

    /// <summary>
    /// 所属 Feed 信息.
    /// </summary>
    [JsonPropertyName("feed")]
    public MinifluxFeed? Feed { get; set; }

    /// <summary>
    /// 标签列表.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Miniflux 文章列表响应.
/// </summary>
internal sealed class MinifluxEntriesResponse
{
    /// <summary>
    /// 总数.
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// 文章列表.
    /// </summary>
    [JsonPropertyName("entries")]
    public List<MinifluxEntry> Entries { get; set; } = [];
}

/// <summary>
/// Miniflux 附件.
/// </summary>
internal sealed class MinifluxEnclosure
{
    /// <summary>
    /// 附件 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 用户 ID.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    /// <summary>
    /// 文章 ID.
    /// </summary>
    [JsonPropertyName("entry_id")]
    public long EntryId { get; set; }

    /// <summary>
    /// 附件 URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// MIME 类型.
    /// </summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>
    /// 文件大小.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// 媒体播放进度.
    /// </summary>
    [JsonPropertyName("media_progression")]
    public int MediaProgression { get; set; }
}
