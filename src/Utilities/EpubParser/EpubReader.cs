// Copyright (c) Richasy. All rights reserved.

using System.IO.Compression;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// EPUB 文件解析的主入口。
/// </summary>
public static class EpubReader
{
    /// <summary>
    /// 从文件路径解析 EPUB 文件。
    /// </summary>
    /// <param name="filePath">EPUB 文件路径。</param>
    /// <returns>解析后的 EPUB 书籍。</returns>
    public static EpubBook Read(string filePath)
    {
        return ReadAsync(filePath).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步从文件路径解析 EPUB 文件。
    /// </summary>
    /// <param name="filePath">EPUB 文件路径。</param>
    /// <returns>解析后的 EPUB 书籍。</returns>
    public static async Task<EpubBook> ReadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("未找到 EPUB 文件", filePath);
        }

        var archive = ZipFile.OpenRead(filePath);

        try
        {
            return await ParseArchiveAsync(archive, filePath).ConfigureAwait(false);
        }
        catch
        {
            archive.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 从流解析 EPUB 文件。
    /// </summary>
    /// <param name="stream">包含 EPUB 文件的流。</param>
    /// <returns>解析后的 EPUB 书籍。</returns>
    public static EpubBook Read(Stream stream)
    {
        return ReadAsync(stream).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步从流解析 EPUB 文件。
    /// </summary>
    /// <param name="stream">包含 EPUB 文件的流。</param>
    /// <returns>解析后的 EPUB 书籍。</returns>
    public static async Task<EpubBook> ReadAsync(Stream stream)
    {
        var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        try
        {
            return await ParseArchiveAsync(archive, null).ConfigureAwait(false);
        }
        catch
        {
            archive.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 从 ZipArchive 解析 EPUB。
    /// </summary>
    private static async Task<EpubBook> ParseArchiveAsync(ZipArchive archive, string? filePath)
    {
        // 读取 container 以获取根文件路径
        var rootFilePath = await ContainerReader.GetRootFilePathAsync(archive).ConfigureAwait(false);
        var contentDirectoryPath = PathHelper.GetDirectoryPath(rootFilePath);

        // 解析 OPF 包
        var packageData = await PackageReader.ParseAsync(archive, rootFilePath, contentDirectoryPath)
            .ConfigureAwait(false);

        // 解析导航
        var navigation = await NavigationParser.ParseAsync(
            archive,
            packageData.Resources,
            contentDirectoryPath,
            packageData.SpineElement,
            packageData.MetadataElement).ConfigureAwait(false);

        // 如果找到封面则创建封面包装器
        EpubCover? cover = null;
        if (packageData.CoverResource != null)
        {
            var coverResource = packageData.CoverResource;
            cover = new EpubCover(coverResource, async () =>
            {
                var entry = archive.GetEntry(coverResource.FullPath)
                    ?? archive.Entries.FirstOrDefault(e =>
                        e.FullName.Equals(coverResource.FullPath, StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                {
                    return [];
                }

                using var stream = entry.Open();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms).ConfigureAwait(false);
                return ms.ToArray();
            });
        }

        return new EpubBook(
            archive,
            filePath,
            contentDirectoryPath,
            packageData.Metadata,
            cover,
            navigation,
            packageData.ReadingOrder,
            packageData.Resources);
    }
}
