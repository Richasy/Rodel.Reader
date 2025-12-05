// Copyright (c) Reader Copilot. All rights reserved.

using System.Xml;
using Richasy.RodelReader.Utilities.FeedParser.Helpers;
using Richasy.RodelReader.Utilities.FeedParser.Internal;

namespace Richasy.RodelReader.Utilities.FeedParser.Parsers;

/// <summary>
/// RSS 解析器实现.
/// </summary>
public sealed class RssParser : IFeedParser
{
    private readonly IXmlReaderFactory _xmlReaderFactory;

    /// <summary>
    /// 初始化 <see cref="RssParser"/> 类的新实例.
    /// </summary>
    public RssParser()
        : this(new XmlReaderFactory())
    {
    }

    /// <summary>
    /// 初始化 <see cref="RssParser"/> 类的新实例（依赖注入）.
    /// </summary>
    /// <param name="xmlReaderFactory">XML 读取器工厂.</param>
    public RssParser(IXmlReaderFactory xmlReaderFactory)
    {
        _xmlReaderFactory = xmlReaderFactory ?? throw new ArgumentNullException(nameof(xmlReaderFactory));
    }

    /// <inheritdoc/>
    public FeedChannel ParseChannel(XmlReader reader)
    {
        var title = string.Empty;
        string? description = null;
        string? language = null;
        string? copyright = null;
        string? generator = null;
        DateTimeOffset? lastBuildDate = null;
        DateTimeOffset? publishedAt = null;

        var links = new List<FeedLink>();
        var images = new List<FeedImage>();
        var contributors = new List<FeedPerson>();
        var categories = new List<FeedCategory>();

        // 确保 reader 定位到 channel 元素内部
        var needsRead = EnsurePositionedAtChannel(reader);

        // 如果 EnsurePositionedAtChannel 返回 true，需要进入 channel 内部
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

            // 遇到 item 元素时停止解析频道信息
            if (reader.LocalName == RssElementNames.Item)
            {
                break;
            }

            // 遇到 channel 结束标签时停止
            if (reader.Depth <= 1)
            {
                break;
            }

            var elementName = reader.LocalName;
            var namespaceUri = reader.NamespaceURI;

            try
            {
                switch (elementName)
                {
                    case RssElementNames.Title when IsRssNamespace(namespaceUri):
                        title = reader.ReadElementContentAsString();
                        break;

                    case RssElementNames.Description when IsRssNamespace(namespaceUri):
                        description = reader.ReadElementContentAsString();
                        break;

                    case RssElementNames.Link when IsRssNamespace(namespaceUri):
                        var linkContent = ReadContentElement(reader);
                        var link = ParseLink(linkContent);
                        links.Add(link);
                        break;

                    case RssElementNames.Language when IsRssNamespace(namespaceUri):
                        language = reader.ReadElementContentAsString();
                        break;

                    case RssElementNames.Copyright when IsRssNamespace(namespaceUri):
                        copyright = reader.ReadElementContentAsString();
                        break;

                    case RssElementNames.Generator when IsRssNamespace(namespaceUri):
                        generator = reader.ReadElementContentAsString();
                        break;

                    case RssElementNames.LastBuildDate when IsRssNamespace(namespaceUri):
                        if (DateTimeHelper.TryParseDate(reader.ReadElementContentAsString(), out var lbd))
                        {
                            lastBuildDate = lbd;
                        }

                        break;

                    case RssElementNames.PubDate when IsRssNamespace(namespaceUri):
                        if (DateTimeHelper.TryParseDate(reader.ReadElementContentAsString(), out var pd))
                        {
                            publishedAt = pd;
                        }

                        break;

                    case RssElementNames.Image when IsRssNamespace(namespaceUri):
                        var imageContent = ReadContentElement(reader);
                        var image = ParseImage(imageContent);
                        images.Add(image);
                        break;

                    case RssElementNames.ManagingEditor when IsRssNamespace(namespaceUri):
                    case RssElementNames.Author when IsRssNamespace(namespaceUri):
                        var personContent = ReadContentElement(reader);
                        var person = ParsePerson(personContent);
                        contributors.Add(person);
                        break;

                    case RssElementNames.Category when IsRssNamespace(namespaceUri):
                        var catContent = ReadContentElement(reader);
                        var category = ParseCategory(catContent);
                        categories.Add(category);
                        break;

                    // iTunes 命名空间元素
                    case RssElementNames.ITunesImage when namespaceUri == RssConstants.ITunesNamespace:
                        var itunesImageUrl = reader.GetAttribute(RssElementNames.Href);
                        if (UriHelper.TryParse(itunesImageUrl, out var itunesUri) && itunesUri != null)
                        {
                            images.Add(new FeedImage(itunesUri, FeedImageType.Logo));
                        }

                        reader.Skip();
                        break;

                    case RssElementNames.ITunesAuthor when namespaceUri == RssConstants.ITunesNamespace:
                        var authorName = reader.ReadElementContentAsString();
                        if (!string.IsNullOrEmpty(authorName))
                        {
                            contributors.Add(new FeedPerson(authorName, FeedPersonType.Author));
                        }

                        break;

                    case RssElementNames.ITunesCategory when namespaceUri == RssConstants.ITunesNamespace:
                        var itunesCatContent = ReadContentElement(reader);
                        var itunesCat = ParseCategory(itunesCatContent);
                        categories.Add(itunesCat);
                        break;

                    // Atom 命名空间链接（支持 RFC 5005 分页）
                    case AtomElementNames.Link when namespaceUri == AtomConstants.Atom10Namespace:
                        var atomLinkContent = ReadContentElement(reader);
                        var atomLink = ParseAtomLink(atomLinkContent);
                        links.Add(atomLink);
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }
            catch
            {
                // 忽略单个元素解析错误，继续处理其他元素
                try
                {
                    reader.Skip();
                }
                catch
                {
                    // 如果 Skip 也失败，尝试读取下一个节点
                    reader.Read();
                }
            }
        }

        // 从链接中提取分页信息
        var pagingLinks = ExtractPagingLinks(links);

        return new FeedChannel
        {
            Title = title,
            Description = description,
            Language = language,
            Copyright = copyright,
            Generator = generator,
            LastBuildDate = lastBuildDate,
            PublishedAt = publishedAt,
            FeedType = FeedType.Rss,
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
        string? description = null;
        string? encodedContent = null;
        string? imageUrl = null;
        DateTimeOffset? publishedAt = null;
        DateTimeOffset? updatedAt = null;
        int? duration = null;

        var links = new List<FeedLink>();
        var contributors = new List<FeedPerson>();
        var categories = new List<FeedCategory>();

        foreach (var field in content.Children ?? [])
        {
            var fieldName = field.Name;
            var fieldNs = field.Namespace;

            try
            {
                // 处理 Atom 命名空间的 updated 元素
                if (fieldNs == AtomConstants.Atom10Namespace && fieldName == "updated")
                {
                    if (DateTimeHelper.TryParseDate(field.Value, out var updated))
                    {
                        updatedAt = updated;
                    }

                    continue;
                }

                if (!IsRssOrITunesOrContentNamespace(fieldNs))
                {
                    continue;
                }

                switch (fieldName)
                {
                    case RssElementNames.Title:
                        title = field.Value ?? string.Empty;
                        break;

                    case RssElementNames.Description:
                        description = field.Value;
                        break;

                    case RssElementNames.Encoded when fieldNs == RssConstants.ContentNamespace:
                        encodedContent = field.Value;
                        break;

                    case RssElementNames.Link:
                        var link = ParseLink(field);
                        links.Add(link);
                        break;

                    case RssElementNames.Guid:
                        id = field.Value;

                        // 检查是否为永久链接
                        var isPermaLink = field.GetAttributeValue(RssElementNames.IsPermaLink);
                        if ((string.IsNullOrEmpty(isPermaLink) || isPermaLink.Equals("true", StringComparison.OrdinalIgnoreCase))
                            && Helpers.UriHelper.TryParse(field.Value, out var guidUri) && guidUri != null)
                        {
                            links.Add(new FeedLink(guidUri, FeedLinkType.Permalink));
                        }

                        break;

                    case RssElementNames.PubDate:
                        if (Helpers.DateTimeHelper.TryParseDate(field.Value, out var pubDate))
                        {
                            publishedAt = pubDate;
                        }

                        break;

                    case RssElementNames.Author:
                        var person = ParsePerson(field);
                        contributors.Add(person);
                        break;

                    case RssElementNames.Category:
                        var category = ParseCategory(field);
                        categories.Add(category);
                        break;

                    case RssElementNames.Enclosure:
                    case RssElementNames.Comments:
                    case RssElementNames.Source:
                        var otherLink = ParseLink(field);
                        links.Add(otherLink);
                        break;

                    // image 元素（RSS 和 iTunes 命名空间都是 "image"）
                    case RssElementNames.Image:
                        imageUrl = string.IsNullOrEmpty(field.Value)
                            ? field.GetAttributeValue(RssElementNames.Href)
                            : field.Value;
                        break;

                    case "duration" when fieldNs == RssConstants.ITunesNamespace:
                        if (DateTimeHelper.TryParseDuration(field.Value, out var dur))
                        {
                            duration = dur;
                        }

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
            Description = description,
            Content = encodedContent,
            ImageUrl = imageUrl,
            PublishedAt = publishedAt,
            UpdatedAt = updatedAt,
            Duration = duration,
            Links = links,
            Contributors = contributors,
            Categories = categories,
        };
    }

    /// <inheritdoc/>
    public FeedLink ParseLink(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        // 获取 URL
        var urlAttr = content.GetAttributeValue("url");
        Uri? uri = null;

        if (!string.IsNullOrEmpty(urlAttr))
        {
            _ = Helpers.UriHelper.TryParse(urlAttr, out uri);
        }
        else if (!string.IsNullOrEmpty(content.Value))
        {
            _ = Helpers.UriHelper.TryParse(content.Value, out uri);
        }

        if (uri == null)
        {
            throw new FeedParseException("无效的链接 URL") { ElementName = content.Name };
        }

        // 获取链接类型
        var linkType = content.Name switch
        {
            RssElementNames.Link => FeedLinkType.Alternate,
            RssElementNames.Enclosure => FeedLinkType.Enclosure,
            RssElementNames.Comments => FeedLinkType.Comments,
            RssElementNames.Source => FeedLinkType.Source,
            RssElementNames.Guid => FeedLinkType.Permalink,
            _ => FeedLinkType.Other,
        };

        // 获取标题
        var title = string.IsNullOrEmpty(urlAttr) ? null : content.Value;

        // 获取长度
        long? length = null;
        if (Helpers.ValueConverter.TryConvert<long>(content.GetAttributeValue("length"), out var len))
        {
            length = len;
        }

        // 获取类型
        var mediaType = content.GetAttributeValue("type");

        return new FeedLink(uri, linkType, title, mediaType, length);
    }

    /// <inheritdoc/>
    public FeedPerson ParsePerson(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrEmpty(content.Value))
        {
            throw new FeedParseException("人员信息不能为空") { ElementName = content.Name };
        }

        var value = content.Value;
        string? email = null;
        string name = value;

        // 解析格式: email@example.com (Name) 或 Name <email@example.com>
        var parenStart = value.IndexOf('(', StringComparison.Ordinal);
        if (parenStart != -1)
        {
            var parenEnd = value.IndexOf(')', StringComparison.Ordinal);
            if (parenEnd > parenStart)
            {
                email = value[..parenStart].Trim();
                name = value[(parenStart + 1)..parenEnd].Trim();
            }
        }
        else
        {
            var angleStart = value.IndexOf('<', StringComparison.Ordinal);
            if (angleStart != -1)
            {
                var angleEnd = value.IndexOf('>', StringComparison.Ordinal);
                if (angleEnd > angleStart)
                {
                    name = value[..angleStart].Trim();
                    email = value[(angleStart + 1)..angleEnd].Trim();
                }
            }
        }

        var personType = content.Name switch
        {
            RssElementNames.ManagingEditor => FeedPersonType.Editor,
            RssElementNames.WebMaster => FeedPersonType.Webmaster,
            _ => FeedPersonType.Author,
        };

        return new FeedPerson(name, personType, email);
    }

    /// <inheritdoc/>
    public FeedCategory ParseCategory(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        // 处理 iTunes 分类
        if (content.Namespace == RssConstants.ITunesNamespace)
        {
            var text = content.GetAttributeValue("text");
            if (string.IsNullOrEmpty(text))
            {
                throw new FeedParseException("iTunes 分类需要 text 属性") { ElementName = content.Name };
            }

            return new FeedCategory(text);
        }

        if (string.IsNullOrEmpty(content.Value))
        {
            throw new FeedParseException("分类名称不能为空") { ElementName = content.Name };
        }

        var scheme = content.GetAttributeValue("domain");

        return new FeedCategory(content.Value, Scheme: scheme);
    }

    /// <inheritdoc/>
    public FeedImage ParseImage(FeedContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        string? title = null;
        string? description = null;
        Uri? url = null;
        FeedLink? link = null;
        int? width = null;
        int? height = null;

        foreach (var field in content.Children ?? [])
        {
            if (!IsRssNamespace(field.Namespace))
            {
                continue;
            }

            switch (field.Name)
            {
                case RssElementNames.Title:
                    title = field.Value;
                    break;

                case RssElementNames.Url:
                    if (UriHelper.TryParse(field.Value, out var parsedUrl))
                    {
                        url = parsedUrl;
                    }

                    break;

                case RssElementNames.Link:
                    link = ParseLink(field);
                    break;

                case RssElementNames.Description:
                    description = field.Value;
                    break;

                case RssElementNames.Width:
                    if (ValueConverter.TryConvert<int>(field.Value, out var w))
                    {
                        width = w;
                    }

                    break;

                case RssElementNames.Height:
                    if (ValueConverter.TryConvert<int>(field.Value, out var h))
                    {
                        height = h;
                    }

                    break;
            }
        }

        // 检查 href 属性（用于 iTunes 图片）
        if (url == null)
        {
            var href = content.GetAttributeValue(RssElementNames.Href);
            if (UriHelper.TryParse(href, out var hrefUrl))
            {
                url = hrefUrl;
            }
        }

        if (url == null)
        {
            throw new FeedParseException("图片 URL 不能为空") { ElementName = content.Name };
        }

        return new FeedImage(url, FeedImageType.Logo, title, description, link, width, height);
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

    private static bool IsRssNamespace(string? namespaceUri)
        => string.IsNullOrEmpty(namespaceUri); // RSS 2.0 不使用命名空间

    private static bool IsRssOrITunesOrContentNamespace(string? namespaceUri)
        => IsRssNamespace(namespaceUri)
        || string.Equals(namespaceUri, RssConstants.ITunesNamespace, StringComparison.Ordinal)
        || string.Equals(namespaceUri, RssConstants.ContentNamespace, StringComparison.Ordinal);

    /// <summary>
    /// 解析 Atom 命名空间的链接（用于 RSS Feed 中的 atom:link）.
    /// </summary>
    /// <param name="content">链接内容.</param>
    /// <returns>解析后的链接.</returns>
    private static FeedLink ParseAtomLink(FeedContent content)
    {
        var href = content.GetAttributeValue(AtomElementNames.Href);
        if (!UriHelper.TryParse(href, out var uri) || uri == null)
        {
            throw new FeedParseException("无效的 atom:link href") { ElementName = content.Name };
        }

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

        var title = content.GetAttributeValue(AtomElementNames.Title);
        var mediaType = content.GetAttributeValue(AtomElementNames.Type);

        long? length = null;
        if (ValueConverter.TryConvert<long>(content.GetAttributeValue(AtomElementNames.Length), out var len))
        {
            length = len;
        }

        return new FeedLink(uri, linkType, title, mediaType, length);
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
    /// 确保 XmlReader 定位到 channel 元素内部.
    /// </summary>
    /// <param name="reader">XML 读取器.</param>
    /// <returns>如果需要调用 Read() 进入 channel 内部则返回 true，否则返回 false.</returns>
    private static bool EnsurePositionedAtChannel(XmlReader reader)
    {
        // 如果已经在 channel 内部的某个元素上，无需移动
        if (reader.NodeType == XmlNodeType.Element && reader.Depth > 0)
        {
            // 检查是否已经在 channel 内部（不是 rss 或 channel 本身）
            var localName = reader.LocalName;
            if (localName != "rss" && localName != RssElementNames.Channel)
            {
                return false; // 已经在 channel 内部，不需要再调用 Read()
            }
        }

        // 如果 reader 还在初始状态或在文档声明处，需要移动到 channel 元素内部
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                // 跳过 rss 根元素
                if (reader.LocalName == "rss")
                {
                    continue;
                }

                // 到达 channel 元素，停止（下次 Read 会进入 channel 内部）
                if (reader.LocalName == RssElementNames.Channel)
                {
                    return true; // 需要调用 Read() 进入 channel 内部
                }
            }
        }

        return false;
    }
}
