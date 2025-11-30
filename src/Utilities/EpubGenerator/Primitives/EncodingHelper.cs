// Copyright (c) Reader Copilot. All rights reserved.

using System.Runtime.CompilerServices;
using System.Text;

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 编码检测辅助类.
/// </summary>
internal static class EncodingHelper
{
    private static readonly Lazy<Encoding> Gb2312Encoding = new(() =>
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding("gb2312");
    });

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

        // 读取更多字节用于编码检测
        var bufferSize = (int)Math.Min(4096, stream.Length);
        var buffer = new byte[bufferSize];
        var bytesRead = stream.Read(buffer, 0, bufferSize);
        stream.Position = originalPosition;

        if (bytesRead < 2)
        {
            return defaultEncoding;
        }

        var data = buffer.AsSpan(0, bytesRead);

        // 首先检测 BOM
        var bomEncoding = DetectEncodingFromBom(data, null);
        if (bomEncoding is not null)
        {
            return bomEncoding;
        }

        // 尝试 UTF-8 无 BOM 验证
        if (IsValidUtf8(data))
        {
            return Encoding.UTF8;
        }

        // 不是有效的 UTF-8，尝试 GB2312/GBK
        return Gb2312Encoding.Value;
    }

    /// <summary>
    /// 从 BOM 检测编码.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Encoding? DetectEncodingFromBom(ReadOnlySpan<byte> bom, Encoding? defaultEncoding)
    {
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
        var bomEncoding = DetectEncodingFromBom(data[..bomLength], null);
        if (bomEncoding is not null)
        {
            return bomEncoding;
        }

        // 尝试 UTF-8 验证
        if (IsValidUtf8(data))
        {
            return Encoding.UTF8;
        }

        // 回退到 GB2312
        return Gb2312Encoding.Value;
    }

    /// <summary>
    /// 验证字节序列是否是有效的 UTF-8.
    /// </summary>
    private static bool IsValidUtf8(ReadOnlySpan<byte> data)
    {
        var i = 0;
        while (i < data.Length)
        {
            var b = data[i];

            if (b <= 0x7F)
            {
                // ASCII
                i++;
            }
            else if ((b & 0xE0) == 0xC0)
            {
                // 2-byte sequence
                if (i + 1 >= data.Length || (data[i + 1] & 0xC0) != 0x80)
                {
                    return false;
                }

                // 检查过长编码
                if ((b & 0x1E) == 0)
                {
                    return false;
                }

                i += 2;
            }
            else if ((b & 0xF0) == 0xE0)
            {
                // 3-byte sequence
                if (i + 2 >= data.Length || (data[i + 1] & 0xC0) != 0x80 || (data[i + 2] & 0xC0) != 0x80)
                {
                    return false;
                }

                // 检查过长编码
                if (b == 0xE0 && (data[i + 1] & 0x20) == 0)
                {
                    return false;
                }

                i += 3;
            }
            else if ((b & 0xF8) == 0xF0)
            {
                // 4-byte sequence
                if (i + 3 >= data.Length || (data[i + 1] & 0xC0) != 0x80 || (data[i + 2] & 0xC0) != 0x80 || (data[i + 3] & 0xC0) != 0x80)
                {
                    return false;
                }

                // 检查过长编码和范围
                if (b == 0xF0 && (data[i + 1] & 0x30) == 0)
                {
                    return false;
                }

                if (b > 0xF4)
                {
                    return false;
                }

                i += 4;
            }
            else
            {
                // 无效的 UTF-8 起始字节
                return false;
            }
        }

        return true;
    }
}
