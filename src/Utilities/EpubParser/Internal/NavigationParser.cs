// Copyright (c) Richasy. All rights reserved.

using System.IO.Compression;
using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 从 NCX（EPUB 2）或 Nav 文档（EPUB 3）解析 EPUB 导航（目录）。
/// </summary>
internal static class NavigationParser
{
    /// <summary>
    /// 从 EPUB 解析导航。
    /// </summary>
    public static async Task<List<EpubNavItem>> ParseAsync(
        ZipArchive archive,
        List<EpubResource> resources,
        string contentDirectoryPath,
        XElement? spineElement,
        XElement? metadataElement)
    {
        // 首先尝试 EPUB 3 nav 文档
        var navResource = FindNavResource(resources);
        if (navResource != null)
        {
            var navItems = await ParseEpub3NavAsync(archive, navResource, contentDirectoryPath).ConfigureAwait(false);
            if (navItems.Count > 0)
            {
                return navItems;
            }
        }

        // 回退到 EPUB 2 NCX
        var ncxResource = FindNcxResource(resources, spineElement);
        if (ncxResource != null)
        {
            return await ParseNcxAsync(archive, ncxResource, contentDirectoryPath).ConfigureAwait(false);
        }

        return [];
    }

    private static EpubResource? FindNavResource(List<EpubResource> resources)
    {
        // EPUB 3: 查找具有 nav 属性的资源
        return resources.FirstOrDefault(r =>
            r.Properties.Contains("nav", StringComparer.OrdinalIgnoreCase));
    }

