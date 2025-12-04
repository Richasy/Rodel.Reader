// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

#region 远程配置

/// <summary>
/// 远程配置响应.
/// </summary>
internal sealed class ExternalRemoteConfig
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("update_time")]
    public string? UpdateTime { get; set; }

    [JsonPropertyName("config")]
    public ExternalApiConfig? Config { get; set; }
}

/// <summary>
/// 外部 API 配置.
/// </summary>
internal sealed class ExternalApiConfig
{
    [JsonPropertyName("api_base_url")]
    public string? ApiBaseUrl { get; set; }

    [JsonPropertyName("tomato_api_base")]
    public string? TomatoApiBase { get; set; }

    [JsonPropertyName("max_workers")]
    public int MaxWorkers { get; set; }

    [JsonPropertyName("max_retries")]
    public int MaxRetries { get; set; }

    [JsonPropertyName("request_timeout")]
    public int RequestTimeout { get; set; }

    [JsonPropertyName("request_rate_limit")]
    public double RequestRateLimit { get; set; }

    [JsonPropertyName("connection_pool_size")]
    public int ConnectionPoolSize { get; set; }

    [JsonPropertyName("api_rate_limit")]
    public int ApiRateLimit { get; set; }

    [JsonPropertyName("rate_limit_window")]
    public double RateLimitWindow { get; set; }

    [JsonPropertyName("async_batch_size")]
    public int AsyncBatchSize { get; set; }

    [JsonPropertyName("download_enabled")]
    public bool DownloadEnabled { get; set; }

    [JsonPropertyName("verbose_logging")]
    public bool VerboseLogging { get; set; }

    [JsonPropertyName("tomato_endpoints")]
    public ExternalApiEndpoints? TomatoEndpoints { get; set; }
}

/// <summary>
/// 外部 API 端点配置.
/// </summary>
internal sealed class ExternalApiEndpoints
{
    [JsonPropertyName("search")]
    public string? Search { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("book")]
    public string? Book { get; set; }

    [JsonPropertyName("directory")]
    public string? Directory { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("chapter")]
    public string? Chapter { get; set; }

    [JsonPropertyName("raw_full")]
    public string? RawFull { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("multi_content")]
    public string? MultiContent { get; set; }

    [JsonPropertyName("ios_content")]
    public string? IosContent { get; set; }

    [JsonPropertyName("ios_register")]
    public string? IosRegister { get; set; }

    [JsonPropertyName("device_pool")]
    public string? DevicePool { get; set; }

    [JsonPropertyName("device_register")]
    public string? DeviceRegister { get; set; }

    [JsonPropertyName("device_status")]
    public string? DeviceStatus { get; set; }
}

#endregion

#region 外部搜索 API

/// <summary>
/// 外部搜索 API 响应.
/// </summary>
internal sealed class ExternalSearchApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ExternalSearchData? Data { get; set; }

    [JsonPropertyName("elapsed_ms")]
    public int ElapsedMs { get; set; }
}

/// <summary>
/// 外部搜索数据.
/// </summary>
internal sealed class ExternalSearchData
{
    [JsonPropertyName("data")]
    public IReadOnlyList<ExternalSearchBookItem>? Books { get; set; }

    [JsonPropertyName("search_id")]
    public string? SearchId { get; set; }

    [JsonPropertyName("has_more")]
    public int HasMore { get; set; }
}

/// <summary>
/// 外部搜索书籍项.
/// </summary>
internal sealed class ExternalSearchBookItem
{
    [JsonPropertyName("book_id")]
    public string? BookId { get; set; }

    [JsonPropertyName("book_name")]
    public string? BookName { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("abstract")]
    public string? Abstract { get; set; }

    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("word_number")]
    public string? WordNumber { get; set; }

    /// <summary>
    /// 连载状态：0=连载中，1=已完结.
    /// </summary>
    [JsonPropertyName("creation_status")]
    public string? CreationStatus { get; set; }

    [JsonPropertyName("score")]
    public string? Score { get; set; }

    [JsonPropertyName("last_chapter_update_time")]
    public string? LastChapterUpdateTime { get; set; }

    [JsonPropertyName("last_chapter_title")]
    public string? LastChapterTitle { get; set; }
}

#endregion

#region 外部书籍详情 API

/// <summary>
/// 外部书籍详情 API 响应.
/// </summary>
internal sealed class ExternalBookDetailApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ExternalBookDetailWrapper? Data { get; set; }

    [JsonPropertyName("elapsed_ms")]
    public int ElapsedMs { get; set; }
}

/// <summary>
/// 外部书籍详情包装器（嵌套 data）.
/// </summary>
internal sealed class ExternalBookDetailWrapper
{
    [JsonPropertyName("data")]
    public ExternalBookDetailData? Data { get; set; }
}

