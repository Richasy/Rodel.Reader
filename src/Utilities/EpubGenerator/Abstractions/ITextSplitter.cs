// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 文本分割器，用于将纯文本切分为章节.
/// </summary>
public interface ITextSplitter
{
    /// <summary>
    /// 将文本内容分割为章节列表.
    /// </summary>
    /// <param name="text">原始文本内容.</param>
    /// <param name="options">分割选项（可选）.</param>
    /// <returns>章节列表.</returns>
    IReadOnlyList<ChapterInfo> Split(ReadOnlySpan<char> text, SplitOptions? options = null);

    /// <summary>
    /// 将文本内容分割为章节列表（字符串重载）.
    /// </summary>
    /// <param name="text">原始文本内容.</param>
    /// <param name="options">分割选项（可选）.</param>
    /// <returns>章节列表.</returns>
    IReadOnlyList<ChapterInfo> Split(string text, SplitOptions? options = null)
        => Split(text.AsSpan(), options);

    /// <summary>
    /// 从文件读取并分割为章节列表.
    /// </summary>
    /// <param name="filePath">文件路径.</param>
    /// <param name="options">分割选项（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>章节列表.</returns>
    Task<IReadOnlyList<ChapterInfo>> SplitFromFileAsync(
        string filePath,
        SplitOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从流读取并分割为章节列表.
    /// </summary>
    /// <param name="stream">输入流.</param>
    /// <param name="options">分割选项（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>章节列表.</returns>
    Task<IReadOnlyList<ChapterInfo>> SplitFromStreamAsync(
        Stream stream,
        SplitOptions? options = null,
        CancellationToken cancellationToken = default);
}
