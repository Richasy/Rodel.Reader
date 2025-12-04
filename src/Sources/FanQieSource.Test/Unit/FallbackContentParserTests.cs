// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.FanQie.Helpers;

namespace Richasy.RodelReader.Sources.FanQie.Test.Unit;

/// <summary>
/// 测试 ParseFallbackHtmlContent 方法的输出.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class FallbackContentParserTests
{
    [TestMethod]
    public void ParseFallbackHtmlContent_WithImageInDiv_ExtractsImageAndCreatesPlaceholder()
    {
        // Arrange - 模拟真实的 rawContent 格式
        var rawContent = """
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html><head></head><body idx="40000">
<h1 class="chapterTitle1" id="heading_id_2" idx="10000" p_idx="40000"><blk p_idx="10000">第260章 何必送目</blk></h1>
<p idx="0" p_idx="40000"><blk p_idx="0">这是往常的一天。</blk></p>
<p idx="1" p_idx="40000"><blk p_idx="1">第二段文字。</blk></p>
<div data-fanqie-type="image" source="user" idx="30000" p_idx="40000">
<p class="picture" group-id="1" idx="20000" p_idx="30000"><img src="http://p3-reading-sign.fqnovelpic.com/test.jpeg" img-width="444" img-height="445" alt="" p_idx="20000" e_idx="0" e_order="77" media-idx="1"/></p>
<p class="pictureDesc" group-id="1" idx="76" p_idx="30000"><blk p_idx="76">图片说明文字</blk></p>
</div>
<p idx="2" p_idx="40000"><blk p_idx="2">图片后的文字。</blk></p>
</body></html>
""";
        var chapterId = "7508071600441262617";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseFallbackHtmlContent(rawContent, chapterId);

        // Assert - 检查图片是否被提取
        Assert.IsNotNull(images, "应该提取到图片");
        Assert.AreEqual(1, images.Count, "应该提取到1张图片");
        Assert.AreEqual("img_7508071600441262617_0", images[0].Id, "图片ID格式应正确");
        Assert.IsTrue(images[0].Url.Contains("p3-reading-sign.fqnovelpic.com", StringComparison.Ordinal), "图片URL应包含正确的域名");

        // Assert - 检查 HTML 内容是否包含占位符
        Console.WriteLine("=== HTML Content ===");
        Console.WriteLine(htmlContent);
        Console.WriteLine();
        
        Assert.IsTrue(htmlContent.Contains("<!-- FANQIE_IMAGE:img_7508071600441262617_0 -->", StringComparison.Ordinal), 
            "HTML 内容应包含图片占位符");

        // Assert - 检查纯文本
        Assert.IsTrue(cleanedText.Contains("这是往常的一天", StringComparison.Ordinal), "纯文本应包含正文");
        Assert.IsTrue(cleanedText.Contains("图片后的文字", StringComparison.Ordinal), "纯文本应包含图片后的正文");
    }

    [TestMethod]
    public void ParseFallbackHtmlContent_PlaceholderShouldBeAtCorrectPosition()
    {
        // Arrange - 简化的测试用例
        var rawContent = """
<p idx="0" p_idx="40000"><blk p_idx="0">第一段。</blk></p>
<div data-fanqie-type="image">
<p class="picture" p_idx="30000"><img src="http://example.com/test.jpg"/></p>
</div>
<p idx="1" p_idx="40000"><blk p_idx="1">第二段。</blk></p>
""";
        var chapterId = "test";

        // Act
        var (images, cleanedText, htmlContent) = ContentParser.ParseFallbackHtmlContent(rawContent, chapterId);

        // Assert
        Assert.IsNotNull(images);
        Assert.AreEqual(1, images.Count);
        
        // 占位符应该在 "第一段" 和 "第二段" 之间
        var placeholderIndex = htmlContent.IndexOf("<!-- FANQIE_IMAGE:", StringComparison.Ordinal);
        var firstParaIndex = htmlContent.IndexOf("第一段", StringComparison.Ordinal);
        var secondParaIndex = htmlContent.IndexOf("第二段", StringComparison.Ordinal);
        
        Console.WriteLine($"First paragraph index: {firstParaIndex}");
        Console.WriteLine($"Placeholder index: {placeholderIndex}");
        Console.WriteLine($"Second paragraph index: {secondParaIndex}");
        Console.WriteLine();
        Console.WriteLine("=== HTML Content ===");
        Console.WriteLine(htmlContent);
        
        Assert.IsTrue(placeholderIndex > firstParaIndex, "占位符应在第一段之后");
        Assert.IsTrue(placeholderIndex < secondParaIndex, "占位符应在第二段之前");
    }
}
