// Copyright (c) Reader Copilot. All rights reserved.

using System.IO.Compression;

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// EPUB 验证器实现.
/// </summary>
internal sealed class EpubValidator : IEpubValidator
{
    private const string ErrorCodeMissingTitle = "E001";
    private const string ErrorCodeMissingChapters = "E002";
    private const string ErrorCodeEmptyChapter = "E003";
    private const string ErrorCodeInvalidCover = "E004";
    private const string ErrorCodeInvalidResource = "E005";
    private const string ErrorCodeInvalidAnchor = "E006";
    private const string ErrorCodeInvalidChapterImage = "E007";
    private const string ErrorCodeInvalidFile = "E008";
    private const string ErrorCodeMissingMimetype = "E009";
    private const string ErrorCodeMissingContainer = "E010";
    private const string ErrorCodeMissingOpf = "E011";
    private const string ErrorCodeInvalidContent = "E012";

    private const string WarningCodeMissingAuthor = "W001";
    private const string WarningCodeMissingLanguage = "W002";
    private const string WarningCodeMissingIdentifier = "W003";
    private const string WarningCodeEmptyChapterContent = "W004";

    /// <inheritdoc/>
    public ValidationResult ValidateInput(EpubMetadata metadata, IReadOnlyList<ChapterInfo> chapters, EpubOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(chapters);

        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        ValidateMetadata(metadata, errors, warnings);
        ValidateChapters(chapters, errors, warnings);
        ValidateCover(metadata.Cover, errors);

        if (options?.Resources is not null)
        {
            ValidateResources(options.Resources, errors);
        }

        return CreateResult(errors, warnings);
    }

