// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 段评列表结果.
/// </summary>
public sealed class CommentListResult
{
    /// <summary>
    /// 评论列表.
    /// </summary>
    public required IReadOnlyList<Comment> Comments { get; init; }

    /// <summary>
    /// 段落索引.
    /// </summary>
    public int ParagraphIndex { get; init; }

    /// <summary>
    /// 是否还有更多评论.
    /// </summary>
    public bool HasMore { get; init; }

    /// <summary>
    /// 下一页偏移量.
    /// </summary>
    public string? NextOffset { get; init; }

    /// <summary>
    /// 该段落的原始内容.
    /// </summary>
    public string? ParagraphContent { get; init; }
}
