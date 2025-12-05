// Copyright (c) Reader Copilot. All rights reserved.

using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 阅读历史记录.
/// </summary>
[SugarTable("ReadHistory")]
[Table("ReadHistory")]
public sealed class RssArticleHistory
{
    /// <summary>
    /// 文章 Id.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    [Key]
    public string ArticleId { get; set; }

    /// <summary>
    /// 最近阅读时间.
    /// </summary>
    public string? LastReadTime { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RssArticleHistory history && ArticleId == history.ArticleId;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(ArticleId);
}
