// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Sources.FanQie.Helpers;

/// <summary>
/// 内容解析器.
/// </summary>
internal static partial class ContentParser
{
    /// <summary>
    /// 解析内容中的图片标签.
    /// </summary>
    /// <param name="content">原始内容.</param>
    /// <returns>解析结果：提取的图片列表和移除图片标签后的纯文本.</returns>
    public static (IReadOnlyList<ChapterImage>? Images, string CleanedText, string HtmlContent) ParseContentWithImages(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return (null, string.Empty, string.Empty);
        }

        var images = new List<ChapterImage>();
        var cleanedText = content;

        // 匹配 FanQie 特殊格式的 img 标签
        // 格式: <img src="\"url\"" img-width="\"602\"" img-height="\"339\"" alt="\"\"" media-idx="\"1\""/>
        var matches = FanQieImageRegex().Matches(content);

        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            var srcValue = match.Groups["src"].Value;

            // 解码 URL（处理 &amp; 等 HTML 实体）
            var url = System.Net.WebUtility.HtmlDecode(srcValue);

            // 尝试获取 media-idx 作为排序依据
            int? offset = null;
            var mediaIdxValue = match.Groups["idx"].Value;
            if (!string.IsNullOrEmpty(mediaIdxValue) && int.TryParse(mediaIdxValue, out var idx))
            {
                offset = idx;
            }

            images.Add(new ChapterImage
            {
                Url = url,
                Offset = offset,
            });
        }

        // 移除图片标签，得到纯文本
        cleanedText = FanQieImageRegex().Replace(content, string.Empty);
        // 清理可能产生的多余空行
        cleanedText = MultipleNewlinesRegex().Replace(cleanedText, "\n\n").Trim();

        // 生成 HTML 内容（保留图片标签但转换为标准格式）
        var htmlContent = content;
        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            var srcValue = match.Groups["src"].Value;
            var url = System.Net.WebUtility.HtmlDecode(srcValue);

            // 获取宽高
            var widthValue = match.Groups["width"].Value;
            var heightValue = match.Groups["height"].Value;

            // 构建标准的 img 标签
            var styleAttr = string.Empty;
            if (!string.IsNullOrEmpty(widthValue) && !string.IsNullOrEmpty(heightValue))
            {
                styleAttr = $" style=\"max-width:100%;\" width=\"{widthValue}\" height=\"{heightValue}\"";
            }

            var standardImgTag = $"<img src=\"{System.Net.WebUtility.HtmlEncode(url)}\"{styleAttr} alt=\"\"/>";
            htmlContent = htmlContent.Replace(match.Value, standardImgTag, StringComparison.Ordinal);
        }

        // 转换为段落格式
        htmlContent = $"<article><p>{htmlContent.Replace("\n", "</p><p>", StringComparison.Ordinal)}</p></article>";

        return (images.Count > 0 ? images : null, cleanedText, htmlContent);
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

    // 匹配 FanQie 格式的图片标签:
    // <img src="\"url\"" img-width="\"602\"" img-height="\"339\"" alt="\"\"" media-idx="\"1\""/>
    [GeneratedRegex("""<img\s+src="\\?"(?<src>[^"]+?)\\?"(?:\s+img-width="\\?"(?<width>\d+)\\?")?(?:\s+img-height="\\?"(?<height>\d+)\\?")?(?:\s+alt="[^"]*")?(?:\s+media-idx="\\?"(?<idx>\d+)\\?")?\s*/?>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FanQieImageRegex();
}
