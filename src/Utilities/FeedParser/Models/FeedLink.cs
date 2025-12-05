// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.FeedParser;

/// <summary>
/// Feed 链接.
/// </summary>
/// <param name="Uri">链接地址.</param>
/// <param name="LinkType">链接类型.</param>
/// <param name="Title">链接标题（可选）.</param>
/// <param name="MediaType">媒体类型（如 audio/mpeg）.</param>
/// <param name="Length">内容长度（字节）.</param>
/// <param name="LastUpdated">最后更新时间.</param>
public sealed record FeedLink(
    Uri Uri,
    FeedLinkType LinkType = FeedLinkType.Alternate,
    string? Title = null,
    string? MediaType = null,
    long? Length = null,
    DateTimeOffset? LastUpdated = null);
