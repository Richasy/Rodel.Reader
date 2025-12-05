// Copyright (c) Reader Copilot. All rights reserved.

using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 源组.
/// </summary>
[SugarTable("Groups")]
[Table("Groups")]
public sealed class RssFeedGroup
{
    /// <summary>
    /// 标识符.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    [Key]
    public string Id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string Name { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RssFeedGroup group && Id == group.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// 克隆对象.
    /// </summary>
    /// <returns><see cref="RssFeedGroup"/>.</returns>
    public RssFeedGroup Clone()
    {
        return new RssFeedGroup
        {
            Id = Id,
            Name = Name,
        };
    }
}
