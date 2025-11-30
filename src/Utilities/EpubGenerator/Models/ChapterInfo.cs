// Copyright (c) Reader Copilot. All rights reserved.

using System.Runtime.CompilerServices;

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 章节信息.
/// </summary>
public sealed class ChapterInfo
{
    private string? _fileName;

    /// <summary>
    /// 章节索引（从 0 开始）.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// 章节标题.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// 章节内容（纯文本或 HTML）.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 内容是否已经是 HTML 格式.
    /// </summary>
    public bool IsHtml { get; init; }

    /// <summary>
    /// 子章节/锚点列表（可选）.
    /// </summary>
    public IReadOnlyList<AnchorInfo>? Anchors { get; init; }

    /// <summary>
    /// 章节内的图片列表（可选）.
    /// </summary>
    /// <remarks>
    /// 图片将根据其 Offset 属性插入到文本的相应位置.
    /// </remarks>
    public IReadOnlyList<ChapterImageInfo>? Images { get; init; }

    /// <summary>
    /// 生成的文件名（不含扩展名）.
    /// </summary>
    /// <remarks>
    /// 格式为 chapter + 索引数字，数字至少 3 位，不足补零.
    /// 例如: chapter000, chapter001, ..., chapter999, chapter1000, ...
    /// </remarks>
    public string FileName => _fileName ??= $"chapter{Index:D3}";

    /// <summary>
    /// 获取内容的只读跨度.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetContentSpan() => Content.AsSpan();

    /// <summary>
    /// 获取标题的只读跨度.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetTitleSpan() => Title.AsSpan();
}
