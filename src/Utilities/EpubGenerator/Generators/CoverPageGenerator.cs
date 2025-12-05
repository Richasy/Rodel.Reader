// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 封面页生成器实现.
/// </summary>
internal sealed class CoverPageGenerator : ICoverPageGenerator
{
    /// <inheritdoc/>
    public string Generate(CoverInfo cover, string title)
    {
        return EpubTemplates.CoverPage
            .ReplaceOrdinal("{{Language}}", "zh")
            .ReplaceOrdinal("{{Title}}", title.XmlEncode())
            .ReplaceOrdinal("{{CoverFileName}}", cover.FileName);
    }
}
