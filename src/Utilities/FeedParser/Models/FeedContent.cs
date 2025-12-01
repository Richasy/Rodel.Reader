// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 原始内容.
/// </summary>
/// <remarks>
/// 用于表示解析过程中的 XML 元素内容，保留原始结构.
/// </remarks>
/// <param name="Name">元素名称.</param>
/// <param name="Namespace">元素命名空间（可选）.</param>
/// <param name="Value">元素文本值（可选）.</param>
/// <param name="Attributes">元素属性列表.</param>
/// <param name="Children">子元素列表.</param>
public sealed record FeedContent(
    string Name,
    string? Namespace = null,
    string? Value = null,
    IReadOnlyList<FeedAttribute>? Attributes = null,
    IReadOnlyList<FeedContent>? Children = null)
{
    /// <summary>
    /// 获取指定名称的属性值.
    /// </summary>
    /// <param name="name">属性名称.</param>
    /// <returns>属性值，若不存在则返回 null.</returns>
    public string? GetAttributeValue(string name)
        => Attributes?.FirstOrDefault(a => a.Name == name)?.Value;

    /// <summary>
    /// 获取指定名称的子元素.
    /// </summary>
    /// <param name="name">子元素名称.</param>
    /// <returns>子元素，若不存在则返回 null.</returns>
    public FeedContent? GetChild(string name)
        => Children?.FirstOrDefault(c => c.Name == name);

    /// <summary>
    /// 获取指定名称和命名空间的子元素.
    /// </summary>
    /// <param name="name">子元素名称.</param>
    /// <param name="ns">命名空间.</param>
    /// <returns>子元素，若不存在则返回 null.</returns>
    public FeedContent? GetChild(string name, string ns)
        => Children?.FirstOrDefault(c => c.Name == name && c.Namespace == ns);
}
