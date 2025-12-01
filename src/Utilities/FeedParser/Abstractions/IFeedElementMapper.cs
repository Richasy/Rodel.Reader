// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 元素类型映射器接口.
/// </summary>
/// <remarks>
/// 负责将 XML 元素名称映射到 Feed 元素类型.
/// </remarks>
public interface IFeedElementMapper
{
    /// <summary>
    /// 将元素名称映射到元素类型.
    /// </summary>
    /// <param name="elementName">元素名称.</param>
    /// <param name="namespaceUri">命名空间 URI.</param>
    /// <returns>元素类型.</returns>
    FeedElementType MapElementType(string elementName, string? namespaceUri);

    /// <summary>
    /// 检查是否为订阅项元素.
    /// </summary>
    /// <param name="elementName">元素名称.</param>
    /// <param name="namespaceUri">命名空间 URI.</param>
    /// <returns>是否为订阅项元素.</returns>
    bool IsItemElement(string elementName, string? namespaceUri);

    /// <summary>
    /// 检查是否为频道/Feed 根元素.
    /// </summary>
    /// <param name="elementName">元素名称.</param>
    /// <param name="namespaceUri">命名空间 URI.</param>
    /// <returns>是否为根元素.</returns>
    bool IsRootElement(string elementName, string? namespaceUri);
}
