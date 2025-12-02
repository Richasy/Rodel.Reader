// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 图片信息.
/// </summary>
/// <param name="Href">图片 URI.</param>
/// <param name="Relation">链接关系（Image 或 Thumbnail）.</param>
/// <param name="MediaType">媒体类型.</param>
public sealed record OpdsImage(
    Uri Href,
    OpdsLinkRelation Relation,
    string? MediaType = null);
