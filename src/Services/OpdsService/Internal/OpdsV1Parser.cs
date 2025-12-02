// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Internal;

/// <summary>
/// OPDS v1.x 解析器实现.
/// </summary>
internal sealed class OpdsV1Parser : IOpdsParser
{
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="OpdsV1Parser"/> 类的新实例.
    /// </summary>
    /// <param name="logger">日志器.</param>
    public OpdsV1Parser(ILogger logger)
    {
        _logger = Guard.NotNull(logger);
    }

    /// <inheritdoc/>
    public OpdsFeed ParseFeed(Stream stream, Uri baseUri)
    {
        Guard.NotNull(stream);
        Guard.NotNull(baseUri);

        _logger.LogDebug("Parsing OPDS feed from stream with base URI {BaseUri}", baseUri);

        var settings = new XmlReaderSettings
        {
            Async = false,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            DtdProcessing = DtdProcessing.Ignore,
        };

        using var reader = XmlReader.Create(stream, settings);
        return ParseFeedInternal(reader, baseUri);
    }

    /// <inheritdoc/>
    public async Task<OpdsFeed> ParseFeedAsync(Stream stream, Uri baseUri, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(stream);
        Guard.NotNull(baseUri);

        _logger.LogDebug("Parsing OPDS feed asynchronously from stream with base URI {BaseUri}", baseUri);

        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            DtdProcessing = DtdProcessing.Ignore,
        };

        using var reader = XmlReader.Create(stream, settings);
        return await ParseFeedInternalAsync(reader, baseUri, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public string? ParseOpenSearchDescription(Stream stream)
    {
        Guard.NotNull(stream);

        _logger.LogDebug("Parsing OpenSearch description");

        var settings = new XmlReaderSettings
        {
            Async = false,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            DtdProcessing = DtdProcessing.Ignore,
        };

        using var reader = XmlReader.Create(stream, settings);
        return ParseOpenSearchDescriptionInternal(reader);
    }

    /// <inheritdoc/>
    public async Task<string?> ParseOpenSearchDescriptionAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(stream);

        _logger.LogDebug("Parsing OpenSearch description asynchronously");

        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            DtdProcessing = DtdProcessing.Ignore,
        };

