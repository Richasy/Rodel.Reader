// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// 表示 FB2 中的导航项（目录条目）。
/// </summary>
public sealed class Fb2NavItem
{
    /// <summary>
    /// 获取或设置导航项的标题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置章节 ID。
    /// </summary>
    public string? SectionId { get; set; }

    /// <summary>
    /// 获取或设置嵌套层级（0 为顶级）。
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 获取或设置子导航项。
    /// </summary>
    public List<Fb2NavItem> Children { get; set; } = [];

    /// <summary>
    /// 获取一个值，指示此项是否有子项。
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <inheritdoc/>
    public override string ToString() => Title;
}
