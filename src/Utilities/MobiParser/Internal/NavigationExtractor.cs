// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// 目录解析器，从 HTML 内容中提取导航信息。
/// </summary>
internal static partial class NavigationExtractor
{
    /// <summary>
    /// 从 HTML 内容中提取目录。
    /// </summary>
    /// <param name="htmlContent">HTML 内容。</param>
    /// <returns>导航项列表。</returns>
    public static List<MobiNavItem> ExtractFromHtml(string htmlContent)
    {
        var navItems = new List<MobiNavItem>();

        if (string.IsNullOrEmpty(htmlContent))
        {
            return navItems;
        }

        // 尝试查找目录相关的标签
        // 首先尝试查找 <nav> 元素
        var navMatch = NavTagRegex().Match(htmlContent);
        if (navMatch.Success)
        {
            return ExtractFromNavElement(navMatch.Value);
        }

        // 查找带有 toc 类或 id 的元素
        var tocMatch = TocDivRegex().Match(htmlContent);
        if (tocMatch.Success)
        {
            return ExtractLinksFromHtml(tocMatch.Value);
        }

        // 查找有序列表或无序列表中的链接
        var listMatches = ListRegex().Matches(htmlContent);
        if (listMatches.Count > 0)
        {
            foreach (Match match in listMatches)
            {
                var items = ExtractLinksFromHtml(match.Value);
                if (items.Count > 0)
                {
                    navItems.AddRange(items);
                }
            }

            if (navItems.Count > 0)
            {
                return navItems;
            }
        }

        // 最后尝试从整个文档中提取所有链接
        return ExtractLinksFromHtml(htmlContent);
    }

    /// <summary>
    /// 从 NCX 内容中提取目录。
    /// </summary>
    /// <param name="ncxContent">NCX 内容。</param>
    /// <returns>导航项列表。</returns>
    public static List<MobiNavItem> ExtractFromNcx(string ncxContent)
    {
        var navItems = new List<MobiNavItem>();

        if (string.IsNullOrEmpty(ncxContent))
        {
            return navItems;
        }

        // 提取 navPoint 元素
        var navPointMatches = NavPointRegex().Matches(ncxContent);
        foreach (Match match in navPointMatches)
        {
            var navPoint = match.Value;

            // 提取标题
            var labelMatch = NavLabelRegex().Match(navPoint);
            var title = labelMatch.Success ? DecodeHtmlEntities(labelMatch.Groups[1].Value.Trim()) : string.Empty;

            // 提取位置（src 属性）
            var srcMatch = ContentSrcRegex().Match(navPoint);
            long position = 0;
            string? anchor = null;

            if (srcMatch.Success)
            {
                var src = srcMatch.Groups[1].Value;
                var hashIndex = src.IndexOf('#', StringComparison.Ordinal);
                if (hashIndex >= 0)
                {
                    anchor = src[(hashIndex + 1)..];
                }

                // 尝试从文件偏移中解析位置
                // 通常 Mobi 的 NCX 使用 filepos 属性
                var fileposMatch = FileposRegex().Match(navPoint);
                if (fileposMatch.Success && long.TryParse(fileposMatch.Groups[1].Value, out var pos))
                {
                    position = pos;
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                navItems.Add(new MobiNavItem
                {
                    Title = title,
                    Position = position,
                    Anchor = anchor,
                });
            }
        }

        return navItems;
    }

    private static List<MobiNavItem> ExtractFromNavElement(string navContent)
    {
        return ExtractLinksFromHtml(navContent);
    }

    private static List<MobiNavItem> ExtractLinksFromHtml(string html)
    {
        var navItems = new List<MobiNavItem>();

        // 提取所有 <a> 标签
        var linkMatches = AnchorRegex().Matches(html);
        foreach (Match match in linkMatches)
        {
            var href = match.Groups[1].Value;
            var text = StripHtmlTags(match.Groups[2].Value).Trim();

            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            long position = 0;
            string? anchor = null;

            // 检查 filepos 属性
            var fileposMatch = FileposInAnchorRegex().Match(match.Value);
            if (fileposMatch.Success && long.TryParse(fileposMatch.Groups[1].Value, out var pos))
            {
                position = pos;
            }

            // 处理 href 中的锚点
            if (!string.IsNullOrEmpty(href))
            {
                var hashIndex = href.IndexOf('#', StringComparison.Ordinal);
                if (hashIndex >= 0)
                {
                    anchor = href[(hashIndex + 1)..];
                }
            }

            navItems.Add(new MobiNavItem
            {
                Title = DecodeHtmlEntities(text),
                Position = position,
                Anchor = anchor,
            });
        }

        return navItems;
    }

    private static string StripHtmlTags(string html)
    {
        return HtmlTagRegex().Replace(html, string.Empty);
    }

    private static string DecodeHtmlEntities(string text)
    {
        return text
            .Replace("&amp;", "&", StringComparison.Ordinal)
            .Replace("&lt;", "<", StringComparison.Ordinal)
            .Replace("&gt;", ">", StringComparison.Ordinal)
            .Replace("&quot;", "\"", StringComparison.Ordinal)
            .Replace("&apos;", "'", StringComparison.Ordinal)
            .Replace("&#39;", "'", StringComparison.Ordinal)
            .Replace("&nbsp;", " ", StringComparison.Ordinal);
    }

    [GeneratedRegex(@"<nav[^>]*>.*?</nav>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex NavTagRegex();

    [GeneratedRegex(@"<div[^>]*(?:id|class)\s*=\s*[""'](?:[^""']*)?toc(?:[^""']*)?\s*[""'][^>]*>.*?</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TocDivRegex();

    [GeneratedRegex(@"<[ou]l[^>]*>.*?</[ou]l>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ListRegex();

    [GeneratedRegex(@"<navPoint[^>]*>.*?(?:</navPoint>|(?=<navPoint))", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex NavPointRegex();

    [GeneratedRegex(@"<navLabel[^>]*>\s*<text[^>]*>(.*?)</text>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex NavLabelRegex();

    [GeneratedRegex(@"<content[^>]*src\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ContentSrcRegex();

    [GeneratedRegex(@"filepos\s*=\s*[""']?(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex FileposRegex();

    [GeneratedRegex(@"<a[^>]*href\s*=\s*[""']([^""']*)[""'][^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AnchorRegex();

    [GeneratedRegex(@"filepos\s*=\s*[""']?(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex FileposInAnchorRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTagRegex();
}
