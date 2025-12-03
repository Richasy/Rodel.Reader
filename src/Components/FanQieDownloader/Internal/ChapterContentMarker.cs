// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Components.FanQie.Internal;

/// <summary>
/// 章节内容标记器，为段落添加番茄特有标记.
/// </summary>
internal static partial class ChapterContentMarker
{
    private const string FanQieIndexAttr = "data-fanqie-index";
    private const string FanQieChapterIdAttr = "data-fanqie-chapter-id";

    /// <summary>
    /// 为 HTML 内容添加番茄标记.
    /// </summary>
    /// <param name="htmlContent">原始 HTML 内容.</param>
    /// <param name="chapterId">章节 ID.</param>
    /// <returns>带标记的 HTML 内容.</returns>
    public static string AddMarkers(string htmlContent, string chapterId)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return htmlContent;
        }

        var paragraphIndex = 0;
        var result = ParagraphRegex().Replace(htmlContent, match =>
        {
            var pContent = match.Value;

            // 检查是否已有标记
            if (pContent.Contains(FanQieIndexAttr, StringComparison.OrdinalIgnoreCase))
            {
                paragraphIndex++;
                return pContent;
            }

            // 添加标记
            var insertPos = match.Value.IndexOf('>', StringComparison.Ordinal);
            if (insertPos > 0)
            {
                var attributes = $" {FanQieIndexAttr}=\"{paragraphIndex}\" {FanQieChapterIdAttr}=\"{chapterId}\"";
                pContent = pContent.Insert(insertPos, attributes);
            }

            paragraphIndex++;
            return pContent;
        });

        return result;
    }

    /// <summary>
    /// 从 HTML 内容中提取章节 ID.
    /// </summary>
    /// <param name="htmlContent">HTML 内容.</param>
    /// <returns>章节 ID，如果未找到则返回 null.</returns>
    public static string? ExtractChapterId(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return null;
        }

        // 尝试从 HTML 注释提取（新格式）
        var commentMatch = CommentChapterIdRegex().Match(htmlContent);
        if (commentMatch.Success)
        {
            return commentMatch.Groups[1].Value;
        }

        // 尝试从 meta 标签提取（旧格式，向后兼容）
        var metaMatch = MetaChapterIdRegex().Match(htmlContent);
        if (metaMatch.Success)
        {
            return metaMatch.Groups[1].Value;
        }

        // 尝试从 data-* 属性提取
        var attrMatch = ParagraphChapterIdRegex().Match(htmlContent);
        if (attrMatch.Success)
        {
            return attrMatch.Groups[1].Value;
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

        // 尝试从 HTML 注释提取（新格式）
        var commentMatch = CommentStatusRegex().Match(htmlContent);
        if (commentMatch.Success)
        {
            return commentMatch.Groups[1].Value.ToLowerInvariant() switch
            {
                "downloaded" => ChapterStatus.Downloaded,
                "failed" => ChapterStatus.Failed,
                "locked" => ChapterStatus.Locked,
                _ => ChapterStatus.Pending,
            };
        }

        // 尝试从 meta 标签提取（旧格式，向后兼容）
        var metaMatch = MetaStatusRegex().Match(htmlContent);
        if (metaMatch.Success)
        {
            return metaMatch.Groups[1].Value.ToLowerInvariant() switch
            {
                "downloaded" => ChapterStatus.Downloaded,
                "failed" => ChapterStatus.Failed,
                "locked" => ChapterStatus.Locked,
                _ => ChapterStatus.Pending,
            };
        }

        // 尝试从 data-fanqie-status 属性提取
        var attrMatch = DataStatusRegex().Match(htmlContent);
        if (attrMatch.Success)
        {
            return attrMatch.Groups[1].Value.ToLowerInvariant() switch
            {
                "downloaded" => ChapterStatus.Downloaded,
                "failed" => ChapterStatus.Failed,
                "locked" => ChapterStatus.Locked,
                _ => ChapterStatus.Pending,
            };
        }

        // 如果包含 chapter-unavailable 类，则为失败
        if (htmlContent.Contains("chapter-unavailable", StringComparison.OrdinalIgnoreCase))
        {
            return ChapterStatus.Failed;
        }

        // 如果有段落内容标记，则为已下载
        if (ParagraphChapterIdRegex().IsMatch(htmlContent))
        {
            return ChapterStatus.Downloaded;
        }

        return ChapterStatus.Pending;
    }

    [GeneratedRegex(@"<p\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ParagraphRegex();

    // 新格式：HTML 注释
    [GeneratedRegex(@"<!--\s*fanqie:chapter-id=(\d+)\s*-->", RegexOptions.IgnoreCase)]
    private static partial Regex CommentChapterIdRegex();

    [GeneratedRegex(@"<!--\s*fanqie:status=(\w+)\s*-->", RegexOptions.IgnoreCase)]
    private static partial Regex CommentStatusRegex();

    // 旧格式：meta 标签（向后兼容）
    [GeneratedRegex(@"<meta\s+name=""fanqie:chapter-id""\s+content=""(\d+)""", RegexOptions.IgnoreCase)]
    private static partial Regex MetaChapterIdRegex();

    [GeneratedRegex(@"<meta\s+name=""fanqie:status""\s+content=""(\w+)""", RegexOptions.IgnoreCase)]
    private static partial Regex MetaStatusRegex();

    // data-* 属性
    [GeneratedRegex(@"data-fanqie-chapter-id=""(\d+)""", RegexOptions.IgnoreCase)]
    private static partial Regex ParagraphChapterIdRegex();

    [GeneratedRegex(@"data-fanqie-status=""(\w+)""", RegexOptions.IgnoreCase)]
    private static partial Regex DataStatusRegex();
}
