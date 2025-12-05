// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// Nav 文档生成器实现 (EPUB 3).
/// </summary>
internal sealed class NavDocGenerator : INavDocGenerator
{
    /// <inheritdoc/>
    public string Generate(EpubMetadata metadata, IReadOnlyList<ChapterInfo> chapters)
    {
        var result = EpubTemplates.NavDoc
            .ReplaceOrdinal("{{Language}}", metadata.Language)
            .ReplaceOrdinal("{{Title}}", metadata.Title.XmlEncode())
            .ReplaceOrdinal("{{NavItems}}", GenerateNavItems(chapters));

        return result;
    }

    private static string GenerateNavItems(IReadOnlyList<ChapterInfo> chapters)
    {
        if (chapters.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();

        foreach (var chapter in chapters)
        {
            if (chapter.Anchors is { Count: > 0 })
            {
                // 有子章节的情况
                sb.AppendLine($"                <li>");
                sb.AppendLine($"                    <a href=\"Text/{chapter.FileName}.xhtml\">{chapter.Title.XmlEncode()}</a>");
                sb.AppendLine($"                    <ol>");
                foreach (var anchor in chapter.Anchors)
                {
                    sb.AppendLine($"                        <li><a href=\"Text/{chapter.FileName}.xhtml#{anchor.Id}\">{anchor.Title.XmlEncode()}</a></li>");
                }

                sb.AppendLine($"                    </ol>");
                sb.AppendLine($"                </li>");
            }
            else
            {
                sb.AppendLine($"                <li><a href=\"Text/{chapter.FileName}.xhtml\">{chapter.Title.XmlEncode()}</a></li>");
            }
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }
}
