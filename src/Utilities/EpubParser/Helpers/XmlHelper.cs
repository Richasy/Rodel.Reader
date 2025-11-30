// Copyright (c) Richasy. All rights reserved.

using System.Xml;
using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 具有容错能力的 XML 解析工具类。
/// </summary>
internal static class XmlHelper
{
    private static readonly XmlReaderSettings RelaxedSettings = new()
    {
        DtdProcessing = DtdProcessing.Ignore,
        IgnoreComments = true,
        IgnoreWhitespace = true,
        XmlResolver = null,
    };

    /// <summary>
    /// 从流中加载 XML 文档，使用宽松的解析设置。
    /// </summary>
    public static async Task<XDocument?> LoadDocumentAsync(Stream stream)
    {
        try
        {
            using var reader = XmlReader.Create(stream, RelaxedSettings);
            return await Task.Run(() => XDocument.Load(reader, LoadOptions.None)).ConfigureAwait(false);
        }
        catch
        {
            // 尝试重置流并以更宽松的方式解析
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            return await LoadWithRecoveryAsync(stream).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 尝试从常见问题中恢复并加载文档。
    /// </summary>
    private static async Task<XDocument?> LoadWithRecoveryAsync(Stream stream)
    {
        try
        {
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);

            // 尝试修复常见问题
            content = FixCommonXmlIssues(content);

            return XDocument.Parse(content, LoadOptions.None);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 尝试修复常见的 XML 问题。
    /// </summary>
    private static string FixCommonXmlIssues(string content)
    {
        // 如果存在 BOM 则移除
        if (content.Length > 0 && content[0] == '\uFEFF')
        {
            content = content[1..];
        }

        // 处理 XML 声明的常见问题
        if (!content.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
        {
            // 如果缺少则添加 XML 声明
            content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + content;
        }

        return content;
    }

    /// <summary>
    /// 获取小写的本地名称。
    /// </summary>
    public static string GetLocalName(this XElement element)
    {
        return element.Name.LocalName.ToLowerInvariant();
    }

    /// <summary>
    /// 获取小写的本地名称。
    /// </summary>
    public static string GetLocalName(this XAttribute attribute)
    {
        return attribute.Name.LocalName.ToLowerInvariant();
    }

    /// <summary>
    /// 尝试获取属性值。
    /// </summary>
    public static string? GetAttributeValue(this XElement element, string name)
    {
        return element.Attribute(name)?.Value
            ?? element.Attributes().FirstOrDefault(a =>
                a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    /// <summary>
    /// 按本地名称获取子元素（不区分大小写）。
    /// </summary>
    public static XElement? GetElement(this XElement element, string localName)
    {
        return element.Elements().FirstOrDefault(e =>
            e.Name.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 按本地名称获取子元素集合（不区分大小写）。
    /// </summary>
    public static IEnumerable<XElement> GetElements(this XElement element, string localName)
    {
        return element.Elements().Where(e =>
            e.Name.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 检查元素是否具有特定的本地名称（不区分大小写）。
    /// </summary>
    public static bool HasLocalName(this XElement element, string localName)
    {
        return element.Name.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase);
    }
}
