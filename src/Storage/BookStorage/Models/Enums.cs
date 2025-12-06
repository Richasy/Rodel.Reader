// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书籍格式类型.
/// </summary>
public enum BookFormat
{
    /// <summary>EPUB 电子书.</summary>
    Epub = 0,

    /// <summary>PDF 文档.</summary>
    Pdf = 1,

    /// <summary>Mobi/AZW3 格式.</summary>
    Mobi = 2,

    /// <summary>FB2 格式.</summary>
    Fb2 = 3,

    /// <summary>漫画压缩包 (CBZ/CBR/ZIP).</summary>
    ComicArchive = 4,

    /// <summary>在线网文.</summary>
    WebNovel = 5,
}

/// <summary>
/// 书籍追踪状态.
/// </summary>
public enum BookTrackStatus
{
    /// <summary>未分类.</summary>
    None = 0,

    /// <summary>想读.</summary>
    WantToRead = 1,

    /// <summary>在读.</summary>
    Reading = 2,

    /// <summary>已读完.</summary>
    Finished = 3,

    /// <summary>搁置.</summary>
    OnHold = 4,

    /// <summary>弃读.</summary>
    Dropped = 5,
}

/// <summary>
/// 书籍来源类型.
/// </summary>
public enum BookSourceType
{
    /// <summary>本地文件导入.</summary>
    Local = 0,

    /// <summary>番茄小说下载.</summary>
    FanQie = 1,

    /// <summary>开源阅读下载.</summary>
    Legado = 2,
}

/// <summary>
/// 批注类型.
/// </summary>
public enum AnnotationType
{
    /// <summary>高亮.</summary>
    Highlight = 0,

    /// <summary>下划线.</summary>
    Underline = 1,

    /// <summary>批注.</summary>
    Note = 2,

    /// <summary>手绘.</summary>
    Drawing = 3,

    /// <summary>图章.</summary>
    Stamp = 4,
}
