// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 从 EPUB 提取的番茄书籍信息.
/// </summary>
public sealed record FanQieBookInfo
{
    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public required string BookId { get; init; }

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
    /// 分类标签.
    /// </summary>
    public IReadOnlyList<string>? Tags { get; init; }

    /// <summary>
    /// 连载状态.
    /// </summary>
    public BookCreationStatus CreationStatus { get; init; }

    /// <summary>
    /// 最后同步时间.
    /// </summary>
    public DateTimeOffset? LastSyncTime { get; init; }

    /// <summary>
    /// 目录哈希（用于验证目录是否变化）.
    /// </summary>
    public string? TocHash { get; init; }

    /// <summary>
    /// 已下载章节 ID 列表.
    /// </summary>
    public IReadOnlyList<string> DownloadedChapterIds { get; init; } = [];

    /// <summary>
    /// 失败章节 ID 列表.
    /// </summary>
    public IReadOnlyList<string> FailedChapterIds { get; init; } = [];

    /// <summary>
    /// 从 BookDetail 创建.
    /// </summary>
    public static FanQieBookInfo FromBookDetail(BookDetail detail)
        => new()
        {
            BookId = detail.BookId,
            Title = detail.Title,
            Author = detail.Author,
            Description = detail.Abstract,
            CoverUrl = detail.CoverUrl,
            Tags = detail.Tags,
            CreationStatus = detail.CreationStatus,
        };
}
