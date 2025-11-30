// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 表示 EPUB 中的导航项（目录条目）。
/// </summary>
public sealed class EpubNavItem
{
    /// <summary>
    /// 获取或设置导航项的标题。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置内容文件路径（相对于内容目录）。
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// 获取或设置在 EPUB 存档中的完整路径。
    /// </summary>
    public string? FullPath { get; set; }

    /// <summary>
    /// 获取或设置锚点/片段标识符（如 "#chapter1"）。
    /// </summary>
    public string? Anchor { get; set; }

    /// <summary>
    /// 获取或设置子导航项。
    /// </summary>
    public List<EpubNavItem> Children { get; set; } = [];

    /// <summary>
    /// 获取一个值，指示此项是否有子项。
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <inheritdoc/>
    public override string ToString() => Title;
}
