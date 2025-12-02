// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 分页结果.
/// </summary>
/// <typeparam name="T">结果项类型.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    /// 获取结果项列表.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// 获取当前页码（从 1 开始）.
    /// </summary>
    public int CurrentPage { get; init; } = 1;

    /// <summary>
    /// 获取总页数.
    /// </summary>
    public int TotalPages { get; init; } = 1;

    /// <summary>
    /// 获取每页大小.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// 获取是否有下一页.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// 获取是否有上一页.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;
}
