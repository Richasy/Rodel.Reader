// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// 表示 Mobi 中的导航项（目录条目）。
/// </summary>
public sealed class MobiNavItem
{
    /// <summary>
    /// 获取或设置导航项的标题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置内容在书籍中的偏移位置。
    /// </summary>
    public long Position { get; set; }

    /// <summary>
    /// 获取或设置锚点/片段标识符。
    /// </summary>
    public string? Anchor { get; set; }

    /// <summary>
    /// 获取或设置子导航项。
    /// </summary>
    public List<MobiNavItem> Children { get; set; } = [];

    /// <summary>
    /// 获取一个值，指示此项是否有子项。
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <inheritdoc/>
    public override string ToString() => Title;
}
