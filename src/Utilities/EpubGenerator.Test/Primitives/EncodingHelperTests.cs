// Copyright (c) Reader Copilot. All rights reserved.

using System.Text;

namespace EpubGenerator.Test.Primitives;

/// <summary>
/// <see cref="EncodingHelper"/> 的单元测试.
/// </summary>
[TestClass]
public class EncodingHelperTests
{
    [TestInitialize]
    public void Initialize()
    {
        // 注册 GB2312/GBK 编码支持
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_WithUtf8Bom_ReturnsUtf8()
    {
        // Arrange
        var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var content = Encoding.UTF8.GetBytes("测试内容");
        var data = utf8Bom.Concat(content).ToArray();
        using var stream = new MemoryStream(data);

        // Act
        var result = EncodingHelper.DetectEncoding(stream);

        // Assert
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_WithUtf16LeBom_ReturnsUnicode()
    {
        // Arrange
        var utf16LeBom = new byte[] { 0xFF, 0xFE };
        var content = Encoding.Unicode.GetBytes("测试内容");
        var data = utf16LeBom.Concat(content).ToArray();
        using var stream = new MemoryStream(data);

        // Act
        var result = EncodingHelper.DetectEncoding(stream);

        // Assert
        Assert.AreEqual(Encoding.Unicode.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_WithUtf16BeBom_ReturnsBigEndianUnicode()
    {
        // Arrange
        var utf16BeBom = new byte[] { 0xFE, 0xFF };
        var content = Encoding.BigEndianUnicode.GetBytes("测试内容");
        var data = utf16BeBom.Concat(content).ToArray();
        using var stream = new MemoryStream(data);

        // Act
        var result = EncodingHelper.DetectEncoding(stream);

        // Assert
        Assert.AreEqual(Encoding.BigEndianUnicode.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_WithValidUtf8NoBom_ReturnsUtf8()
    {
        // Arrange - UTF-8 without BOM
        var content = Encoding.UTF8.GetBytes("这是测试内容，包含中文字符。");
        using var stream = new MemoryStream(content);

        // Act
        var result = EncodingHelper.DetectEncoding(stream);

        // Assert
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_WithGb2312Content_ReturnsGb2312()
    {
        // Arrange - GB2312 encoded content
        var gb2312 = Encoding.GetEncoding("gb2312");
        var content = gb2312.GetBytes("这是测试内容，包含中文字符。第一章 开始");
        using var stream = new MemoryStream(content);

        // Act
        var result = EncodingHelper.DetectEncoding(stream);

        // Assert
        // GB2312 的 CodePage 是 936
        Assert.AreEqual(936, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_WithAsciiOnly_ReturnsUtf8()
    {
        // Arrange - ASCII only (valid UTF-8)
        var content = Encoding.ASCII.GetBytes("Hello World! This is ASCII text.");
        using var stream = new MemoryStream(content);

        // Act
        var result = EncodingHelper.DetectEncoding(stream);

        // Assert
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_WithEmptyStream_ReturnsDefaultEncoding()
    {
        // Arrange
        using var stream = new MemoryStream([]);

        // Act
        var result = EncodingHelper.DetectEncoding(stream);

        // Assert
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_ResetsStreamPosition()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("Test content");
        using var stream = new MemoryStream(content);
        stream.Position = 5; // 设置到中间位置

        // Act
        var originalPosition = stream.Position;
        _ = EncodingHelper.DetectEncoding(stream);

        // Assert - 位置应该被恢复
        Assert.AreEqual(originalPosition, stream.Position);
    }

    [TestMethod]
    public void DetectEncodingFromBom_WithUtf8Bom_ReturnsUtf8()
    {
        // Arrange
        ReadOnlySpan<byte> bom = [0xEF, 0xBB, 0xBF];

        // Act
        var result = EncodingHelper.DetectEncodingFromBom(bom, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncodingFromBom_WithUtf32LeBom_ReturnsUtf32()
    {
        // Arrange
        ReadOnlySpan<byte> bom = [0xFF, 0xFE, 0x00, 0x00];

        // Act
        var result = EncodingHelper.DetectEncodingFromBom(bom, null);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(Encoding.UTF32.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncodingFromBom_WithUtf32BeBom_ReturnsUtf32BE()
    {
        // Arrange
        ReadOnlySpan<byte> bom = [0x00, 0x00, 0xFE, 0xFF];

        // Act
        var result = EncodingHelper.DetectEncodingFromBom(bom, null);

        // Assert
        Assert.IsNotNull(result);
        // UTF-32 BE
        Assert.IsTrue(result is UTF32Encoding);
    }

    [TestMethod]
    public void DetectEncodingFromBom_WithNoBom_ReturnsDefault()
    {
        // Arrange
        ReadOnlySpan<byte> data = [0x48, 0x65, 0x6C, 0x6C, 0x6F]; // "Hello"
        var defaultEncoding = Encoding.ASCII;

        // Act
        var result = EncodingHelper.DetectEncodingFromBom(data, defaultEncoding);

        // Assert
        Assert.AreEqual(defaultEncoding, result);
    }

    [TestMethod]
    public void DetectEncodingFromBom_WithNoBomAndNullDefault_ReturnsNull()
    {
        // Arrange
        ReadOnlySpan<byte> data = [0x48, 0x65, 0x6C, 0x6C, 0x6F]; // "Hello"

        // Act
        var result = EncodingHelper.DetectEncodingFromBom(data, null);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void DetectEncoding_FromBytes_WithValidUtf8_ReturnsUtf8()
    {
        // Arrange
        ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes("这是UTF-8编码的中文内容");

        // Act
        var result = EncodingHelper.DetectEncoding(data);

        // Assert
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromBytes_WithGb2312_ReturnsGb2312()
    {
        // Arrange
        var gb2312 = Encoding.GetEncoding("gb2312");
        ReadOnlySpan<byte> data = gb2312.GetBytes("这是GB2312编码的中文内容");

        // Act
        var result = EncodingHelper.DetectEncoding(data);

        // Assert
        Assert.AreEqual(936, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromBytes_WithSingleByte_ReturnsUtf8()
    {
        // Arrange
        ReadOnlySpan<byte> data = [0x41]; // 'A'

        // Act
        var result = EncodingHelper.DetectEncoding(data);

        // Assert
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromBytes_WithEmptyData_ReturnsUtf8()
    {
        // Arrange
        ReadOnlySpan<byte> data = [];

        // Act
        var result = EncodingHelper.DetectEncoding(data);

        // Assert
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromBytes_WithMixedContent_ReturnsCorrectEncoding()
    {
        // Arrange - 混合 ASCII 和中文
        var content = "Chapter 1 第一章 Introduction 简介";
        ReadOnlySpan<byte> utf8Data = Encoding.UTF8.GetBytes(content);

        // Act
        var result = EncodingHelper.DetectEncoding(utf8Data);

        // Assert
        Assert.AreEqual(Encoding.UTF8.CodePage, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromStream_WithLargeFile_DetectsCorrectly()
    {
        // Arrange - 模拟大文件（只读取前 4KB）
        var gb2312 = Encoding.GetEncoding("gb2312");
        var repeatedContent = string.Concat(Enumerable.Repeat("这是一段测试内容。", 1000));
        var content = gb2312.GetBytes(repeatedContent);
        using var stream = new MemoryStream(content);

        // Act
        var result = EncodingHelper.DetectEncoding(stream);

        // Assert
        Assert.AreEqual(936, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromBytes_WithInvalidUtf8Sequence_ReturnsGb2312()
    {
        // Arrange - 无效的 UTF-8 序列（但可能是有效的 GB2312）
        // 0xC0 0x80 是过长编码，不是有效的 UTF-8
        byte[] data = [0xC0, 0x80, 0x41, 0x42];

        // Act
        var result = EncodingHelper.DetectEncoding(data);

        // Assert
        Assert.AreEqual(936, result.CodePage);
    }

    [TestMethod]
    public void DetectEncoding_FromBytes_WithTruncatedUtf8_ReturnsGb2312()
    {
        // Arrange - 截断的 UTF-8 多字节序列
        // 0xE4 开始一个 3 字节序列，但后面没有足够的字节
        byte[] data = [0xE4, 0xB8];

        // Act
        var result = EncodingHelper.DetectEncoding(data);

        // Assert
        Assert.AreEqual(936, result.CodePage);
    }
}
