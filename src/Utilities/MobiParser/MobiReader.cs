// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// Mobi 文件解析的主入口。
/// </summary>
public static class MobiReader
{
    /// <summary>
    /// 从文件路径解析 Mobi 文件。
    /// </summary>
    /// <param name="filePath">Mobi 文件路径（.mobi, .azw, .azw3）。</param>
    /// <returns>解析后的 Mobi 书籍。</returns>
    public static MobiBook Read(string filePath)
    {
        return ReadAsync(filePath).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步从文件路径解析 Mobi 文件。
    /// </summary>
    /// <param name="filePath">Mobi 文件路径（.mobi, .azw, .azw3）。</param>
    /// <returns>解析后的 Mobi 书籍。</returns>
    public static async Task<MobiBook> ReadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("未找到 Mobi 文件", filePath);
        }

        var stream = File.OpenRead(filePath);

        try
        {
            return await ParseStreamAsync(stream, filePath).ConfigureAwait(false);
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 从流解析 Mobi 文件。
    /// </summary>
    /// <param name="stream">包含 Mobi 文件的流。</param>
    /// <returns>解析后的 Mobi 书籍。</returns>
    public static MobiBook Read(Stream stream)
    {
        return ReadAsync(stream).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步从流解析 Mobi 文件。
    /// </summary>
    /// <param name="stream">包含 Mobi 文件的流。</param>
    /// <returns>解析后的 Mobi 书籍。</returns>
    public static async Task<MobiBook> ReadAsync(Stream stream)
    {
        // 如果流不支持 Seek，复制到 MemoryStream
        if (!stream.CanSeek)
        {
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            return await ParseStreamAsync(ms, null).ConfigureAwait(false);
        }

        return await ParseStreamAsync(stream, null).ConfigureAwait(false);
    }

    /// <summary>
    /// 解析 Mobi 流。
    /// </summary>
    private static async Task<MobiBook> ParseStreamAsync(Stream stream, string? filePath)
    {
        // 解析 PalmDB 头
        var (name, records) = await PalmDbParser.ParseAsync(stream).ConfigureAwait(false);

        if (records.Count == 0)
        {
            throw new MobiParseException("Mobi 文件中没有记录");
        }

        // 读取记录 0（包含 PalmDOC 头、MOBI 头和 EXTH 头）
        var record0 = await ReadRecordAsync(stream, records, 0).ConfigureAwait(false);

        // 解析 MOBI 头
        var mobiHeader = MobiHeaderParser.Parse(record0);

        // 解析 EXTH 头获取元数据
        var exthRecords = ExthParser.Parse(record0, mobiHeader);
        var encoding = mobiHeader.GetTextEncoding();
        var metadata = ExthParser.ExtractMetadata(exthRecords, encoding);

        // 设置标题
        var fullName = MobiHeaderParser.ExtractFullName(record0, mobiHeader);
        metadata.Title = fullName ?? name;

        // 设置语言
        if (string.IsNullOrEmpty(metadata.Language) && mobiHeader.LanguageCode != 0)
        {
            metadata.Language = LanguageCodeConverter.ToLanguageTag(mobiHeader.LanguageCode);
        }

        // 设置 Mobi 版本
        metadata.MobiVersion = (int)mobiHeader.FileVersion;

        // 获取封面偏移
        var coverOffset = ExthParser.GetCoverOffset(exthRecords);

        // 解析图片列表
        var images = await ParseImagesAsync(stream, records, mobiHeader).ConfigureAwait(false);

        // 创建封面
        MobiCover? cover = null;
        if (coverOffset.HasValue && mobiHeader.FirstImageIndex > 0)
        {
            var coverImageIndex = (int)(mobiHeader.FirstImageIndex + coverOffset.Value);
            var coverImage = images.FirstOrDefault(i => i.Index == coverImageIndex);
            if (coverImage != null)
            {
                cover = new MobiCover(coverImage.MediaType, async () =>
                {
                    return await ReadRecordAsync(stream, records, coverImageIndex).ConfigureAwait(false);
                });
            }
        }

        // 如果没有通过 EXTH 找到封面，尝试使用第一张图片
        cover ??= TryFindCover(stream, records, mobiHeader, images);

        // 解析目录
        var navigation = await ParseNavigationAsync(stream, records, mobiHeader).ConfigureAwait(false);

        // 创建图片加载器
        async Task<byte[]> ImageLoaderAsync(int imageIndex)
        {
            var image = images.FirstOrDefault(i => i.Index == imageIndex);
            if (image == null)
            {
                throw new MobiParseException($"未找到图片索引: {imageIndex}");
            }

            return await ReadRecordAsync(stream, records, imageIndex).ConfigureAwait(false);
        }

        return new MobiBook(
            stream,
            filePath,
            metadata,
            cover,
            navigation,
            images,
            ImageLoaderAsync);
    }

    /// <summary>
    /// 读取指定索引的记录。
    /// </summary>
    private static async Task<byte[]> ReadRecordAsync(Stream stream, List<PalmDbRecord> records, int index)
    {
        if (index < 0 || index >= records.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "记录索引超出范围");
        }

        var record = records[index];
        var nextOffset = index + 1 < records.Count ? records[index + 1].Offset : (uint)stream.Length;
        var length = (int)(nextOffset - record.Offset);

        stream.Position = record.Offset;
        var data = new byte[length];
        await stream.ReadExactlyAsync(data, 0, length).ConfigureAwait(false);
        return data;
    }

    /// <summary>
    /// 解析图片列表。
    /// </summary>
    private static async Task<List<MobiImage>> ParseImagesAsync(Stream stream, List<PalmDbRecord> records, MobiHeader mobiHeader)
    {
        var images = new List<MobiImage>();

        if (mobiHeader.FirstImageIndex == 0 || mobiHeader.FirstImageIndex >= records.Count)
        {
            return images;
        }

        // 从第一个图片记录开始遍历
        for (var i = (int)mobiHeader.FirstImageIndex; i < records.Count; i++)
        {
            try
            {
                var data = await ReadRecordAsync(stream, records, i).ConfigureAwait(false);
                var mediaType = ImageDetector.DetectMediaType(data);

                if (mediaType != null)
                {
                    images.Add(new MobiImage
                    {
                        Index = i,
                        MediaType = mediaType,
                        Size = data.Length,
                    });
                }
            }
            catch
            {
                // 忽略无法读取的记录
            }
        }

        return images;
    }

    /// <summary>
    /// 尝试查找封面。
    /// </summary>
    private static MobiCover? TryFindCover(Stream stream, List<PalmDbRecord> records, MobiHeader mobiHeader, List<MobiImage> images)
    {
        // 尝试使用第一张图片作为封面
        if (images.Count > 0)
        {
            var firstImage = images[0];
            return new MobiCover(firstImage.MediaType, async () => await ReadRecordAsync(stream, records, firstImage.Index).ConfigureAwait(false));
        }

        return null;
    }

    /// <summary>
    /// 解析目录。
    /// </summary>
    private static async Task<List<MobiNavItem>> ParseNavigationAsync(Stream stream, List<PalmDbRecord> records, MobiHeader mobiHeader)
    {
        var navigation = new List<MobiNavItem>();

        // 尝试从文本内容中提取目录
        try
        {
            var textContent = await ExtractTextContentAsync(stream, records, mobiHeader).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(textContent))
            {
                navigation = NavigationExtractor.ExtractFromHtml(textContent);
            }
        }
        catch
        {
            // 忽略解析错误
        }

        return navigation;
    }

