// Copyright (c) Reader Copilot. All rights reserved.

using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// 播客.
/// </summary>
[SugarTable("Podcasts")]
[Table("Podcasts")]
public sealed class RssPodcast
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

    /// <summary>
    /// 描述.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 订阅源地址.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 网站地址.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 封面.
    /// </summary>
    public string? Cover { get; set; }

    /// <summary>
    /// 类别.
    /// </summary>
    public string? Categories { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RssPodcast podcast && Id == podcast.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// 获取分组ID列表.
    /// </summary>
    /// <returns>ID列表.</returns>
    public List<string> GetCategories()
        => string.IsNullOrEmpty(Categories) ? [] : [.. Categories.Split(',')];

    /// <summary>
    /// 设置分组ID列表.
    /// </summary>
    /// <param name="ids">分组ID列表.</param>
    public void SetCategories(IEnumerable<string> ids)
        => Categories = string.Join(',', ids);

    /// <summary>
    /// 添加分组ID.
    /// </summary>
    /// <param name="groupId">ID.</param>
    public void AddCategoryId(string groupId)
    {
        var list = GetCategories();
        if (!list.Contains(groupId))
        {
            list.Add(groupId);
            SetCategories(list);
        }
    }
}
