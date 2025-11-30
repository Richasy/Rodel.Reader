// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Internal;

/// <summary>
/// NavigationExtractor 测试。
/// </summary>
[TestClass]
public sealed class NavigationExtractorTests
{
    /// <summary>
    /// 测试从简单 HTML 提取导航。
    /// </summary>
    [TestMethod]
    public void ExtractFromHtml_SimpleLinks_ShouldExtractNavItems()
    {
        // Arrange
        var html = """
            <html>
            <body>
                <ul>
                    <li><a href="chapter1.html">第一章</a></li>
                    <li><a href="chapter2.html">第二章</a></li>
                    <li><a href="chapter3.html">第三章</a></li>
                </ul>
            </body>
            </html>
            """;

        // Act
        var navItems = NavigationExtractor.ExtractFromHtml(html);

        // Assert
        Assert.IsTrue(navItems.Count >= 3);
        Assert.IsTrue(navItems.Any(n => n.Title == "第一章"));
        Assert.IsTrue(navItems.Any(n => n.Title == "第二章"));
        Assert.IsTrue(navItems.Any(n => n.Title == "第三章"));
    }

    /// <summary>
    /// 测试从带 filepos 的链接提取导航。
    /// </summary>
    [TestMethod]
    public void ExtractFromHtml_WithFilepos_ShouldExtractPosition()
    {
        // Arrange
        var html = """
            <html>
            <body>
                <a href="#" filepos="12345">章节一</a>
                <a href="#" filepos="67890">章节二</a>
            </body>
            </html>
            """;

        // Act
        var navItems = NavigationExtractor.ExtractFromHtml(html);

        // Assert
        Assert.AreEqual(2, navItems.Count);
        Assert.AreEqual("章节一", navItems[0].Title);
        Assert.AreEqual(12345, navItems[0].Position);
        Assert.AreEqual("章节二", navItems[1].Title);
        Assert.AreEqual(67890, navItems[1].Position);
    }

    /// <summary>
    /// 测试从带锚点的链接提取导航。
    /// </summary>
    [TestMethod]
    public void ExtractFromHtml_WithAnchor_ShouldExtractAnchor()
    {
        // Arrange
        var html = """
            <a href="chapter1.html#section1">第一节</a>
            """;

        // Act
        var navItems = NavigationExtractor.ExtractFromHtml(html);

        // Assert
        Assert.AreEqual(1, navItems.Count);
        Assert.AreEqual("第一节", navItems[0].Title);
        Assert.AreEqual("section1", navItems[0].Anchor);
    }

    /// <summary>
    /// 测试空 HTML。
    /// </summary>
    [TestMethod]
    public void ExtractFromHtml_EmptyHtml_ShouldReturnEmptyList()
    {
        // Act
        var navItems = NavigationExtractor.ExtractFromHtml(string.Empty);

        // Assert
        Assert.AreEqual(0, navItems.Count);
    }

    /// <summary>
    /// 测试 null HTML。
    /// </summary>
    [TestMethod]
    public void ExtractFromHtml_NullHtml_ShouldReturnEmptyList()
    {
        // Act
        var navItems = NavigationExtractor.ExtractFromHtml(null!);

        // Assert
        Assert.AreEqual(0, navItems.Count);
    }

    /// <summary>
    /// 测试 HTML 实体解码。
    /// </summary>
    [TestMethod]
    public void ExtractFromHtml_WithHtmlEntities_ShouldDecodeEntities()
    {
        // Arrange
        var html = """
            <a href="#">Tom &amp; Jerry</a>
            <a href="#">&lt;Special&gt;</a>
            """;

        // Act
        var navItems = NavigationExtractor.ExtractFromHtml(html);

        // Assert
        Assert.AreEqual(2, navItems.Count);
        Assert.AreEqual("Tom & Jerry", navItems[0].Title);
        Assert.AreEqual("<Special>", navItems[1].Title);
    }

    /// <summary>
    /// 测试从 NCX 提取导航。
    /// </summary>
    [TestMethod]
    public void ExtractFromNcx_ValidNcx_ShouldExtractNavItems()
    {
        // Arrange
        var ncx = """
            <?xml version="1.0" encoding="UTF-8"?>
            <ncx>
                <navMap>
                    <navPoint id="navpoint-1">
                        <navLabel><text>第一章</text></navLabel>
                        <content src="chapter1.html"/>
                    </navPoint>
                    <navPoint id="navpoint-2">
                        <navLabel><text>第二章</text></navLabel>
                        <content src="chapter2.html#section"/>
                    </navPoint>
                </navMap>
            </ncx>
            """;

        // Act
        var navItems = NavigationExtractor.ExtractFromNcx(ncx);

        // Assert
        Assert.AreEqual(2, navItems.Count);
        Assert.AreEqual("第一章", navItems[0].Title);
        Assert.AreEqual("第二章", navItems[1].Title);
        Assert.AreEqual("section", navItems[1].Anchor);
    }

    /// <summary>
    /// 测试从带 filepos 的 NCX 提取导航。
    /// </summary>
    [TestMethod]
    public void ExtractFromNcx_WithFilepos_ShouldExtractPosition()
    {
        // Arrange
        var ncx = """
            <ncx>
                <navMap>
                    <navPoint id="navpoint-1" filepos="1000">
                        <navLabel><text>章节</text></navLabel>
                        <content src="chapter.html"/>
                    </navPoint>
                </navMap>
            </ncx>
            """;

        // Act
        var navItems = NavigationExtractor.ExtractFromNcx(ncx);

        // Assert
        Assert.AreEqual(1, navItems.Count);
        Assert.AreEqual(1000, navItems[0].Position);
    }

    /// <summary>
    /// 测试空 NCX。
    /// </summary>
    [TestMethod]
    public void ExtractFromNcx_EmptyNcx_ShouldReturnEmptyList()
    {
        // Act
        var navItems = NavigationExtractor.ExtractFromNcx(string.Empty);

        // Assert
        Assert.AreEqual(0, navItems.Count);
    }
}
