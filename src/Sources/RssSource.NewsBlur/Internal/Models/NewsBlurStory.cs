// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.NewsBlur.Internal;

/// <summary>
/// NewsBlur 文章/故事.
/// </summary>
internal sealed class NewsBlurStory
{
    /// <summary>
    /// 故事哈希（唯一标识）.
    /// </summary>
    [JsonPropertyName("story_hash")]
    public string StoryHash { get; set; } = string.Empty;

    /// <summary>
    /// 故事标题.
    /// </summary>
    [JsonPropertyName("story_title")]
    public string StoryTitle { get; set; } = string.Empty;

    /// <summary>
    /// 故事内容（HTML）.
    /// </summary>
    [JsonPropertyName("story_content")]
    public string? StoryContent { get; set; }

    /// <summary>
    /// 故事链接.
    /// </summary>
    [JsonPropertyName("story_permalink")]
    public string? StoryPermalink { get; set; }

    /// <summary>
    /// 故事日期.
    /// </summary>
    [JsonPropertyName("story_date")]
    public string? StoryDate { get; set; }

    /// <summary>
    /// 故事时间戳.
    /// </summary>
    [JsonPropertyName("story_timestamp")]
    public string? StoryTimestamp { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    [JsonPropertyName("story_authors")]
    public string? StoryAuthors { get; set; }

    /// <summary>
    /// 订阅源 ID.
    /// </summary>
    [JsonPropertyName("story_feed_id")]
    public long StoryFeedId { get; set; }

    /// <summary>
    /// 图片 URL 列表.
    /// </summary>
    [JsonPropertyName("image_urls")]
    public List<string>? ImageUrls { get; set; }

    /// <summary>
    /// 阅读状态（0=未读, 1=已读）.
    /// </summary>
    [JsonPropertyName("read_status")]
    public int ReadStatus { get; set; }

    /// <summary>
    /// 评分.
    /// </summary>
    [JsonPropertyName("score")]
    public int Score { get; set; }

    /// <summary>
    /// 故事 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 故事标签.
    /// </summary>
    [JsonPropertyName("story_tags")]
    public List<string>? StoryTags { get; set; }
}
