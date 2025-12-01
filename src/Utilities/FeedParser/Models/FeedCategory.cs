// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.FeedParser;

/// <summary>
/// Feed 分类.
/// </summary>
/// <param name="Name">分类名称.</param>
/// <param name="Label">显示标签（可选）.</param>
/// <param name="Scheme">分类方案 URI（可选）.</param>
public sealed record FeedCategory(
    string Name,
    string? Label = null,
    string? Scheme = null);
