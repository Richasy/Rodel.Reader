// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 章节内图片信息.
/// </summary>
public sealed class ChapterImageInfo
{
    /// <summary>
    /// 图片唯一标识符.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 图片在文本中的插入位置（字符偏移量）.
    /// </summary>
    /// <remarks>
    /// 偏移量指的是图片应插入在第几个字符之后。
    /// 例如：Offset = 0 表示插入在文本开头，Offset = 100 表示插入在第100个字符之后.
    /// </remarks>
    public required int Offset { get; init; }

    /// <summary>
    /// 图片数据.
    /// </summary>
    public required ReadOnlyMemory<byte> ImageData { get; init; }

    /// <summary>
    /// 图片媒体类型 (image/jpeg, image/png, image/gif, image/svg+xml).
    /// </summary>
    public required string MediaType { get; init; }

    /// <summary>
    /// 图片替代文本（用于无障碍访问）.
    /// </summary>
    public string? AltText { get; init; }

    /// <summary>
    /// 图片标题/说明文字（可选）.
    /// </summary>
    public string? Caption { get; init; }

    /// <summary>
    /// 获取图片文件扩展名.
    /// </summary>
    public string Extension => MediaType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/gif" => ".gif",
        "image/svg+xml" => ".svg",
        "image/webp" => ".webp",
        _ => ".jpg",
    };

    /// <summary>
    /// 获取图片文件名（不含路径）.
    /// </summary>
    public string FileName => $"{Id}{Extension}";

    /// <summary>
    /// 从字节数组创建章节图片信息.
    /// </summary>
    public static ChapterImageInfo FromBytes(string id, int offset, byte[] imageData, string mediaType, string? altText = null)
        => new() { Id = id, Offset = offset, ImageData = imageData, MediaType = mediaType, AltText = altText };

    /// <summary>
    /// 从 ReadOnlyMemory 创建章节图片信息.
    /// </summary>
    public static ChapterImageInfo FromMemory(string id, int offset, ReadOnlyMemory<byte> imageData, string mediaType, string? altText = null)
        => new() { Id = id, Offset = offset, ImageData = imageData, MediaType = mediaType, AltText = altText };
}
