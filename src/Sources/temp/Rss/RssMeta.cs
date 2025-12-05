// Copyright (c) Reader Copilot. All rights reserved.

using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 元数据.
/// </summary>
[SugarTable("Metadata")]
[Table("Metadata")]
public sealed class RssMeta
{
    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    [Key]
    public string Name { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    public string Value { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RssMeta meta && Name == meta.Name;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Name);
}
