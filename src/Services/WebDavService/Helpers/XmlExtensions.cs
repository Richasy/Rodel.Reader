// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// XML 扩展方法.
/// </summary>
internal static class XmlExtensions
{
    /// <summary>
    /// 获取元素的本地名称匹配的第一个子元素.
    /// </summary>
    /// <param name="element">父元素.</param>
    /// <param name="localName">本地名称.</param>
    /// <returns>匹配的元素，如果不存在则返回 null.</returns>
    public static XElement? LocalElement(this XElement element, string localName)
    {
        return element.Elements().FirstOrDefault(e => e.Name.LocalName == localName);
    }

    /// <summary>
    /// 获取元素的本地名称匹配的所有子元素.
    /// </summary>
    /// <param name="element">父元素.</param>
    /// <param name="localName">本地名称.</param>
    /// <returns>匹配的元素集合.</returns>
    public static IEnumerable<XElement> LocalElements(this XElement element, string localName)
    {
        return element.Elements().Where(e => e.Name.LocalName == localName);
    }

    /// <summary>
    /// 获取文档根元素的本地名称匹配的第一个子元素.
    /// </summary>
    /// <param name="document">文档.</param>
    /// <param name="localName">本地名称.</param>
    /// <returns>匹配的元素，如果不存在则返回 null.</returns>
    public static XElement? LocalElement(this XDocument document, string localName)
    {
        return document.Root?.LocalElement(localName);
    }

    /// <summary>
    /// 获取元素的本地名称匹配的后代元素.
    /// </summary>
    /// <param name="element">父元素.</param>
    /// <param name="localName">本地名称.</param>
    /// <returns>匹配的元素集合.</returns>
    public static IEnumerable<XElement> LocalDescendants(this XElement element, string localName)
    {
        return element.Descendants().Where(e => e.Name.LocalName == localName);
    }

    /// <summary>
    /// 安全地尝试解析 XML 文档.
    /// </summary>
    /// <param name="xml">XML 字符串.</param>
    /// <param name="document">解析后的文档.</param>
    /// <returns>是否成功解析.</returns>
    public static bool TryParse(string? xml, out XDocument? document)
    {
        document = null;
        if (string.IsNullOrWhiteSpace(xml))
        {
            return false;
        }

        try
        {
            document = XDocument.Parse(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
