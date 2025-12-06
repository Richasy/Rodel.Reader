// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions;

/// <summary>
/// RSS 订阅源.
/// </summary>
public sealed class RssFeed
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
    /// 订阅源地址.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 网站地址.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 图标 URL.
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// 所属分组 ID 列表（逗号分隔）.
    /// </summary>
    public string? GroupIds { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 是否需要获取完整内容.
    /// </summary>
    public bool IsFullContentRequired { get; set; }

    /// <summary>
    /// 获取分组 ID 列表.
    /// </summary>
    /// <returns>ID 列表.</returns>
    public IReadOnlyList<string> GetGroupIdList()
        => string.IsNullOrEmpty(GroupIds)
            ? []
            : GroupIds.Split(',', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// 设置分组 ID 列表.
    /// </summary>
    /// <param name="ids">分组 ID 列表.</param>
    public void SetGroupIdList(IEnumerable<string> ids)
        => GroupIds = string.Join(',', ids);

    /// <summary>
    /// 克隆当前对象.
    /// </summary>
    /// <returns>新的 <see cref="RssFeed"/> 实例.</returns>
    public RssFeed Clone()
        => new()
        {
            Id = Id,
            Name = Name,
            Url = Url,
            Website = Website,
            Description = Description,
            IconUrl = IconUrl,
            GroupIds = GroupIds,
            Comment = Comment,
            IsFullContentRequired = IsFullContentRequired,
        };

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is RssFeed feed && Id == feed.Id;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(Id);
}
