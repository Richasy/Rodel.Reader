// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

/// <summary>
/// 搜索 API 响应.
/// </summary>
internal sealed class SearchApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public SearchApiData? Data { get; set; }
}

internal sealed class SearchApiData
{
    [JsonPropertyName("ret_data")]
    public IReadOnlyList<SearchBookItem>? RetData { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("search_id")]
    public string? SearchId { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }
}

internal sealed class SearchBookItem
{
    [JsonPropertyName("book_id")]
    public string? BookId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("abstract")]
    public string? Abstract { get; set; }

    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("creation_status")]
    public string? CreationStatus { get; set; }

    [JsonPropertyName("score")]
    public string? Score { get; set; }
}
