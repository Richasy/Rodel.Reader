// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 作者信息.
/// </summary>
/// <param name="Name">作者名称.</param>
/// <param name="Uri">作者相关链接.</param>
public sealed record OpdsAuthor(
    string Name,
    Uri? Uri = null);
