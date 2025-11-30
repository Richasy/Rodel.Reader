// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// EPUB 生成选项.
/// </summary>
public sealed class EpubOptions
{
    /// <summary>
    /// EPUB 版本.
    /// </summary>
    public EpubVersion Version { get; init; } = EpubVersion.Epub2;

    /// <summary>
    /// 书写方向.
    /// </summary>
    public WritingDirection Direction { get; init; } = WritingDirection.Ltr;

    /// <summary>
    /// 页面翻页方向.
    /// </summary>
    public PageProgression PageProgression { get; init; } = PageProgression.Ltr;

    /// <summary>
    /// 是否生成可视化目录页.
    /// </summary>
    public bool IncludeTocPage { get; init; } = true;

    /// <summary>
    /// 是否生成版权页.
    /// </summary>
    public bool IncludeCopyrightPage { get; init; }

    /// <summary>
    /// 自定义 CSS 样式（可选，追加到默认样式之后）.
    /// </summary>
    public string? CustomCss { get; init; }

    /// <summary>
    /// 嵌入的资源文件列表（图片、字体等）.
    /// </summary>
    public IReadOnlyList<ResourceInfo>? Resources { get; init; }
}
