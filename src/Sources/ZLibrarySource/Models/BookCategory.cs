// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 书籍分类信息.
/// </summary>
public sealed record BookCategory
{
    /// <summary>
    /// 获取分类名称.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 获取分类 URL.
    /// </summary>
    public string? Url { get; init; }
}