    /// <summary>
    /// 提取文本内容（用于目录解析）。
    /// </summary>
    private static async Task<string> ExtractTextContentAsync(Stream stream, List<PalmDbRecord> records, MobiHeader mobiHeader)
    {
        if (mobiHeader.RecordCount == 0)
        {
            return string.Empty;
        }

        var textRecordCount = Math.Min((int)mobiHeader.RecordCount, 10); // 只读取前几个记录用于目录提取
        var allBytes = new List<byte>();

        for (var i = 1; i <= textRecordCount && i < records.Count; i++)
        {
            try
            {
                var data = await ReadRecordAsync(stream, records, i).ConfigureAwait(false);

                // 根据压缩类型解压缩
                byte[] decompressed;
                switch (mobiHeader.Compression)
                {
                    case PalmDocCompression.NoCompression:
                        decompressed = data;
                        break;
                    case PalmDocCompression.PalmDoc:
                        decompressed = PalmDocCompression.Decompress(data);
                        break;
                    default:
                        // 不支持的压缩格式，跳过
                        continue;
                }

                allBytes.AddRange(decompressed);
            }
            catch
            {
                // 忽略解析错误
            }
        }

        if (allBytes.Count == 0)
        {
            return string.Empty;
        }

        var encoding = mobiHeader.GetTextEncoding();
        return encoding.GetString(allBytes.ToArray());
    }
}
