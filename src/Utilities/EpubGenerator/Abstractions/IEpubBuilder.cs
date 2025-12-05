// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// EPUB 构建器.
/// </summary>
public interface IEpubBuilder
{
    /// <summary>
    /// 从章节列表构建 EPUB 并写入流.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <param name="chapters">章节列表.</param>
    /// <param name="outputStream">输出流.</param>
    /// <param name="options">生成选项（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task BuildAsync(
        EpubMetadata metadata,
        IReadOnlyList<ChapterInfo> chapters,
        Stream outputStream,
        EpubOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从章节列表构建 EPUB 并保存到文件.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <param name="chapters">章节列表.</param>
    /// <param name="outputPath">输出文件路径.</param>
    /// <param name="options">生成选项（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task BuildToFileAsync(
        EpubMetadata metadata,
        IReadOnlyList<ChapterInfo> chapters,
        string outputPath,
        EpubOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从章节列表构建 EPUB 并返回字节数组.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <param name="chapters">章节列表.</param>
    /// <param name="options">生成选项（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>EPUB 文件的字节数组.</returns>
    Task<byte[]> BuildToBytesAsync(
        EpubMetadata metadata,
        IReadOnlyList<ChapterInfo> chapters,
        EpubOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成 EPUB 内容集合（不打包）.
    /// </summary>
    /// <param name="metadata">书籍元数据.</param>
    /// <param name="chapters">章节列表.</param>
    /// <param name="options">生成选项（可选）.</param>
    /// <returns>EPUB 内容集合.</returns>
    EpubContent GenerateContent(
        EpubMetadata metadata,
        IReadOnlyList<ChapterInfo> chapters,
        EpubOptions? options = null);
}
