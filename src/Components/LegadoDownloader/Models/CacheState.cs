// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Models;

/// <summary>
/// 缓存状态.
/// </summary>
public sealed record CacheState
{
    /// <summary>
    /// 书籍链接（唯一标识）.
    /// </summary>
    public required string BookUrl { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// 书源链接.
    /// </summary>
    public string? BookSource { get; init; }

    /// <summary>
    /// 服务地址.
    /// </summary>
    public string? ServerUrl { get; init; }

    /// <summary>
    /// 缓存时的目录哈希.
    /// </summary>
    public string? TocHash { get; init; }

    /// <summary>
    /// 已缓存的章节索引列表.
    /// </summary>
    public IReadOnlyList<int> CachedChapterIndexes { get; init; } = [];

    /// <summary>
    /// 失败的章节索引列表.
    /// </summary>
    public IReadOnlyList<int> FailedChapterIndexes { get; init; } = [];

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
