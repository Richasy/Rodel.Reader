// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 缓存状态.
/// </summary>
public sealed record CacheState
{
    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public required string BookId { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// 缓存时的目录哈希.
    /// </summary>
    public string? TocHash { get; init; }

    /// <summary>
    /// 已缓存的章节 ID 列表.
    /// </summary>
    public IReadOnlyList<string> CachedChapterIds { get; init; } = [];

    /// <summary>
    /// 失败的章节 ID 列表.
    /// </summary>
    public IReadOnlyList<string> FailedChapterIds { get; init; } = [];

    /// <summary>
    /// 缓存创建时间.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 缓存最后更新时间.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// 缓存是否可用（目录哈希匹配）.
    /// </summary>
    public bool IsValid(string currentTocHash)
        => TocHash == currentTocHash;
}
