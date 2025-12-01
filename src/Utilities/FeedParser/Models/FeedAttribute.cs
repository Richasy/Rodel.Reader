// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 属性.
/// </summary>
/// <remarks>
/// 表示 XML 元素上的属性.
/// </remarks>
/// <param name="Name">属性名称.</param>
/// <param name="Value">属性值.</param>
/// <param name="Namespace">属性的命名空间（可选）.</param>
public sealed record FeedAttribute(
    string Name,
    string Value,
    string? Namespace = null);
