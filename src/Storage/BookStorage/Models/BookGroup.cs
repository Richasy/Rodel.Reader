// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书籍分组（书架内）.
/// </summary>
public sealed class BookGroup
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 所属书架 ID.
    /// </summary>
    public string ShelfId { get; set; } = string.Empty;

    /// <summary>
    /// 名称.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排序索引.
    /// </summary>
    public int SortIndex { get; set; }

    /// <summary>
    /// 是否折叠.
    /// </summary>
    public bool IsCollapsed { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BookGroup group && Id == group.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
