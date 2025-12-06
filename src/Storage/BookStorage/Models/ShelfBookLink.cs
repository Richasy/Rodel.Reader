// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书架-书籍关联.
/// </summary>
public sealed class ShelfBookLink
{
    /// <summary>
    /// 标识符 (BookId_ShelfId).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// 书架 ID.
    /// </summary>
    public string ShelfId { get; set; } = string.Empty;

    /// <summary>
    /// 分组 ID (可选).
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// 排序索引.
    /// </summary>
    public int SortIndex { get; set; }

    /// <summary>
    /// 添加到书架的时间.
    /// </summary>
    public DateTimeOffset AddedAt { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ShelfBookLink link && Id == link.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