    private static EpubResource? FindNcxResource(List<EpubResource> resources, XElement? spineElement)
    {
        // 首先尝试 spine toc 属性
        var tocId = SpineParser.GetNcxId(spineElement);
        if (!string.IsNullOrEmpty(tocId))
        {
            var ncx = ManifestParser.GetById(resources, tocId);
            if (ncx != null)
            {
                return ncx;
            }
        }

        // 回退：按媒体类型查找
        return resources.FirstOrDefault(r =>
            r.MediaType.Equals("application/x-dtbncx+xml", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 解析 EPUB 3 导航文档。
    /// </summary>
    private static async Task<List<EpubNavItem>> ParseEpub3NavAsync(
        ZipArchive archive,
        EpubResource navResource,
        string contentDirectoryPath)
    {
        var entry = archive.GetEntry(navResource.FullPath)
            ?? archive.Entries.FirstOrDefault(e =>
                e.FullName.Equals(navResource.FullPath, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            return [];
        }

        using var stream = entry.Open();
        var document = await XmlHelper.LoadDocumentAsync(stream).ConfigureAwait(false);

        if (document?.Root == null)
        {
            return [];
        }

        // 查找具有 epub:type="toc" 的 nav 元素
        var navBaseDirectory = PathHelper.GetDirectoryPath(navResource.FullPath);
        var tocNav = FindTocNavElement(document.Root);

        if (tocNav == null)
        {
            return [];
        }

        // 在 nav 内查找 ol 元素
        var ol = tocNav.GetElement("ol");
        if (ol == null)
        {
            return [];
        }

        return ParseNavOl(ol, navBaseDirectory, contentDirectoryPath);
    }

    private static XElement? FindTocNavElement(XElement root)
    {
        // 搜索具有 epub:type="toc" 的 nav 元素
        foreach (var nav in root.Descendants())
        {
            if (!nav.HasLocalName("nav"))
            {
                continue;
            }

            foreach (var attr in nav.Attributes())
            {
                if (attr.GetLocalName() == "type" &&
                    attr.Value.Contains("toc", StringComparison.OrdinalIgnoreCase))
                {
                    return nav;
                }
            }
        }

        // 回退：查找第一个 nav 元素
        return root.Descendants().FirstOrDefault(e => e.HasLocalName("nav"));
    }

    private static List<EpubNavItem> ParseNavOl(XElement ol, string navBaseDirectory, string contentDirectoryPath)
    {
        var items = new List<EpubNavItem>();

        foreach (var li in ol.Elements())
        {
            if (!li.HasLocalName("li"))
            {
                continue;
            }

            var navItem = ParseNavLi(li, navBaseDirectory, contentDirectoryPath);
            if (navItem != null)
            {
                items.Add(navItem);
            }
        }

        return items;
    }

    private static EpubNavItem? ParseNavLi(XElement li, string navBaseDirectory, string contentDirectoryPath)
    {
        // 查找 anchor 或 span
        var anchor = li.GetElement("a");
        var span = li.GetElement("span");

        string? title = null;
        string? href = null;

        if (anchor != null)
        {
            title = GetTextContent(anchor);
            href = anchor.GetAttributeValue("href");
        }
        else if (span != null)
        {
            title = GetTextContent(span);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var navItem = new EpubNavItem { Title = title.Trim() };

        if (!string.IsNullOrEmpty(href) && !PathHelper.IsRemoteUrl(href))
        {
            var (path, anchor_) = PathHelper.SplitAnchor(href);
            navItem.Href = path;
            navItem.Anchor = anchor_;
            navItem.FullPath = PathHelper.Combine(navBaseDirectory, path);
        }

        // 解析嵌套的 ol
        var nestedOl = li.GetElement("ol");
        if (nestedOl != null)
        {
            navItem.Children = ParseNavOl(nestedOl, navBaseDirectory, contentDirectoryPath);
        }

        return navItem;
    }

    /// <summary>
    /// 解析 EPUB 2 NCX 文档。
    /// </summary>
    private static async Task<List<EpubNavItem>> ParseNcxAsync(
        ZipArchive archive,
        EpubResource ncxResource,
        string contentDirectoryPath)
    {
        var entry = archive.GetEntry(ncxResource.FullPath)
            ?? archive.Entries.FirstOrDefault(e =>
                e.FullName.Equals(ncxResource.FullPath, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            return [];
        }

        using var stream = entry.Open();
        var document = await XmlHelper.LoadDocumentAsync(stream).ConfigureAwait(false);

        if (document?.Root == null)
        {
            return [];
        }

        var ncxBaseDirectory = PathHelper.GetDirectoryPath(ncxResource.FullPath);

        // 查找 navMap
        var navMap = document.Root.GetElement("navMap");
        if (navMap == null)
        {
            return [];
        }

        return ParseNavPoints(navMap, ncxBaseDirectory, contentDirectoryPath);
    }

    private static List<EpubNavItem> ParseNavPoints(XElement parent, string ncxBaseDirectory, string contentDirectoryPath)
    {
        var items = new List<EpubNavItem>();

        foreach (var navPoint in parent.GetElements("navPoint"))
        {
            var navItem = ParseNavPoint(navPoint, ncxBaseDirectory, contentDirectoryPath);
            if (navItem != null)
            {
                items.Add(navItem);
            }
        }

        return items;
    }

    private static EpubNavItem? ParseNavPoint(XElement navPoint, string ncxBaseDirectory, string contentDirectoryPath)
    {
        // 从 navLabel/text 获取标题
        var navLabel = navPoint.GetElement("navLabel");
        var text = navLabel?.GetElement("text");
        var title = text?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var navItem = new EpubNavItem { Title = title };

        // 获取 content src
        var content = navPoint.GetElement("content");
        var src = content?.GetAttributeValue("src");

        if (!string.IsNullOrEmpty(src) && !PathHelper.IsRemoteUrl(src))
        {
            src = Uri.UnescapeDataString(src);
            var (path, anchor) = PathHelper.SplitAnchor(src);
            navItem.Href = path;
            navItem.Anchor = anchor;
            navItem.FullPath = PathHelper.Combine(ncxBaseDirectory, path);
        }

        // 解析嵌套的 navPoints
        navItem.Children = ParseNavPoints(navPoint, ncxBaseDirectory, contentDirectoryPath);

        return navItem;
    }

    private static string GetTextContent(XElement element)
    {
        // 获取所有文本节点，忽略嵌套元素如 img
        return string.Concat(element.Nodes()
            .OfType<XText>()
            .Select(t => t.Value))
            .Trim();
    }
}
