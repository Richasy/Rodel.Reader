// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.EpubParser;

/// <summary>
/// 从 OPF 包解析 EPUB 元数据。
/// </summary>
internal static class MetadataParser
{
    /// <summary>
    /// 从 metadata 元素解析元数据。
    /// </summary>
    public static EpubMetadata Parse(XElement? metadataElement)
    {
        var metadata = new EpubMetadata();

        if (metadataElement == null)
        {
            return metadata;
        }

        foreach (var element in metadataElement.Elements())
        {
            var localName = element.GetLocalName();
            var value = element.Value?.Trim();

            switch (localName)
            {
                case "title":
                    if (!string.IsNullOrEmpty(value))
                    {
                        metadata.Title ??= value;
                    }
                    break;

                case "creator":
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        metadata.Authors.Add(value);
                    }
                    break;

                case "contributor":
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        metadata.Contributors.Add(value);
                    }
                    break;

                case "description":
                    if (!string.IsNullOrEmpty(value))
                    {
                        metadata.Description ??= CleanDescription(value);
                    }
                    break;

                case "publisher":
                    if (!string.IsNullOrEmpty(value))
                    {
                        metadata.Publisher ??= value;
                    }
                    break;

                case "language":
                    if (!string.IsNullOrEmpty(value))
                    {
                        metadata.Language ??= value;
                    }
                    break;

                case "date":
                    if (!string.IsNullOrEmpty(value))
                    {
                        metadata.PublishDate ??= value;
                    }
                    break;

                case "identifier":
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (metadata.Identifier == null || IsIsbn(value))
                        {
                            metadata.Identifier = value;
                        }
                    }
                    break;

                case "subject":
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        metadata.Subjects.Add(value);
                    }
                    break;

                case "rights":
                    if (!string.IsNullOrEmpty(value))
                    {
                        metadata.Rights ??= value;
                    }
                    break;

                case "meta":
                    var metaItem = ParseMetaElement(element);
                    if (metaItem != null)
                    {
                        metadata.MetaItems.Add(metaItem);

                        // 同时添加到自定义元数据字典
                        var key = metaItem.Name ?? metaItem.Property;
                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(metaItem.Content))
                        {
                            metadata.CustomMetadata.TryAdd(key, metaItem.Content);
                        }
                    }
                    break;
            }
        }

        return metadata;
    }

    /// <summary>
    /// 解析 meta 元素。
    /// </summary>
    private static EpubMetaItem? ParseMetaElement(XElement element)
    {
        var name = element.GetAttributeValue("name");
        var content = element.GetAttributeValue("content");
        var property = element.GetAttributeValue("property");
        var refines = element.GetAttributeValue("refines");
        var scheme = element.GetAttributeValue("scheme");
        var id = element.GetAttributeValue("id");

        // EPUB 3 格式：内容在元素值中
        if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(property))
        {
            content = element.Value?.Trim();
        }

        // 至少需要有 name/property 或 content
        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(property) && string.IsNullOrEmpty(content))
        {
            return null;
        }

        return new EpubMetaItem
        {
            Name = name,
            Content = content,
            Property = property,
            Refines = refines,
            Scheme = scheme,
            Id = id,
        };
    }

    private static string CleanDescription(string description)
    {
        // 如果存在 HTML 标签则移除
        if (description.Contains('<', StringComparison.Ordinal))
        {
            description = System.Text.RegularExpressions.Regex.Replace(
                description, "<[^>]+>", " ");
            description = System.Text.RegularExpressions.Regex.Replace(
                description, @"\s+", " ").Trim();
        }

        return description;
    }

    private static bool IsIsbn(string value)
    {
        // 简单检查 ISBN 格式
        return value.StartsWith("978", StringComparison.Ordinal) ||
               value.StartsWith("979", StringComparison.Ordinal) ||
               value.Contains("isbn", StringComparison.OrdinalIgnoreCase);
    }
}
