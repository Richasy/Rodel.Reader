// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 获取信息（下载链接）.
/// </summary>
public sealed record OpdsAcquisition
{
    /// <summary>
    /// 获取类型（免费、购买、借阅等）.
    /// </summary>
    public AcquisitionType Type { get; init; }

    /// <summary>
    /// 下载/获取地址.
    /// </summary>
    public Uri Href { get; init; } = null!;

    /// <summary>
    /// 直接获取的媒体类型（如 application/epub+zip）.
    /// </summary>
    public string? MediaType { get; init; }

    /// <summary>
    /// 价格信息（如果需要购买）.
    /// </summary>
    public OpdsPrice? Price { get; init; }

    /// <summary>
    /// 间接获取的最终媒体类型列表.
    /// 当需要经过中间步骤（如 DRM 解包）才能获取最终文件时使用.
    /// </summary>
    public IReadOnlyList<string> IndirectMediaTypes { get; init; } = [];
}
