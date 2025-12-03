// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

#region 搜索 API

/// <summary>
/// 后备搜索 API 响应.
/// </summary>
internal sealed class FallbackSearchApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public FallbackSearchData? Data { get; set; }
}

/// <summary>
/// 后备搜索数据.
/// </summary>
internal sealed class FallbackSearchData
{
    [JsonPropertyName("books")]
    public IReadOnlyList<FallbackSearchBookItem>? Books { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }

    [JsonPropertyName("searchId")]
    public string? SearchId { get; set; }
}

/// <summary>
/// 后备搜索书籍项.
/// </summary>
internal sealed class FallbackSearchBookItem
{
    [JsonPropertyName("bookId")]
    public string? BookId { get; set; }

    [JsonPropertyName("bookName")]
    public string? BookName { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("coverUrl")]
    public string? CoverUrl { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; set; }

    [JsonPropertyName("wordCount")]
    public int WordCount { get; set; }

    /// <summary>
    /// 连载状态：0=连载中，1=已完结.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [JsonPropertyName("updateTime")]
    public long UpdateTime { get; set; }

    [JsonPropertyName("lastChapterTitle")]
    public string? LastChapterTitle { get; set; }
}

#endregion

#region 书籍详情 API

/// <summary>
/// 后备书籍详情 API 响应.
/// </summary>
internal sealed class FallbackBookDetailApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public FallbackBookDetailData? Data { get; set; }
}

/// <summary>
/// 后备书籍详情数据.
/// </summary>
internal sealed class FallbackBookDetailData
{
    [JsonPropertyName("bookId")]
    public string? BookId { get; set; }

    [JsonPropertyName("bookName")]
    public string? BookName { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("coverUrl")]
    public string? CoverUrl { get; set; }

    /// <summary>
    /// 连载状态：0=连载中，1=已完结.
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("totalChapters")]
    public int TotalChapters { get; set; }

    [JsonPropertyName("wordNumber")]
    public string? WordNumber { get; set; }

    /// <summary>
    /// 逗号分隔的标签字符串.
    /// </summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    [JsonPropertyName("lastChapterTitle")]
    public string? LastChapterTitle { get; set; }
}

#endregion

#region 书籍目录 API

/// <summary>
/// 后备书籍目录 API 响应.
/// </summary>
internal sealed class FallbackBookTocApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public FallbackBookTocData? Data { get; set; }
}

/// <summary>
/// 后备书籍目录数据.
/// </summary>
internal sealed class FallbackBookTocData
{
    [JsonPropertyName("item_data_list")]
    public IReadOnlyList<FallbackTocChapterItem>? ItemDataList { get; set; }

    [JsonPropertyName("book_info")]
    public FallbackTocBookInfo? BookInfo { get; set; }
}

/// <summary>
/// 后备目录章节项.
/// </summary>
internal sealed class FallbackTocChapterItem
{
    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("volume_name")]
    public string? VolumeName { get; set; }

    [JsonPropertyName("first_pass_time")]
    public long FirstPassTime { get; set; }

    [JsonPropertyName("chapter_word_number")]
    public int ChapterWordNumber { get; set; }
}

/// <summary>
/// 后备目录书籍信息.
/// </summary>
internal sealed class FallbackTocBookInfo
{
    [JsonPropertyName("book_id")]
    public string? BookId { get; set; }

    [JsonPropertyName("book_name")]
    public string? BookName { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("score")]
    public string? Score { get; set; }
}

#endregion

#region 批量章节内容 API

/// <summary>
/// 后备批量章节内容 API 响应.
/// </summary>
internal sealed class FallbackBatchContentApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public FallbackBatchContentData? Data { get; set; }
}

/// <summary>
/// 后备批量章节内容数据.
/// </summary>
internal sealed class FallbackBatchContentData
{
    [JsonPropertyName("bookId")]
    public string? BookId { get; set; }

    [JsonPropertyName("bookInfo")]
    public FallbackBatchBookInfo? BookInfo { get; set; }

    /// <summary>
    /// 章节内容字典，键为章节ID.
    /// </summary>
    [JsonPropertyName("chapters")]
    public Dictionary<string, FallbackChapterContent>? Chapters { get; set; }

    [JsonPropertyName("successCount")]
    public int SuccessCount { get; set; }

    [JsonPropertyName("totalRequested")]
    public int TotalRequested { get; set; }
}

/// <summary>
/// 后备批量书籍信息.
/// </summary>
internal sealed class FallbackBatchBookInfo
{
    [JsonPropertyName("bookId")]
    public string? BookId { get; set; }

    [JsonPropertyName("bookName")]
    public string? BookName { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("coverUrl")]
    public string? CoverUrl { get; set; }
}

/// <summary>
/// 后备章节内容.
/// </summary>
internal sealed class FallbackChapterContent
{
    [JsonPropertyName("chapterName")]
    public string? ChapterName { get; set; }

    /// <summary>
    /// 原始 HTML 内容.
    /// </summary>
    [JsonPropertyName("rawContent")]
    public string? RawContent { get; set; }

    /// <summary>
    /// 纯文本内容.
    /// </summary>
    [JsonPropertyName("txtContent")]
    public string? TxtContent { get; set; }

    [JsonPropertyName("wordCount")]
    public int WordCount { get; set; }

    [JsonPropertyName("isFree")]
    public bool IsFree { get; set; }
}

/// <summary>
/// 后备批量章节请求体.
/// </summary>
internal sealed class FallbackBatchContentRequest
{
    [JsonPropertyName("bookId")]
    public string? BookId { get; set; }

    [JsonPropertyName("chapterIds")]
    public IEnumerable<string>? ChapterIds { get; set; }
}

#endregion
