// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;
using Richasy.RodelReader.Utilities.FeedParser.Helpers;
using Richasy.RodelReader.Utilities.FeedParser.Internal;

namespace Richasy.RodelReader.Utilities.FeedParser.Parsers;

/// <summary>
/// Atom 解析器实现.
/// </summary>
public sealed class AtomParser : IFeedParser
{
    private readonly IXmlReaderFactory _xmlReaderFactory;

    /// <summary>
    /// 初始化 <see cref="AtomParser"/> 类的新实例.
    /// </summary>
    public AtomParser()
        : this(new XmlReaderFactory())
    {
    }

    /// <summary>
    /// 初始化 <see cref="AtomParser"/> 类的新实例（依赖注入）.
    /// </summary>
    /// <param name="xmlReaderFactory">XML 读取器工厂.</param>
    public AtomParser(IXmlReaderFactory xmlReaderFactory)
    {
        _xmlReaderFactory = xmlReaderFactory ?? throw new ArgumentNullException(nameof(xmlReaderFactory));
    }

    /// <inheritdoc/>
    public FeedChannel ParseChannel(XmlReader reader)
    {
        string? id = null;
        var title = string.Empty;
        string? description = null;
        string? copyright = null;
        string? generator = null;
        DateTimeOffset? updatedAt = null;

        var links = new List<FeedLink>();
        var images = new List<FeedImage>();
        var contributors = new List<FeedPerson>();
        var categories = new List<FeedCategory>();

        // 确保 reader 定位到 feed 元素内部
        var needsRead = EnsurePositionedAtFeed(reader);

        // 如果 EnsurePositionedAtFeed 返回 true，需要进入 feed 内部
        if (needsRead)
        {
            reader.Read();
        }

        while (!reader.EOF)
        {
            // 跳过非元素节点
            if (reader.NodeType != XmlNodeType.Element)
            {
                if (!reader.Read())
                {
                    break;
                }

                continue;
            }

            // 遇到 entry 元素时停止解析频道信息
            if (reader.LocalName == AtomElementNames.Entry && reader.NamespaceURI == AtomConstants.Atom10Namespace)
            {
                break;
            }

            // 遇到 feed 结束标签时停止
            if (reader.Depth <= 0)
            {
                break;
            }

            var elementName = reader.LocalName;
            var namespaceUri = reader.NamespaceURI;

            if (namespaceUri != AtomConstants.Atom10Namespace)
            {
                reader.Skip();
                continue;
            }

            try
            {
                switch (elementName)
                {
                    case AtomElementNames.Id:
                        id = reader.ReadElementContentAsString();
                        break;

                    case AtomElementNames.Title:
                        title = reader.ReadElementContentAsString();
                        break;

                    case AtomElementNames.Subtitle:
                        description = reader.ReadElementContentAsString();
                        break;

                    case AtomElementNames.Updated:
                        if (DateTimeHelper.TryParseDate(reader.ReadElementContentAsString(), out var updated))
                        {
                            updatedAt = updated;
                        }

                        break;

                    case AtomElementNames.Rights:
                        copyright = reader.ReadElementContentAsString();
                        break;

                    case AtomElementNames.Generator:
                        generator = reader.ReadElementContentAsString();
                        break;

                    case AtomElementNames.Link:
                        var linkContent = ReadContentElement(reader);
                        var link = ParseLink(linkContent);
                        links.Add(link);
                        break;

                    case AtomElementNames.Author:
                    case AtomElementNames.Contributor:
                        var personContent = ReadContentElement(reader);
                        var person = ParsePerson(personContent);
                        contributors.Add(person);
                        break;

                    case AtomElementNames.Category:
                        var catContent = ReadContentElement(reader);
                        var category = ParseCategory(catContent);
                        categories.Add(category);
                        break;

                    case AtomElementNames.Logo:
                        var logoUrl = reader.ReadElementContentAsString();
                        if (UriHelper.TryParse(logoUrl, out var logoUri) && logoUri != null)
                        {
                            images.Add(new FeedImage(logoUri, FeedImageType.Logo));
                        }

                        break;

                    case AtomElementNames.Icon:
                        var iconUrl = reader.ReadElementContentAsString();
                        if (UriHelper.TryParse(iconUrl, out var iconUri) && iconUri != null)
                        {
                            images.Add(new FeedImage(iconUri, FeedImageType.Icon));
                        }

                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }
            catch
            {
                // 忽略单个元素解析错误
                try
                {
                    reader.Skip();
                }
                catch
                {
                    reader.Read();
                }
            }
        }

        // 从链接中提取分页信息
        var pagingLinks = ExtractPagingLinks(links);

        return new FeedChannel
        {
            Id = id,
            Title = title,
            Description = description,
            Copyright = copyright,
            Generator = generator,
            LastBuildDate = updatedAt,
            FeedType = FeedType.Atom,
            Links = links,
            Images = images,
            Contributors = contributors,
            Categories = categories,
            PagingLinks = pagingLinks,
        };
    }

    /// <inheritdoc/>
    public FeedItem ParseItem(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        string? id = null;
        string title = string.Empty;
        string? summary = null;
        string? htmlContent = null;
        DateTimeOffset? publishedAt = null;
        DateTimeOffset? updatedAt = null;

        var links = new List<FeedLink>();
        var contributors = new List<FeedPerson>();
        var categories = new List<FeedCategory>();

        foreach (var field in content.Children ?? [])
        {
            if (field.Namespace != AtomConstants.Atom10Namespace)
            {
                continue;
            }

            try
            {
                switch (field.Name)
                {
                    case AtomElementNames.Id:
                        id = field.Value;
                        break;

                    case AtomElementNames.Title:
                        title = field.Value ?? string.Empty;
                        break;

                    case AtomElementNames.Summary:
                        summary = field.Value;
                        break;

                    case AtomElementNames.Content:
                        htmlContent = GetContentValue(field);
                        break;

                    case AtomElementNames.Published:
                        if (DateTimeHelper.TryParseDate(field.Value, out var published))
                        {
                            publishedAt = published;
                        }

                        break;

                    case AtomElementNames.Updated:
                        if (DateTimeHelper.TryParseDate(field.Value, out var updated))
                        {
                            updatedAt = updated;
                        }

                        break;

                    case AtomElementNames.Link:
                        var link = ParseLink(field);
                        links.Add(link);
                        break;

                    case AtomElementNames.Author:
                    case AtomElementNames.Contributor:
                        var person = ParsePerson(field);
                        contributors.Add(person);
                        break;

                    case AtomElementNames.Category:
                        var category = ParseCategory(field);
                        categories.Add(category);
                        break;
                }
            }
            catch
            {
                // 忽略单个字段解析错误
            }
        }

        return new FeedItem
        {
            Id = id,
            Title = title,
            Description = summary,
            Content = htmlContent,
            PublishedAt = publishedAt,
            UpdatedAt = updatedAt,
            Links = links,
            Contributors = contributors,
            Categories = categories,
        };
    }

    /// <inheritdoc/>
    public FeedLink ParseLink(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        // 获取 href
        var href = content.GetAttributeValue(AtomElementNames.Href);

        // 尝试获取 src（用于某些 content 元素）
        if (string.IsNullOrEmpty(href))
        {
            href = content.GetAttributeValue(AtomElementNames.Src);
        }

        if (!UriHelper.TryParse(href, out var uri) || uri == null)
        {
            throw new FeedParseException("无效的链接 href") { ElementName = content.Name };
        }

        // 获取 rel
        var rel = content.GetAttributeValue(AtomElementNames.Rel) ?? AtomElementNames.RelAlternate;

        var linkType = rel switch
        {
            AtomElementNames.RelAlternate => FeedLinkType.Alternate,
            AtomElementNames.RelSelf => FeedLinkType.Self,
            AtomElementNames.RelEnclosure => FeedLinkType.Enclosure,
            AtomElementNames.RelRelated => FeedLinkType.Related,
            AtomElementNames.RelVia => FeedLinkType.Source,
            // RFC 5005 分页链接
            AtomElementNames.RelFirst => FeedLinkType.First,
            AtomElementNames.RelPrevious or AtomElementNames.RelPrev => FeedLinkType.Previous,
            AtomElementNames.RelNext => FeedLinkType.Next,
            AtomElementNames.RelLast => FeedLinkType.Last,
            AtomElementNames.RelCurrentArchive => FeedLinkType.CurrentArchive,
            AtomElementNames.RelPreviousArchive => FeedLinkType.PreviousArchive,
            AtomElementNames.RelNextArchive => FeedLinkType.NextArchive,
            _ => FeedLinkType.Other,
        };

        // 获取标题
        var title = content.GetAttributeValue(AtomElementNames.Title);

        // 获取类型
        var mediaType = content.GetAttributeValue(AtomElementNames.Type);

        // 获取长度
        long? length = null;
        if (ValueConverter.TryConvert<long>(content.GetAttributeValue(AtomElementNames.Length), out var len))
        {
            length = len;
        }

        return new FeedLink(uri, linkType, title, mediaType, length);
    }

    /// <inheritdoc/>
    public FeedPerson ParsePerson(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        string? name = null;
        string? email = null;
        string? uri = null;

        foreach (var field in content.Children ?? [])
        {
            if (field.Namespace != AtomConstants.Atom10Namespace)
            {
                continue;
            }

            switch (field.Name)
            {
                case AtomElementNames.Name:
                    name = field.Value;
                    break;

                case AtomElementNames.Email:
                    email = field.Value;
                    break;

                case AtomElementNames.Uri:
                    uri = field.Value;
                    break;
            }
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new FeedParseException("Atom 人员必须包含 name 元素") { ElementName = content.Name };
        }

        var personType = content.Name switch
        {
            AtomElementNames.Contributor => FeedPersonType.Contributor,
            _ => FeedPersonType.Author,
        };

        return new FeedPerson(name, personType, email, uri);
    }

    /// <inheritdoc/>
    public FeedCategory ParseCategory(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var term = content.GetAttributeValue(AtomElementNames.Term);

        if (string.IsNullOrEmpty(term))
        {
            throw new FeedParseException("Atom 分类必须包含 term 属性") { ElementName = content.Name };
        }

        var scheme = content.GetAttributeValue(AtomElementNames.Scheme);
        var label = content.GetAttributeValue(AtomElementNames.Label);

        return new FeedCategory(term, label, scheme);
    }

    /// <inheritdoc/>
    public FeedImage ParseImage(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!UriHelper.TryParse(content.Value, out var uri) || uri == null)
        {
            throw new FeedParseException("无效的图片 URL") { ElementName = content.Name };
        }

        var imageType = content.Name switch
        {
            AtomElementNames.Icon => FeedImageType.Icon,
            AtomElementNames.Logo => FeedImageType.Logo,
            _ => FeedImageType.Logo,
        };

        return new FeedImage(uri, imageType);
    }

