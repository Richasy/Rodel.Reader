// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 订阅项（文章/条目）.
/// </summary>
/// <remarks>
/// 表示 RSS 的 item 或 Atom 的 entry.
/// </remarks>
public sealed record FeedItem
{
    /// <summary>
    /// 唯一标识符.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// 标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 摘要/描述.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 完整内容（HTML 编码）.
    /// </summary>
    /// <remarks>
    /// 对应 RSS 的 content:encoded 或 Atom 的 content.
    /// </remarks>
    public string? Content { get; init; }

    /// <summary>
    /// 封面图片 URL.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    /// 最后更新时间.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// 音频/视频时长（秒）.
    /// </summary>
    /// <remarks>
    /// 主要用于播客订阅.
    /// </remarks>
    public int? Duration { get; init; }

    /// <summary>
    /// 作者/贡献者列表.
    /// </summary>
    public IReadOnlyList<FeedPerson> Contributors { get; init; } = [];

    /// <summary>
    /// 链接列表.
    /// </summary>
    public IReadOnlyList<FeedLink> Links { get; init; } = [];

    /// <summary>
    /// 分类列表.
    /// </summary>
    public IReadOnlyList<FeedCategory> Categories { get; init; } = [];

    /// <summary>
    /// 获取主链接（网页链接）.
    /// </summary>
    /// <returns>主链接 URL，若不存在则返回 null.</returns>
    public Uri? GetPrimaryLink()
    {
        foreach (var link in Links)
        {
            if (link.LinkType == FeedLinkType.Alternate)
            {
                return link.Uri;
            }
        }

        return Links.Count > 0 ? Links[0].Uri : null;
    }

    /// <summary>
    /// 获取附件链接（如播客音频）.
    /// </summary>
    /// <returns>附件链接，若不存在则返回 null.</returns>
    public FeedLink? GetEnclosure()
    {
        foreach (var link in Links)
        {
            if (link.LinkType == FeedLinkType.Enclosure)
            {
                return link;
            }
        }

        return null;
    }
}
