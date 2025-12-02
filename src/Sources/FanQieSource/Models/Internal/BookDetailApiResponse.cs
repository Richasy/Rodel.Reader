// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

/// <summary>
/// 书籍详情 API 响应.
/// </summary>
internal sealed class BookDetailApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public IReadOnlyList<BookDetailData>? Data { get; set; }
}

internal sealed class BookDetailData
{
    [JsonPropertyName("book_id")]
    public string? BookId { get; set; }

    [JsonPropertyName("book_name")]
    public string? BookName { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("author_id")]
    public string? AuthorId { get; set; }

    [JsonPropertyName("abstract")]
    public string? Abstract { get; set; }

    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("word_number")]
    public string? WordNumber { get; set; }

    [JsonPropertyName("creation_status")]
    public string? CreationStatus { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("last_publish_time")]
    public string? LastPublishTime { get; set; }

    [JsonPropertyName("create_time")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("score")]
    public string? Score { get; set; }

    /// <summary>
    /// 标签（可能是空字符串或逗号分隔的字符串）.
    /// </summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    [JsonPropertyName("category_v2")]
    public string? CategoryV2 { get; set; }

    [JsonPropertyName("serial_count")]
    public string? SerialCountString { get; set; }
}

/// <summary>
/// 分类信息（用于解析 category_v2 JSON 字符串）.
/// </summary>
internal sealed class CategoryV2Item
{
    [JsonPropertyName("ObjectId")]
    public int ObjectId { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Gender")]
    public int Gender { get; set; }
}
