// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 可视化目录页生成器实现.
/// </summary>
internal sealed class TocPageGenerator : ITocPageGenerator
{
    private const string DefaultTocTitle = "目录";

    /// <inheritdoc/>
    public string Generate(IReadOnlyList<ChapterInfo> chapters, string? title = null)
    {
        title ??= DefaultTocTitle;

        return EpubTemplates.TocPage
            .ReplaceOrdinal("{{Language}}", "zh")
            .ReplaceOrdinal("{{TocTitle}}", title.XmlEncode())
            .ReplaceOrdinal("{{TocItems}}", GenerateTocItems(chapters));
    }

    private static string GenerateTocItems(IReadOnlyList<ChapterInfo> chapters)
    {
        if (chapters.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();

        foreach (var chapter in chapters)
        {
            sb.AppendLine($"                    <li><a href=\"{chapter.FileName}.xhtml\">{chapter.Title.XmlEncode()}</a></li>");
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }
}
