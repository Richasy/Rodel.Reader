// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// 表示 FB2 中的章节。
/// </summary>
public sealed class Fb2Section
{
    /// <summary>
    /// 获取或设置章节 ID。
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 获取或设置章节标题。
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 获取或设置章节内容（原始 XML）。
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置纯文本内容。
    /// </summary>
    public string PlainText { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置嵌套层级（0 为顶级）。
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 获取或设置子章节。
    /// </summary>
    public List<Fb2Section> Children { get; set; } = [];

    /// <summary>
    /// 获取一个值，指示此章节是否有子章节。
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <summary>
    /// 获取或设置章节中引用的图片 ID 列表。
    /// </summary>
    public List<string> ImageIds { get; set; } = [];

    /// <inheritdoc/>
    public override string ToString() => Title ?? $"Section {Id}";
}
