// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 播客订阅.
/// </summary>
public sealed class Podcast
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 标题.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 描述.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// RSS 订阅地址.
    /// </summary>
    public string? FeedUrl { get; set; }

    /// <summary>
    /// 网站地址.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// 封面 URL.
    /// </summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// 本地封面路径.
    /// </summary>
    public string? CoverPath { get; set; }

    /// <summary>
    /// 语言.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 分类 (JSON 数组).
    /// </summary>
    public string? Categories { get; set; }

    /// <summary>
    /// 来源类型.
    /// </summary>
    public PodcastSourceType SourceType { get; set; }

    /// <summary>
    /// 来源相关数据 (JSON).
    /// </summary>
    public string? SourceData { get; set; }

    /// <summary>
    /// 分组 ID 列表 (逗号分隔).
    /// </summary>
    public string? GroupIds { get; set; }

    /// <summary>
    /// 单集数量.
    /// </summary>
    public int? EpisodeCount { get; set; }

    /// <summary>
    /// 最新单集日期.
    /// </summary>
    public DateTimeOffset? LatestEpisodeDate { get; set; }

    /// <summary>
    /// 是否已订阅.
    /// </summary>
    public bool IsSubscribed { get; set; } = true;

    /// <summary>
    /// 排序索引（用于手动排序）.
    /// </summary>
    public int? SortIndex { get; set; }

    /// <summary>
    /// 添加时间.
    /// </summary>
    public DateTimeOffset AddedAt { get; set; }

    /// <summary>
    /// 最后刷新时间.
    /// </summary>
    public DateTimeOffset? LastRefreshedAt { get; set; }

    /// <summary>
    /// 更新时间.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Podcast podcast && Id == podcast.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// 获取分组 ID 列表.
    /// </summary>
    /// <returns>分组 ID 列表.</returns>
    public List<string> GetGroupIdList()
        => string.IsNullOrEmpty(GroupIds) ? [] : [.. GroupIds.Split(',', StringSplitOptions.RemoveEmptyEntries)];

    /// <summary>
    /// 设置分组 ID 列表.
    /// </summary>
    /// <param name="ids">分组 ID 列表.</param>
    public void SetGroupIdList(IEnumerable<string> ids)
        => GroupIds = string.Join(',', ids);

    /// <summary>
    /// 添加分组 ID.
    /// </summary>
    /// <param name="groupId">分组 ID.</param>
    public void AddGroupId(string groupId)
    {
        var list = GetGroupIdList();
        if (!list.Contains(groupId))
        {
            list.Add(groupId);
            SetGroupIdList(list);
        }
    }

    /// <summary>
    /// 移除分组 ID.
    /// </summary>
    /// <param name="groupId">分组 ID.</param>
    public void RemoveGroupId(string groupId)
    {
        var list = GetGroupIdList();
        if (list.Remove(groupId))
        {
            SetGroupIdList(list);
        }
    }
}
