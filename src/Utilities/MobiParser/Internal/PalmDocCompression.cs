// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// PalmDOC 压缩/解压缩。
/// </summary>
internal static class PalmDocCompression
{
    /// <summary>
    /// 无压缩。
    /// </summary>
    public const ushort NoCompression = 1;

    /// <summary>
    /// PalmDOC 压缩。
    /// </summary>
    public const ushort PalmDoc = 2;

    /// <summary>
    /// HUFF/CDIC 压缩。
    /// </summary>
    public const ushort HuffCdicCompression = 17480;

    /// <summary>
    /// 解压缩 PalmDOC 数据。
    /// </summary>
    /// <param name="compressedData">压缩数据。</param>
    /// <returns>解压缩后的数据。</returns>
    public static byte[] Decompress(byte[] compressedData)
    {
        var output = new List<byte>();
        var i = 0;

        while (i < compressedData.Length)
        {
            var c = compressedData[i++];

            if (c >= 0x01 && c <= 0x08)
            {
                // 字面复制 c 个字节
                for (var j = 0; j < c && i < compressedData.Length; j++)
                {
                    output.Add(compressedData[i++]);
                }
            }
            else if (c <= 0x7F)
            {
                // 单个字节
                output.Add(c);
            }
            else if (c >= 0x80 && c <= 0xBF)
            {
                // 回退复制
                if (i >= compressedData.Length)
                {
                    break;
                }

                var next = compressedData[i++];
                var distance = ((c & 0x3F) << 8) | next;
                var distBack = (distance >> 3) & 0x7FF;
                var length = (distance & 0x07) + 3;

                if (distBack > 0 && distBack <= output.Count)
                {
                    var startPos = output.Count - distBack;
                    for (var j = 0; j < length; j++)
                    {
                        output.Add(output[startPos + (j % distBack)]);
                    }
                }
            }
            else
            {
                // 0xC0 - 0xFF: 空格 + 字符
                output.Add(0x20); // 空格
                output.Add((byte)(c ^ 0x80));
            }
        }

        return [.. output];
    }
}
