// Copyright (c) Reader Copilot. All rights reserved.

using System.IO.Compression;
using System.Text;

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 基于ZIP的EPUB打包器实现.
/// </summary>
/// <remarks>
/// 此打包器接收已生成好的 <see cref="EpubContent"/>，并将其打包成符合 EPUB 规范的 ZIP 文件.
/// EPUB 规范要求 mimetype 文件必须是压缩包中的第一个文件，且不能压缩.
/// </remarks>
internal sealed class ZipEpubPackager : IEpubPackager
{
    private const string MetaInfFolder = "META-INF";
    private const string OebpsFolder = "OEBPS";
    private const string ContainerFileName = "container.xml";
    private const string ContentOpfFileName = "content.opf";
    private const string TocNcxFileName = "toc.ncx";
    private const string NavFileName = "nav.xhtml";
    private const string StylesFolder = "Styles";
    private const string ImagesFolder = "Images";
    private const string FontsFolder = "Fonts";
    private const string TextFolder = "Text";
    private const string DefaultStyleFileName = "main.css";
    private const string MimeTypeFileName = "mimetype";

    /// <inheritdoc/>
    public async Task PackageAsync(
        EpubContent content,
        Stream outputStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(outputStream);

        using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true);

        // 1. 添加 mimetype（必须是第一个文件，且不压缩）
        await AddTextEntryAsync(archive, MimeTypeFileName, content.Mimetype, CompressionLevel.NoCompression, cancellationToken).ConfigureAwait(false);

        // 2. 添加 META-INF/container.xml
        await AddTextEntryAsync(archive, $"{MetaInfFolder}/{ContainerFileName}", content.ContainerXml, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);

        // 3. 添加 OEBPS/content.opf
        await AddTextEntryAsync(archive, $"{OebpsFolder}/{ContentOpfFileName}", content.ContentOpf, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);

        // 4. 添加 OEBPS/toc.ncx
        await AddTextEntryAsync(archive, $"{OebpsFolder}/{TocNcxFileName}", content.TocNcx, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);

        // 5. 添加 OEBPS/nav.xhtml（EPUB 3 导航，可选）
        if (!string.IsNullOrEmpty(content.NavDoc))
        {
            await AddTextEntryAsync(archive, $"{OebpsFolder}/{NavFileName}", content.NavDoc, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);
        }

        // 6. 添加样式表
        await AddTextEntryAsync(archive, $"{OebpsFolder}/{StylesFolder}/{DefaultStyleFileName}", content.StyleSheet, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);

        // 7. 添加封面页（可选）
        if (!string.IsNullOrEmpty(content.CoverPage))
        {
            await AddTextEntryAsync(archive, $"{OebpsFolder}/{TextFolder}/cover.xhtml", content.CoverPage, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);
        }

        // 8. 添加封面图片（可选）
        if (content.Cover != null && content.Cover.ImageData.Length > 0)
        {
            var coverPath = $"{OebpsFolder}/{ImagesFolder}/{content.Cover.FileName}";
            await AddBinaryEntryAsync(archive, coverPath, content.Cover.ImageData, cancellationToken).ConfigureAwait(false);
        }

        // 9. 添加标题页
        await AddTextEntryAsync(archive, $"{OebpsFolder}/{TextFolder}/titlepage.xhtml", content.TitlePage, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);

        // 10. 添加目录页（可选）
        if (!string.IsNullOrEmpty(content.TocPage))
        {
            await AddTextEntryAsync(archive, $"{OebpsFolder}/{TextFolder}/toc.xhtml", content.TocPage, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);
        }

        // 11. 添加版权页（可选）
        if (!string.IsNullOrEmpty(content.CopyrightPage))
        {
            await AddTextEntryAsync(archive, $"{OebpsFolder}/{TextFolder}/copyright.xhtml", content.CopyrightPage, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);
        }

        // 12. 添加章节内容
        foreach (var (fileName, chapterXhtml) in content.Chapters)
        {
            var chapterPath = $"{OebpsFolder}/{TextFolder}/{fileName}";
            await AddTextEntryAsync(archive, chapterPath, chapterXhtml, CompressionLevel.Optimal, cancellationToken).ConfigureAwait(false);
        }

        // 13. 添加章节图片
        if (content.ChapterImages != null)
        {
            foreach (var image in content.ChapterImages)
            {
                var imagePath = $"{OebpsFolder}/{ImagesFolder}/{image.FileName}";
                await AddBinaryEntryAsync(archive, imagePath, image.ImageData, cancellationToken).ConfigureAwait(false);
            }
        }

        // 14. 添加资源文件（图片、字体等）
        if (content.Resources != null)
        {
            foreach (var resource in content.Resources)
            {
                var folder = resource.Type switch
                {
                    ResourceType.Image => ImagesFolder,
                    ResourceType.Font => FontsFolder,
                    _ => "Misc"
                };

                var resourcePath = $"{OebpsFolder}/{folder}/{resource.FileName}";
                await AddBinaryEntryAsync(archive, resourcePath, resource.Data, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc/>
    public async Task PackageToFileAsync(
        EpubContent content,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.Asynchronous);

        await PackageAsync(content, fileStream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<byte[]> PackageToBytesAsync(
        EpubContent content,
        CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await PackageAsync(content, memoryStream, cancellationToken).ConfigureAwait(false);
        return memoryStream.ToArray();
    }

    private static async Task AddTextEntryAsync(
        ZipArchive archive,
        string path,
        string content,
        CompressionLevel compressionLevel,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(path, compressionLevel);
        await using var stream = entry.Open();
        var bytes = Encoding.UTF8.GetBytes(content);
        await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
    }

    private static async Task AddBinaryEntryAsync(
        ZipArchive archive,
        string path,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }
}
