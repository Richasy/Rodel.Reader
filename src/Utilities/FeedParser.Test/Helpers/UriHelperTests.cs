// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.RodelReader.Utilities.FeedParser.Helpers;

namespace FeedParser.Test.Helpers;

/// <summary>
/// UriHelper 单元测试.
/// </summary>
[TestClass]
public sealed class UriHelperTests
{
    [TestMethod]
    [DataRow("https://example.com")]
    [DataRow("http://example.com/path")]
    [DataRow("https://example.com/path?query=value")]
    [DataRow("https://example.com:8080/path")]
    public void TryParse_ValidAbsoluteUri_ShouldReturnTrue(string input)
    {
        // Act
        var result = UriHelper.TryParse(input, out var uri);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(uri);
        Assert.AreEqual(input, uri.ToString().TrimEnd('/'));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow(null)]
    public void TryParse_NullOrEmpty_ShouldReturnFalse(string? input)
    {
        // Act
        var result = UriHelper.TryParse(input, out var uri);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(uri);
    }

    [TestMethod]
    [DataRow("")]  // 空字符串
    [DataRow("   ")]  // 仅空白
    public void TryParse_InvalidUri_ShouldReturnFalse(string input)
    {
        // Act
        var result = UriHelper.TryParse(input, out var uri);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(uri);
    }

    [TestMethod]
    [DataRow("/relative/path")]  // 相对路径
    [DataRow("not a uri")]  // 普通文本
    [DataRow("://invalid")]  // 无scheme
    public void TryParse_RelativeOrAmbiguousUri_ShouldSucceed(string input)
    {
        // Act - 库宽容地接受各种URI格式（包括相对路径）
        var result = UriHelper.TryParse(input, out var uri);

        // Assert - 这些都能被Uri.TryCreate解析（RelativeOrAbsolute）
        Assert.IsTrue(result, $"应能解析 '{input}'");
        Assert.IsNotNull(uri);
    }

    [TestMethod]
    public void TryParse_UriWithUnicodeCharacters_ShouldParse()
    {
        // Arrange
        var input = "https://example.com/路径/文件.html";

        // Act
        var result = UriHelper.TryParse(input, out var uri);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(uri);
    }

    [TestMethod]
    public void TryParse_UriWithEncodedCharacters_ShouldParse()
    {
        // Arrange
        var input = "https://example.com/path%20with%20spaces";

        // Act
        var result = UriHelper.TryParse(input, out var uri);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(uri);
    }
}
