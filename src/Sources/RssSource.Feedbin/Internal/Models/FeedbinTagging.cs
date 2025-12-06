// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.Rss.Feedbin.Internal;

/// <summary>
/// Feedbin 标签关联（用于分组）.
/// </summary>
internal sealed class FeedbinTagging
{
    /// <summary>
    /// 标签关联 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Feed ID.
    /// </summary>
    [JsonPropertyName("feed_id")]
    public int FeedId { get; set; }

    /// <summary>
    /// 标签名称（即分组名称）.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
