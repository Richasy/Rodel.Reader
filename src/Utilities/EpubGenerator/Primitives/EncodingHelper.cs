// Copyright (c) Reader Copilot. All rights reserved.

using System.Runtime.CompilerServices;
using System.Text;

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 编码检测辅助类.
/// </summary>
internal static class EncodingHelper
{
    /// <summary>
    /// 检测流的编码.
    /// </summary>
    /// <param name="stream">输入流（会被重置到起始位置）.</param>
    /// <param name="defaultEncoding">默认编码.</param>
    /// <returns>检测到的编码.</returns>
    public static Encoding DetectEncoding(Stream stream, Encoding? defaultEncoding = null)
    {
        defaultEncoding ??= Encoding.UTF8;

        if (!stream.CanSeek)
        {
            return defaultEncoding;
        }

        var originalPosition = stream.Position;
        stream.Position = 0;

        Span<byte> bom = stackalloc byte[4];
        var bytesRead = stream.Read(bom);
        stream.Position = originalPosition;

        if (bytesRead < 2)
        {
            return defaultEncoding;
        }

        return DetectEncodingFromBom(bom[..bytesRead], defaultEncoding);
    }

    /// <summary>
    /// 从 BOM 检测编码.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Encoding DetectEncodingFromBom(ReadOnlySpan<byte> bom, Encoding? defaultEncoding = null)
    {
        defaultEncoding ??= Encoding.UTF8;

        if (bom.Length >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        {
            return Encoding.UTF8;
        }

        if (bom.Length >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
        {
            if (bom.Length >= 4 && bom[2] == 0x00 && bom[3] == 0x00)
            {
                return Encoding.UTF32;
            }

            return Encoding.Unicode; // UTF-16 LE
        }

        if (bom.Length >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode; // UTF-16 BE
        }

        if (bom.Length >= 4 && bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE && bom[3] == 0xFF)
        {
            return new UTF32Encoding(bigEndian: true, byteOrderMark: true);
        }

        return defaultEncoding;
    }

    /// <summary>
    /// 从字节数组检测编码.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Encoding DetectEncoding(ReadOnlySpan<byte> data, Encoding? defaultEncoding = null)
    {
        if (data.Length < 2)
        {
            return defaultEncoding ?? Encoding.UTF8;
        }

        var bomLength = Math.Min(4, data.Length);
        return DetectEncodingFromBom(data[..bomLength], defaultEncoding);
    }
}
