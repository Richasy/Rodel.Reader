// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Internal;

/// <summary>
/// LanguageCodeConverter 测试。
/// </summary>
[TestClass]
public sealed class LanguageCodeConverterTests
{
    /// <summary>
    /// 测试常见语言代码转换。
    /// </summary>
    [TestMethod]
    [DataRow(0x0409u, "en")]
    [DataRow(0x0804u, "zh-CN")]
    [DataRow(0x0404u, "zh-TW")]
    [DataRow(0x0411u, "ja")]
    [DataRow(0x0412u, "ko")]
    [DataRow(0x040Cu, "fr")]
    [DataRow(0x0407u, "de")]
    [DataRow(0x0419u, "ru")]
    public void ToLanguageTag_KnownCode_ShouldReturnCorrectTag(uint code, string expected)
    {
        // Act
        var result = LanguageCodeConverter.ToLanguageTag(code);

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// 测试未知语言代码。
    /// </summary>
    [TestMethod]
    public void ToLanguageTag_UnknownCode_ShouldReturnNull()
    {
        // Arrange
        var unknownCode = 0xFFFFu;

        // Act
        var result = LanguageCodeConverter.ToLanguageTag(unknownCode);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// 测试零代码。
    /// </summary>
    [TestMethod]
    public void ToLanguageTag_ZeroCode_ShouldReturnNull()
    {
        // Act
        var result = LanguageCodeConverter.ToLanguageTag(0);

        // Assert
        Assert.IsNull(result);
    }
}
