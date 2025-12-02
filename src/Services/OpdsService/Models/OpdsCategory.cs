// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 分类信息.
/// </summary>
/// <param name="Term">分类标识.</param>
/// <param name="Label">分类显示名称.</param>
/// <param name="Scheme">分类方案 URI.</param>
public sealed record OpdsCategory(
    string Term,
    string? Label = null,
    string? Scheme = null);
