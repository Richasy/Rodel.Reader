// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 分面组.
/// </summary>
/// <param name="Title">分面组标题.</param>
/// <param name="Facets">分面列表.</param>
public sealed record OpdsFacetGroup(
    string Title,
    IReadOnlyList<OpdsFacet> Facets);
