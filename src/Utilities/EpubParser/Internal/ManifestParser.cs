// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 解析 OPF 包的 manifest 部分。
/// </summary>
internal static class ManifestParser
{
    /// <summary>
    /// 解析 manifest 项目。
    /// </summary>
    public static List<EpubResource> Parse(XElement? manifestElement, string contentDirectoryPath)
    {
        var resources = new List<EpubResource>();

        if (manifestElement == null)
        {
            return resources;
        }

        foreach (var item in manifestElement.Elements())
        {
            if (!item.HasLocalName("item"))
            {
                continue;
            }

            var id = item.GetAttributeValue("id");
            var href = item.GetAttributeValue("href");
            var mediaType = item.GetAttributeValue("media-type");
            var propertiesAttr = item.GetAttributeValue("properties");

            // 跳过缺少必要信息的项目
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(href))
            {
                continue;
            }

            // 解码 href
            href = Uri.UnescapeDataString(href);

            // 跳过远程资源
            if (PathHelper.IsRemoteUrl(href))
            {
                continue;
            }

            var resource = new EpubResource
            {
                Id = id,
                Href = href,
                FullPath = PathHelper.Combine(contentDirectoryPath, href),
                MediaType = mediaType ?? "application/octet-stream",
            };

            // 解析属性（EPUB 3）
            if (!string.IsNullOrEmpty(propertiesAttr))
            {
                resource.Properties = propertiesAttr
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            resources.Add(resource);
        }

        return resources;
    }

    /// <summary>
    /// 从 EPUB 3 属性中查找封面图片资源。
    /// </summary>
    public static EpubResource? FindEpub3Cover(List<EpubResource> resources)
    {
        return resources.FirstOrDefault(r =>
            r.Properties.Contains("cover-image", StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 根据 ID 获取资源。
    /// </summary>
    public static EpubResource? GetById(List<EpubResource> resources, string id)
    {
        return resources.FirstOrDefault(r =>
            r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 根据 href 获取资源。
    /// </summary>
    public static EpubResource? GetByHref(List<EpubResource> resources, string href, string baseDirectory = "")
    {
        var normalizedHref = PathHelper.NormalizePath(href);
        var fullPath = string.IsNullOrEmpty(baseDirectory)
            ? normalizedHref
            : PathHelper.Combine(baseDirectory, href);

        return resources.FirstOrDefault(r =>
            r.Href.Equals(normalizedHref, StringComparison.OrdinalIgnoreCase) ||
            r.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase) ||
            r.FullPath.Equals(normalizedHref, StringComparison.OrdinalIgnoreCase));
    }
}
