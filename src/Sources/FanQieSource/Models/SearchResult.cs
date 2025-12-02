// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 搜索结果.
/// </summary>
/// <typeparam name="T">结果项类型.</typeparam>
public sealed class SearchResult<T>
{
    /// <summary>
    /// 结果列表.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// 是否有更多结果.
    /// </summary>
    public bool HasMore { get; init; }

    /// <summary>
    /// 下一页偏移量.
    /// </summary>
    public int NextOffset { get; init; }

    /// <summary>
    /// 搜索 ID.
    /// </summary>
    public string? SearchId { get; init; }
}
