// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 书单信息.
/// </summary>
public sealed record Booklist
{
    /// <summary>
    /// 获取书单名称.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 获取书单 URL.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// 获取书单描述.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 获取书单作者.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// 获取书籍数量.
    /// </summary>
    public string? BookCount { get; init; }

    /// <summary>
    /// 获取浏览次数.
    /// </summary>
    public string? Views { get; init; }

    /// <summary>
    /// 获取书单中的书籍预览列表.
    /// </summary>
    public IReadOnlyList<BookItem>? PreviewBooks { get; init; }
}
