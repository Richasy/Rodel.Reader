// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 书籍基本信息.
/// </summary>
public sealed record BookItem
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
    /// 获取 ISBN.
    /// </summary>
    public string? Isbn { get; init; }

    /// <summary>
    /// 获取书籍详情 URL.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// 获取封面图片 URL.
    /// </summary>
    public string? CoverUrl { get; init; }

    /// <summary>
    /// 获取作者列表.
    /// </summary>
    public IReadOnlyList<string>? Authors { get; init; }

    /// <summary>
    /// 获取出版社.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// 获取出版年份.
    /// </summary>
    public string? Year { get; init; }

    /// <summary>
    /// 获取语言.
    /// </summary>
    public string? Language { get; init; }

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
    /// 获取质量评分.
    /// </summary>
    public string? Quality { get; init; }
}
