// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// 表示 Mobi 文件中的图片资源。
/// </summary>
public sealed class MobiImage
{
    /// <summary>
    /// 获取或设置图片在记录中的索引（从 1 开始）。
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 获取或设置图片的媒体类型。
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置图片的大小（字节）。
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// 获取一个值，指示此资源是否为有效图片。
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(MediaType) && Size > 0;

    /// <inheritdoc/>
    public override string ToString() => $"Image {Index} ({MediaType}, {Size} bytes)";
}
