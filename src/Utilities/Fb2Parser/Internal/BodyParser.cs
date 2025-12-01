// Copyright (c) Richasy. All rights reserved.

using System.Text;
using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// FB2 body 节点解析器。
/// </summary>
internal static class BodyParser
{
    private static readonly XNamespace FbNs = "http://www.gribuser.ru/xml/fictionbook/2.0";
    private static readonly XNamespace XlinkNs = "http://www.w3.org/1999/xlink";

    /// <summary>
    /// 解析 body 元素，提取章节列表。
    /// </summary>
    /// <param name="bodyElements">body 元素列表。</param>
    /// <returns>章节列表。</returns>
    public static List<Fb2Section> Parse(IEnumerable<XElement> bodyElements)
    {
        var sections = new List<Fb2Section>();

        foreach (var body in bodyElements)
        {
            // 跳过注释类型的 body（如 notes）
            var bodyName = body.Attribute("name")?.Value;

            foreach (var sectionElement in GetElements(body, "section"))
            {
                var section = ParseSection(sectionElement, 0, bodyName);
                if (section != null)
                {
                    sections.Add(section);
                }
            }

            // 处理 body 的标题（如果有）
            var bodyTitle = GetElement(body, "title");
            if (bodyTitle != null && sections.Count == 0)
            {
                // 如果 body 有标题但没有 section，创建一个虚拟 section
                var section = new Fb2Section
                {
                    Title = ExtractTitleText(bodyTitle),
                    Content = body.ToString(),
                    PlainText = ExtractPlainText(body),
                    Level = 0,
                };

                ExtractImageIds(body, section.ImageIds);
                sections.Add(section);
            }
        }

        return sections;
    }

    private static Fb2Section? ParseSection(XElement sectionElement, int level, string? bodyName)
    {
        var section = new Fb2Section
        {
            Level = level,
        };

        // 获取 ID
        section.Id = sectionElement.Attribute("id")?.Value;

        // 解析标题
        var title = GetElement(sectionElement, "title");
        if (title != null)
        {
            section.Title = ExtractTitleText(title);
        }

        // 提取内容（原始 XML）
        section.Content = sectionElement.ToString();

        // 提取纯文本
        section.PlainText = ExtractPlainText(sectionElement);

        // 提取图片引用
        ExtractImageIds(sectionElement, section.ImageIds);

        // 递归解析子章节
        foreach (var childSection in GetElements(sectionElement, "section"))
        {
            var child = ParseSection(childSection, level + 1, bodyName);
            if (child != null)
            {
                section.Children.Add(child);
            }
        }

        return section;
    }

    private static string ExtractTitleText(XElement titleElement)
    {
        var parts = new List<string>();

        foreach (var p in GetElements(titleElement, "p"))
        {
            var text = p.Value.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                parts.Add(text);
            }
        }

        if (parts.Count == 0)
        {
            // 如果没有 p 元素，直接获取文本
            return titleElement.Value.Trim();
        }

        return string.Join(" ", parts);
    }

    private static string ExtractPlainText(XElement element)
    {
        var sb = new StringBuilder();
        ExtractPlainTextRecursive(element, sb);
        return sb.ToString().Trim();
    }

    private static void ExtractPlainTextRecursive(XElement element, StringBuilder sb)
    {
        foreach (var node in element.Nodes())
        {
            if (node is XText text)
            {
                sb.Append(text.Value);
            }
            else if (node is XElement child)
            {
                var localName = child.Name.LocalName;

                // 跳过标题（单独处理）
                if (localName == "title")
                {
                    continue;
                }

                // 跳过嵌套的 section（单独处理）
                if (localName == "section")
                {
                    continue;
                }

                // 段落添加换行
                if (localName == "p" || localName == "empty-line")
                {
                    if (sb.Length > 0 && sb[^1] != '\n')
                    {
                        sb.Append('\n');
                    }

                    ExtractPlainTextRecursive(child, sb);
                    sb.Append('\n');
                }
                else if (localName == "poem" || localName == "stanza" || localName == "cite")
                {
                    if (sb.Length > 0 && sb[^1] != '\n')
                    {
                        sb.Append('\n');
                    }

                    ExtractPlainTextRecursive(child, sb);
                    sb.Append('\n');
                }
                else if (localName == "v")
                {
                    // 诗行
                    ExtractPlainTextRecursive(child, sb);
                    sb.Append('\n');
                }
                else if (localName == "image")
                {
                    // 图片跳过
                    continue;
                }
                else
                {
                    ExtractPlainTextRecursive(child, sb);
                }
            }
        }
    }

    private static void ExtractImageIds(XElement element, List<string> imageIds)
    {
        foreach (var image in element.Descendants().Where(e => e.Name.LocalName == "image"))
        {
            var href = image.Attribute(XlinkNs + "href")?.Value
                ?? image.Attribute("href")?.Value
                ?? image.Attributes().FirstOrDefault(a => a.Name.LocalName == "href")?.Value;

            if (!string.IsNullOrEmpty(href))
            {
                var id = href.TrimStart('#');
                if (!imageIds.Contains(id))
                {
                    imageIds.Add(id);
                }
            }
        }
    }

    private static XElement? GetElement(XElement parent, string localName)
    {
        return parent.Element(FbNs + localName)
            ?? parent.Elements().FirstOrDefault(e => e.Name.LocalName == localName);
    }

    private static List<XElement> GetElements(XElement parent, string localName)
    {
        var elements = parent.Elements(FbNs + localName).ToList();
        if (elements.Count == 0)
        {
            elements = parent.Elements().Where(e => e.Name.LocalName == localName).ToList();
        }

        return elements;
    }
}
