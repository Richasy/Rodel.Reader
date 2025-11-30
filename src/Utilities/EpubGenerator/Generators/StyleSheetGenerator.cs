// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 样式表生成器实现.
/// </summary>
internal sealed class StyleSheetGenerator : IStyleSheetGenerator
{
    /// <inheritdoc/>
    public string Generate(EpubOptions? options = null)
    {
        var defaultCss = EpubTemplates.DefaultStyleSheet;

        if (options?.CustomCss is { Length: > 0 } customCss)
        {
            return $"{defaultCss}\n\n/* Custom Styles */\n{customCss}";
        }

        return defaultCss;
    }
}