    /// <inheritdoc/>
    public FeedContent ParseContent(string xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            throw new ArgumentNullException(nameof(xml));
        }

        using var reader = _xmlReaderFactory.CreateReader(xml, async: false);
        reader.MoveToContent();

        return ReadContentElement(reader);
    }

    /// <inheritdoc/>
    public bool TryParseValue<T>(string? value, out T? result)
        => ValueConverter.TryConvert(value, out result);

    private static string? GetContentValue(FeedContent content)
    {
        var type = content.GetAttributeValue(AtomElementNames.Type);

        // 如果是 XHTML，需要特殊处理
        if (type?.Equals("xhtml", StringComparison.OrdinalIgnoreCase) == true)
        {
            // 查找 div 子元素
            var div = content.Children?.FirstOrDefault(c => c.Name == "div");
            if (div != null)
            {
                return div.Value;
            }
        }

        // 检查是否有 src 属性（外部内容）
        var src = content.GetAttributeValue(AtomElementNames.Src);
        if (!string.IsNullOrEmpty(src))
        {
            return null;
        }

        return content.Value;
    }

    private static FeedContent ReadContentElement(XmlReader reader)
    {
        var name = reader.LocalName;
        var ns = reader.NamespaceURI;
        string? value = null;

        var attributes = new List<FeedAttribute>();
        var children = new List<FeedContent>();

        // 读取属性
        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                // 跳过 xmlns 声明
                if (reader.Prefix == "xmlns" || reader.Name == "xmlns")
                {
                    continue;
                }

                attributes.Add(new FeedAttribute(reader.LocalName, reader.Value, reader.NamespaceURI));
            }

            reader.MoveToContent();
        }

        // 读取内容
        if (!reader.IsEmptyElement)
        {
            reader.ReadStartElement();

            // 跳到内容节点
            reader.MoveToContent();

            if (reader.HasValue)
            {
                value = reader.ReadContentAsString();
            }
            else
            {
                while (reader.IsStartElement())
                {
                    children.Add(ReadContentElement(reader));
                    // 处理完一个子元素后，跳过空白到下一个内容节点
                    reader.MoveToContent();
                }
            }

            if (reader.NodeType == XmlNodeType.EndElement)
            {
                reader.ReadEndElement();
            }
        }
        else
        {
            reader.Skip();
        }

        return new FeedContent(
            name,
            string.IsNullOrEmpty(ns) ? null : ns,
            value,
            attributes.Count > 0 ? attributes : null,
            children.Count > 0 ? children : null);
    }

    /// <summary>
    /// 从链接列表中提取分页信息.
    /// </summary>
    /// <param name="links">链接列表.</param>
    /// <returns>分页链接，若没有分页信息则返回 null.</returns>
    private static FeedPagingLinks? ExtractPagingLinks(List<FeedLink> links)
    {
        Uri? first = null;
        Uri? previous = null;
        Uri? next = null;
        Uri? last = null;
        Uri? current = null;

        foreach (var link in links)
        {
            switch (link.LinkType)
            {
                case FeedLinkType.First:
                    first = link.Uri;
                    break;
                case FeedLinkType.Previous:
                    previous = link.Uri;
                    break;
                case FeedLinkType.Next:
                    next = link.Uri;
                    break;
                case FeedLinkType.Last:
                    last = link.Uri;
                    break;
                case FeedLinkType.Self:
                    current = link.Uri;
                    break;
            }
        }

        // 如果没有任何分页链接，返回 null
        if (first == null && previous == null && next == null && last == null)
        {
            return null;
        }

        return new FeedPagingLinks
        {
            First = first,
            Previous = previous,
            Next = next,
            Last = last,
            Current = current,
        };
    }

    /// <summary>
    /// 确保 XmlReader 定位到 feed 元素内部.
    /// </summary>
    /// <param name="reader">XML 读取器.</param>
    /// <returns>如果需要调用 Read() 进入 feed 内部则返回 true，否则返回 false.</returns>
    private static bool EnsurePositionedAtFeed(XmlReader reader)
    {
        // 如果已经在 feed 内部的某个元素上，无需移动
        if (reader.NodeType == XmlNodeType.Element && reader.Depth > 0)
        {
            // 检查是否已经在 feed 内部（不是 feed 本身）
            if (reader.LocalName != AtomElementNames.Feed)
            {
                return false; // 已经在 feed 内部，不需要再调用 Read()
            }
        }

        // 如果 reader 还在初始状态或在文档声明处，需要移动到 feed 元素内部
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                // 到达 feed 元素，停止（下次 Read 会进入 feed 内部）
                if (reader.LocalName == AtomElementNames.Feed && reader.NamespaceURI == AtomConstants.Atom10Namespace)
                {
                    return true; // 需要调用 Read() 进入 feed 内部
                }
            }
        }

        return false;
    }
}
