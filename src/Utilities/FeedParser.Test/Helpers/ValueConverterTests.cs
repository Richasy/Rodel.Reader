// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelReader.Utilities.FeedParser.Helpers;

namespace FeedParser.Test.Helpers;

/// <summary>
/// ValueConverter 单元测试.
/// </summary>
[TestClass]
public sealed class ValueConverterTests
{
    #region 整数转换测试

    [TestMethod]
    [DataRow("123", 123)]
    [DataRow("0", 0)]
    [DataRow("-456", -456)]
    [DataRow("2147483647", int.MaxValue)]
    public void TryConvert_ValidInt_ShouldReturnTrue(string input, int expected)
    {
        // Act
        var result = ValueConverter.TryConvert<int>(input, out var value);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expected, value);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(null)]
    [DataRow("not a number")]
    [DataRow("12.34")]
    public void TryConvert_InvalidInt_ShouldReturnFalse(string? input)
    {
        // Act
        var result = ValueConverter.TryConvert<int>(input, out var value);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(default(int), value);
    }

    #endregion

    #region 长整数转换测试

    [TestMethod]
    [DataRow("9223372036854775807", long.MaxValue)]
    [DataRow("12345678901234", 12345678901234L)]
    public void TryConvert_ValidLong_ShouldReturnTrue(string input, long expected)
    {
        // Act
        var result = ValueConverter.TryConvert<long>(input, out var value);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expected, value);
    }

    #endregion

    #region 布尔转换测试

    [TestMethod]
    [DataRow("true", true)]
    [DataRow("True", true)]
    [DataRow("TRUE", true)]
    [DataRow("false", false)]
    [DataRow("False", false)]
    [DataRow("FALSE", false)]
    public void TryConvert_ValidBool_ShouldReturnTrue(string input, bool expected)
    {
        // Act
        var result = ValueConverter.TryConvert<bool>(input, out var value);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expected, value);
    }

    [TestMethod]
    [DataRow("yes", true)]   // iTunes常用
    [DataRow("no", false)]   // iTunes常用
    [DataRow("1", true)]     // 数字形式
    [DataRow("0", false)]    // 数字形式
    [DataRow("YES", true)]   // 大写
    [DataRow("NO", false)]   // 大写
    public void TryConvert_ExtendedBoolFormats_ShouldSucceed(string input, bool expected)
    {
        // Act - RSS/Podcast 中常用这些格式表示布尔值
        var result = ValueConverter.TryConvert<bool>(input, out var value);

        // Assert
        Assert.IsTrue(result, $"应能解析 '{input}' 为布尔值");
        Assert.AreEqual(expected, value);
    }

    [TestMethod]
    [DataRow("maybe")]
    [DataRow("oui")]
    [DataRow("2")]
    public void TryConvert_InvalidBool_ShouldReturnFalse(string input)
    {
        // Act
        var result = ValueConverter.TryConvert<bool>(input, out var value);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(default(bool), value);
    }

    #endregion

    #region 浮点数转换测试

    [TestMethod]
    [DataRow("3.14", 3.14)]
    [DataRow("0.0", 0.0)]
    [DataRow("-123.456", -123.456)]
    public void TryConvert_ValidDouble_ShouldReturnTrue(string input, double expected)
    {
        // Act
        var result = ValueConverter.TryConvert<double>(input, out var value);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expected, value, 0.0001);
    }

    #endregion

    #region 字符串转换测试

    [TestMethod]
    public void TryConvert_String_ShouldReturnInput()
    {
        // Arrange
        var input = "test string";

        // Act
        var result = ValueConverter.TryConvert<string>(input, out var value);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(input, value);
    }

    [TestMethod]
    public void TryConvert_NullString_ShouldReturnEmptyString()
    {
        // Act - 对于string类型，null输入返回空字符串
        var result = ValueConverter.TryConvert<string>(null, out var value);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(string.Empty, value);
    }

    #endregion
}
