// Copyright (c) Richasy. All rights reserved.

using System.IO.Compression;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 表示已解析的 EPUB 书籍。
/// </summary>
public sealed class EpubBook : IDisposable
{
    private readonly ZipArchive? _archive;
    private readonly string _contentDirectoryPath;
    private bool _disposed;

    internal EpubBook(
        ZipArchive? archive,
        string? filePath,
        string contentDirectoryPath,
        EpubMetadata metadata,
        EpubCover? cover,
        List<EpubNavItem> navigation,
        List<EpubResource> readingOrder,
        List<EpubResource> resources)
    {
        _archive = archive;
        _contentDirectoryPath = contentDirectoryPath;
        FilePath = filePath;
        Metadata = metadata;
        Cover = cover;
        Navigation = navigation;
        ReadingOrder = readingOrder;
        Resources = resources;
        Images = resources.Where(r => r.IsImage).ToList();
    }

    /// <summary>
    /// 获取 EPUB 文件的路径，如果从流加载则为 null。
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// 获取书籍的元数据。
    /// </summary>
    public EpubMetadata Metadata { get; }

    /// <summary>
    /// 获取书籍的封面，如果不可用则为 null。
    /// </summary>
    public EpubCover? Cover { get; }

    /// <summary>
    /// 获取导航项（目录）。
    /// </summary>
    public IReadOnlyList<EpubNavItem> Navigation { get; }

    /// <summary>
    /// 获取书籍的阅读顺序（spine）。
    /// </summary>
    public IReadOnlyList<EpubResource> ReadingOrder { get; }

    /// <summary>
    /// 获取书籍中的所有资源。
    /// </summary>
    public IReadOnlyList<EpubResource> Resources { get; }

    /// <summary>
    /// 获取书籍中的所有图片资源。
    /// </summary>
    public IReadOnlyList<EpubResource> Images { get; }

    /// <summary>
    /// 读取资源的内容。
    /// </summary>
    /// <param name="resource">要读取的资源。</param>
    /// <returns>内容的字节数组。</returns>
    public byte[] ReadResourceContent(EpubResource resource)
    {
        return ReadResourceContentAsync(resource).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步读取资源的内容。
    /// </summary>
    /// <param name="resource">要读取的资源。</param>
    /// <returns>内容的字节数组。</returns>
    public async Task<byte[]> ReadResourceContentAsync(EpubResource resource)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_archive == null)
        {
            throw new InvalidOperationException("无法读取内容：存档不可用。");
        }

        var entry = _archive.GetEntry(resource.FullPath)
            ?? _archive.Entries.FirstOrDefault(e =>
                e.FullName.Equals(resource.FullPath, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            throw new EpubParseException($"在存档中未找到资源：{resource.FullPath}");
        }

        using var stream = entry.Open();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms).ConfigureAwait(false);
        return ms.ToArray();
    }

    /// <summary>
    /// 打开一个流来读取资源。
    /// </summary>
    /// <param name="resource">要读取的资源。</param>
    /// <returns>用于读取资源内容的流。</returns>
    public Stream OpenResourceStream(EpubResource resource)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_archive == null)
        {
            throw new InvalidOperationException("无法读取内容：存档不可用。");
        }

        var entry = _archive.GetEntry(resource.FullPath)
            ?? _archive.Entries.FirstOrDefault(e =>
                e.FullName.Equals(resource.FullPath, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            throw new EpubParseException($"在存档中未找到资源：{resource.FullPath}");
        }

        return entry.Open();
    }

    /// <summary>
    /// 将资源内容读取为字符串。
    /// </summary>
    /// <param name="resource">要读取的资源。</param>
    /// <returns>内容字符串。</returns>
    public async Task<string> ReadResourceContentAsStringAsync(EpubResource resource)
    {
        var bytes = await ReadResourceContentAsync(resource).ConfigureAwait(false);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 通过 href 查找资源。
    /// </summary>
    /// <param name="href">要搜索的 href。</param>
    /// <returns>找到的资源，如果未找到则为 null。</returns>
    public EpubResource? FindResourceByHref(string href)
    {
        // 如果存在锚点则移除
        var anchorIndex = href.IndexOf('#', StringComparison.Ordinal);
        var cleanHref = anchorIndex >= 0 ? href[..anchorIndex] : href;

        return Resources.FirstOrDefault(r =>
            r.Href.Equals(cleanHref, StringComparison.OrdinalIgnoreCase) ||
            r.FullPath.Equals(cleanHref, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 通过 ID 查找资源。
    /// </summary>
    /// <param name="id">要搜索的 ID。</param>
    /// <returns>找到的资源，如果未找到则为 null。</returns>
    public EpubResource? FindResourceById(string id)
    {
        return Resources.FirstOrDefault(r =>
            r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _archive?.Dispose();
            _disposed = true;
        }
    }
}
