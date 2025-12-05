// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Components.Legado.Internal;

/// <summary>
/// 章节内容及其关联的图片资源.
/// </summary>
internal sealed record ChapterWithImages
{
    /// <summary>
    /// 章节 body 内部的 HTML 内容片段（不包含完整 HTML 结构）.
    /// </summary>
    public required string BodyContent { get; init; }

    /// <summary>
    /// 章节中的图片资源.
    /// </summary>
    public IReadOnlyList<ImageResource>? Images { get; init; }
}

/// <summary>
/// 图片资源.
/// </summary>
internal sealed record ImageResource
{
    /// <summary>
    /// 图片 ID（文件名，不含路径）.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 图片数据.
    /// </summary>
    public required byte[] Data { get; init; }

    /// <summary>
    /// 媒体类型.
    /// </summary>
    public required string MediaType { get; init; }
}

/// <summary>
/// Legado EPUB 分析器，从 EPUB 文件中提取 Legado 标记信息.
/// </summary>
internal sealed partial class LegadoEpubAnalyzer
{
    private const string MetaBookUrlName = "legado:book-url";
    private const string MetaBookSourceName = "legado:book-source";
    private const string MetaServerUrlName = "legado:server-url";
    private const string MetaSyncTimeName = "legado:sync-time";
    private const string MetaTocHashName = "legado:toc-hash";
    private const string MetaFailedChaptersName = "legado:failed-chapters";

    /// <summary>
    /// 从 EPUB 文件中提取 Legado 书籍信息.
    /// </summary>
    /// <param name="epubPath">EPUB 文件路径.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>Legado 书籍信息，如果不是 Legado 书籍则返回 null.</returns>
    public static async Task<LegadoBookInfo?> AnalyzeAsync(
        string epubPath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(epubPath))
        {
            return null;
        }

