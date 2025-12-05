// Copyright (c) Reader Copilot. All rights reserved.

using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 订阅源.
/// </summary>
[SugarTable("Feeds")]
[Table("Feeds")]
public sealed class RssFeed
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
    /// 订阅源地址.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 网址.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// 所属分组标识符.
    /// </summary>
    public string? GroupIds { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 是否需要完整内容.
    /// </summary>
    [JsonConverter(typeof(BoolJsonConverter))]
    public bool IsFullRequired { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RssFeed feed && Id == feed.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// 克隆一个新的对象.
    /// </summary>
    /// <returns><see cref="RssFeed"/>.</returns>
    public RssFeed Clone() => new()
    {
        Id = Id,
        Name = Name,
        Url = Url,
        Website = Website,
        GroupIds = GroupIds,
        Description = Description,
        Comment = Comment,
        IsFullRequired = IsFullRequired,
    };

    /// <summary>
    /// 获取分组ID列表.
    /// </summary>
    /// <returns>ID列表.</returns>
    public List<string> GetGroupIds()
        => string.IsNullOrEmpty(GroupIds) ? [] : [.. GroupIds.Split(',')];

    /// <summary>
    /// 设置分组ID列表.
    /// </summary>
    /// <param name="ids">分组ID列表.</param>
    public void SetGroupIds(IEnumerable<string> ids)
        => GroupIds = string.Join(',', ids);

    /// <summary>
    /// 添加分组ID.
    /// </summary>
    /// <param name="groupId">ID.</param>
    public void AddGroupId(string groupId)
    {
        var list = GetGroupIds();
        if (!list.Contains(groupId))
        {
            list.Add(groupId);
            SetGroupIds(list);
        }
    }
}
