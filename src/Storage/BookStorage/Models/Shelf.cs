// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书架.
/// </summary>
public sealed class Shelf
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 名称.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 图标 Emoji.
    /// </summary>
    public string? IconEmoji { get; set; }

    /// <summary>
    /// 排序索引.
    /// </summary>
    public int SortIndex { get; set; }

    /// <summary>
    /// 是否默认书架.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新时间.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Shelf shelf && Id == shelf.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
