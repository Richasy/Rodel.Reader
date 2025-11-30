// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// Mobi 头信息。
/// </summary>
internal sealed class MobiHeader
{
    /// <summary>
    /// 获取或设置压缩类型。
    /// </summary>
    public ushort Compression { get; set; }

    /// <summary>
    /// 获取或设置文本长度。
    /// </summary>
    public uint TextLength { get; set; }

    /// <summary>
    /// 获取或设置记录数量。
    /// </summary>
    public ushort RecordCount { get; set; }

    /// <summary>
    /// 获取或设置每条记录的大小。
    /// </summary>
    public ushort RecordSize { get; set; }

    /// <summary>
    /// 获取或设置加密类型。
    /// </summary>
    public ushort EncryptionType { get; set; }

    /// <summary>
    /// 获取或设置 Mobi 头长度。
    /// </summary>
    public uint MobiHeaderLength { get; set; }

    /// <summary>
    /// 获取或设置 Mobi 类型。
    /// </summary>
    public uint MobiType { get; set; }

    /// <summary>
    /// 获取或设置文本编码。
    /// </summary>
    public uint TextEncoding { get; set; }

    /// <summary>
    /// 获取或设置唯一 ID。
    /// </summary>
    public uint UniqueId { get; set; }

    /// <summary>
    /// 获取或设置文件版本。
    /// </summary>
    public uint FileVersion { get; set; }

    /// <summary>
    /// 获取或设置完整名称偏移。
    /// </summary>
    public uint FullNameOffset { get; set; }

    /// <summary>
    /// 获取或设置完整名称长度。
    /// </summary>
    public uint FullNameLength { get; set; }

    /// <summary>
    /// 获取或设置语言代码。
    /// </summary>
    public uint LanguageCode { get; set; }

    /// <summary>
    /// 获取或设置第一个非文本记录索引。
    /// </summary>
    public uint FirstNonBookIndex { get; set; }

    /// <summary>
    /// 获取或设置第一个图片记录索引。
    /// </summary>
    public uint FirstImageIndex { get; set; }

    /// <summary>
    /// 获取或设置 EXTH 标志。
    /// </summary>
    public uint ExthFlags { get; set; }

    /// <summary>
    /// 获取一个值，指示是否有 EXTH 头。
    /// </summary>
    public bool HasExth => (ExthFlags & 0x40) != 0;

    /// <summary>
    /// 获取或设置 INDX 记录偏移。
    /// </summary>
    public uint IndxRecordOffset { get; set; }

    /// <summary>
    /// 获取文本编码的 Encoding 对象。
    /// </summary>
    public Encoding GetTextEncoding()
    {
        return TextEncoding switch
        {
            1252 => Encoding.GetEncoding(1252), // Windows-1252
            65001 => Encoding.UTF8,
            _ => Encoding.UTF8,
        };
    }
}

/// <summary>
/// Mobi 头解析器。
/// </summary>
internal static class MobiHeaderParser
{
    /// <summary>
    /// PalmDOC 头大小。
    /// </summary>
    public const int PalmDocHeaderSize = 16;

    /// <summary>
    /// Mobi 头最小大小。
    /// </summary>
    public const int MobiHeaderMinSize = 132;

    /// <summary>
    /// Mobi 魔数。
    /// </summary>
    public static readonly byte[] MobiMagic = "MOBI"u8.ToArray();

    /// <summary>
    /// 解析 Mobi 头。
    /// </summary>
    /// <param name="data">记录 0 的数据。</param>
    /// <returns>解析后的 Mobi 头。</returns>
    public static MobiHeader Parse(byte[] data)
    {
        var header = new MobiHeader();

        // PalmDOC 头
        header.Compression = PalmDbParser.ReadBigEndianUInt16(data, 0);
        header.TextLength = PalmDbParser.ReadBigEndianUInt32(data, 4);
        header.RecordCount = PalmDbParser.ReadBigEndianUInt16(data, 8);
        header.RecordSize = PalmDbParser.ReadBigEndianUInt16(data, 10);
        header.EncryptionType = PalmDbParser.ReadBigEndianUInt16(data, 12);

        // 检查是否有 MOBI 头
        if (data.Length < PalmDocHeaderSize + 4)
        {
            return header;
        }

        // 验证 MOBI 魔数
        var hasMobi = data[16] == MobiMagic[0] &&
                      data[17] == MobiMagic[1] &&
                      data[18] == MobiMagic[2] &&
                      data[19] == MobiMagic[3];

        if (!hasMobi)
        {
            return header;
        }

        // MOBI 头
        header.MobiHeaderLength = PalmDbParser.ReadBigEndianUInt32(data, 20);
        header.MobiType = PalmDbParser.ReadBigEndianUInt32(data, 24);
        header.TextEncoding = PalmDbParser.ReadBigEndianUInt32(data, 28);
        header.UniqueId = PalmDbParser.ReadBigEndianUInt32(data, 32);
        header.FileVersion = PalmDbParser.ReadBigEndianUInt32(data, 36);

        if (header.MobiHeaderLength >= 84)
        {
            header.FullNameOffset = PalmDbParser.ReadBigEndianUInt32(data, 84);
            header.FullNameLength = PalmDbParser.ReadBigEndianUInt32(data, 88);
        }

        if (header.MobiHeaderLength >= 100)
        {
            header.LanguageCode = PalmDbParser.ReadBigEndianUInt32(data, 92);
        }

        if (header.MobiHeaderLength >= 108)
        {
            header.FirstNonBookIndex = PalmDbParser.ReadBigEndianUInt32(data, 80);
            header.ExthFlags = PalmDbParser.ReadBigEndianUInt32(data, 128);
        }

        if (header.MobiHeaderLength >= 112)
        {
            header.FirstImageIndex = PalmDbParser.ReadBigEndianUInt32(data, 108);
        }

        if (header.MobiHeaderLength >= 244)
        {
            header.IndxRecordOffset = PalmDbParser.ReadBigEndianUInt32(data, 244);
        }

        return header;
    }

    /// <summary>
    /// 从记录 0 中提取完整标题名。
    /// </summary>
    /// <param name="data">记录 0 的数据。</param>
    /// <param name="header">Mobi 头。</param>
    /// <returns>完整标题名。</returns>
    public static string? ExtractFullName(byte[] data, MobiHeader header)
    {
        if (header.FullNameOffset == 0 || header.FullNameLength == 0)
        {
            return null;
        }

        if (header.FullNameOffset + header.FullNameLength > data.Length)
        {
            return null;
        }

        var encoding = header.GetTextEncoding();
        return encoding.GetString(data, (int)header.FullNameOffset, (int)header.FullNameLength);
    }
}
