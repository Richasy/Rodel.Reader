// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace Richasy.RodelReader.Utilities.MobiParser;

/// <summary>
/// EXTH 头记录。
/// </summary>
internal sealed class ExthRecord
{
    /// <summary>
    /// 获取或设置记录类型。
    /// </summary>
    public uint Type { get; set; }

    /// <summary>
    /// 获取或设置记录数据。
    /// </summary>
    public byte[] Data { get; set; } = [];

    /// <summary>
    /// 将数据解析为字符串。
    /// </summary>
    /// <param name="encoding">字符编码。</param>
    /// <returns>字符串值。</returns>
    public string GetString(Encoding encoding)
    {
        return encoding.GetString(Data).Trim('\0');
    }

    /// <summary>
    /// 将数据解析为 32 位无符号整数。
    /// </summary>
    /// <returns>整数值。</returns>
    public uint GetUInt32()
    {
        if (Data.Length < 4)
        {
            return 0;
        }

        return PalmDbParser.ReadBigEndianUInt32(Data, 0);
    }
}

/// <summary>
/// EXTH 头解析器。
/// </summary>
internal static class ExthParser
{
    /// <summary>
    /// EXTH 魔数。
    /// </summary>
    public static readonly byte[] ExthMagic = "EXTH"u8.ToArray();

    // 常见 EXTH 记录类型
    public const uint TypeAuthor = 100;
    public const uint TypePublisher = 101;
    public const uint TypeDescription = 103;
    public const uint TypeIsbn = 104;
    public const uint TypeSubject = 105;
    public const uint TypePublishDate = 106;
    public const uint TypeReview = 107;
    public const uint TypeContributor = 108;
    public const uint TypeRights = 109;
    public const uint TypeAsin = 113;
    public const uint TypeLanguage = 524;
    public const uint TypeCoverOffset = 201;
    public const uint TypeThumbOffset = 202;

    /// <summary>
    /// 解析 EXTH 头。
    /// </summary>
    /// <param name="data">记录 0 的数据。</param>
    /// <param name="mobiHeader">Mobi 头信息。</param>
    /// <returns>EXTH 记录列表。</returns>
    public static List<ExthRecord> Parse(byte[] data, MobiHeader mobiHeader)
    {
        var records = new List<ExthRecord>();

        if (!mobiHeader.HasExth)
        {
            return records;
        }

        // EXTH 头位于 MOBI 头之后
        var exthOffset = (int)(MobiHeaderParser.PalmDocHeaderSize + mobiHeader.MobiHeaderLength);
        if (exthOffset + 12 > data.Length)
        {
            return records;
        }

        // 验证 EXTH 魔数
        var hasExth = data[exthOffset] == ExthMagic[0] &&
                      data[exthOffset + 1] == ExthMagic[1] &&
                      data[exthOffset + 2] == ExthMagic[2] &&
                      data[exthOffset + 3] == ExthMagic[3];

        if (!hasExth)
        {
            return records;
        }

        // EXTH 头长度
        var exthLength = PalmDbParser.ReadBigEndianUInt32(data, exthOffset + 4);

        // 记录数量
        var recordCount = PalmDbParser.ReadBigEndianUInt32(data, exthOffset + 8);

        var offset = exthOffset + 12;
        for (var i = 0; i < recordCount && offset + 8 <= data.Length; i++)
        {
            var type = PalmDbParser.ReadBigEndianUInt32(data, offset);
            var length = PalmDbParser.ReadBigEndianUInt32(data, offset + 4);

            if (length < 8 || offset + length > data.Length)
            {
                break;
            }

            var dataLength = (int)length - 8;
            var recordData = new byte[dataLength];
            Array.Copy(data, offset + 8, recordData, 0, dataLength);

            records.Add(new ExthRecord
            {
                Type = type,
                Data = recordData,
            });

            offset += (int)length;
        }

        return records;
    }

    /// <summary>
    /// 从 EXTH 记录中提取元数据。
    /// </summary>
    /// <param name="records">EXTH 记录列表。</param>
    /// <param name="encoding">文本编码。</param>
    /// <returns>元数据对象。</returns>
    public static MobiMetadata ExtractMetadata(List<ExthRecord> records, Encoding encoding)
    {
        var metadata = new MobiMetadata();

        foreach (var record in records)
        {
            switch (record.Type)
            {
                case TypeAuthor:
                    metadata.Authors.Add(record.GetString(encoding));
                    break;
                case TypePublisher:
                    metadata.Publisher = record.GetString(encoding);
                    break;
                case TypeDescription:
                    metadata.Description = record.GetString(encoding);
                    break;
                case TypeIsbn:
                    metadata.Isbn = record.GetString(encoding);
                    metadata.Identifier ??= metadata.Isbn;
                    break;
                case TypeSubject:
                    metadata.Subjects.Add(record.GetString(encoding));
                    break;
                case TypePublishDate:
                    metadata.PublishDate = record.GetString(encoding);
                    break;
                case TypeContributor:
                    metadata.Contributors.Add(record.GetString(encoding));
                    break;
                case TypeRights:
                    metadata.Rights = record.GetString(encoding);
                    break;
                case TypeAsin:
                    metadata.Asin = record.GetString(encoding);
                    metadata.Identifier ??= metadata.Asin;
                    break;
                case TypeLanguage:
                    metadata.Language = record.GetString(encoding);
                    break;
                default:
                    // 存储未知类型到自定义元数据
                    var value = record.GetString(encoding);
                    if (!string.IsNullOrEmpty(value))
                    {
                        metadata.CustomMetadata[$"exth_{record.Type}"] = value;
                    }

                    break;
            }
        }

        return metadata;
    }

    /// <summary>
    /// 获取封面偏移索引。
    /// </summary>
    /// <param name="records">EXTH 记录列表。</param>
    /// <returns>封面图片索引，如果未找到则为 null。</returns>
    public static uint? GetCoverOffset(List<ExthRecord> records)
    {
        var coverRecord = records.FirstOrDefault(r => r.Type == TypeCoverOffset);
        if (coverRecord != null)
        {
            return coverRecord.GetUInt32();
        }

        return null;
    }
}
