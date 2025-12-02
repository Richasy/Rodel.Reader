// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models.Internal;

/// <summary>
/// 搜索 API 响应.
/// </summary>
internal sealed class SearchApiResponse
{
    /// <summary>
    /// 获取或设置成功标志.
    /// </summary>
    [JsonPropertyName("success")]
    public int Success { get; set; }

    /// <summary>
    /// 获取或设置书籍列表.
    /// </summary>
    [JsonPropertyName("books")]
    public IList<SearchApiBook>? Books { get; set; }

    /// <summary>
    /// 获取或设置精确匹配数量.
    /// </summary>
    [JsonPropertyName("exactBooksCount")]
    public int ExactBooksCount { get; set; }

    /// <summary>
    /// 获取或设置分页信息.
    /// </summary>
    [JsonPropertyName("pagination")]
    public SearchApiPagination? Pagination { get; set; }
}

/// <summary>
/// 搜索分页信息.
/// </summary>
internal sealed class SearchApiPagination
{
    /// <summary>
    /// 获取或设置每页数量限制.
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// 获取或设置当前页码.
    /// </summary>
    [JsonPropertyName("current")]
    public int Current { get; set; }

    /// <summary>
    /// 获取或设置是否有上一页.
    /// </summary>
    [JsonPropertyName("before")]
    public object? Before { get; set; }

    /// <summary>
    /// 获取或设置下一页页码（可能是 int 或 false）.
    /// </summary>
    [JsonPropertyName("next")]
    public object? Next { get; set; }

    /// <summary>
    /// 获取或设置总项目数.
    /// </summary>
    [JsonPropertyName("total_items")]
    public int TotalItems { get; set; }

    /// <summary>
    /// 获取或设置总页数.
    /// </summary>
    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }
}

/// <summary>
/// 搜索结果书籍.
/// </summary>
internal sealed class SearchApiBook
{
    /// <summary>
    /// 获取或设置书籍 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 获取或设置内容类型.
    /// </summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    /// <summary>
    /// 获取或设置标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 获取或设置作者.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// 获取或设置卷号.
    /// </summary>
    [JsonPropertyName("volume")]
    public string? Volume { get; set; }

    /// <summary>
    /// 获取或设置年份.
    /// </summary>
    [JsonPropertyName("year")]
    public int Year { get; set; }

    /// <summary>
    /// 获取或设置版本.
    /// </summary>
    [JsonPropertyName("edition")]
    public string? Edition { get; set; }

    /// <summary>
    /// 获取或设置出版商.
    /// </summary>
    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    /// <summary>
    /// 获取或设置标识符（ISBN等）.
    /// </summary>
    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }

    /// <summary>
    /// 获取或设置语言.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// 获取或设置页数.
    /// </summary>
    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    /// <summary>
    /// 获取或设置系列.
    /// </summary>
    [JsonPropertyName("series")]
    public string? Series { get; set; }

    /// <summary>
    /// 获取或设置封面 URL.
    /// </summary>
    [JsonPropertyName("cover")]
    public string? Cover { get; set; }

    /// <summary>
    /// 获取或设置文件大小（字节）.
    /// </summary>
    [JsonPropertyName("filesize")]
    public int Filesize { get; set; }

    /// <summary>
    /// 获取或设置文件大小字符串.
    /// </summary>
    [JsonPropertyName("filesizeString")]
    public string? FilesizeString { get; set; }

    /// <summary>
    /// 获取或设置文件扩展名.
    /// </summary>
    [JsonPropertyName("extension")]
    public string? Extension { get; set; }

    /// <summary>
    /// 获取或设置书籍链接.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    /// <summary>
    /// 获取或设置哈希值.
    /// </summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    /// <summary>
    /// 获取或设置是否支持 Kindle.
    /// </summary>
    [JsonPropertyName("kindleAvailable")]
    public bool KindleAvailable { get; set; }

    /// <summary>
    /// 获取或设置是否支持发送到邮箱.
    /// </summary>
    [JsonPropertyName("sendToEmailAvailable")]
    public bool SendToEmailAvailable { get; set; }

    /// <summary>
    /// 获取或设置兴趣评分.
    /// </summary>
    [JsonPropertyName("interestScore")]
    public string? InterestScore { get; set; }

    /// <summary>
    /// 获取或设置质量评分.
    /// </summary>
    [JsonPropertyName("qualityScore")]
    public string? QualityScore { get; set; }

    /// <summary>
    /// 获取或设置下载链接.
    /// </summary>
    [JsonPropertyName("dl")]
    public string? Dl { get; set; }

    /// <summary>
    /// 获取或设置在线阅读 URL.
    /// </summary>
    [JsonPropertyName("readOnlineUrl")]
    public string? ReadOnlineUrl { get; set; }

    /// <summary>
    /// 获取或设置描述.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 获取或设置是否支持在线阅读.
    /// </summary>
    [JsonPropertyName("readOnlineAvailable")]
    public bool ReadOnlineAvailable { get; set; }
}
