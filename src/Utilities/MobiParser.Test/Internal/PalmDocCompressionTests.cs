// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Internal;

/// <summary>
/// PalmDocCompression 测试。
/// </summary>
[TestClass]
public sealed class PalmDocCompressionTests
{
    /// <summary>
    /// 测试无压缩数据。
    /// </summary>
    [TestMethod]
    public void Decompress_LiteralBytes_ShouldDecompressCorrectly()
    {
        // Arrange: 0x01-0x08 表示后续 n 个字节是字面值
        var compressed = new byte[] { 0x03, 0x41, 0x42, 0x43 }; // 3 个字面字节: ABC

        // Act
        var decompressed = PalmDocCompression.Decompress(compressed);

        // Assert
        Assert.AreEqual(3, decompressed.Length);
        Assert.AreEqual((byte)'A', decompressed[0]);
        Assert.AreEqual((byte)'B', decompressed[1]);
        Assert.AreEqual((byte)'C', decompressed[2]);
    }

    /// <summary>
    /// 测试单字节（0x09-0x7F）。
    /// </summary>
    [TestMethod]
    public void Decompress_SingleBytes_ShouldPassThrough()
    {
        // Arrange: 0x09-0x7F 的字节直接输出
        var compressed = new byte[] { 0x41, 0x42, 0x43 }; // ABC

        // Act
        var decompressed = PalmDocCompression.Decompress(compressed);

        // Assert
        Assert.AreEqual(3, decompressed.Length);
        Assert.AreEqual((byte)'A', decompressed[0]);
        Assert.AreEqual((byte)'B', decompressed[1]);
        Assert.AreEqual((byte)'C', decompressed[2]);
    }

    /// <summary>
    /// 测试空格+字符（0xC0-0xFF）。
    /// </summary>
    [TestMethod]
    public void Decompress_SpacePlusChar_ShouldDecompressCorrectly()
    {
        // Arrange: 0xC0-0xFF 表示空格 + (字节 XOR 0x80)
        var compressed = new byte[] { 0xC1 }; // 空格 + 'A' (0x41)

        // Act
        var decompressed = PalmDocCompression.Decompress(compressed);

        // Assert
        Assert.AreEqual(2, decompressed.Length);
        Assert.AreEqual(0x20, decompressed[0]); // 空格
        Assert.AreEqual(0x41, decompressed[1]); // 'A'
    }

    /// <summary>
    /// 测试空数据。
    /// </summary>
    [TestMethod]
    public void Decompress_EmptyData_ShouldReturnEmptyArray()
    {
        // Act
        var decompressed = PalmDocCompression.Decompress([]);

        // Assert
        Assert.AreEqual(0, decompressed.Length);
    }

    /// <summary>
    /// 测试混合数据。
    /// </summary>
    [TestMethod]
    public void Decompress_MixedData_ShouldDecompressCorrectly()
    {
        // Arrange: 混合各种类型
        var compressed = new byte[]
        {
            0x48, 0x65, 0x6C, 0x6C, 0x6F, // "Hello" (单字节)
            0xE7,                          // 空格 + 'g' (0x67)
        };

        // Act
        var decompressed = PalmDocCompression.Decompress(compressed);

        // Assert
        var result = System.Text.Encoding.ASCII.GetString(decompressed);
        Assert.AreEqual("Hello g", result);
    }
}
