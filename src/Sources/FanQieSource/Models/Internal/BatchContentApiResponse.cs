// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

/// <summary>
/// 批量章节内容 API 响应.
/// </summary>
internal sealed class BatchContentApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public IReadOnlyList<ChapterFullData>? Data { get; set; }
}

internal sealed class ChapterFullData
{
    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("word_number")]
    public int WordNumber { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("is_encrypted")]
    public int IsEncrypted { get; set; }

    [JsonPropertyName("images")]
    public IReadOnlyList<ChapterImageData>? Images { get; set; }

    [JsonPropertyName("first_pass_time")]
    public string? FirstPassTime { get; set; }
}

internal sealed class ChapterImageData
{
    [JsonPropertyName("origin_uri")]
    public string? OriginUri { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }
}
