// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 书籍作者详细信息.
/// </summary>
public sealed record BookAuthor
{
    /// <summary>
    /// 获取作者名称.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 获取作者详情 URL.
    /// </summary>
    public string? Url { get; init; }
}
