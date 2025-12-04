// Copyright (c) Richasy. All rights reserved.

using System.Text.RegularExpressions;

namespace Richasy.RodelReader.Sources.Legado.Helpers;

/// <summary>
/// 内容格式化辅助类.
/// </summary>
internal static partial class ContentFormatter
{
    /// <summary>
    /// 将纯文本内容转换为 HTML 格式.
    /// </summary>
    /// <param name="content">纯文本内容.</param>
    /// <returns>HTML 格式内容.</returns>
    public static string ConvertToHtml(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        var lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            // 如果已经是 HTML 标签，保持原样
            if (HtmlTagRegex().IsMatch(trimmedLine))
            {
                result.AppendLine(trimmedLine);
            }
            else
            {
                // 普通文本包裹在 <p> 标签中
                result.AppendLine($"<p>{trimmedLine}</p>");
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// 清理 HTML 内容中的空白.
    /// </summary>
    /// <param name="html">HTML 内容.</param>
    /// <returns>清理后的内容.</returns>
    public static string CleanHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // 移除连续的空行
        return MultipleNewlinesRegex().Replace(html, "\n\n").Trim();
    }

    [GeneratedRegex(@"^<.*?>$")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex MultipleNewlinesRegex();
}
