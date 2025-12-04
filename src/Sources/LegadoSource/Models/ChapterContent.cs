// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Models;

/// <summary>
/// 章节内容.
/// </summary>
public sealed class ChapterContent
{
    /// <summary>
    /// 书籍链接.
    /// </summary>
    public string BookUrl { get; set; } = string.Empty;

    /// <summary>
    /// 章节索引.
    /// </summary>
    public int ChapterIndex { get; set; }

    /// <summary>
    /// 章节标题.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 章节内容（HTML 格式）.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
