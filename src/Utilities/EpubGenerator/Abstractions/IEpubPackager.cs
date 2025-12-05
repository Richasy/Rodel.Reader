// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// EPUB 打包器.
/// </summary>
public interface IEpubPackager
{
    /// <summary>
    /// 将所有内容打包为 EPUB 并写入流.
    /// </summary>
    /// <param name="content">EPUB 内容集合.</param>
    /// <param name="outputStream">输出流.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task PackageAsync(EpubContent content, Stream outputStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将所有内容打包为 EPUB 文件.
    /// </summary>
    /// <param name="content">EPUB 内容集合.</param>
    /// <param name="outputPath">输出文件路径.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task PackageToFileAsync(EpubContent content, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将所有内容打包为 EPUB 并返回字节数组.
    /// </summary>
    /// <param name="content">EPUB 内容集合.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>EPUB 文件的字节数组.</returns>
    Task<byte[]> PackageToBytesAsync(EpubContent content, CancellationToken cancellationToken = default);
}
