// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

/// <summary>
/// 外部内容 API 响应（单个章节）.
/// </summary>
internal sealed class ExternalContentApiResponse
{
    /// <summary>
    /// 响应码.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 耗时（毫秒）.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("elapsed_ms")]
    public int ElapsedMs { get; set; }

    /// <summary>
    /// 响应数据.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public ExternalContentData? Data { get; set; }
}

/// <summary>
/// 外部内容数据.
/// </summary>
internal sealed class ExternalContentData
{
    /// <summary>
    /// 章节内容（纯文本）.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("content")]
    public string? Content { get; set; }
}

/// <summary>
/// 外部批量内容 API 响应.
/// </summary>
internal sealed class ExternalBatchContentApiResponse
{
    /// <summary>
    /// 响应码.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 耗时（毫秒）.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("elapsed_ms")]
    public int ElapsedMs { get; set; }

    /// <summary>
    /// 响应数据.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public ExternalBatchContentData? Data { get; set; }
}

/// <summary>
/// 外部批量内容数据.
/// </summary>
internal sealed class ExternalBatchContentData
{
    /// <summary>
    /// 章节内容列表.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("chapters")]
    public List<ExternalChapterItem>? Chapters { get; set; }
}

/// <summary>
/// 外部章节项.
/// </summary>
internal sealed class ExternalChapterItem
{
    /// <summary>
    /// 章节标题.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 章节内容.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// 响应码.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 段落数.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("paragraphs_num")]
    public int ParagraphsNum { get; set; }
}
