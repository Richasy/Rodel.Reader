// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 解析 OPF 包的 spine 部分。
/// </summary>
internal static class SpineParser
{
    /// <summary>
    /// 解析 spine 并返回阅读顺序。
    /// </summary>
    public static List<EpubResource> Parse(XElement? spineElement, List<EpubResource> resources)
    {
        var readingOrder = new List<EpubResource>();

        if (spineElement == null)
        {
            return readingOrder;
        }

        foreach (var itemref in spineElement.Elements())
        {
            if (!itemref.HasLocalName("itemref"))
            {
                continue;
            }

            var idref = itemref.GetAttributeValue("idref");
            if (string.IsNullOrEmpty(idref))
            {
                continue;
            }

            // 检查是否线性（默认为是）
            var linear = itemref.GetAttributeValue("linear");
            if (linear?.Equals("no", StringComparison.OrdinalIgnoreCase) == true)
            {
                continue;
            }

            var resource = ManifestParser.GetById(resources, idref);
            if (resource != null)
            {
                readingOrder.Add(resource);
            }
        }

        return readingOrder;
    }

    /// <summary>
    /// 从 spine 的 toc 属性获取 NCX ID。
    /// </summary>
    public static string? GetNcxId(XElement? spineElement)
    {
        return spineElement?.GetAttributeValue("toc");
    }
}
