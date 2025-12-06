// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书籍.
/// </summary>
public sealed class Book
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 书名.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 副标题.
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// 作者列表 (JSON 数组).
    /// </summary>
    public string? Authors { get; set; }

    /// <summary>
    /// 译者列表 (JSON 数组).
    /// </summary>
    public string? Translators { get; set; }

    /// <summary>
    /// 出版商.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// 出版日期.
    /// </summary>
    public string? PublishDate { get; set; }

    /// <summary>
    /// 语言代码 (BCP 47).
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// ISBN.
    /// </summary>
    public string? ISBN { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 标签 (JSON 数组).
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 所属系列名称.
    /// </summary>
    public string? Series { get; set; }

    /// <summary>
    /// 系列中的顺序.
    /// </summary>
    public int? SeriesIndex { get; set; }

    /// <summary>
    /// 分类 (JSON 数组).
    /// </summary>
    public string? Categories { get; set; }

    /// <summary>
    /// 书籍格式.
    /// </summary>
    public BookFormat Format { get; set; }

    /// <summary>
    /// 本地文件路径.
    /// </summary>
    public string? LocalPath { get; set; }

    /// <summary>
    /// 封面图片路径（本地）.
    /// </summary>
    public string? CoverPath { get; set; }

    /// <summary>
    /// 封面图片 URL（远程）.
    /// </summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// 文件大小 (bytes).
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// 文件哈希值.
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// 总页数 (PDF/漫画).
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// 字数统计.
    /// </summary>
    public int? WordCount { get; set; }

    /// <summary>
    /// 章节数.
    /// </summary>
    public int? ChapterCount { get; set; }

    /// <summary>
    /// 书籍来源类型.
    /// </summary>
    public BookSourceType SourceType { get; set; }

    /// <summary>
    /// 来源附加数据 (JSON).
    /// </summary>
    public string? SourceData { get; set; }

    /// <summary>
    /// 在线网址.
    /// </summary>
    public string? WebUrl { get; set; }

    /// <summary>
    /// 追踪状态.
    /// </summary>
    public BookTrackStatus TrackStatus { get; set; }

    /// <summary>
    /// 用户评分 (1-5).
    /// </summary>
    public int? UserRating { get; set; }

    /// <summary>
    /// 用户短评.
    /// </summary>
    public string? UserReview { get; set; }

    /// <summary>
    /// 用户自定义标签 (JSON 数组).
    /// </summary>
    public string? UserTags { get; set; }

    /// <summary>
    /// 是否使用漫画阅读模式（纯图模式）.
    /// </summary>
    public bool UseComicReader { get; set; }

    /// <summary>
    /// 添加时间 (ISO 8601).
    /// </summary>
    public string AddedAt { get; set; } = string.Empty;

    /// <summary>
    /// 最后打开时间.
    /// </summary>
    public string? LastOpenedAt { get; set; }

    /// <summary>
    /// 读完时间.
    /// </summary>
    public string? FinishedAt { get; set; }

    /// <summary>
    /// 更新时间.
    /// </summary>
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// 扩展数据 (JSON).
    /// </summary>
    public string? ExtraData { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Book book && Id == book.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
