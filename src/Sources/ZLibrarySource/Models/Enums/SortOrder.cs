// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 排序选项.
/// </summary>
public enum SortOrder
{
    /// <summary>
    /// 按热门程度排序.
    /// </summary>
    Popular,

    /// <summary>
    /// 按创建时间排序（最新）.
    /// </summary>
    Newest,

    /// <summary>
    /// 按更新时间排序（最近更新）.
    /// </summary>
    Recent,
}
