// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 表示 EPUB 文件中的资源。
/// </summary>
public sealed class EpubResource
{
    /// <summary>
    /// 获取或设置资源在 manifest 中声明的 ID。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置资源相对于内容目录的路径。
    /// </summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置资源在 EPUB 存档中的绝对路径。
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置资源的媒体类型。
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// 获取一个值，指示此资源是否为图片。
    /// </summary>
    public bool IsImage => MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 获取一个值，指示此资源是否为 HTML/XHTML 文档。
    /// </summary>
    public bool IsHtml => MediaType.Contains("html", StringComparison.OrdinalIgnoreCase) ||
                          MediaType.Contains("xml", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 获取一个值，指示此资源是否为 CSS 样式表。
    /// </summary>
    public bool IsCss => MediaType.Contains("css", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 获取或设置 EPUB 3 的附加属性。
    /// </summary>
    public List<string> Properties { get; set; } = [];

    /// <inheritdoc/>
    public override string ToString() => $"{Id}: {Href} ({MediaType})";
}
