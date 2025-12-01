// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelPlayer.Utilities.FeedParser.Internal;

namespace Richasy.RodelPlayer.Utilities.FeedParser.Parsers;

/// <summary>
/// Atom 元素类型映射器.
/// </summary>
public sealed class AtomElementMapper : IFeedElementMapper
{
    /// <inheritdoc/>
    public FeedElementType MapElementType(string elementName, string? namespaceUri)
    {
        if (!IsAtomNamespace(namespaceUri))
        {
            return FeedElementType.Content;
        }

        return elementName switch
        {
            AtomElementNames.Entry => FeedElementType.Item,
            AtomElementNames.Link => FeedElementType.Link,
            AtomElementNames.Category => FeedElementType.Category,
            AtomElementNames.Author or AtomElementNames.Contributor => FeedElementType.Person,
            AtomElementNames.Logo or AtomElementNames.Icon => FeedElementType.Image,
            _ => FeedElementType.Content,
        };
    }

    /// <inheritdoc/>
    public bool IsItemElement(string elementName, string? namespaceUri)
        => elementName == AtomElementNames.Entry && IsAtomNamespace(namespaceUri);

    /// <inheritdoc/>
    public bool IsRootElement(string elementName, string? namespaceUri)
        => elementName == AtomElementNames.Feed && IsAtomNamespace(namespaceUri);

    private static bool IsAtomNamespace(string? namespaceUri)
        => namespaceUri == AtomConstants.Atom10Namespace;
}
