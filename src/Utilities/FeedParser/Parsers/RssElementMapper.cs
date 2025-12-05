// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelReader.Utilities.FeedParser.Internal;

namespace Richasy.RodelReader.Utilities.FeedParser.Parsers;

/// <summary>
/// RSS 元素类型映射器.
/// </summary>
public sealed class RssElementMapper : IFeedElementMapper
{
    /// <inheritdoc/>
    public FeedElementType MapElementType(string elementName, string? namespaceUri)
    {
        // 检查是否为 RSS 命名空间或 iTunes 命名空间
        if (!IsRssNamespace(namespaceUri) && !IsITunesNamespace(namespaceUri))
        {
            return FeedElementType.Content;
        }

        return elementName switch
        {
            RssElementNames.Item => FeedElementType.Item,
            RssElementNames.Link => FeedElementType.Link,
            RssElementNames.Category => FeedElementType.Category,
            RssElementNames.Author or RssElementNames.ManagingEditor => FeedElementType.Person,
            RssElementNames.Image => FeedElementType.Image,
            _ => FeedElementType.Content,
        };
    }

    /// <inheritdoc/>
    public bool IsItemElement(string elementName, string? namespaceUri)
        => elementName == RssElementNames.Item && IsRssNamespace(namespaceUri);

    /// <inheritdoc/>
    public bool IsRootElement(string elementName, string? namespaceUri)
        => elementName == RssElementNames.Channel && IsRssNamespace(namespaceUri);

    private static bool IsRssNamespace(string? namespaceUri)
        => string.IsNullOrEmpty(namespaceUri); // RSS 2.0 不使用命名空间

    private static bool IsITunesNamespace(string? namespaceUri)
        => string.Equals(namespaceUri, RssConstants.ITunesNamespace, StringComparison.Ordinal);
}
