// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 版权页生成器实现.
/// </summary>
internal sealed class CopyrightPageGenerator : ICopyrightPageGenerator
{
    /// <inheritdoc/>
    public string Generate(EpubMetadata metadata)
    {
        var copyright = metadata.Copyright;

        return EpubTemplates.CopyrightPage
            .ReplaceOrdinal("{{Language}}", metadata.Language)
            .ReplaceOrdinal("{{Title}}", metadata.Title.XmlEncode())
            .ReplaceOrdinal("{{AuthorSection}}", GenerateSection("作者", metadata.Author))
            .ReplaceOrdinal("{{PublisherSection}}", GenerateSection("出版", metadata.Publisher))
            .ReplaceOrdinal("{{IsbnSection}}", GenerateSection("ISBN", copyright?.Isbn))
            .ReplaceOrdinal("{{EditionSection}}", GenerateSection("版次", copyright?.Edition))
            .ReplaceOrdinal("{{PublishDateSection}}", GenerateDateSection("出版日期", copyright?.PublishDate ?? metadata.PublishDate))
            .ReplaceOrdinal("{{CopyrightSection}}", GenerateSection("版权", copyright?.Copyright))
            .ReplaceOrdinal("{{RightsSection}}", GenerateSection("权利声明", copyright?.Rights));
    }

    private static string GenerateSection(string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return $"            <p><strong>{label}：</strong>{value.XmlEncode()}</p>";
    }

    private static string GenerateDateSection(string label, DateTimeOffset? date)
    {
        if (date is null)
        {
            return string.Empty;
        }

        return $"            <p><strong>{label}：</strong>{date.Value:yyyy年M月d日}</p>";
    }
}
