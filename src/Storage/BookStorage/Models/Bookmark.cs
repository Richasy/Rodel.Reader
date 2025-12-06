// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书签.
/// </summary>
public sealed class Bookmark
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// 书签标题.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 书签备注.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 位置标识.
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// 章节 ID.
    /// </summary>
    public string? ChapterId { get; set; }

    /// <summary>
    /// 章节标题.
    /// </summary>
    public string? ChapterTitle { get; set; }

    /// <summary>
    /// 页码.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// 颜色标记.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Bookmark bookmark && Id == bookmark.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
