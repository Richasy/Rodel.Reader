// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 文本分割选项.
/// </summary>
public sealed class SplitOptions
{
    /// <summary>
    /// 章节标题匹配正则表达式.
    /// </summary>
    public string? ChapterPattern { get; init; }

    /// <summary>
    /// 额外的章节关键词（如：序、前言、后记）.
    /// </summary>
    public IReadOnlyList<string>? ExtraChapterKeywords { get; init; }

    /// <summary>
    /// 章节标题最大长度.
    /// </summary>
    public int MaxTitleLength { get; init; } = 50;

    /// <summary>
    /// 默认书名（用于第一章之前的内容）.
    /// </summary>
    public string? DefaultFirstChapterTitle { get; init; }

    /// <summary>
    /// 是否移除空行.
    /// </summary>
    public bool RemoveEmptyLines { get; init; }

    /// <summary>
    /// 是否修剪每行首尾空白.
    /// </summary>
    public bool TrimLines { get; init; } = true;
}
