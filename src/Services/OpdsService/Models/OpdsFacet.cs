// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 分面.
/// </summary>
/// <param name="Href">分面链接.</param>
/// <param name="Title">分面标题.</param>
/// <param name="FacetGroup">所属分面组.</param>
/// <param name="Count">项目数量.</param>
/// <param name="IsActive">是否已激活.</param>
public sealed record OpdsFacet(
    Uri Href,
    string Title,
    string? FacetGroup = null,
    int? Count = null,
    bool IsActive = false);
