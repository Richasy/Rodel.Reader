// Copyright (c) Reader Copilot. All rights reserved.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Richasy.ReaderKernel.Toolkits.WinUI;

/// <summary>
/// 内核文本文件工具箱.
/// </summary>
public sealed partial class KernelTxtToolkit
{
    private static bool IsTitle(string line, Regex matchRegex)
    {
        var result = matchRegex.IsMatch(line);
        if (result)
        {
            var title = matchRegex.Match(line).Value;
            if (title.Length > MAX_CHAPTER_TITLE_LENGTH)
            {
                return false;
            }
        }

        return result;
    }

    private static bool IsExtra(string line) => line.Length <= MAX_CHAPTER_TITLE_LENGTH && CHAPTER_EXTRA_KEYS.Any(line.StartsWith);

    private static async Task GenerateHtmlFromTxtAsync(string title, string[] lines, int index, string folderPath = "")
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            folderPath = TXT_FOLDER_PATH;
        }

        var fileTitle = NormalizeTitle(title);
        var chapterTemplate = await ReadFileContentWithLongPathAsync(ASSETS_PAGE_PATH).ConfigureAwait(false);
        chapterTemplate = chapterTemplate.Replace("{{title}}", fileTitle, StringComparison.InvariantCultureIgnoreCase)
                                         .Replace("{{body}}", string.Concat(lines.Select(p => $"<p>{p}</p>")), StringComparison.InvariantCultureIgnoreCase)
                                         .Replace("{{style}}", string.Empty, StringComparison.InvariantCultureIgnoreCase);
        _chapterNames ??= [];

        _chapterNames.Add(index.ToString("0000", CultureInfo.InvariantCulture), title);
        var filePath = Path.Combine(folderPath, $"{index:0000}.html");
        await WriteFileContentWithLongPathAsync(filePath, chapterTemplate).ConfigureAwait(false);
    }

    private static async Task GenerateTitlePageFromTxtAsync(string title, string folderPath = "")
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            folderPath = TXT_FOLDER_PATH;
        }

        var titleTemplate = await ReadFileContentWithLongPathAsync(ASSETS_TITLE_PAGE_PATH).ConfigureAwait(false);
        titleTemplate = titleTemplate.Replace("{{title}}", title, StringComparison.InvariantCultureIgnoreCase);
        await WriteFileContentWithLongPathAsync(Path.Combine(folderPath, "titlepage.html"), titleTemplate).ConfigureAwait(false);
    }

    private static string NormalizeTitle(string title)
    {
        return title.Replace("&", "&amp;", StringComparison.InvariantCultureIgnoreCase)
                .Replace("<", "&lt;", StringComparison.InvariantCultureIgnoreCase)
                .Replace(">", "&gt;", StringComparison.InvariantCultureIgnoreCase)
                .Replace("\"", "&quot;", StringComparison.InvariantCultureIgnoreCase)
                .Replace("\'", "&apos;", StringComparison.InvariantCultureIgnoreCase);
    }
}
