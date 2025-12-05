// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Models;

/// <summary>
/// 从 EPUB 提取的 Legado 书籍信息.
/// </summary>
public sealed record LegadoBookInfo
{
    /// <summary>
    /// 书籍链接（唯一标识）.
    /// </summary>
    public required string BookUrl { get; init; }

    /// <summary>
    /// 书源链接.
    /// </summary>
    public required string BookSource { get; init; }

    /// <summary>
    /// 服务地址.
    /// </summary>
    public required string ServerUrl { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// 简介.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 封面 URL.
    /// </summary>
    public string? CoverUrl { get; init; }

    /// <summary>
    /// 最后同步时间.
    /// </summary>
    public DateTimeOffset? LastSyncTime { get; init; }

    /// <summary>
    /// 目录哈希（用于验证目录是否变化）.
    /// </summary>
    public string? TocHash { get; init; }

    /// <summary>
    /// 已下载章节索引列表.
    /// </summary>
    public IReadOnlyList<int> DownloadedChapterIndexes { get; init; } = [];

    /// <summary>
    /// 失败章节索引列表.
    /// </summary>
    public IReadOnlyList<int> FailedChapterIndexes { get; init; } = [];

    /// <summary>
    /// 从 Book 创建.
    /// </summary>
    public static LegadoBookInfo FromBook(Book book, string serverUrl)
        => new()
        {
            BookUrl = book.BookUrl,
            BookSource = book.Origin ?? string.Empty,
            ServerUrl = serverUrl,
            Title = book.Name,
            Author = book.Author,
            Description = book.Intro,
            CoverUrl = book.CoverUrl,
        };
}
