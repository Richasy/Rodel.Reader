// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 书籍详细信息.
/// </summary>
public sealed record BookDetail
{
    /// <summary>
    /// 获取书籍 ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 获取书籍名称.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 获取书籍详情 URL.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// 获取封面图片 URL.
    /// </summary>
    public string? CoverUrl { get; init; }

    /// <summary>
    /// 获取书籍描述.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 获取作者详细信息列表.
    /// </summary>
    public IReadOnlyList<BookAuthor>? Authors { get; init; }

    /// <summary>
    /// 获取出版社.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// 获取出版年份.
    /// </summary>
    public string? Year { get; init; }

    /// <summary>
    /// 获取版本信息.
    /// </summary>
    public string? Edition { get; init; }

    /// <summary>
    /// 获取语言.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// 获取 ISBN-10.
    /// </summary>
    public string? Isbn10 { get; init; }

    /// <summary>
    /// 获取 ISBN-13.
    /// </summary>
    public string? Isbn13 { get; init; }

    /// <summary>
    /// 获取分类信息.
    /// </summary>
    public BookCategory? Category { get; init; }

    /// <summary>
    /// 获取文件格式.
    /// </summary>
    public string? Extension { get; init; }

    /// <summary>
    /// 获取文件大小.
    /// </summary>
    public string? FileSize { get; init; }

    /// <summary>
    /// 获取评分.
    /// </summary>
    public string? Rating { get; init; }

    /// <summary>
    /// 获取下载 URL.
    /// </summary>
    public string? DownloadUrl { get; init; }

    /// <summary>
    /// 获取是否可以下载.
    /// </summary>
    public bool IsDownloadAvailable { get; init; }
}
