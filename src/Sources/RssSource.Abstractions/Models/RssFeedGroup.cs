// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 订阅源分组.
/// </summary>
public sealed class RssFeedGroup
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
    /// 克隆当前对象.
    /// </summary>
    /// <returns>新的 <see cref="RssFeedGroup"/> 实例.</returns>
    public RssFeedGroup Clone()
        => new()
        {
            Id = Id,
            Name = Name,
        };

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is RssFeedGroup group && Id == group.Id;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(Id);
}