/// <summary>
/// 外部书籍详情数据.
/// </summary>
internal sealed class ExternalBookDetailData
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

    /// <summary>
    /// 连载状态：0=连载中，1=已完结.
    /// </summary>
    [JsonPropertyName("creation_status")]
    public string? CreationStatus { get; set; }

    [JsonPropertyName("word_number")]
    public string? WordNumber { get; set; }

    [JsonPropertyName("serial_count")]
    public string? SerialCount { get; set; }

    [JsonPropertyName("score")]
    public string? Score { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("last_publish_time")]
    public string? LastPublishTime { get; set; }

    [JsonPropertyName("create_time")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("category_v2")]
    public string? CategoryV2 { get; set; }

    [JsonPropertyName("tags")]
    public string? Tags { get; set; }
}

#endregion

#region 外部书籍目录 API

/// <summary>
/// 外部书籍目录 API 响应.
/// </summary>
internal sealed class ExternalBookTocApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ExternalBookTocWrapper? Data { get; set; }

    [JsonPropertyName("elapsed_ms")]
    public int ElapsedMs { get; set; }
}

/// <summary>
/// 外部书籍目录包装器（嵌套 data）.
/// </summary>
internal sealed class ExternalBookTocWrapper
{
    [JsonPropertyName("data")]
    public ExternalBookTocData? Data { get; set; }
}

/// <summary>
/// 外部书籍目录数据.
/// </summary>
internal sealed class ExternalBookTocData
{
    [JsonPropertyName("allItemIds")]
    public IReadOnlyList<string>? AllItemIds { get; set; }

    [JsonPropertyName("chapterListWithVolume")]
    public IReadOnlyList<IReadOnlyList<ExternalTocChapterItem>>? ChapterListWithVolume { get; set; }

    [JsonPropertyName("volumeNameList")]
    public IReadOnlyList<string>? VolumeNameList { get; set; }
}

/// <summary>
/// 外部目录章节项.
/// </summary>
internal sealed class ExternalTocChapterItem
{
    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemIdAlt { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("firstPassTime")]
    public string? FirstPassTime { get; set; }

    [JsonPropertyName("first_pass_time")]
    public string? FirstPassTimeAlt { get; set; }

    /// <summary>
    /// 获取实际的 ItemId.
    /// </summary>
    public string? GetItemId() => ItemId ?? ItemIdAlt;

    /// <summary>
    /// 获取实际的 FirstPassTime.
    /// </summary>
    public string? GetFirstPassTime() => FirstPassTime ?? FirstPassTimeAlt;
}

#endregion

#region 外部章节内容 API

/// <summary>
/// 外部单章节内容 API 响应（tab=小说）.
/// </summary>
internal sealed class ExternalChapterContentApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ExternalChapterContentData? Data { get; set; }

    [JsonPropertyName("elapsed_ms")]
    public int ElapsedMs { get; set; }
}

/// <summary>
/// 外部单章节内容数据.
/// </summary>
internal sealed class ExternalChapterContentData
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

/// <summary>
/// 外部批量章节内容 API 响应（tab=批量）.
/// </summary>
internal sealed class ExternalBatchContentApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ExternalBatchContentData? Data { get; set; }

    [JsonPropertyName("elapsed_ms")]
    public int ElapsedMs { get; set; }
}

/// <summary>
/// 外部批量章节内容数据.
/// </summary>
internal sealed class ExternalBatchContentData
{
    [JsonPropertyName("chapters")]
    public IReadOnlyList<ExternalBatchChapterItem>? Chapters { get; set; }
}

/// <summary>
/// 外部批量章节项.
/// </summary>
internal sealed class ExternalBatchChapterItem
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("author_speak")]
    public string? AuthorSpeak { get; set; }

    [JsonPropertyName("paragraphs_num")]
    public int ParagraphsNum { get; set; }

    [JsonPropertyName("novel_data")]
    public ExternalBatchNovelData? NovelData { get; set; }
}

/// <summary>
/// 外部批量章节的小说数据（包含章节元信息）.
/// </summary>
internal sealed class ExternalBatchNovelData
{
    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("chapter_word_number")]
    public string? ChapterWordNumber { get; set; }

    [JsonPropertyName("real_chapter_order")]
    public string? RealChapterOrder { get; set; }

    [JsonPropertyName("volume_name")]
    public string? VolumeName { get; set; }

    [JsonPropertyName("first_pass_time")]
    public string? FirstPassTime { get; set; }

    [JsonPropertyName("book_id")]
    public string? BookId { get; set; }

    [JsonPropertyName("book_name")]
    public string? BookName { get; set; }
}

#endregion

#region 外部整书下载 API

/// <summary>
/// 外部整书下载 API 响应（tab=下载）.
/// </summary>
internal sealed class ExternalFullBookApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ExternalFullBookData? Data { get; set; }

    [JsonPropertyName("elapsed_ms")]
    public int ElapsedMs { get; set; }
}

/// <summary>
/// 外部整书下载数据.
/// </summary>
internal sealed class ExternalFullBookData
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

#endregion
