// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// NCX 导航文件生成器实现.
/// </summary>
internal sealed class NcxGenerator : INcxGenerator
{
    /// <inheritdoc/>
    public string Generate(EpubMetadata metadata, IReadOnlyList<ChapterInfo> chapters)
    {
        var identifier = metadata.Identifier ?? Guid.NewGuid().ToString();
        var author = metadata.Author ?? string.Empty;

        var result = EpubTemplates.TocNcx
            .ReplaceOrdinal("{{Identifier}}", identifier)
            .ReplaceOrdinal("{{Title}}", metadata.Title.XmlEncode())
            .ReplaceOrdinal("{{Author}}", author.XmlEncode())
            .ReplaceOrdinal("{{NavPoints}}", GenerateNavPoints(chapters));

        return result;
    }

    private static string GenerateNavPoints(IReadOnlyList<ChapterInfo> chapters)
    {
        if (chapters.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();
        var playOrder = 1;

        foreach (var chapter in chapters)
        {
            sb.AppendLine($"        <navPoint id=\"navpoint-{chapter.FileName}\" playOrder=\"{playOrder}\">");
            sb.AppendLine($"            <navLabel>");
            sb.AppendLine($"                <text>{chapter.Title.XmlEncode()}</text>");
            sb.AppendLine($"            </navLabel>");
            sb.AppendLine($"            <content src=\"Text/{chapter.FileName}.xhtml\"/>");

            // 处理锚点/子章节
            if (chapter.Anchors is { Count: > 0 })
            {
                foreach (var anchor in chapter.Anchors)
                {
                    playOrder++;
                    sb.AppendLine($"            <navPoint id=\"navpoint-{chapter.FileName}-{anchor.Id}\" playOrder=\"{playOrder}\">");
                    sb.AppendLine($"                <navLabel>");
                    sb.AppendLine($"                    <text>{anchor.Title.XmlEncode()}</text>");
                    sb.AppendLine($"                </navLabel>");
                    sb.AppendLine($"                <content src=\"Text/{chapter.FileName}.xhtml#{anchor.Id}\"/>");
                    sb.AppendLine($"            </navPoint>");
                }
            }

            sb.AppendLine($"        </navPoint>");
            playOrder++;
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }
}
