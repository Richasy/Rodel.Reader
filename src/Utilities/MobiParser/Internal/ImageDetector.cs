// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// 图片格式检测器。
/// </summary>
internal static class ImageDetector
{
    // JPEG 魔数
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];

    // PNG 魔数
    private static readonly byte[] PngMagic = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    // GIF 魔数
    private static readonly byte[] Gif87Magic = "GIF87a"u8.ToArray();
    private static readonly byte[] Gif89Magic = "GIF89a"u8.ToArray();

    // BMP 魔数
    private static readonly byte[] BmpMagic = "BM"u8.ToArray();

    // WebP 魔数
    private static readonly byte[] WebPMagic = "WEBP"u8.ToArray();
    private static readonly byte[] RiffMagic = "RIFF"u8.ToArray();

    /// <summary>
    /// 检测图片格式并返回媒体类型。
    /// </summary>
    /// <param name="data">图片数据。</param>
    /// <returns>媒体类型，如果无法识别则返回 null。</returns>
    public static string? DetectMediaType(byte[] data)
    {
        if (data == null || data.Length < 8)
        {
            return null;
        }

        // JPEG
        if (StartsWith(data, JpegMagic))
        {
            return "image/jpeg";
        }

        // PNG
        if (StartsWith(data, PngMagic))
        {
            return "image/png";
        }

        // GIF
        if (StartsWith(data, Gif87Magic) || StartsWith(data, Gif89Magic))
        {
            return "image/gif";
        }

        // BMP
        if (StartsWith(data, BmpMagic))
        {
            return "image/bmp";
        }

        // WebP
        if (StartsWith(data, RiffMagic) && data.Length >= 12)
        {
            if (data[8] == WebPMagic[0] &&
                data[9] == WebPMagic[1] &&
                data[10] == WebPMagic[2] &&
                data[11] == WebPMagic[3])
            {
                return "image/webp";
            }
        }

        return null;
    }

    /// <summary>
    /// 检查是否是有效的图片数据。
    /// </summary>
    /// <param name="data">图片数据。</param>
    /// <returns>如果是有效图片则返回 true。</returns>
    public static bool IsImage(byte[] data)
    {
        return DetectMediaType(data) != null;
    }

    private static bool StartsWith(byte[] data, byte[] prefix)
    {
        if (data.Length < prefix.Length)
        {
            return false;
        }

        for (var i = 0; i < prefix.Length; i++)
        {
            if (data[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }
}
