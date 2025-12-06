// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 播客分组.
/// </summary>
public sealed class PodcastGroup
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 分组名称.
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
    /// 创建时间.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PodcastGroup group && Id == group.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