    /// <inheritdoc/>
    public ValidationResult ValidateContent(EpubContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        // 验证必需的内容字段
        if (string.IsNullOrWhiteSpace(content.Mimetype))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidContent,
                Message = "Mimetype 内容为空"
            });
        }

        if (string.IsNullOrWhiteSpace(content.ContainerXml))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidContent,
                Message = "Container.xml 内容为空"
            });
        }

        if (string.IsNullOrWhiteSpace(content.ContentOpf))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidContent,
                Message = "Content.opf 内容为空"
            });
        }

        if (string.IsNullOrWhiteSpace(content.TocNcx))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidContent,
                Message = "Toc.ncx 内容为空"
            });
        }

        if (content.Chapters is null || content.Chapters.Count == 0)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeMissingChapters,
                Message = "没有章节内容"
            });
        }

        // 验证封面
        if (content.Cover is not null)
        {
            ValidateCover(content.Cover, errors);
        }

        // 验证章节图片
        if (content.ChapterImages is not null)
        {
            foreach (var image in content.ChapterImages)
            {
                ValidateChapterImageInfo(image, errors);
            }
        }

        // 验证资源
        if (content.Resources is not null)
        {
            ValidateResources(content.Resources, errors);
        }

        return CreateResult(errors, warnings);
    }

    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        if (!File.Exists(filePath))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidFile,
                Message = "指定的文件不存在",
                FilePath = filePath
            });
            return ValidationResult.Failure(errors);
        }

        try
        {
            await Task.Run(() => ValidateEpubStructure(filePath, errors, warnings), cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidDataException)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidFile,
                Message = "文件不是有效的 ZIP 格式",
                FilePath = filePath
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidFile,
                Message = $"无法读取文件: {ex.Message}",
                FilePath = filePath
            });
        }

        return CreateResult(errors, warnings);
    }

    private static ValidationResult CreateResult(List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        if (errors.Count > 0)
        {
            return ValidationResult.Failure(errors, warnings.Count > 0 ? warnings : null);
        }

        return warnings.Count > 0
            ? new ValidationResult { IsValid = true, Warnings = warnings }
            : ValidationResult.Success();
    }

    private static void ValidateMetadata(EpubMetadata metadata, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        if (string.IsNullOrWhiteSpace(metadata.Title))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeMissingTitle,
                Message = "缺少标题"
            });
        }

        if (string.IsNullOrWhiteSpace(metadata.Author))
        {
            warnings.Add(new ValidationWarning
            {
                Code = WarningCodeMissingAuthor,
                Message = "未指定作者"
            });
        }

        if (string.IsNullOrWhiteSpace(metadata.Language))
        {
            warnings.Add(new ValidationWarning
            {
                Code = WarningCodeMissingLanguage,
                Message = "未指定语言，将使用默认值"
            });
        }

        if (string.IsNullOrWhiteSpace(metadata.Identifier))
        {
            warnings.Add(new ValidationWarning
            {
                Code = WarningCodeMissingIdentifier,
                Message = "未指定标识符，将自动生成"
            });
        }
    }

    private static void ValidateChapters(IReadOnlyList<ChapterInfo> chapters, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        if (chapters.Count == 0)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeMissingChapters,
                Message = "至少需要一个章节"
            });
            return;
        }

        for (var i = 0; i < chapters.Count; i++)
        {
            var chapter = chapters[i];
            var chapterName = $"第 {i + 1} 章";

            if (string.IsNullOrWhiteSpace(chapter.Title))
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeEmptyChapter,
                    Message = $"{chapterName} 缺少标题",
                    FilePath = $"chapter_{i}.xhtml"
                });
            }

            if (string.IsNullOrWhiteSpace(chapter.Content))
            {
                warnings.Add(new ValidationWarning
                {
                    Code = WarningCodeEmptyChapterContent,
                    Message = $"{chapterName} 内容为空",
                    FilePath = $"chapter_{i}.xhtml"
                });
            }

            ValidateAnchors(chapter.Anchors, chapterName, i, errors);
            ValidateChapterImages(chapter.Images, chapterName, i, errors);
        }
    }

    private static void ValidateAnchors(IReadOnlyList<AnchorInfo>? anchors, string chapterName, int chapterIndex, List<ValidationError> errors)
    {
        if (anchors is null || anchors.Count == 0)
        {
            return;
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var anchor in anchors)
        {
            if (string.IsNullOrWhiteSpace(anchor.Id))
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeInvalidAnchor,
                    Message = $"{chapterName} 包含无效的锚点：缺少 ID",
                    FilePath = $"chapter_{chapterIndex}.xhtml"
                });
            }
            else if (!seenIds.Add(anchor.Id))
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeInvalidAnchor,
                    Message = $"{chapterName} 包含重复的锚点 ID: {anchor.Id}",
                    FilePath = $"chapter_{chapterIndex}.xhtml"
                });
            }
            else if (!IsValidXmlId(anchor.Id))
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeInvalidAnchor,
                    Message = $"{chapterName} 包含无效的锚点 ID 格式: {anchor.Id}",
                    FilePath = $"chapter_{chapterIndex}.xhtml"
                });
            }

            if (string.IsNullOrWhiteSpace(anchor.Title))
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeInvalidAnchor,
                    Message = $"{chapterName} 包含无效的锚点：缺少标题",
                    FilePath = $"chapter_{chapterIndex}.xhtml"
                });
            }
        }
    }

    private static void ValidateChapterImages(IReadOnlyList<ChapterImageInfo>? images, string chapterName, int chapterIndex, List<ValidationError> errors)
    {
        if (images is null || images.Count == 0)
        {
            return;
        }

        foreach (var image in images)
        {
            ValidateChapterImageInfoWithContext(image, chapterName, chapterIndex, errors);
        }
    }

    private static void ValidateChapterImageInfoWithContext(ChapterImageInfo image, string chapterName, int chapterIndex, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(image.Id))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidChapterImage,
                Message = $"{chapterName} 包含无效的图片：缺少 ID",
                FilePath = $"chapter_{chapterIndex}.xhtml"
            });
        }

        if (image.ImageData.IsEmpty)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidChapterImage,
                Message = $"{chapterName} 包含无效的图片：数据为空",
                FilePath = $"chapter_{chapterIndex}.xhtml"
            });
        }

        if (string.IsNullOrWhiteSpace(image.MediaType))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidChapterImage,
                Message = $"{chapterName} 包含无效的图片：缺少媒体类型",
                FilePath = $"chapter_{chapterIndex}.xhtml"
            });
        }
    }

    private static void ValidateChapterImageInfo(ChapterImageInfo image, List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(image.Id))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidChapterImage,
                Message = "章节图片缺少 ID"
            });
        }

        if (image.ImageData.IsEmpty)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidChapterImage,
                Message = $"章节图片 '{image.Id}' 数据为空"
            });
        }

        if (string.IsNullOrWhiteSpace(image.MediaType))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidChapterImage,
                Message = $"章节图片 '{image.Id}' 缺少媒体类型"
            });
        }
    }

    private static void ValidateCover(CoverInfo? cover, List<ValidationError> errors)
    {
        if (cover is null)
        {
            return;
        }

        if (cover.ImageData.IsEmpty)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidCover,
                Message = "封面图片数据为空"
            });
        }

        if (string.IsNullOrWhiteSpace(cover.MediaType))
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeInvalidCover,
                Message = "封面图片缺少媒体类型"
            });
        }
    }

    private static void ValidateResources(IReadOnlyList<ResourceInfo> resources, List<ValidationError> errors)
    {
        if (resources.Count == 0)
        {
            return;
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var resource in resources)
        {
            if (string.IsNullOrWhiteSpace(resource.Id))
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeInvalidResource,
                    Message = "资源缺少 ID"
                });
            }
            else if (!seenIds.Add(resource.Id))
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeInvalidResource,
                    Message = $"资源 ID 重复: {resource.Id}"
                });
            }

            if (resource.Data.IsEmpty)
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeInvalidResource,
                    Message = $"资源 '{resource.Id}' 数据为空"
                });
            }

            if (string.IsNullOrWhiteSpace(resource.MediaType))
            {
                errors.Add(new ValidationError
                {
                    Code = ErrorCodeInvalidResource,
                    Message = $"资源 '{resource.Id}' 缺少媒体类型"
                });
            }
        }
    }

    private static void ValidateEpubStructure(string filePath, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        using var archive = ZipFile.OpenRead(filePath);

        // 检查 mimetype 文件
        var mimetypeEntry = archive.GetEntry("mimetype");
        if (mimetypeEntry is null)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeMissingMimetype,
                Message = "缺少 mimetype 文件",
                FilePath = "mimetype"
            });
        }

        // 检查 container.xml
        var containerEntry = archive.GetEntry("META-INF/container.xml");
        if (containerEntry is null)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeMissingContainer,
                Message = "缺少 META-INF/container.xml 文件",
                FilePath = "META-INF/container.xml"
            });
        }

        // 检查 OPF 文件
        var hasOpf = false;
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(".opf", StringComparison.OrdinalIgnoreCase))
            {
                hasOpf = true;
                break;
            }
        }

        if (!hasOpf)
        {
            errors.Add(new ValidationError
            {
                Code = ErrorCodeMissingOpf,
                Message = "缺少 OPF 文件",
                FilePath = "OEBPS/content.opf"
            });
        }
    }

    private static bool IsValidXmlId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        // XML ID 必须以字母或下划线开头
        var firstChar = id[0];
        if (!char.IsLetter(firstChar) && firstChar != '_')
        {
            return false;
        }

        // 其余字符可以是字母、数字、连字符、下划线或点
        for (var i = 1; i < id.Length; i++)
        {
            var c = id[i];
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != '.')
            {
                return false;
            }
        }

        return true;
    }
}
