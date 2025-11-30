// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// 表示已解析的 Mobi 书籍。
/// </summary>
public sealed class MobiBook : IDisposable
{
    private readonly Stream? _stream;
    private readonly Func<int, Task<byte[]>> _imageLoader;
    private bool _disposed;

    internal MobiBook(
        Stream? stream,
        string? filePath,
        MobiMetadata metadata,
        MobiCover? cover,
        List<MobiNavItem> navigation,
        List<MobiImage> images,
        Func<int, Task<byte[]>> imageLoader)
    {
        _stream = stream;
        _imageLoader = imageLoader;
        FilePath = filePath;
        Metadata = metadata;
        Cover = cover;
        Navigation = navigation;
        Images = images;
    }

    /// <summary>
    /// 获取 Mobi 文件的路径，如果从流加载则为 null。
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// 获取书籍的元数据。
    /// </summary>
    public MobiMetadata Metadata { get; }

    /// <summary>
    /// 获取书籍的封面，如果不可用则为 null。
    /// </summary>
    public MobiCover? Cover { get; }

    /// <summary>
    /// 获取导航项（目录）。
    /// </summary>
    public IReadOnlyList<MobiNavItem> Navigation { get; }

    /// <summary>
    /// 获取书籍中的所有图片资源。
    /// </summary>
    public IReadOnlyList<MobiImage> Images { get; }

    /// <summary>
    /// 读取指定索引的图片内容。
    /// </summary>
    /// <param name="imageIndex">图片索引（从 1 开始）。</param>
    /// <returns>图片的字节数组。</returns>
    public byte[] ReadImageContent(int imageIndex)
    {
        return ReadImageContentAsync(imageIndex).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步读取指定索引的图片内容。
    /// </summary>
    /// <param name="imageIndex">图片索引（从 1 开始）。</param>
    /// <returns>图片的字节数组。</returns>
    public async Task<byte[]> ReadImageContentAsync(int imageIndex)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _imageLoader(imageIndex).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取指定图片的内容。
    /// </summary>
    /// <param name="image">要读取的图片。</param>
    /// <returns>图片的字节数组。</returns>
    public byte[] ReadImageContent(MobiImage image)
    {
        return ReadImageContent(image.Index);
    }

    /// <summary>
    /// 异步读取指定图片的内容。
    /// </summary>
    /// <param name="image">要读取的图片。</param>
    /// <returns>图片的字节数组。</returns>
    public Task<byte[]> ReadImageContentAsync(MobiImage image)
    {
        return ReadImageContentAsync(image.Index);
    }

    /// <summary>
    /// 通过索引查找图片。
    /// </summary>
    /// <param name="index">要搜索的索引。</param>
    /// <returns>找到的图片，如果未找到则为 null。</returns>
    public MobiImage? FindImageByIndex(int index)
    {
        return Images.FirstOrDefault(i => i.Index == index);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _stream?.Dispose();
            _disposed = true;
        }
    }
}
