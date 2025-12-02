// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 链接.
/// </summary>
/// <param name="Href">链接地址.</param>
/// <param name="Relation">链接关系类型.</param>
/// <param name="MediaType">媒体类型.</param>
/// <param name="Title">链接标题.</param>
/// <param name="Length">内容长度（字节）.</param>
public sealed record OpdsLink(
    Uri Href,
    OpdsLinkRelation Relation,
    string? MediaType = null,
    string? Title = null,
    long? Length = null);
