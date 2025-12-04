// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.Legado.Helpers;

namespace Richasy.RodelReader.Sources.Legado.Test.Unit;

/// <summary>
/// ContentFormatter 单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ContentFormatterTests
{
    [TestMethod]
    public void ConvertToHtml_WithNull_ReturnsEmpty()
    {
        // Act
        var result = ContentFormatter.ConvertToHtml(null);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ConvertToHtml_WithEmpty_ReturnsEmpty()
    {
        // Act
        var result = ContentFormatter.ConvertToHtml(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ConvertToHtml_WithWhitespace_ReturnsEmpty()
    {
        // Act
        var result = ContentFormatter.ConvertToHtml("   ");

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ConvertToHtml_WithPlainText_WrapsInPTags()
    {
        // Arrange
        var text = "这是第一行\n这是第二行";

        // Act
        var result = ContentFormatter.ConvertToHtml(text);

        // Assert
        Assert.IsTrue(result.Contains("<p>这是第一行</p>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("<p>这是第二行</p>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ConvertToHtml_WithHtmlTags_PreservesTags()
    {
        // Arrange
        var text = "<img src=\"test.jpg\">\n普通文本";

        // Act
        var result = ContentFormatter.ConvertToHtml(text);

        // Assert
        Assert.IsTrue(result.Contains("<img src=\"test.jpg\">", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("<p>普通文本</p>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ConvertToHtml_WithMixedNewlines_HandlesCorrectly()
    {
        // Arrange
        var text = "第一行\r\n第二行\n第三行";

        // Act
        var result = ContentFormatter.ConvertToHtml(text);

        // Assert
        Assert.IsTrue(result.Contains("<p>第一行</p>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("<p>第二行</p>", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("<p>第三行</p>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void CleanHtml_WithNull_ReturnsEmpty()
    {
        // Act
        var result = ContentFormatter.CleanHtml(null);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void CleanHtml_WithMultipleNewlines_ReducesToTwo()
    {
        // Arrange
        var html = "<p>第一段</p>\n\n\n\n<p>第二段</p>";

        // Act
        var result = ContentFormatter.CleanHtml(html);

        // Assert
        Assert.IsFalse(result.Contains("\n\n\n", StringComparison.Ordinal));
    }
}
