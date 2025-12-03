// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Components.FanQie.Internal;

/// <summary>
/// 番茄 EPUB 分析器，从 EPUB 文件中提取番茄标记信息.
/// </summary>
internal sealed partial class FanQieEpubAnalyzer
{
    private const string ChapterFilePrefix = "chapter_";
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

            var chapterId = ExtractChapterIdFromHref(resource.Href);
            if (chapterId == null)
            {
                continue;
            }

            // 检查是否为失败章节
            if (failedIds.Contains(chapterId))
            {
                continue;
            }

            // 尝试从 HTML 内容确认状态
            try
            {
                var html = await book.ReadResourceContentAsStringAsync(resource).ConfigureAwait(false);
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
                // 无法读取内容，假设已下载
                downloadedIds.Add(chapterId);
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
    /// 从 EPUB 文件中读取指定章节的内容.
    /// </summary>
    /// <param name="book">EPUB 书籍.</param>
    /// <param name="chapterId">章节 ID.</param>
    /// <returns>章节 HTML 内容，如果未找到则返回 null.</returns>
    public static async Task<string?> ReadChapterContentAsync(EpubBook book, string chapterId)
    {
        var href = $"Text/{ChapterFilePrefix}{chapterId}.xhtml";
        var resource = book.Resources.FirstOrDefault(r =>
            r.Href.EndsWith($"{ChapterFilePrefix}{chapterId}.xhtml", StringComparison.OrdinalIgnoreCase));

        if (resource == null)
        {
            return null;
        }

        try
        {
            return await book.ReadResourceContentAsStringAsync(resource).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
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

    [GeneratedRegex(@"chapter_(\d+)\.xhtml", RegexOptions.IgnoreCase)]
    private static partial Regex ChapterIdFromHrefRegex();
}
