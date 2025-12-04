// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Sources.FanQie.Helpers;

/// <summary>
/// 内容解析器.
/// </summary>
internal static partial class ContentParser
{
    /// <summary>
    /// 图片占位符格式.
    /// </summary>
    public const string ImagePlaceholderFormat = "<!-- FANQIE_IMAGE:{0} -->";

    /// <summary>
    /// 解析内容中的图片标签（用于正用 API 返回的纯文本+图片 HTML 标签组合）.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <param name="chapterId">章节 ID，用于生成图片唯一标识.</param>
    /// <param name="removeFirstLine">是否移除第一行（通常是章节标题）.</param>
    /// <returns>解析结果：提取的图片列表和带占位符的内容.</returns>
    public static (IReadOnlyList<ChapterImage>? Images, string CleanedText, string HtmlContent) ParseContentWithImages(string content, string chapterId, bool removeFirstLine = true)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return (null, string.Empty, string.Empty);
        }

        // 移除第一行（章节标题），因为标题会在 EPUB 生成时单独添加
        var processedContent = content;
        if (removeFirstLine)
        {
            var firstNewlineIndex = content.IndexOf('\n', StringComparison.Ordinal);
            if (firstNewlineIndex > 0)
            {
                processedContent = content[(firstNewlineIndex + 1)..].TrimStart();
            }
        }

        var images = new List<ChapterImage>();
        var htmlContent = processedContent;
        var imageIndex = 0;

        // 匹配 FanQie 特殊格式的 img 标签
        var matches = FanQieImageRegex().Matches(processedContent);

        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            var srcValue = match.Groups["src"].Value;

            // 解码 URL（处理 &amp; 等 HTML 实体）
            var url = System.Net.WebUtility.HtmlDecode(srcValue);

            // 生成唯一的图片 ID
            var imageId = $"img_{chapterId}_{imageIndex}";
            imageIndex++;

            images.Add(new ChapterImage
            {
                Id = imageId,
                Url = url,
            });

            // 将原图片标签替换为占位符
            var placeholder = string.Format(ImagePlaceholderFormat, imageId);
            htmlContent = htmlContent.Replace(match.Value, placeholder, StringComparison.Ordinal);
        }

        // 移除图片标签，得到纯文本
        var cleanedText = FanQieImageRegex().Replace(processedContent, string.Empty);
        // 清理可能产生的多余空行
        cleanedText = MultipleNewlinesRegex().Replace(cleanedText, "\n\n").Trim();

        // 转换为段落格式
        htmlContent = $"<article><p>{htmlContent.Replace("\n", "</p><p>", StringComparison.Ordinal)}</p></article>";

        return (images.Count > 0 ? images : null, cleanedText, htmlContent);
    }

    /// <summary>
    /// 解析备用 API 返回的 HTML 内容（rawContent）.
    /// </summary>
    /// <param name="rawContent">原始 HTML 内容.</param>
    /// <param name="chapterId">章节 ID，用于生成图片唯一标识.</param>
    /// <returns>解析结果：提取的图片列表、纯文本和带占位符的 HTML.</returns>
    public static (IReadOnlyList<ChapterImage>? Images, string CleanedText, string HtmlContent) ParseFallbackHtmlContent(string rawContent, string chapterId)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return (null, string.Empty, string.Empty);
        }

        var images = new List<ChapterImage>();
        var paragraphs = new List<string>();
        var imageIndex = 0;

        // 第一步：提取所有图片并生成占位符映射
        var imgMatches = StandardImageRegex().Matches(rawContent);
        var imagePlaceholderMap = new Dictionary<string, string>(); // imgTag -> placeholder

        foreach (Match match in imgMatches)
        {
            if (!match.Success)
            {
                continue;
            }

            var srcValue = match.Groups["src"].Value;
            var url = System.Net.WebUtility.HtmlDecode(srcValue);

            // 生成唯一的图片 ID
            var imageId = $"img_{chapterId}_{imageIndex}";
            imageIndex++;

            images.Add(new ChapterImage
            {
                Id = imageId,
                Url = url,
            });

            var placeholder = string.Format(ImagePlaceholderFormat, imageId);
            imagePlaceholderMap[match.Value] = placeholder;
        }

        // 第二步：在原始内容中替换图片标签为占位符
        var contentWithPlaceholders = rawContent;
        foreach (var (imgTag, placeholder) in imagePlaceholderMap)
        {
            contentWithPlaceholders = contentWithPlaceholders.Replace(imgTag, placeholder, StringComparison.Ordinal);
        }

        // 第三步：提取所有带 p_idx 属性的段落，处理文本和占位符
        var cleanedHtmlParagraphs = new List<string>();
        var pMatches = ParagraphWithPIdxRegex().Matches(contentWithPlaceholders);

        foreach (Match match in pMatches)
        {
            if (!match.Success)
            {
                continue;
            }

            var paragraphContent = match.Groups["content"].Value;

            // 检查段落内容是否包含占位符
            var placeholderMatch = ImagePlaceholderRegex().Match(paragraphContent);
            if (placeholderMatch.Success)
            {
                // 段落包含占位符，直接添加占位符
                cleanedHtmlParagraphs.Add(placeholderMatch.Value);
                continue;
            }

            // 从段落内容中提取文本（移除 <blk> 等标签）
            var textContent = HtmlTagRegex().Replace(paragraphContent, string.Empty);
            textContent = System.Net.WebUtility.HtmlDecode(textContent).Trim();

            if (!string.IsNullOrEmpty(textContent))
            {
                paragraphs.Add(textContent);
                cleanedHtmlParagraphs.Add($"<p>{System.Net.WebUtility.HtmlEncode(textContent)}</p>");
            }
        }

        // 生成纯文本
        var cleanedText = string.Join("\n\n", paragraphs);

        var finalHtml = $"<article>{string.Join("\n", cleanedHtmlParagraphs)}</article>";

        return (images.Count > 0 ? images : null, cleanedText, finalHtml);
    }

    /// <summary>
    /// 将原始 HTML 内容清洗为纯文本.
    /// </summary>
    /// <param name="htmlContent">原始 HTML 内容.</param>
    /// <returns>纯文本内容.</returns>
    public static string ToPlainText(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return string.Empty;
        }

        // 替换段落标签为换行
        var text = ParagraphRegex().Replace(htmlContent, "\n\n");
        text = LineBreakRegex().Replace(text, "\n");

        // 移除所有 HTML 标签
        text = HtmlTagRegex().Replace(text, string.Empty);

        // 解码 HTML 实体
        text = System.Net.WebUtility.HtmlDecode(text);

        // 清理多余空白
        text = MultipleNewlinesRegex().Replace(text, "\n\n");
        text = MultipleSpacesRegex().Replace(text, " ");

        return text.Trim();
    }

    /// <summary>
    /// 清洗 HTML 内容（保留基本格式用于 Epub）.
    /// </summary>
    /// <param name="htmlContent">原始 HTML 内容.</param>
    /// <returns>清洗后的 HTML 内容.</returns>
    public static string CleanHtml(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return string.Empty;
        }

        // 保留段落结构，移除不需要的标签
        var cleaned = StyleRegex().Replace(htmlContent, string.Empty);
        cleaned = ScriptRegex().Replace(cleaned, string.Empty);

        // 确保段落标签正确
        cleaned = DivRegex().Replace(cleaned, "<p>");
        cleaned = DivEndRegex().Replace(cleaned, "</p>");

        // 清理空段落
        cleaned = EmptyParagraphRegex().Replace(cleaned, string.Empty);

        return cleaned.Trim();
    }

    /// <summary>
    /// 计算纯文本字数.
    /// </summary>
    /// <param name="text">纯文本.</param>
    /// <returns>字数.</returns>
    public static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // 移除空白字符后计数
        var cleaned = WhitespaceRegex().Replace(text, string.Empty);
        return cleaned.Length;
    }

    [GeneratedRegex(@"</p>", RegexOptions.IgnoreCase)]
    private static partial Regex ParagraphRegex();

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreakRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex MultipleNewlinesRegex();

    [GeneratedRegex(@" {2,}")]
    private static partial Regex MultipleSpacesRegex();

    [GeneratedRegex(@"<style[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StyleRegex();

    [GeneratedRegex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptRegex();

    [GeneratedRegex(@"<div[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex DivRegex();

    [GeneratedRegex(@"</div>", RegexOptions.IgnoreCase)]
    private static partial Regex DivEndRegex();

    [GeneratedRegex(@"<p>\s*</p>", RegexOptions.IgnoreCase)]
    private static partial Regex EmptyParagraphRegex();

    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();

    // 匹配 FanQie 格式的图片标签 (从 JSON 解析后的格式):
    // 格式1: <img src=\"url\" img-width=\"602\" img-height=\"339\" alt=\"\" media-idx=\"1\"/> (主 API，带转义引号)
    // 格式2: <img src="url" img-width="602" img-height="339" alt="" media-idx="1"/> (后备 API，标准 HTML)
    // 使用 \\?" 同时匹配可选的反斜杠转义
    [GeneratedRegex("""<img\s+src=\\?"(?<src>[^"\\]+)\\?"(?:\s+img-width=\\?"(?<width>\d+)\\?")?(?:\s+img-height=\\?"(?<height>\d+)\\?")?(?:\s+alt=\\?"[^"\\]*\\?")?(?:[^>]*\s+)?(?:media-idx=\\?"(?<idx>\d+)\\?")?\s*/?>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FanQieImageRegex();

    // 匹配带 p_idx 属性的段落标签（备用 API 返回的有效正文段落）
    [GeneratedRegex("""<p\s+[^>]*p_idx="[^"]*"[^>]*>(?<content>.*?)</p>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ParagraphWithPIdxRegex();

    // 匹配标准 HTML 格式的 img 标签
    [GeneratedRegex("""<img\s+[^>]*src="(?<src>[^"]+)"[^>]*/?>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StandardImageRegex();

    // 匹配图片占位符
    [GeneratedRegex(@"<!-- FANQIE_IMAGE:([^>]+) -->")]
    private static partial Regex ImagePlaceholderRegex();
}
