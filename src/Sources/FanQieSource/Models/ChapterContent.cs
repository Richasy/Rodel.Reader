// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models;

/// <summary>
/// 章节内容（解密后）.
/// </summary>
public sealed record ChapterContent
{
    /// <summary>
    /// 章节 ID（item_id）.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// 所属书籍 ID.
    /// </summary>
    public required string BookId { get; init; }

    /// <summary>
    /// 书籍标题.
    /// </summary>
    public required string BookTitle { get; init; }

    /// <summary>
    /// 章节标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 解密并清洗后的纯文本内容.
    /// </summary>
    public required string TextContent { get; init; }

    /// <summary>
    /// 解密并清洗后的 HTML 内容（用于 Epub）.
    /// </summary>
    public required string HtmlContent { get; init; }

    /// <summary>
    /// 字数.
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// 章节序号.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// 卷名称.
    /// </summary>
    public string? VolumeName { get; init; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    public DateTimeOffset? PublishTime { get; init; }

    /// <summary>
    /// 章节内图片列表.
    /// </summary>
    public IReadOnlyList<ChapterImage>? Images { get; init; }
}

/// <summary>
/// 章节图片.
/// </summary>
public sealed record ChapterImage
{
    /// <summary>
    /// 图片 URL.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// 在文本中的位置（字符偏移）.
    /// </summary>
    public int? Offset { get; init; }
}
