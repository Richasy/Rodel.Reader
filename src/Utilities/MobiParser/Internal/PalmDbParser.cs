// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// PalmDB 记录头。
/// </summary>
internal sealed class PalmDbRecord
{
    /// <summary>
    /// 获取或设置记录在文件中的偏移位置。
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// 获取或设置记录属性。
    /// </summary>
    public byte Attributes { get; set; }

    /// <summary>
    /// 获取或设置唯一 ID。
    /// </summary>
    public uint UniqueId { get; set; }
}

/// <summary>
/// PalmDB 头解析器。
/// </summary>
internal static class PalmDbParser
{
    /// <summary>
    /// PalmDB 头的固定大小。
    /// </summary>
    public const int HeaderSize = 78;

    /// <summary>
    /// 每个记录条目的大小。
    /// </summary>
    public const int RecordEntrySize = 8;

    /// <summary>
    /// 解析 PalmDB 头。
    /// </summary>
    /// <param name="stream">输入流。</param>
    /// <returns>数据库名称和记录列表。</returns>
    public static async Task<(string Name, List<PalmDbRecord> Records)> ParseAsync(Stream stream)
    {
        var header = new byte[HeaderSize];
        await stream.ReadExactlyAsync(header, 0, HeaderSize).ConfigureAwait(false);

        // 数据库名称（前 32 字节，以 null 结尾）
        var nameBytes = new byte[32];
        Array.Copy(header, 0, nameBytes, 0, 32);
        var nullIndex = Array.IndexOf(nameBytes, (byte)0);
        var name = Encoding.Latin1.GetString(nameBytes, 0, nullIndex >= 0 ? nullIndex : 32);

        // 记录数量（偏移 76，2 字节）
        var recordCount = ReadBigEndianUInt16(header, 76);

        // 读取记录列表
        var records = new List<PalmDbRecord>(recordCount);
        var recordBuffer = new byte[RecordEntrySize];

        for (var i = 0; i < recordCount; i++)
        {
            await stream.ReadExactlyAsync(recordBuffer, 0, RecordEntrySize).ConfigureAwait(false);

            records.Add(new PalmDbRecord
            {
                Offset = ReadBigEndianUInt32(recordBuffer, 0),
                Attributes = recordBuffer[4],
                UniqueId = (uint)((recordBuffer[5] << 16) | (recordBuffer[6] << 8) | recordBuffer[7]),
            });
        }

        return (name, records);
    }

    /// <summary>
    /// 读取大端序 16 位无符号整数。
    /// </summary>
    public static ushort ReadBigEndianUInt16(byte[] data, int offset)
    {
        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    /// <summary>
    /// 读取大端序 32 位无符号整数。
    /// </summary>
    public static uint ReadBigEndianUInt32(byte[] data, int offset)
    {
        return (uint)((data[offset] << 24) | (data[offset + 1] << 16) |
                      (data[offset + 2] << 8) | data[offset + 3]);
    }
}
