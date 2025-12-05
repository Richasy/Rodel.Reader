// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.BookScraper.Internal.Responses.DouBan;

/// <summary>
/// 豆瓣搜索响应.
/// </summary>
internal sealed class DouBanSearchResponse
{
    /// <summary>
    /// 本次返回的数量.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// 起始位置.
    /// </summary>
    [JsonPropertyName("start")]
    public int Start { get; set; }

    /// <summary>
    /// 总数.
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// 书籍列表.
    /// </summary>
    [JsonPropertyName("books")]
    public List<DouBanBook>? Books { get; set; }
}

/// <summary>
/// 豆瓣书籍.
/// </summary>
internal sealed class DouBanBook
{
    /// <summary>
    /// 书籍 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 书名.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 副标题.
    /// </summary>
    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    /// <summary>
    /// 原名.
    /// </summary>
    [JsonPropertyName("origin_title")]
    public string? OriginTitle { get; set; }

    /// <summary>
    /// 作者列表.
    /// </summary>
    [JsonPropertyName("author")]
    public List<string>? Author { get; set; }

    /// <summary>
    /// 译者列表.
    /// </summary>
    [JsonPropertyName("translator")]
    public List<string>? Translator { get; set; }

    /// <summary>
    /// 出版社.
    /// </summary>
    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    /// <summary>
    /// 出版日期.
    /// </summary>
    [JsonPropertyName("pubdate")]
    public string? PubDate { get; set; }

    /// <summary>
    /// 评分信息.
    /// </summary>
    [JsonPropertyName("rating")]
    public DouBanRating? Rating { get; set; }

    /// <summary>
    /// 封面图片（中等尺寸）.
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// 图片列表.
    /// </summary>
    [JsonPropertyName("images")]
    public DouBanImages? Images { get; set; }

    /// <summary>
    /// 页数.
    /// </summary>
    [JsonPropertyName("pages")]
    public string? Pages { get; set; }

    /// <summary>
    /// 装帧.
    /// </summary>
    [JsonPropertyName("binding")]
    public string? Binding { get; set; }

    /// <summary>
    /// ISBN-10.
    /// </summary>
    [JsonPropertyName("isbn10")]
    public string? Isbn10 { get; set; }

    /// <summary>
    /// ISBN-13.
    /// </summary>
    [JsonPropertyName("isbn13")]
    public string? Isbn13 { get; set; }

    /// <summary>
    /// 书籍链接.
    /// </summary>
    [JsonPropertyName("alt")]
    public string? Alt { get; set; }

    /// <summary>
    /// API 链接.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// 作者简介.
    /// </summary>
    [JsonPropertyName("author_intro")]
    public string? AuthorIntro { get; set; }

    /// <summary>
    /// 目录.
    /// </summary>
    [JsonPropertyName("catalog")]
    public string? Catalog { get; set; }

    /// <summary>
    /// 价格.
    /// </summary>
    [JsonPropertyName("price")]
    public string? Price { get; set; }

    /// <summary>
    /// 标签列表.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<DouBanTag>? Tags { get; set; }

    /// <summary>
    /// 系列信息.
    /// </summary>
    [JsonPropertyName("series")]
    public DouBanSeries? Series { get; set; }
}

/// <summary>
/// 豆瓣评分.
/// </summary>
internal sealed class DouBanRating
{
    /// <summary>
    /// 最高分.
    /// </summary>
    [JsonPropertyName("max")]
    public int Max { get; set; }

    /// <summary>
    /// 最低分.
    /// </summary>
    [JsonPropertyName("min")]
    public int Min { get; set; }

    /// <summary>
    /// 评分人数.
    /// </summary>
    [JsonPropertyName("numRaters")]
    public int NumRaters { get; set; }

    /// <summary>
    /// 平均分.
    /// </summary>
    [JsonPropertyName("average")]
    public string? Average { get; set; }
}

/// <summary>
/// 豆瓣图片.
/// </summary>
internal sealed class DouBanImages
{
    /// <summary>
    /// 小图.
    /// </summary>
    [JsonPropertyName("small")]
    public string? Small { get; set; }

    /// <summary>
    /// 中图.
    /// </summary>
    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    /// <summary>
    /// 大图.
    /// </summary>
    [JsonPropertyName("large")]
    public string? Large { get; set; }
}

/// <summary>
/// 豆瓣标签.
/// </summary>
internal sealed class DouBanTag
{
    /// <summary>
    /// 数量.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

/// <summary>
/// 豆瓣系列.
/// </summary>
internal sealed class DouBanSeries
{
    /// <summary>
    /// 系列 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 系列标题.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
