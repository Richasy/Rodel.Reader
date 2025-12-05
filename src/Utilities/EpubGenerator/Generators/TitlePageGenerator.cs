// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 标题页生成器实现.
/// </summary>
internal sealed class TitlePageGenerator : ITitlePageGenerator
{
    /// <inheritdoc/>
    public string Generate(EpubMetadata metadata)
    {
        var authorSection = string.IsNullOrWhiteSpace(metadata.Author)
            ? string.Empty
            : $"            <p class=\"book-author\">{metadata.Author.XmlEncode()}</p>";

        return EpubTemplates.TitlePage
            .ReplaceOrdinal("{{Language}}", metadata.Language)
            .ReplaceOrdinal("{{Title}}", metadata.Title.XmlEncode())
            .ReplaceOrdinal("{{AuthorSection}}", authorSection);
    }
}
