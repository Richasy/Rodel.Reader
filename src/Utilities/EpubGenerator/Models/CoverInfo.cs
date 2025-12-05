// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 封面信息.
/// </summary>
public sealed class CoverInfo
{
    /// <summary>
    /// 封面图片数据.
    /// </summary>
    public required ReadOnlyMemory<byte> ImageData { get; init; }

    /// <summary>
    /// 图片媒体类型 (image/jpeg, image/png, image/gif, image/svg+xml).
    /// </summary>
    public required string MediaType { get; init; }

    /// <summary>
    /// 获取图片文件名（不含路径）.
    /// </summary>
    public string FileName => MediaType switch
    {
        "image/jpeg" => "cover.jpg",
        "image/png" => "cover.png",
        "image/gif" => "cover.gif",
        "image/svg+xml" => "cover.svg",
        _ => "cover.jpg",
    };

    /// <summary>
    /// 从字节数组创建封面信息.
    /// </summary>
    public static CoverInfo FromBytes(byte[] imageData, string mediaType)
        => new() { ImageData = imageData, MediaType = mediaType };

    /// <summary>
    /// 从 ReadOnlyMemory 创建封面信息.
    /// </summary>
    public static CoverInfo FromMemory(ReadOnlyMemory<byte> imageData, string mediaType)
        => new() { ImageData = imageData, MediaType = mediaType };
}
