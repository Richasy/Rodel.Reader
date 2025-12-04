// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.FanQie.Helpers;

namespace Richasy.RodelReader.Sources.FanQie.Test.Unit;

/// <summary>
/// 内容解析器测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ContentParserTests
{
    [TestMethod]
    public void ToPlainText_WithHtmlContent_ReturnsPlainText()
    {
        // Arrange
        var html = "<p>第一段落</p><p>第二段落</p>";

        // Act
        var result = ContentParser.ToPlainText(html);

        // Assert
        Assert.IsTrue(result.Contains("第一段落", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("第二段落", StringComparison.Ordinal));
        Assert.IsFalse(result.Contains("<p>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ToPlainText_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var html = string.Empty;

        // Act
        var result = ContentParser.ToPlainText(html);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ToPlainText_WithNull_ReturnsEmpty()
    {
        // Arrange
        string? html = null;

        // Act
        var result = ContentParser.ToPlainText(html!);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ToPlainText_WithBrTags_ConvertsToNewlines()
    {
        // Arrange
        var html = "第一行<br/>第二行<br>第三行";

        // Act
        var result = ContentParser.ToPlainText(html);

        // Assert
        Assert.IsTrue(result.Contains("第一行", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("第二行", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("第三行", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ToPlainText_WithHtmlEntities_DecodesEntities()
    {
        // Arrange
        var html = "<p>测试 &amp; 验证 &lt;内容&gt;</p>";

        // Act
        var result = ContentParser.ToPlainText(html);

        // Assert
        Assert.IsTrue(result.Contains('&', StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("<内容>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void CleanHtml_WithStyleTags_RemovesStyles()
    {
        // Arrange
        var html = "<style>.test { color: red; }</style><p>内容</p>";

        // Act
        var result = ContentParser.CleanHtml(html);

        // Assert
        Assert.IsFalse(result.Contains("<style>", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(result.Contains("<p>", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void CleanHtml_WithScriptTags_RemovesScripts()
    {
        // Arrange
        var html = "<script>alert('test');</script><p>内容</p>";

        // Act
        var result = ContentParser.CleanHtml(html);

        // Assert
        Assert.IsFalse(result.Contains("<script>", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(result.Contains("<p>", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void CleanHtml_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var html = string.Empty;

        // Act
        var result = ContentParser.CleanHtml(html);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void CountWords_WithChineseText_ReturnsCorrectCount()
    {
        // Arrange
        var text = "这是一段测试文字";

        // Act
        var result = ContentParser.CountWords(text);

        // Assert
        Assert.AreEqual(8, result);
    }

    [TestMethod]
    public void CountWords_WithMixedContent_IgnoresSpaces()
    {
        // Arrange
        var text = "测试 文字 内容";

        // Act
        var result = ContentParser.CountWords(text);

        // Assert
        Assert.AreEqual(6, result); // 不含空格
    }

    [TestMethod]
    public void CountWords_WithEmptyString_ReturnsZero()
    {
        // Arrange
        var text = string.Empty;

        // Act
        var result = ContentParser.CountWords(text);

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void CountWords_WithOnlyWhitespace_ReturnsZero()
    {
        // Arrange
        var text = "   \t\n   ";

        // Act
        var result = ContentParser.CountWords(text);

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void ParseContentWithImages_WithNoImages_ReturnsOriginalText()
    {
        // Arrange
        var content = "这是一段普通文字内容。\n第二段内容。";

        // Act - 设置 removeFirstLine = false 以保留完整内容
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter", removeFirstLine: false);

        // Assert
        Assert.IsNull(images);
        Assert.AreEqual(content, cleanedText);
        Assert.IsTrue(htmlContent.Contains("<article>", StringComparison.Ordinal));
        Assert.IsTrue(htmlContent.Contains("这是一段普通文字内容。", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ParseContentWithImages_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsNull(images);
        Assert.AreEqual(string.Empty, cleanedText);
        Assert.AreEqual(string.Empty, htmlContent);
    }

    [TestMethod]
    public void ParseContentWithImages_WithNull_ReturnsEmpty()
    {
        // Arrange
        string? content = null;

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content!, "test_chapter");

        // Assert
        Assert.IsNull(images);
        Assert.AreEqual(string.Empty, cleanedText);
        Assert.AreEqual(string.Empty, htmlContent);
    }

    [TestMethod]
    public void ParseContentWithImages_WithFanQieImageTag_ExtractsImageUrl()
    {
        // Arrange - JSON 解析后的格式，双引号被转义为 \"
        var content = "文字内容<img src=\\\"http://example.com/image.jpg\\\" img-width=\\\"602\\\" img-height=\\\"339\\\" alt=\\\"\\\" media-idx=\\\"1\\\"/>更多文字";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsNotNull(images);
        Assert.AreEqual(1, images.Count);
        Assert.AreEqual("http://example.com/image.jpg", images[0].Url);
        Assert.AreEqual("img_test_chapter_0", images[0].Id);
        Assert.IsFalse(cleanedText.Contains("<img", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(cleanedText.Contains("文字内容", StringComparison.Ordinal));
        Assert.IsTrue(cleanedText.Contains("更多文字", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ParseContentWithImages_WithMultipleImages_ExtractsAllImages()
    {
        // Arrange - JSON 解析后的格式
        var content = "第一张<img src=\\\"http://example.com/1.jpg\\\" media-idx=\\\"1\\\"/>中间<img src=\\\"http://example.com/2.jpg\\\" media-idx=\\\"2\\\"/>最后";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsNotNull(images);
        Assert.AreEqual(2, images.Count);
        Assert.AreEqual("http://example.com/1.jpg", images[0].Url);
        Assert.AreEqual("img_test_chapter_0", images[0].Id);
        Assert.AreEqual("http://example.com/2.jpg", images[1].Url);
        Assert.AreEqual("img_test_chapter_1", images[1].Id);
    }

    [TestMethod]
    public void ParseContentWithImages_WithEncodedUrl_DecodesUrl()
    {
        // Arrange - JSON 解析后的格式
        var content = "<img src=\\\"http://example.com/image.jpg?a=1&amp;b=2\\\" media-idx=\\\"1\\\"/>";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsNotNull(images);
        Assert.AreEqual(1, images.Count);
        Assert.AreEqual("http://example.com/image.jpg?a=1&b=2", images[0].Url);
    }

    [TestMethod]
    public void ParseContentWithImages_HtmlContentContainsPlaceholder()
    {
        // Arrange - JSON 解析后的格式
        var content = "文字<img src=\\\"http://example.com/image.jpg\\\" img-width=\\\"100\\\" img-height=\\\"200\\\" media-idx=\\\"1\\\"/>结束";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsTrue(htmlContent.Contains("<!-- FANQIE_IMAGE:img_test_chapter_0 -->", StringComparison.Ordinal));
        Assert.IsFalse(htmlContent.Contains("\\\"", StringComparison.Ordinal)); // 不应该有转义引号
    }

    [TestMethod]
    public void ParseContentWithImages_WithImageWithoutMediaIdx_HasValidId()
    {
        // Arrange - JSON 解析后的格式，只有 src 属性
        var content = "<img src=\\\"http://example.com/image.jpg\\\"/>";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsNotNull(images);
        Assert.AreEqual(1, images.Count);
        Assert.AreEqual("http://example.com/image.jpg", images[0].Url);
        Assert.AreEqual("img_test_chapter_0", images[0].Id);
    }

    [TestMethod]
    public void ParseContentWithImages_WithRealWorldUrl_ExtractsCorrectly()
    {
        // Arrange - 模拟真实的番茄小说图片 URL (JSON 解析后的格式)
        var content = "<img src=\\\"http://p3-reading-sign.fqnovelpic.com/novel-pic-r/abc123~tplv-noop.jpeg?lk3s=8d963091&amp;x-expires=1859300599&amp;x-signature=Yc5gCHn3jkXkXFdcFmD7xY0WFi4%3D\\\" img-width=\\\"602\\\" img-height=\\\"339\\\" alt=\\\"\\\" media-idx=\\\"1\\\"/>";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsNotNull(images);
        Assert.AreEqual(1, images.Count);
        Assert.IsTrue(images[0].Url.StartsWith("http://p3-reading-sign.fqnovelpic.com", StringComparison.Ordinal));
        Assert.IsTrue(images[0].Url.Contains("x-expires=1859300599", StringComparison.Ordinal));
        Assert.IsTrue(images[0].Url.Contains("&x-signature=", StringComparison.Ordinal)); // &amp; 应该被解码为 &
    }

    [TestMethod]
    public void ParseContentWithImages_WithStandardHtmlFormat_ExtractsImageUrl()
    {
        // Arrange - 标准 HTML 格式（后备 API 返回的格式）
        var content = "文字内容<img src=\"http://example.com/image.jpg\" img-width=\"602\" img-height=\"339\" alt=\"\" media-idx=\"1\"/>更多文字";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsNotNull(images);
        Assert.AreEqual(1, images.Count);
        Assert.AreEqual("http://example.com/image.jpg", images[0].Url);
        Assert.AreEqual("img_test_chapter_0", images[0].Id);
        Assert.IsFalse(cleanedText.Contains("<img", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void ParseContentWithImages_WithFallbackApiFormat_ExtractsCorrectly()
    {
        // Arrange - 模拟后备 API 返回的真实格式
        var content = """<p idx="76" p_idx="30000">注释文字</p><div data-fanqie-type="image"><p class="picture"><img src="http://p3-reading-sign.fqnovelpic.com/novel-pic-r/6b2820e0f11809dab6da8d6b6c0f71d8~tplv-noop.jpeg?lk3s=8d963091&amp;x-expires=1859337975&amp;x-signature=8VQfzWKDYhwhRkHNp2gyeEX7R%2Fg%3D" img-width="444" img-height="445" alt="" p_idx="20000" e_idx="0" e_order="77" media-idx="1"/></p></div>""";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseContentWithImages(content, "test_chapter");

        // Assert
        Assert.IsNotNull(images);
        Assert.AreEqual(1, images.Count);
        Assert.IsTrue(images[0].Url.StartsWith("http://p3-reading-sign.fqnovelpic.com", StringComparison.Ordinal));
        Assert.AreEqual("img_test_chapter_0", images[0].Id);
    }
}
