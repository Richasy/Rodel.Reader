// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models.Internal;

/// <summary>
/// 书单 API 响应.
/// </summary>
internal sealed class BooklistApiResponse
{
    /// <summary>
    /// 获取或设置书籍列表.
    /// </summary>
    [JsonPropertyName("books")]
    public List<BooklistBookWrapper>? Books { get; set; }

    /// <summary>
    /// 获取或设置分页信息.
    /// </summary>
    [JsonPropertyName("pagination")]
    public BooklistPagination? Pagination { get; set; }
}

/// <summary>
/// 书单书籍包装器.
/// </summary>
internal sealed class BooklistBookWrapper
{
    /// <summary>
    /// 获取或设置书籍数据.
    /// </summary>
    [JsonPropertyName("book")]
    public BooklistBookData? Book { get; set; }
}

/// <summary>
/// 书单书籍数据.
/// </summary>
internal sealed class BooklistBookData
{
    /// <summary>
    /// 获取或设置书籍 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 获取或设置书籍标识符（ISBN）.
    /// </summary>
    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }

    /// <summary>
    /// 获取或设置书籍链接.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    /// <summary>
    /// 获取或设置封面 URL.
    /// </summary>
    [JsonPropertyName("cover")]
    public string? Cover { get; set; }

    /// <summary>
    /// 获取或设置书籍标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 获取或设置出版社.
    /// </summary>
    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    /// <summary>
    /// 获取或设置作者（逗号分隔）.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// 获取或设置出版年份.
    /// </summary>
    [JsonPropertyName("year")]
    public string? Year { get; set; }

    /// <summary>
    /// 获取或设置语言.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// 获取或设置文件格式.
    /// </summary>
    [JsonPropertyName("extension")]
    public string? Extension { get; set; }

    /// <summary>
    /// 获取或设置文件大小字符串.
    /// </summary>
    [JsonPropertyName("filesizeString")]
    public string? FilesizeString { get; set; }

    /// <summary>
    /// 获取或设置质量评分.
    /// </summary>
    [JsonPropertyName("qualityScore")]
    public string? QualityScore { get; set; }
}

/// <summary>
/// 书单分页信息.
/// </summary>
internal sealed class BooklistPagination
{
    /// <summary>
    /// 获取或设置总页数.
    /// </summary>
    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }
}
