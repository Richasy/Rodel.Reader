// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 在 EPUB 中查找封面图片。
/// </summary>
internal static class CoverFinder
{
    /// <summary>
    /// 查找封面资源。
    /// </summary>
    public static EpubResource? Find(
        List<EpubResource> resources,
        XElement? metadataElement,
        XElement? guideElement)
    {
        // 1. EPUB 3: cover-image 属性
        var cover = ManifestParser.FindEpub3Cover(resources);
        if (cover != null)
        {
            return cover;
        }

        // 2. EPUB 2: meta name="cover"
        cover = FindFromMetadata(resources, metadataElement);
        if (cover != null)
        {
            return cover;
        }

        // 3. Guide: type="cover"
        cover = FindFromGuide(resources, guideElement);
        if (cover != null)
        {
            return cover;
        }

        // 4. 回退：名称中包含 "cover" 的第一张图片
        cover = resources.FirstOrDefault(r =>
            r.IsImage &&
            r.Href.Contains("cover", StringComparison.OrdinalIgnoreCase));

        return cover;
    }

    private static EpubResource? FindFromMetadata(
        List<EpubResource> resources,
        XElement? metadataElement)
    {
        if (metadataElement == null)
        {
            return null;
        }

        foreach (var meta in metadataElement.GetElements("meta"))
        {
            var name = meta.GetAttributeValue("name");
            if (name?.Equals("cover", StringComparison.OrdinalIgnoreCase) != true)
            {
                continue;
            }

            var content = meta.GetAttributeValue("content");
            if (string.IsNullOrEmpty(content))
            {
                continue;
            }

            // content 是 manifest 项目的 ID
            var coverResource = ManifestParser.GetById(resources, content);
            if (coverResource?.IsImage == true)
            {
                return coverResource;
            }
        }

        return null;
    }

    private static EpubResource? FindFromGuide(
        List<EpubResource> resources,
        XElement? guideElement)
    {
        if (guideElement == null)
        {
            return null;
        }

        foreach (var reference in guideElement.GetElements("reference"))
        {
            var type = reference.GetAttributeValue("type");
            if (type?.Equals("cover", StringComparison.OrdinalIgnoreCase) != true)
            {
                continue;
            }

            var href = reference.GetAttributeValue("href");
            if (string.IsNullOrEmpty(href))
            {
                continue;
            }

            href = Uri.UnescapeDataString(href);
            var (path, _) = PathHelper.SplitAnchor(href);

            // 这可能是包含封面的 HTML 页面
            var resource = ManifestParser.GetByHref(resources, path);
            if (resource?.IsImage == true)
            {
                return resource;
            }
        }

        return null;
    }
}
