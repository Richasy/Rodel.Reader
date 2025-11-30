// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace MobiParser.Test;

/// <summary>
/// 测试数据工厂。
/// </summary>
internal static class TestDataFactory
{
    /// <summary>
    /// 创建最小的有效 Mobi 文件流。
    /// </summary>
    public static MemoryStream CreateMinimalMobiStream()
    {
        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        // PalmDB 头 (78 字节)
        // 数据库名称 (32 字节)
        var name = Encoding.Latin1.GetBytes("Test Book\0");
        writer.Write(name);
        writer.Write(new byte[32 - name.Length]); // 填充

        // 属性 (2 字节)
        writer.Write((ushort)0);

        // 版本 (2 字节)
        writer.Write((ushort)0);

        // 创建日期 (4 字节)
        writer.Write(0u);

        // 修改日期 (4 字节)
        writer.Write(0u);

        // 备份日期 (4 字节)
        writer.Write(0u);

        // 修改号 (4 字节)
        writer.Write(0u);

        // AppInfo 偏移 (4 字节)
        writer.Write(0u);

        // SortInfo 偏移 (4 字节)
        writer.Write(0u);

        // 类型 (4 字节) - BOOK
        writer.Write(Encoding.ASCII.GetBytes("BOOK"));

        // 创建者 (4 字节) - MOBI
        writer.Write(Encoding.ASCII.GetBytes("MOBI"));

        // 唯一 ID 种子 (4 字节)
        writer.Write(0u);

        // 下一个记录列表 ID (4 字节)
        writer.Write(0u);

        // 记录数量 (2 字节) - 2 条记录
        WriteBigEndianUInt16(writer, 2);

        // 计算记录偏移
        var headerSize = 78;
        var recordListSize = 2 * 8; // 2 条记录，每条 8 字节
        var record0Offset = headerSize + recordListSize + 2; // +2 用于填充

        // 记录 0 条目 (8 字节)
        WriteBigEndianUInt32(writer, (uint)record0Offset);
        writer.Write((byte)0); // 属性
        writer.Write((byte)0); // UniqueId 高位
        writer.Write((byte)0); // UniqueId 中位
        writer.Write((byte)0); // UniqueId 低位

        // 记录 1 条目 (8 字节)
        var record1Offset = record0Offset + 256; // 记录 0 大小
        WriteBigEndianUInt32(writer, (uint)record1Offset);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)1);

        // 填充 (2 字节)
        writer.Write((ushort)0);

        // 记录 0: PalmDOC 头 + MOBI 头
        var record0 = CreateRecord0();
        writer.Write(record0);

        // 记录 1: 一些文本内容
        var content = "<html><body><h1>Test</h1><p>Content</p></body></html>";
        writer.Write(Encoding.UTF8.GetBytes(content));

        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 创建包含图片的 Mobi 文件流。
    /// </summary>
    public static MemoryStream CreateMobiWithImageStream()
    {
        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        // PalmDB 头 (78 字节)
        var name = Encoding.Latin1.GetBytes("Manga Book\0");
        writer.Write(name);
        writer.Write(new byte[32 - name.Length]);

        // 属性、版本等
        writer.Write((ushort)0);
        writer.Write((ushort)0);
        writer.Write(0u); // 创建日期
        writer.Write(0u); // 修改日期
        writer.Write(0u); // 备份日期
        writer.Write(0u); // 修改号
        writer.Write(0u); // AppInfo
        writer.Write(0u); // SortInfo
        writer.Write(Encoding.ASCII.GetBytes("BOOK"));
        writer.Write(Encoding.ASCII.GetBytes("MOBI"));
        writer.Write(0u); // UID 种子
        writer.Write(0u); // 下一个记录列表

        // 3 条记录
        WriteBigEndianUInt16(writer, 3);

        var headerSize = 78;
        var recordListSize = 3 * 8;
        var record0Offset = headerSize + recordListSize + 2;

        // 记录 0
        WriteBigEndianUInt32(writer, (uint)record0Offset);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);

        // 记录 1 (文本)
        var record1Offset = record0Offset + 300;
        WriteBigEndianUInt32(writer, (uint)record1Offset);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)1);

        // 记录 2 (图片)
        var record2Offset = record1Offset + 100;
        WriteBigEndianUInt32(writer, (uint)record2Offset);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)2);

        // 填充
        writer.Write((ushort)0);

        // 记录 0
        var record0 = CreateRecord0WithImage(2); // 图片从索引 2 开始
        writer.Write(record0);

        // 记录 1: 文本内容
        var content = "<html><body><h1>Page 1</h1></body></html>";
        var paddedContent = new byte[100];
        var contentBytes = Encoding.UTF8.GetBytes(content);
        Array.Copy(contentBytes, paddedContent, Math.Min(contentBytes.Length, 100));
        writer.Write(paddedContent);

        // 记录 2: 最小 JPEG
        writer.Write(CreateMinimalJpeg());

        stream.Position = 0;
        return stream;
    }

    private static byte[] CreateRecord0()
    {
        var record = new byte[256];
        var offset = 0;

        // PalmDOC 头 (16 字节)
        // 压缩类型 (2 字节) - 无压缩
        WriteBigEndian16(record, offset, 1);
        offset += 2;

        // 未使用 (2 字节)
        offset += 2;

        // 文本长度 (4 字节)
        WriteBigEndian32(record, offset, 100);
        offset += 4;

        // 记录数 (2 字节)
        WriteBigEndian16(record, offset, 1);
        offset += 2;

        // 记录大小 (2 字节)
        WriteBigEndian16(record, offset, 4096);
        offset += 2;

        // 加密类型 (2 字节)
        offset += 2;

        // 未使用 (2 字节)
        offset += 2;

        // MOBI 头
        // 标识符 "MOBI" (4 字节)
        record[16] = (byte)'M';
        record[17] = (byte)'O';
        record[18] = (byte)'B';
        record[19] = (byte)'I';

        // 头长度 (4 字节)
        WriteBigEndian32(record, 20, 232);

        // Mobi 类型 (4 字节)
        WriteBigEndian32(record, 24, 2); // MOBI book

        // 文本编码 (4 字节) - UTF-8
        WriteBigEndian32(record, 28, 65001);

        // 唯一 ID (4 字节)
        WriteBigEndian32(record, 32, 12345);

        // 文件版本 (4 字节)
        WriteBigEndian32(record, 36, 6);

        // 完整名称偏移
        WriteBigEndian32(record, 84, 248);

        // 完整名称长度
        WriteBigEndian32(record, 88, 4);

        // 语言代码
        WriteBigEndian32(record, 92, 0x0409); // en-US

        // First Non-Book Index
        WriteBigEndian32(record, 80, 2);

        // EXTH 标志 (无 EXTH)
        WriteBigEndian32(record, 128, 0);

        // 完整名称
        record[248] = (byte)'T';
        record[249] = (byte)'e';
        record[250] = (byte)'s';
        record[251] = (byte)'t';

        return record;
    }

    private static byte[] CreateRecord0WithImage(int firstImageIndex)
    {
        var record = new byte[300];
        var offset = 0;

        // PalmDOC 头
        WriteBigEndian16(record, offset, 1); // 无压缩
        offset += 4;
        WriteBigEndian32(record, offset, 100); // 文本长度
        offset += 4;
        WriteBigEndian16(record, offset, 1); // 记录数
        offset += 2;
        WriteBigEndian16(record, offset, 4096); // 记录大小
        offset += 4;

        // MOBI 头
        record[16] = (byte)'M';
        record[17] = (byte)'O';
        record[18] = (byte)'B';
        record[19] = (byte)'I';

        WriteBigEndian32(record, 20, 232); // 头长度
        WriteBigEndian32(record, 24, 2); // Mobi 类型
        WriteBigEndian32(record, 28, 65001); // UTF-8
        WriteBigEndian32(record, 32, 12345); // UID
        WriteBigEndian32(record, 36, 6); // 版本
        WriteBigEndian32(record, 80, (uint)firstImageIndex); // First Non-Book
        WriteBigEndian32(record, 84, 280); // 名称偏移
        WriteBigEndian32(record, 88, 10); // 名称长度
        WriteBigEndian32(record, 92, 0x0411); // ja
        WriteBigEndian32(record, 108, (uint)firstImageIndex); // First Image Index
        WriteBigEndian32(record, 128, 0); // 无 EXTH

        // 名称
        var nameBytes = Encoding.UTF8.GetBytes("Manga Book");
        Array.Copy(nameBytes, 0, record, 280, nameBytes.Length);

        return record;
    }

    /// <summary>
    /// 创建最小的有效 JPEG 图片（1x1 像素）。
    /// </summary>
    public static byte[] CreateMinimalJpeg()
    {
        return
        [
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
            0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
            0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
            0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
            0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
            0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
            0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
            0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
            0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x1F, 0x00, 0x00,
            0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0xFF, 0xC4, 0x00, 0xB5, 0x10, 0x00, 0x02, 0x01, 0x03,
            0x03, 0x02, 0x04, 0x03, 0x05, 0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7D,
            0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06,
            0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xA1, 0x08,
            0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00, 0x7F, 0xFF,
            0xD9
        ];
    }

    /// <summary>
    /// 创建最小的有效 PNG 图片（1x1 像素）。
    /// </summary>
    public static byte[] CreateMinimalPng()
    {
        return
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00, 0x00, 0x00,
            0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
            0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49,
            0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        ];
    }

    private static void WriteBigEndianUInt16(BinaryWriter writer, ushort value)
    {
        writer.Write((byte)(value >> 8));
        writer.Write((byte)(value & 0xFF));
    }

    private static void WriteBigEndianUInt32(BinaryWriter writer, uint value)
    {
        writer.Write((byte)(value >> 24));
        writer.Write((byte)((value >> 16) & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
        writer.Write((byte)(value & 0xFF));
    }

    private static void WriteBigEndian16(byte[] buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)(value & 0xFF);
    }

    private static void WriteBigEndian32(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 3] = (byte)(value & 0xFF);
    }
}
