// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 批注/高亮.
/// </summary>
public sealed class Annotation
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// 开始位置.
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// 结束位置.
    /// </summary>
    public string? EndPosition { get; set; }

    /// <summary>
    /// 章节 ID.
    /// </summary>
    public string? ChapterId { get; set; }

    /// <summary>
    /// 章节标题.
    /// </summary>
    public string? ChapterTitle { get; set; }

    /// <summary>
    /// 页码.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// 选中的文本.
    /// </summary>
    public string? SelectedText { get; set; }

    /// <summary>
    /// 批注内容.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 批注类型.
    /// </summary>
    public AnnotationType Type { get; set; }

    /// <summary>
    /// 颜色.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// 样式 (JSON).
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// PDF 矩形区域 (JSON).
    /// </summary>
    public string? RectJson { get; set; }

    /// <summary>
    /// 手绘路径.
    /// </summary>
    public string? SvgPath { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新时间.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Annotation annotation && Id == annotation.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
