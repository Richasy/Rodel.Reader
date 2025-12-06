// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 阅读进度.
/// </summary>
public sealed class ReadProgress
{
    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// 阅读进度 (0.0 - 1.0).
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// 位置标识 (CFI/页码等).
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// 当前章节 ID.
    /// </summary>
    public string? ChapterId { get; set; }

    /// <summary>
    /// 当前章节标题.
    /// </summary>
    public string? ChapterTitle { get; set; }

    /// <summary>
    /// 当前页码.
    /// </summary>
    public int? CurrentPage { get; set; }

    /// <summary>
    /// 位置信息 (JSON).
    /// </summary>
    public string? Locations { get; set; }

    /// <summary>
    /// 更新时间.
    /// </summary>
    public string UpdatedAt { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ReadProgress progress && BookId == progress.BookId;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(BookId);
}
