// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// OPF 文件生成器实现.
/// </summary>
internal sealed class OpfGenerator : IOpfGenerator
{
    /// <inheritdoc/>
    public string Generate(EpubMetadata metadata, IReadOnlyList<ChapterInfo> chapters, EpubOptions? options = null)
    {
        options ??= new EpubOptions();
        var template = options.Version == EpubVersion.Epub3
            ? EpubTemplates.ContentOpfEpub3
            : EpubTemplates.ContentOpfEpub2;

        var identifier = metadata.Identifier ?? Guid.NewGuid().ToString();
        var author = metadata.Author ?? string.Empty;
        var hasCover = metadata.Cover is not null;
        var hasTocPage = options.IncludeTocPage;
        var hasCopyrightPage = options.IncludeCopyrightPage && metadata.Copyright is not null;

        var result = template
            .ReplaceOrdinal("{{Title}}", metadata.Title.XmlEncode())
            .ReplaceOrdinal("{{Author}}", author.XmlEncode())
            .ReplaceOrdinal("{{Language}}", metadata.Language)
            .ReplaceOrdinal("{{Identifier}}", identifier)
            .ReplaceOrdinal("{{Date}}", (metadata.PublishDate ?? DateTimeOffset.Now).ToString("yyyy-MM-dd"))
            .ReplaceOrdinal("{{ModifiedDate}}", DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            .ReplaceOrdinal("{{Direction}}", GetDirection(options.Direction))
            .ReplaceOrdinal("{{PageProgression}}", GetPageProgression(options.PageProgression));

        // 可选元数据
        result = result
            .ReplaceOrdinal("{{Description}}", GenerateOptionalElement("dc:description", metadata.Description))
            .ReplaceOrdinal("{{Publisher}}", GenerateOptionalElement("dc:publisher", metadata.Publisher))
            .ReplaceOrdinal("{{Subjects}}", GenerateSubjects(metadata.Subjects))
            .ReplaceOrdinal("{{Contributors}}", GenerateContributors(metadata.Contributors))
            .ReplaceOrdinal("{{CustomMetadata}}", GenerateCustomMetadata(metadata.CustomMetadata, options.Version));

        // 封面相关
        result = result
            .ReplaceOrdinal("{{CoverMeta}}", hasCover ? "        <meta name=\"cover\" content=\"cover-image\"/>" : string.Empty)
            .ReplaceOrdinal("{{CoverImageItem}}", hasCover ? $"        <item id=\"cover-image\" href=\"Images/{metadata.Cover!.FileName}\" media-type=\"{metadata.Cover.MediaType}\" properties=\"cover-image\"/>" : string.Empty)
            .ReplaceOrdinal("{{CoverPageItem}}", hasCover ? "        <item id=\"cover\" href=\"Text/cover.xhtml\" media-type=\"application/xhtml+xml\"/>" : string.Empty)
            .ReplaceOrdinal("{{CoverPageRef}}", hasCover ? "        <itemref idref=\"cover\"/>" : string.Empty)
            .ReplaceOrdinal("{{CoverGuide}}", hasCover ? "        <reference type=\"cover\" title=\"Cover\" href=\"Text/cover.xhtml\"/>" : string.Empty);

        // 目录页
        result = result
            .ReplaceOrdinal("{{TocPageItem}}", hasTocPage ? "        <item id=\"toc-page\" href=\"Text/toc.xhtml\" media-type=\"application/xhtml+xml\"/>" : string.Empty)
            .ReplaceOrdinal("{{TocPageRef}}", hasTocPage ? "        <itemref idref=\"toc-page\"/>" : string.Empty)
            .ReplaceOrdinal("{{TocGuide}}", hasTocPage ? "        <reference type=\"toc\" title=\"Table of Contents\" href=\"Text/toc.xhtml\"/>" : string.Empty);

        // 版权页
        result = result
            .ReplaceOrdinal("{{CopyrightPageItem}}", hasCopyrightPage ? "        <item id=\"copyright\" href=\"Text/copyright.xhtml\" media-type=\"application/xhtml+xml\"/>" : string.Empty)
            .ReplaceOrdinal("{{CopyrightPageRef}}", hasCopyrightPage ? "        <itemref idref=\"copyright\"/>" : string.Empty);

        // 章节
        result = result
            .ReplaceOrdinal("{{ChapterItems}}", GenerateChapterItems(chapters))
            .ReplaceOrdinal("{{ChapterRefs}}", GenerateChapterRefs(chapters));

        // 章节图片
        result = result.ReplaceOrdinal("{{ChapterImageItems}}", GenerateChapterImageItems(chapters));

        // 资源
        result = result.ReplaceOrdinal("{{ResourceItems}}", GenerateResourceItems(options.Resources));

        // 清理空行
        return CleanupEmptyLines(result);
    }

    private static string GetDirection(WritingDirection direction) => direction switch
    {
        WritingDirection.Rtl => "rtl",
        WritingDirection.Ttb => "ltr", // EPUB 不直接支持 ttb，使用 CSS 处理
        _ => "ltr",
    };

    private static string GetPageProgression(PageProgression progression) => progression switch
    {
        PageProgression.Rtl => "rtl",
        _ => "ltr",
    };

    private static string GenerateOptionalElement(string element, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return $"        <{element}>{value.XmlEncode()}</{element}>";
    }

    private static string GenerateSubjects(IReadOnlyList<string>? subjects)
    {
        if (subjects is null || subjects.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();
        foreach (var subject in subjects)
        {
            sb.AppendLine($"        <dc:subject>{subject.XmlEncode()}</dc:subject>");
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string GenerateContributors(IReadOnlyList<string>? contributors)
    {
        if (contributors is null || contributors.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();
        foreach (var contributor in contributors)
        {
            sb.AppendLine($"        <dc:contributor>{contributor.XmlEncode()}</dc:contributor>");
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string GenerateChapterItems(IReadOnlyList<ChapterInfo> chapters)
    {
        if (chapters.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();
        foreach (var chapter in chapters)
        {
            sb.AppendLine($"        <item id=\"{chapter.FileName}\" href=\"Text/{chapter.FileName}.xhtml\" media-type=\"application/xhtml+xml\"/>");
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string GenerateChapterRefs(IReadOnlyList<ChapterInfo> chapters)
    {
        if (chapters.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();
        foreach (var chapter in chapters)
        {
            sb.AppendLine($"        <itemref idref=\"{chapter.FileName}\"/>");
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string GenerateResourceItems(IReadOnlyList<ResourceInfo>? resources)
    {
        if (resources is null || resources.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();
        foreach (var resource in resources)
        {
            var folder = resource.Type switch
            {
                ResourceType.Image => "Images",
                ResourceType.Font => "Fonts",
                ResourceType.Audio => "Audio",
                ResourceType.Video => "Video",
                _ => "Misc",
            };
            sb.AppendLine($"        <item id=\"{resource.Id}\" href=\"{folder}/{resource.FileName}\" media-type=\"{resource.MediaType}\"/>");
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string GenerateChapterImageItems(IReadOnlyList<ChapterInfo> chapters)
    {
        var sb = StringBuilderPool.Rent();
        var addedImages = new HashSet<string>(StringComparer.Ordinal);

        foreach (var chapter in chapters)
        {
            if (chapter.Images is not { Count: > 0 })
            {
                continue;
            }

            foreach (var image in chapter.Images)
            {
                // 避免重复添加同一图片
                if (addedImages.Add(image.Id))
                {
                    sb.AppendLine($"        <item id=\"{image.Id}\" href=\"Images/{image.FileName}\" media-type=\"{image.MediaType}\"/>");
                }
            }
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string GenerateCustomMetadata(IReadOnlyList<CustomMetadata>? customMetadata, EpubVersion version)
    {
        if (customMetadata is null || customMetadata.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();

        foreach (var meta in customMetadata)
        {
            if (version == EpubVersion.Epub3)
            {
                // EPUB 3 格式: <meta property="name" refines="#id">value</meta>
                if (!string.IsNullOrEmpty(meta.RefinesId))
                {
                    sb.AppendLine($"        <meta property=\"{meta.Name.XmlEncode()}\" refines=\"#{meta.RefinesId}\">{meta.Value.XmlEncode()}</meta>");
                }
                else
                {
                    sb.AppendLine($"        <meta property=\"{meta.Name.XmlEncode()}\">{meta.Value.XmlEncode()}</meta>");
                }
            }
            else
            {
                // EPUB 2 格式: <meta name="name" content="value" scheme="scheme"/>
                if (!string.IsNullOrEmpty(meta.Scheme))
                {
                    sb.AppendLine($"        <meta name=\"{meta.Name.XmlEncode()}\" content=\"{meta.Value.XmlEncode()}\" scheme=\"{meta.Scheme.XmlEncode()}\"/>");
                }
                else
                {
                    sb.AppendLine($"        <meta name=\"{meta.Name.XmlEncode()}\" content=\"{meta.Value.XmlEncode()}\"/>");
                }
            }
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string CleanupEmptyLines(string content)
    {
        var sb = StringBuilderPool.Rent();
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.TrimEnd('\r');
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                sb.AppendLine(trimmed);
            }
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }
}