        using var reader = XmlReader.Create(stream, settings);
        return await ParseOpenSearchDescriptionInternalAsync(reader, cancellationToken).ConfigureAwait(false);
    }

    private OpdsFeed ParseFeedInternal(XmlReader reader, Uri baseUri)
    {
        string? id = null;
        var title = string.Empty;
        string? subtitle = null;
        DateTimeOffset? updatedAt = null;
        Uri? icon = null;

        var entries = new List<OpdsEntry>();
        var links = new List<OpdsLink>();
        var facets = new List<OpdsFacet>();

        // 移动到 feed 元素
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == OpdsElementNames.Feed)
            {
                break;
            }
        }

        if (reader.EOF)
        {
            _logger.LogWarning("No feed element found in OPDS document");
            return new OpdsFeed { Title = title };
        }

        // 读取 feed 内容 - 使用 ReadToFollowing 和手动控制避免跳过元素
        var feedDepth = reader.Depth;
        while (reader.Read())
        {
            // 如果读到了 feed 的结束标签，停止
            if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == OpdsElementNames.Feed)
            {
                break;
            }

            // 只处理直接子元素 (深度为 feedDepth + 1)
            if (reader.NodeType != XmlNodeType.Element || reader.Depth != feedDepth + 1)
            {
                continue;
            }

            var elementName = reader.LocalName;
            var namespaceUri = reader.NamespaceURI;

            try
            {
                if (namespaceUri == OpdsConstants.AtomNamespace || string.IsNullOrEmpty(namespaceUri))
                {
                    switch (elementName)
                    {
                        case OpdsElementNames.Id:
                            id = ReadElementText(reader);
                            break;

                        case OpdsElementNames.Title:
                            title = ReadElementText(reader);
                            break;

                        case OpdsElementNames.Subtitle:
                            subtitle = ReadElementText(reader);
                            break;

                        case OpdsElementNames.Updated:
                            if (DateTimeHelper.TryParseDate(ReadElementText(reader), out var updated))
                            {
                                updatedAt = updated;
                            }

                            break;

                        case OpdsElementNames.Icon:
                            var iconUrl = ReadElementText(reader);
                            icon = UriHelper.ResolveUri(baseUri, iconUrl);
                            break;

                        case OpdsElementNames.Link:
                            var link = ParseLink(reader, baseUri);
                            if (link != null)
                            {
                                // 检查是否是 facet 链接
                                if (link.Relation == OpdsLinkRelation.Facet)
                                {
                                    var facet = ConvertLinkToFacet(link, reader);
                                    if (facet != null)
                                    {
                                        facets.Add(facet);
                                    }
                                }
                                else
                                {
                                    links.Add(link);
                                }
                            }

                            break;

                        case OpdsElementNames.Entry:
                            var entry = ParseEntry(reader, baseUri);
                            if (entry != null)
                            {
                                entries.Add(entry);
                            }

                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing element {ElementName}", elementName);
                try
                {
                    reader.Skip();
                }
                catch
                {
                    // 忽略
                }
            }
        }

        var facetGroups = GroupFacets(facets);

        _logger.LogDebug("Parsed OPDS feed with {EntryCount} entries and {LinkCount} links", entries.Count, links.Count);

        return new OpdsFeed
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            UpdatedAt = updatedAt,
            Icon = icon,
            Entries = entries,
            Links = links,
            FacetGroups = facetGroups,
        };
    }

    /// <summary>
    /// 安全地读取元素文本内容，不会使 reader 跳过下一个元素.
    /// </summary>
    private static string ReadElementText(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    sb.Append(reader.Value);
                    break;
                case XmlNodeType.EndElement:
                    return sb.ToString();
            }
        }

        return sb.ToString();
    }

    private async Task<OpdsFeed> ParseFeedInternalAsync(XmlReader reader, Uri baseUri, CancellationToken cancellationToken)
    {
        string? id = null;
        var title = string.Empty;
        string? subtitle = null;
        DateTimeOffset? updatedAt = null;
        Uri? icon = null;

        var entries = new List<OpdsEntry>();
        var links = new List<OpdsLink>();
        var facets = new List<OpdsFacet>();

        // 移动到 feed 元素
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == OpdsElementNames.Feed)
            {
                break;
            }
        }

        if (reader.EOF)
        {
            _logger.LogWarning("No feed element found in OPDS document");
            return new OpdsFeed { Title = title };
        }

        // 读取 feed 内容 - 使用深度检查避免跳过元素
        var feedDepth = reader.Depth;
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 如果读到了 feed 的结束标签，停止
            if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == OpdsElementNames.Feed)
            {
                break;
            }

            // 只处理直接子元素 (深度为 feedDepth + 1)
            if (reader.NodeType != XmlNodeType.Element || reader.Depth != feedDepth + 1)
            {
                continue;
            }

            var elementName = reader.LocalName;
            var namespaceUri = reader.NamespaceURI;

            try
            {
                if (namespaceUri == OpdsConstants.AtomNamespace || string.IsNullOrEmpty(namespaceUri))
                {
                    switch (elementName)
                    {
                        case OpdsElementNames.Id:
                            id = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.Title:
                            title = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.Subtitle:
                            subtitle = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.Updated:
                            if (DateTimeHelper.TryParseDate(await ReadElementTextAsync(reader).ConfigureAwait(false), out var updated))
                            {
                                updatedAt = updated;
                            }

                            break;

                        case OpdsElementNames.Icon:
                            var iconUrl = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            icon = UriHelper.ResolveUri(baseUri, iconUrl);
                            break;

                        case OpdsElementNames.Link:
                            var link = ParseLink(reader, baseUri);
                            if (link != null)
                            {
                                if (link.Relation == OpdsLinkRelation.Facet)
                                {
                                    var facet = ConvertLinkToFacet(link, reader);
                                    if (facet != null)
                                    {
                                        facets.Add(facet);
                                    }
                                }
                                else
                                {
                                    links.Add(link);
                                }
                            }

                            break;

                        case OpdsElementNames.Entry:
                            var entry = await ParseEntryAsync(reader, baseUri, cancellationToken).ConfigureAwait(false);
                            if (entry != null)
                            {
                                entries.Add(entry);
                            }

                            break;

                        default:
                            await reader.SkipAsync().ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    await reader.SkipAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Error parsing element {ElementName}", elementName);
                try
                {
                    await reader.SkipAsync().ConfigureAwait(false);
                }
                catch
                {
                    // 忽略
                }
            }
        }

        var facetGroups = GroupFacets(facets);

        _logger.LogDebug("Parsed OPDS feed with {EntryCount} entries and {LinkCount} links", entries.Count, links.Count);

        return new OpdsFeed
        {
            Id = id,
            Title = title,
            Subtitle = subtitle,
            UpdatedAt = updatedAt,
            Icon = icon,
            Entries = entries,
            Links = links,
            FacetGroups = facetGroups,
        };
    }

    /// <summary>
    /// 异步安全地读取元素文本内容，不会使 reader 跳过下一个元素.
    /// </summary>
    private static async Task<string> ReadElementTextAsync(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    sb.Append(reader.Value);
                    break;
                case XmlNodeType.EndElement:
                    return sb.ToString();
            }
        }

        return sb.ToString();
    }

    private OpdsEntry? ParseEntry(XmlReader reader, Uri baseUri)
    {
        if (reader.IsEmptyElement)
        {
            return null;
        }

        string? id = null;
        var title = string.Empty;
        string? summary = null;
        string? content = null;
        DateTimeOffset? updatedAt = null;
        DateTimeOffset? publishedAt = null;
        string? language = null;
        string? publisher = null;
        string? identifier = null;

        var authors = new List<OpdsAuthor>();
        var categories = new List<OpdsCategory>();
        var links = new List<OpdsLink>();
        var images = new List<OpdsImage>();
        var acquisitions = new List<OpdsAcquisition>();

        var entryDepth = reader.Depth;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == entryDepth)
            {
                break;
            }

            // 只处理直接子元素
            if (reader.NodeType != XmlNodeType.Element || reader.Depth != entryDepth + 1)
            {
                continue;
            }

            var elementName = reader.LocalName;
            var namespaceUri = reader.NamespaceURI;

            try
            {
                // Atom 元素
                if (namespaceUri == OpdsConstants.AtomNamespace || string.IsNullOrEmpty(namespaceUri))
                {
                    switch (elementName)
                    {
                        case OpdsElementNames.Id:
                            id = ReadElementText(reader);
                            break;

                        case OpdsElementNames.Title:
                            title = ReadElementText(reader);
                            break;

                        case OpdsElementNames.Summary:
                            summary = ReadElementText(reader);
                            break;

                        case OpdsElementNames.Content:
                            content = ReadElementText(reader);
                            break;

                        case OpdsElementNames.Updated:
                            if (DateTimeHelper.TryParseDate(ReadElementText(reader), out var updated))
                            {
                                updatedAt = updated;
                            }

                            break;

                        case OpdsElementNames.Published:
                            if (DateTimeHelper.TryParseDate(ReadElementText(reader), out var published))
                            {
                                publishedAt = published;
                            }

                            break;

                        case OpdsElementNames.Author:
                            var author = ParseAuthor(reader);
                            if (author != null)
                            {
                                authors.Add(author);
                            }

                            break;

                        case OpdsElementNames.Category:
                            var category = ParseCategory(reader);
                            if (category != null)
                            {
                                categories.Add(category);
                            }

                            break;

                        case OpdsElementNames.Link:
                            var link = ParseLink(reader, baseUri);
                            if (link != null)
                            {
                                ProcessEntryLink(link, links, images, acquisitions, reader);
                            }

                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }
                // Dublin Core 元素
                else if (namespaceUri == OpdsConstants.DublinCoreNamespace || namespaceUri == OpdsConstants.DublinCoreTermsNamespace)
                {
                    switch (elementName)
                    {
                        case OpdsElementNames.DcLanguage:
                            language = ReadElementText(reader);
                            break;

                        case OpdsElementNames.DcPublisher:
                            publisher = ReadElementText(reader);
                            break;

                        case OpdsElementNames.DcIdentifier:
                            identifier = ReadElementText(reader);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing entry element {ElementName}", elementName);
                try
                {
                    reader.Skip();
                }
                catch
                {
                    // 忽略
                }
            }
        }

        return new OpdsEntry
        {
            Id = id,
            Title = title,
            Summary = summary,
            Content = content,
            UpdatedAt = updatedAt,
            PublishedAt = publishedAt,
            Language = language,
            Publisher = publisher,
            Identifier = identifier,
            Authors = authors,
            Categories = categories,
            Links = links,
            Images = images,
            Acquisitions = acquisitions,
        };
    }

    private async Task<OpdsEntry?> ParseEntryAsync(XmlReader reader, Uri baseUri, CancellationToken cancellationToken)
    {
        if (reader.IsEmptyElement)
        {
            return null;
        }

        string? id = null;
        var title = string.Empty;
        string? summary = null;
        string? content = null;
        DateTimeOffset? updatedAt = null;
        DateTimeOffset? publishedAt = null;
        string? language = null;
        string? publisher = null;
        string? identifier = null;

        var authors = new List<OpdsAuthor>();
        var categories = new List<OpdsCategory>();
        var links = new List<OpdsLink>();
        var images = new List<OpdsImage>();
        var acquisitions = new List<OpdsAcquisition>();

        var entryDepth = reader.Depth;

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == entryDepth)
            {
                break;
            }

            // 只处理直接子元素
            if (reader.NodeType != XmlNodeType.Element || reader.Depth != entryDepth + 1)
            {
                continue;
            }

            var elementName = reader.LocalName;
            var namespaceUri = reader.NamespaceURI;

            try
            {
                if (namespaceUri == OpdsConstants.AtomNamespace || string.IsNullOrEmpty(namespaceUri))
                {
                    switch (elementName)
                    {
                        case OpdsElementNames.Id:
                            id = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.Title:
                            title = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.Summary:
                            summary = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.Content:
                            content = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.Updated:
                            if (DateTimeHelper.TryParseDate(await ReadElementTextAsync(reader).ConfigureAwait(false), out var updated))
                            {
                                updatedAt = updated;
                            }

                            break;

                        case OpdsElementNames.Published:
                            if (DateTimeHelper.TryParseDate(await ReadElementTextAsync(reader).ConfigureAwait(false), out var published))
                            {
                                publishedAt = published;
                            }

                            break;

                        case OpdsElementNames.Author:
                            var author = ParseAuthor(reader);
                            if (author != null)
                            {
                                authors.Add(author);
                            }

                            break;

                        case OpdsElementNames.Category:
                            var category = ParseCategory(reader);
                            if (category != null)
                            {
                                categories.Add(category);
                            }

                            break;

                        case OpdsElementNames.Link:
                            var link = ParseLink(reader, baseUri);
                            if (link != null)
                            {
                                ProcessEntryLink(link, links, images, acquisitions, reader);
                            }

                            break;

                        default:
                            await reader.SkipAsync().ConfigureAwait(false);
                            break;
                    }
                }
                else if (namespaceUri == OpdsConstants.DublinCoreNamespace || namespaceUri == OpdsConstants.DublinCoreTermsNamespace)
                {
                    switch (elementName)
                    {
                        case OpdsElementNames.DcLanguage:
                            language = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.DcPublisher:
                            publisher = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        case OpdsElementNames.DcIdentifier:
                            identifier = await ReadElementTextAsync(reader).ConfigureAwait(false);
                            break;

                        default:
                            await reader.SkipAsync().ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    await reader.SkipAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Error parsing entry element {ElementName}", elementName);
                try
                {
                    await reader.SkipAsync().ConfigureAwait(false);
                }
                catch
                {
                    // 忽略
                }
            }
        }

        return new OpdsEntry
        {
            Id = id,
            Title = title,
            Summary = summary,
            Content = content,
            UpdatedAt = updatedAt,
            PublishedAt = publishedAt,
            Language = language,
            Publisher = publisher,
            Identifier = identifier,
            Authors = authors,
            Categories = categories,
            Links = links,
            Images = images,
            Acquisitions = acquisitions,
        };
    }

    private static OpdsLink? ParseLink(XmlReader reader, Uri baseUri)
    {
        var href = reader.GetAttribute(OpdsElementNames.Href);
        if (string.IsNullOrEmpty(href))
        {
            return null;
        }

        var resolvedHref = UriHelper.ResolveUri(baseUri, href);
        if (resolvedHref == null)
        {
            return null;
        }

        var rel = reader.GetAttribute(OpdsElementNames.Rel) ?? string.Empty;
        var mediaType = reader.GetAttribute(OpdsElementNames.Type);
        var title = reader.GetAttribute(OpdsElementNames.Title);
        var lengthStr = reader.GetAttribute(OpdsElementNames.Length);

        long? length = null;
        if (!string.IsNullOrEmpty(lengthStr) && long.TryParse(lengthStr, out var parsedLength))
        {
            length = parsedLength;
        }

        var relation = ParseLinkRelation(rel);

        return new OpdsLink(resolvedHref, relation, mediaType, title, length);
    }

    private static OpdsAuthor? ParseAuthor(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            return null;
        }

        string? name = null;
        Uri? uri = null;

        var authorDepth = reader.Depth;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == authorDepth)
            {
                break;
            }

            // 只处理直接子元素
            if (reader.NodeType != XmlNodeType.Element || reader.Depth != authorDepth + 1)
            {
                continue;
            }

            switch (reader.LocalName)
            {
                case OpdsElementNames.Name:
                    name = ReadElementText(reader);
                    break;

                case OpdsElementNames.Uri:
                    var uriStr = ReadElementText(reader);
                    if (UriHelper.TryParse(uriStr, out var parsedUri))
                    {
                        uri = parsedUri;
                    }

                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        return string.IsNullOrEmpty(name) ? null : new OpdsAuthor(name, uri);
    }

    private static OpdsCategory? ParseCategory(XmlReader reader)
    {
        var term = reader.GetAttribute(OpdsElementNames.Term);
        if (string.IsNullOrEmpty(term))
        {
            return null;
        }

        var label = reader.GetAttribute(OpdsElementNames.Label);
        var scheme = reader.GetAttribute(OpdsElementNames.Scheme);

        return new OpdsCategory(term, label, scheme);
    }

    private static void ProcessEntryLink(OpdsLink link, List<OpdsLink> links, List<OpdsImage> images, List<OpdsAcquisition> acquisitions, XmlReader reader)
    {
        switch (link.Relation)
        {
            case OpdsLinkRelation.Image:
            case OpdsLinkRelation.Thumbnail:
                images.Add(new OpdsImage(link.Href, link.Relation, link.MediaType));
                break;

            case OpdsLinkRelation.Acquisition:
            case OpdsLinkRelation.AcquisitionOpenAccess:
            case OpdsLinkRelation.AcquisitionBorrow:
            case OpdsLinkRelation.AcquisitionBuy:
            case OpdsLinkRelation.AcquisitionSample:
            case OpdsLinkRelation.AcquisitionSubscribe:
                var acquisition = ConvertLinkToAcquisition(link, reader);
                if (acquisition != null)
                {
                    acquisitions.Add(acquisition);
                }

                break;

            default:
                links.Add(link);
                break;
        }
    }

    private static OpdsAcquisition? ConvertLinkToAcquisition(OpdsLink link, XmlReader reader)
    {
        var type = link.Relation switch
        {
            OpdsLinkRelation.AcquisitionOpenAccess => AcquisitionType.OpenAccess,
            OpdsLinkRelation.AcquisitionBorrow => AcquisitionType.Borrow,
            OpdsLinkRelation.AcquisitionBuy => AcquisitionType.Buy,
            OpdsLinkRelation.AcquisitionSample => AcquisitionType.Sample,
            OpdsLinkRelation.AcquisitionSubscribe => AcquisitionType.Subscribe,
            _ => AcquisitionType.Generic,
        };

        var indirectMediaTypes = new List<string>();
        OpdsPrice? price = null;

        // 解析子元素（价格、间接获取）
        if (!reader.IsEmptyElement)
        {
            var linkDepth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == linkDepth)
                {
                    break;
                }

                // 只处理直接子元素
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != linkDepth + 1)
                {
                    continue;
                }

                if (reader.NamespaceURI == OpdsConstants.Opds1Namespace)
                {
                    switch (reader.LocalName)
                    {
                        case OpdsElementNames.Price:
                            price = ParsePrice(reader);
                            break;

                        case OpdsElementNames.IndirectAcquisition:
                            ParseIndirectAcquisition(reader, indirectMediaTypes);
                            break;
                    }
                }
            }
        }

        return new OpdsAcquisition
        {
            Type = type,
            Href = link.Href,
            MediaType = link.MediaType,
            Price = price,
            IndirectMediaTypes = indirectMediaTypes,
        };
    }

    private static OpdsPrice? ParsePrice(XmlReader reader)
    {
        var currencyCode = reader.GetAttribute("currencycode") ?? "USD";
        var valueStr = ReadElementText(reader);

        if (decimal.TryParse(valueStr, out var value))
        {
            return new OpdsPrice(value, currencyCode);
        }

        return null;
    }

    private static void ParseIndirectAcquisition(XmlReader reader, List<string> mediaTypes)
    {
        var mediaType = reader.GetAttribute(OpdsElementNames.Type);
        if (!string.IsNullOrEmpty(mediaType))
        {
            mediaTypes.Add(mediaType);
        }

        // 递归解析嵌套的 indirectAcquisition
        if (!reader.IsEmptyElement)
        {
            var depth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth)
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == OpdsElementNames.IndirectAcquisition &&
                    reader.NamespaceURI == OpdsConstants.Opds1Namespace)
                {
                    ParseIndirectAcquisition(reader, mediaTypes);
                }
            }
        }
    }

    private static OpdsFacet? ConvertLinkToFacet(OpdsLink link, XmlReader reader)
    {
        var facetGroup = reader.GetAttribute(OpdsElementNames.FacetGroup, OpdsConstants.Opds1Namespace);
        var activeStr = reader.GetAttribute(OpdsElementNames.ActiveFacet, OpdsConstants.Opds1Namespace);
        var countStr = reader.GetAttribute(OpdsElementNames.Count, OpdsConstants.ThreadingNamespace);

        var isActive = string.Equals(activeStr, "true", StringComparison.OrdinalIgnoreCase);
        int? count = null;
        if (!string.IsNullOrEmpty(countStr) && int.TryParse(countStr, out var parsedCount))
        {
            count = parsedCount;
        }

        return new OpdsFacet(link.Href, link.Title ?? string.Empty, facetGroup, count, isActive);
    }

    private static List<OpdsFacetGroup> GroupFacets(List<OpdsFacet> facets)
    {
        if (facets.Count == 0)
        {
            return [];
        }

        return facets
            .GroupBy(f => f.FacetGroup ?? string.Empty)
            .Select(g => new OpdsFacetGroup(g.Key, g.ToList()))
            .ToList();
    }

    private static OpdsLinkRelation ParseLinkRelation(string rel)
    {
        return rel switch
        {
            OpdsLinkRelations.Alternate => OpdsLinkRelation.Alternate,
            OpdsLinkRelations.Self => OpdsLinkRelation.Self,
            OpdsLinkRelations.Start => OpdsLinkRelation.Start,
            OpdsLinkRelations.Subsection => OpdsLinkRelation.Subsection,
            OpdsLinkRelations.Related => OpdsLinkRelation.Related,
            OpdsLinkRelations.Search => OpdsLinkRelation.Search,
            OpdsLinkRelations.Facet => OpdsLinkRelation.Facet,
            OpdsLinkRelations.Image => OpdsLinkRelation.Image,
            OpdsLinkRelations.Thumbnail => OpdsLinkRelation.Thumbnail,
            OpdsLinkRelations.Acquisition => OpdsLinkRelation.Acquisition,
            OpdsLinkRelations.AcquisitionOpenAccess => OpdsLinkRelation.AcquisitionOpenAccess,
            OpdsLinkRelations.AcquisitionBorrow => OpdsLinkRelation.AcquisitionBorrow,
            OpdsLinkRelations.AcquisitionBuy => OpdsLinkRelation.AcquisitionBuy,
            OpdsLinkRelations.AcquisitionSample => OpdsLinkRelation.AcquisitionSample,
            OpdsLinkRelations.AcquisitionSubscribe => OpdsLinkRelation.AcquisitionSubscribe,
            OpdsLinkRelations.Next => OpdsLinkRelation.Next,
            OpdsLinkRelations.Previous or OpdsLinkRelations.Prev => OpdsLinkRelation.Previous,
            OpdsLinkRelations.First => OpdsLinkRelation.First,
            OpdsLinkRelations.Last => OpdsLinkRelation.Last,
            OpdsLinkRelations.Crawlable => OpdsLinkRelation.Crawlable,
            OpdsLinkRelations.Popular => OpdsLinkRelation.Popular,
            OpdsLinkRelations.Featured => OpdsLinkRelation.Featured,
            OpdsLinkRelations.New => OpdsLinkRelation.New,
            OpdsLinkRelations.Shelf => OpdsLinkRelation.Shelf,
            _ => OpdsLinkRelation.Other,
        };
    }

    private static string? ParseOpenSearchDescriptionInternal(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == OpdsElementNames.Url)
            {
                var type = reader.GetAttribute(OpdsElementNames.Type);
                if (type?.Contains("application/atom+xml", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return reader.GetAttribute(OpdsElementNames.Template);
                }
            }
        }

        return null;
    }

    private static async Task<string?> ParseOpenSearchDescriptionInternalAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == OpdsElementNames.Url)
            {
                var type = reader.GetAttribute(OpdsElementNames.Type);
                if (type?.Contains("application/atom+xml", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return reader.GetAttribute(OpdsElementNames.Template);
                }
            }
        }

        return null;
    }
}
