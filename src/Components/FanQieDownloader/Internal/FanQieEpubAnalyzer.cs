// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Components.FanQie.Internal;

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
/// 番茄 EPUB 分析器，从 EPUB 文件中提取番茄标记信息.
/// </summary>
internal sealed partial class FanQieEpubAnalyzer
{
    private const string MetaBookIdName = "fanqie:book-id";
    private const string MetaSyncTimeName = "fanqie:sync-time";
    private const string MetaTocHashName = "fanqie:toc-hash";
    private const string MetaFailedChaptersName = "fanqie:failed-chapters";

    /// <summary>
    /// 从 EPUB 文件中提取番茄书籍信息.
    /// </summary>
    /// <param name="epubPath">EPUB 文件路径.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>番茄书籍信息，如果不是番茄书籍则返回 null.</returns>
    public static async Task<FanQieBookInfo?> AnalyzeAsync(
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
    /// 从已打开的 EPUB 书籍中提取番茄书籍信息.
    /// </summary>
    /// <param name="book">EPUB 书籍.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>番茄书籍信息，如果不是番茄书籍则返回 null.</returns>
    public static async Task<FanQieBookInfo?> ExtractInfoAsync(
        EpubBook book,
        CancellationToken cancellationToken = default)
    {
        // 1. 从元数据提取 fanqie:book-id
        var bookId = GetMetaValue(book, MetaBookIdName);

        if (string.IsNullOrEmpty(bookId))
        {
            // 尝试从 Identifier 提取
            if (book.Metadata.Identifier?.StartsWith("fanqie-", StringComparison.OrdinalIgnoreCase) == true)
            {
                bookId = book.Metadata.Identifier["fanqie-".Length..];
            }
        }

        if (string.IsNullOrEmpty(bookId))
        {
            return null; // 不是番茄书籍
        }

        // 2. 提取其他元数据
        var syncTimeStr = GetMetaValue(book, MetaSyncTimeName);
        var tocHash = GetMetaValue(book, MetaTocHashName);
        var failedChaptersStr = GetMetaValue(book, MetaFailedChaptersName);

        var failedIds = failedChaptersStr?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList() ?? [];

        // 3. 遍历阅读顺序，提取章节 ID
        var downloadedIds = new List<string>();

        foreach (var resource in book.ReadingOrder)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 跳过非章节文件（如导航、目录等）
            if (!resource.Href.Contains("chapter", StringComparison.OrdinalIgnoreCase) ||
                resource.Href.Contains("nav", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // 从 HTML 内容中提取章节 ID 和状态
            try
            {
                var html = await book.ReadResourceContentAsStringAsync(resource).ConfigureAwait(false);

                // 从内容中提取真正的番茄章节 ID
                var chapterId = ChapterContentMarker.ExtractChapterId(html);
                if (string.IsNullOrEmpty(chapterId))
                {
                    continue;
                }

                // 检查是否为元数据中标记的失败章节
                if (failedIds.Contains(chapterId))
                {
                    continue;
                }

                var status = ChapterContentMarker.ExtractStatus(html);

                if (status == ChapterStatus.Downloaded)
                {
                    downloadedIds.Add(chapterId);
                }
                else if (status == ChapterStatus.Failed && !failedIds.Contains(chapterId))
                {
                    failedIds.Add(chapterId);
                }
            }
            catch
            {
                // 无法读取内容，跳过
            }
        }

        return new FanQieBookInfo
        {
            BookId = bookId,
            Title = book.Metadata.Title ?? string.Empty,
            Author = book.Metadata.Authors.FirstOrDefault(),
            Description = book.Metadata.Description,
            LastSyncTime = DateTimeOffset.TryParse(syncTimeStr, out var dt) ? dt : null,
            TocHash = tocHash,
            DownloadedChapterIds = downloadedIds,
            FailedChapterIds = failedIds,
        };
    }

    /// <summary>
    /// 从 EPUB 文件中读取指定章节的内容及其图片资源.
    /// </summary>
    /// <param name="book">EPUB 书籍.</param>
    /// <param name="chapterId">番茄章节 ID.</param>
    /// <returns>章节内容及图片资源，如果未找到则返回 null.</returns>
    public static async Task<ChapterWithImages?> ReadChapterContentAsync(EpubBook book, string chapterId)
    {
        // 遍历所有章节资源，通过 HTML 内容中的 chapterId 标记来匹配
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
                var extractedId = ChapterContentMarker.ExtractChapterId(html);

                if (extractedId == chapterId)
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
    /// 从完整 HTML 文档中提取 chapter-content 内部的内容.
    /// </summary>
    /// <param name="html">完整的 HTML 文档.</param>
    /// <returns>chapter-content 内部的内容，如果无法提取则返回 null.</returns>
    private static string? ExtractBodyContent(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        // 优先提取 <div class="chapter-content"> 内部的内容
        // 这样可以避免重复包装 chapter-container 和 chapter-title
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
        // 检查是否以 XML/HTML 声明开头
        if (html.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
            html.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
            html.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
        {
            // 这是完整 HTML 但没有匹配到 body，返回 null
            return null;
        }

        // 看起来是内容片段，直接返回
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
                // 查找匹配的资源（可能在 Images 文件夹或其他位置）
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
    /// 从资源 Href 提取章节 ID.
    /// </summary>
    private static string? ExtractChapterIdFromHref(string? href)
    {
        if (string.IsNullOrEmpty(href))
        {
            return null;
        }

        // 匹配 chapter_123456.xhtml 格式
        var match = ChapterIdFromHrefRegex().Match(href);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
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

    [GeneratedRegex(@"chapter_?(\d+)\.xhtml", RegexOptions.IgnoreCase)]
    private static partial Regex ChapterIdFromHrefRegex();

    [GeneratedRegex(@"<body[^>]*>(.*)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex BodyContentRegex();

    [GeneratedRegex(@"<div[^>]*class=""chapter-content""[^>]*>(.*)</div>\s*</div>\s*</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ChapterContentDivRegex();

    [GeneratedRegex(@"<img[^>]+src=[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ImageSrcRegex();
}
