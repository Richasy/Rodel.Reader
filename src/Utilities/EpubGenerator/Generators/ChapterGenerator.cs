// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 章节页面生成器实现.
/// </summary>
internal sealed class ChapterGenerator : IChapterGenerator
{
    /// <inheritdoc/>
    public string Generate(ChapterInfo chapter)
    {
        var content = chapter.IsHtml
            ? InsertImagesToHtml(chapter.Content, chapter.Images)
            : ConvertTextToHtmlWithImages(chapter.Content, chapter.Images);

        return EpubTemplates.ChapterPage
            .ReplaceOrdinal("{{Language}}", "zh")
            .ReplaceOrdinal("{{Title}}", chapter.Title.XmlEncode())
            .ReplaceOrdinal("{{Content}}", content);
    }

    private static string ConvertTextToHtmlWithImages(string text, IReadOnlyList<ChapterImageInfo>? images)
    {
        if (string.IsNullOrEmpty(text))
        {
            return GenerateImagesOnlyHtml(images);
        }

        // 如果没有图片，使用简单的转换逻辑
        if (images is null || images.Count == 0)
        {
            return ConvertTextToHtml(text);
        }

        // 按偏移量排序图片（从大到小处理，避免偏移量变化）
        var sortedImages = images.OrderBy(img => img.Offset).ToList();

        var sb = StringBuilderPool.Rent();
        var lines = text.Split('\n');
        var currentOffset = 0;
        var imageIndex = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.TrimEnd('\r').Trim();
            var lineLength = line.Length + 1; // +1 for newline

            // 在当前行之前插入所有偏移量在此范围内的图片
            while (imageIndex < sortedImages.Count && sortedImages[imageIndex].Offset <= currentOffset)
            {
                sb.AppendLine(GenerateImageHtml(sortedImages[imageIndex]));
                imageIndex++;
            }

            if (!string.IsNullOrEmpty(trimmedLine))
            {
                sb.AppendLine($"            <p>{trimmedLine.XmlEncode()}</p>");
            }

            currentOffset += lineLength;
        }

        // 添加剩余的图片（偏移量超出文本长度的）
        while (imageIndex < sortedImages.Count)
        {
            sb.AppendLine(GenerateImageHtml(sortedImages[imageIndex]));
            imageIndex++;
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string ConvertTextToHtml(string text)
    {
        var sb = StringBuilderPool.Rent();
        var lines = text.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.TrimEnd('\r').Trim();
            if (string.IsNullOrEmpty(trimmedLine))
            {
                continue;
            }

            sb.AppendLine($"            <p>{trimmedLine.XmlEncode()}</p>");
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }

    private static string InsertImagesToHtml(string html, IReadOnlyList<ChapterImageInfo>? images)
    {
        if (images is null || images.Count == 0)
        {
            return html;
        }

        // 对于已经是 HTML 的内容，按偏移量插入图片
        // 这里使用简单策略：在偏移位置后的下一个闭合标签后插入
        var sortedImages = images.OrderByDescending(img => img.Offset).ToList();

        var result = html;
        foreach (var image in sortedImages)
        {
            var imageHtml = GenerateImageHtml(image);

            if (image.Offset >= result.Length)
            {
                // 偏移量超出内容，追加到末尾
                result = result + "\n" + imageHtml;
            }
            else
            {
                // 在偏移位置后寻找合适的插入点（下一个 > 或换行）
                var insertPos = FindInsertPosition(result, image.Offset);
                result = result.Insert(insertPos, "\n" + imageHtml);
            }
        }

        return result;
    }

    private static int FindInsertPosition(string html, int offset)
    {
        // 从偏移位置开始，找到下一个闭合标签 > 或换行符
        for (var i = offset; i < html.Length; i++)
        {
            if (html[i] == '>' || html[i] == '\n')
            {
                return i + 1;
            }
        }

        return html.Length;
    }

    private static string GenerateImageHtml(ChapterImageInfo image)
    {
        var altText = image.AltText ?? "插图";
        var imgTag = $"            <img src=\"../Images/{image.FileName}\" alt=\"{altText.XmlEncode()}\" />";

        if (string.IsNullOrEmpty(image.Caption))
        {
            return $"            <div class=\"image-container\">\n{imgTag}\n            </div>";
        }

        return $"            <figure class=\"image-figure\">\n{imgTag}\n                <figcaption>{image.Caption.XmlEncode()}</figcaption>\n            </figure>";
    }

    private static string GenerateImagesOnlyHtml(IReadOnlyList<ChapterImageInfo>? images)
    {
        if (images is null || images.Count == 0)
        {
            return string.Empty;
        }

        var sb = StringBuilderPool.Rent();
        foreach (var image in images.OrderBy(img => img.Offset))
        {
            sb.AppendLine(GenerateImageHtml(image));
        }

        return StringBuilderPool.ToStringAndReturn(sb).TrimEnd();
    }
}
