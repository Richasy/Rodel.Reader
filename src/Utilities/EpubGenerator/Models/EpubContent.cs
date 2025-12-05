// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// EPUB 内容集合（打包前的所有文件内容）.
/// </summary>
public sealed class EpubContent
{
    /// <summary>
    /// mimetype 文件内容.
    /// </summary>
    public required string Mimetype { get; init; }

    /// <summary>
    /// container.xml 内容.
    /// </summary>
    public required string ContainerXml { get; init; }

    /// <summary>
    /// content.opf 内容.
    /// </summary>
    public required string ContentOpf { get; init; }

    /// <summary>
    /// toc.ncx 内容（EPUB2 导航）.
    /// </summary>
    public required string TocNcx { get; init; }

    /// <summary>
    /// nav.xhtml 内容（EPUB3 导航，可选）.
    /// </summary>
    public string? NavDoc { get; init; }

    /// <summary>
    /// 封面页 XHTML（可选）.
    /// </summary>
    public string? CoverPage { get; init; }

    /// <summary>
    /// 标题页 XHTML.
    /// </summary>
    public required string TitlePage { get; init; }

    /// <summary>
    /// 目录页 XHTML（可选）.
    /// </summary>
    public string? TocPage { get; init; }

    /// <summary>
    /// 版权页 XHTML（可选）.
    /// </summary>
    public string? CopyrightPage { get; init; }

    /// <summary>
    /// 主样式表 CSS.
    /// </summary>
    public required string StyleSheet { get; init; }

    /// <summary>
    /// 章节内容字典（文件名 -> XHTML 内容）.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Chapters { get; init; }

    /// <summary>
    /// 封面信息（可选）.
    /// </summary>
    public CoverInfo? Cover { get; init; }

    /// <summary>
    /// 章节内的图片列表（可选）.
    /// </summary>
    public IReadOnlyList<ChapterImageInfo>? ChapterImages { get; init; }

    /// <summary>
    /// 嵌入的资源文件（图片、字体等，可选）.
    /// </summary>
    public IReadOnlyList<ResourceInfo>? Resources { get; init; }
}