        try
        {
            using var book = await EpubReader.ReadAsync(epubPath).ConfigureAwait(false);
            return await ExtractInfoAsync(book, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从已打开的 EPUB 书籍中提取 Legado 书籍信息.
    /// </summary>
    /// <param name="book">EPUB 书籍.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>Legado 书籍信息，如果不是 Legado 书籍则返回 null.</returns>
    public static async Task<LegadoBookInfo?> ExtractInfoAsync(
        EpubBook book,
        CancellationToken cancellationToken = default)
    {
        // 1. 从元数据提取 legado:book-url
        var bookUrl = GetMetaValue(book, MetaBookUrlName);

        if (string.IsNullOrEmpty(bookUrl))
        {
            // 尝试从 Identifier 提取
            if (book.Metadata.Identifier?.StartsWith("legado-", StringComparison.OrdinalIgnoreCase) == true)
            {
                bookUrl = book.Metadata.Identifier["legado-".Length..];
            }
        }

        if (string.IsNullOrEmpty(bookUrl))
        {
            return null; // 不是 Legado 书籍
        }

        // 2. 提取其他元数据
        var bookSource = GetMetaValue(book, MetaBookSourceName) ?? string.Empty;
        var serverUrl = GetMetaValue(book, MetaServerUrlName) ?? string.Empty;
        var syncTimeStr = GetMetaValue(book, MetaSyncTimeName);
        var tocHash = GetMetaValue(book, MetaTocHashName);
        var failedChaptersStr = GetMetaValue(book, MetaFailedChaptersName);

        var failedIndexes = failedChaptersStr?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToList() ?? [];

        // 3. 遍历阅读顺序，提取章节索引
        var downloadedIndexes = new List<int>();

        foreach (var resource in book.ReadingOrder)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 跳过非章节文件（如导航、目录等）
            if (!resource.Href.Contains("chapter", StringComparison.OrdinalIgnoreCase) ||
                resource.Href.Contains("nav", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // 从 HTML 内容中提取章节索引和状态
            try
            {
                var html = await book.ReadResourceContentAsStringAsync(resource).ConfigureAwait(false);

                // 从内容中提取章节索引
                var chapterIndex = ExtractChapterIndex(html);
                if (!chapterIndex.HasValue)
                {
                    continue;
                }

                // 检查是否为元数据中标记的失败章节
                if (failedIndexes.Contains(chapterIndex.Value))
                {
                    continue;
                }

                var status = ExtractStatus(html);

                if (status == ChapterStatus.Downloaded || status == ChapterStatus.Volume)
                {
                    downloadedIndexes.Add(chapterIndex.Value);
                }
                else if (status == ChapterStatus.Failed && !failedIndexes.Contains(chapterIndex.Value))
                {
                    failedIndexes.Add(chapterIndex.Value);
                }
            }
            catch
            {
                // 无法读取内容，跳过
            }
        }

        return new LegadoBookInfo
        {
            BookUrl = bookUrl,
            BookSource = bookSource,
            ServerUrl = serverUrl,
            Title = book.Metadata.Title ?? string.Empty,
            Author = book.Metadata.Authors.FirstOrDefault(),
            Description = book.Metadata.Description,
            LastSyncTime = DateTimeOffset.TryParse(syncTimeStr, out var dt) ? dt : null,
            TocHash = tocHash,
            DownloadedChapterIndexes = downloadedIndexes,
            FailedChapterIndexes = failedIndexes,
        };
    }

    /// <summary>
    /// 从 EPUB 文件中读取指定章节的内容及其图片资源.
    /// </summary>
    /// <param name="book">EPUB 书籍.</param>
    /// <param name="chapterIndex">章节索引.</param>
    /// <returns>章节内容及图片资源，如果未找到则返回 null.</returns>
    public static async Task<ChapterWithImages?> ReadChapterContentAsync(EpubBook book, int chapterIndex)
    {
        // 遍历所有章节资源，通过 HTML 内容中的章节索引标记来匹配
        foreach (var resource in book.ReadingOrder)
        {
            // 跳过非章节文件
            if (!resource.Href.Contains("chapter", StringComparison.OrdinalIgnoreCase) ||
                resource.Href.Contains("nav", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                var html = await book.ReadResourceContentAsStringAsync(resource).ConfigureAwait(false);
                var extractedIndex = ExtractChapterIndex(html);

                if (extractedIndex == chapterIndex)
                {
                    // 提取 body 内部的内容（不包含完整 HTML 结构）
                    var bodyContent = ExtractBodyContent(html);
                    if (string.IsNullOrEmpty(bodyContent))
                    {
                        return null;
                    }

                    // 提取章节中引用的图片
                    var images = await ExtractChapterImagesAsync(book, html).ConfigureAwait(false);

                    return new ChapterWithImages
                    {
                        BodyContent = bodyContent,
                        Images = images.Count > 0 ? images : null,
                    };
                }
            }
            catch
            {
                // 无法读取，继续下一个
            }
        }

        return null;
    }

    /// <summary>
    /// 从 HTML 内容中提取章节索引.
    /// </summary>
    /// <param name="htmlContent">HTML 内容.</param>
    /// <returns>章节索引，如果未找到则返回 null.</returns>
    public static int? ExtractChapterIndex(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return null;
        }

        // 尝试从 HTML 注释提取
        var commentMatch = CommentChapterIndexRegex().Match(htmlContent);
        if (commentMatch.Success && int.TryParse(commentMatch.Groups[1].Value, out var index))
        {
            return index;
        }

        // 尝试从 data-* 属性提取
        var attrMatch = DataChapterIndexRegex().Match(htmlContent);
        if (attrMatch.Success && int.TryParse(attrMatch.Groups[1].Value, out index))
        {
            return index;
        }

        return null;
    }

    /// <summary>
    /// 从 HTML 内容中提取章节状态.
    /// </summary>
    /// <param name="htmlContent">HTML 内容.</param>
    /// <returns>章节状态.</returns>
    public static ChapterStatus ExtractStatus(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return ChapterStatus.Pending;
        }

        // 尝试从 HTML 注释提取
        var commentMatch = CommentStatusRegex().Match(htmlContent);
        if (commentMatch.Success)
        {
            var statusStr = commentMatch.Groups[1].Value.ToLowerInvariant();
            return statusStr switch
            {
                "downloaded" => ChapterStatus.Downloaded,
                "failed" => ChapterStatus.Failed,
                "volume" => ChapterStatus.Volume,
                _ => ChapterStatus.Pending,
            };
        }

        // 尝试从 data-* 属性提取
        var attrMatch = DataStatusRegex().Match(htmlContent);
        if (attrMatch.Success)
        {
            var statusStr = attrMatch.Groups[1].Value.ToLowerInvariant();
            return statusStr switch
            {
                "downloaded" => ChapterStatus.Downloaded,
                "failed" => ChapterStatus.Failed,
                "volume" => ChapterStatus.Volume,
                _ => ChapterStatus.Pending,
            };
        }

        return ChapterStatus.Pending;
    }

    /// <summary>
    /// 从完整 HTML 文档中提取 chapter-content 内部的内容.
    /// </summary>
    private static string? ExtractBodyContent(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        // 优先提取 <div class="chapter-content"> 内部的内容
        var chapterContentMatch = ChapterContentDivRegex().Match(html);
        if (chapterContentMatch.Success)
        {
            return chapterContentMatch.Groups[1].Value.Trim();
        }

        // 如果没有 chapter-content，尝试提取 body 内容
        var bodyMatch = BodyContentRegex().Match(html);
        if (bodyMatch.Success)
        {
            return bodyMatch.Groups[1].Value.Trim();
        }

        // 如果没有 body 标签，假设内容本身就是 body 内容片段
        if (html.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
            html.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
            html.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return html;
    }

    /// <summary>
    /// 从章节 HTML 中提取引用的图片资源.
    /// </summary>
    private static async Task<List<ImageResource>> ExtractChapterImagesAsync(EpubBook book, string html)
    {
        var images = new List<ImageResource>();
        var imageMatches = ImageSrcRegex().Matches(html);

        foreach (Match match in imageMatches)
        {
            var imgSrc = match.Groups[1].Value;

            // 提取图片文件名（去掉路径）
            var imgFileName = Path.GetFileName(imgSrc);
            if (string.IsNullOrEmpty(imgFileName))
            {
                continue;
            }

            // 在 EPUB 资源中查找图片
            try
            {
                var imageResource = book.Resources.FirstOrDefault(r =>
                    r.Href.EndsWith(imgFileName, StringComparison.OrdinalIgnoreCase) ||
                    r.Href.EndsWith(imgSrc.TrimStart('.', '/'), StringComparison.OrdinalIgnoreCase));

                if (imageResource != null)
                {
                    var imageData = await book.ReadResourceContentAsync(imageResource).ConfigureAwait(false);
                    if (imageData != null && imageData.Length > 0)
                    {
                        images.Add(new ImageResource
                        {
                            Id = Path.GetFileNameWithoutExtension(imgFileName),
                            Data = imageData,
                            MediaType = !string.IsNullOrEmpty(imageResource.MediaType)
                                ? imageResource.MediaType
                                : GuessMediaType(imgFileName),
                        });
                    }
                }
            }
            catch
            {
                // 无法读取图片，跳过
            }
        }

        return images;
    }

    /// <summary>
    /// 根据文件扩展名猜测媒体类型.
    /// </summary>
    private static string GuessMediaType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "image/jpeg",
        };
    }

    /// <summary>
    /// 获取元数据值.
    /// </summary>
    private static string? GetMetaValue(EpubBook book, string name)
    {
        // 先从 MetaItems 查找
        var meta = book.Metadata.MetaItems.FirstOrDefault(m =>
            m.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true ||
            m.Property?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

        if (meta != null)
        {
            return meta.Content;
        }

        // 再从 CustomMetadata 查找
        if (book.Metadata.CustomMetadata.TryGetValue(name, out var value))
        {
            return value;
        }

        return null;
    }

    [GeneratedRegex(@"<!--\s*legado:chapter-index=(\d+)\s*-->", RegexOptions.IgnoreCase)]
    private static partial Regex CommentChapterIndexRegex();

    [GeneratedRegex(@"data-legado-chapter-index=[""'](\d+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex DataChapterIndexRegex();

    [GeneratedRegex(@"<!--\s*legado:status=(\w+)\s*-->", RegexOptions.IgnoreCase)]
    private static partial Regex CommentStatusRegex();

    [GeneratedRegex(@"data-legado-status=[""'](\w+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex DataStatusRegex();

    [GeneratedRegex(@"<body[^>]*>(.*)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex BodyContentRegex();

    [GeneratedRegex(@"<div[^>]*class=""chapter-content""[^>]*>(.*)</div>\s*</div>\s*</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ChapterContentDivRegex();

    [GeneratedRegex(@"<img[^>]+src=[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ImageSrcRegex();
}
