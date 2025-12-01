// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 频道/源信息.
/// </summary>
/// <remarks>
/// 表示 RSS 的 channel 或 Atom 的 feed 元数据.
/// </remarks>
public sealed record FeedChannel
{
    /// <summary>
    /// 唯一标识符.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// 频道标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 频道描述.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 频道语言.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// 版权信息.
    /// </summary>
    public string? Copyright { get; init; }

    /// <summary>
    /// 生成器信息.
    /// </summary>
    public string? Generator { get; init; }

    /// <summary>
    /// 最后构建/更新时间.
    /// </summary>
    public DateTimeOffset? LastBuildDate { get; init; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    /// Feed 类型.
    /// </summary>
    public FeedType FeedType { get; init; } = FeedType.Unknown;

    /// <summary>
    /// 频道图片列表.
    /// </summary>
    public IReadOnlyList<FeedImage> Images { get; init; } = [];

    /// <summary>
    /// 链接列表.
    /// </summary>
    public IReadOnlyList<FeedLink> Links { get; init; } = [];

    /// <summary>
    /// 作者/贡献者列表.
    /// </summary>
    public IReadOnlyList<FeedPerson> Contributors { get; init; } = [];

    /// <summary>
    /// 分类列表.
    /// </summary>
    public IReadOnlyList<FeedCategory> Categories { get; init; } = [];

    /// <summary>
    /// 分页链接（RFC 5005）.
    /// </summary>
    /// <remarks>
    /// 用于支持 Feed 归档和历史遍历.
    /// </remarks>
    public FeedPagingLinks? PagingLinks { get; init; }

    /// <summary>
    /// 获取主链接（网站链接）.
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
    /// 获取主图片（Logo 或 Icon）.
    /// </summary>
    /// <returns>图片 URL，若不存在则返回 null.</returns>
    public Uri? GetPrimaryImage()
    {
        foreach (var image in Images)
        {
            if (image.ImageType == FeedImageType.Logo)
            {
                return image.Url;
            }
        }

        foreach (var image in Images)
        {
            if (image.ImageType == FeedImageType.Icon)
            {
                return image.Url;
            }
        }

        return Images.Count > 0 ? Images[0].Url : null;
    }
}
