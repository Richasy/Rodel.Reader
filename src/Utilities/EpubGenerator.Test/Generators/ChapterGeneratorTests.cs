// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class ChapterGeneratorTests
{
    private ChapterGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new ChapterGenerator();
    }

    [TestMethod]
    public void Generate_WithPlainText_ShouldReturnValidXhtml()
    {
        // Arrange
        var chapter = new ChapterInfo
        {
            Index = 0,
            Title = "测试章节",
            Content = "这是测试内容。",
            IsHtml = false,
        };

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        ContainsText(result, "<?xml version=\"1.0\"");
        ContainsText(result, "<!DOCTYPE html>");
        ContainsText(result, "xmlns=\"http://www.w3.org/1999/xhtml\"");
    }

    [TestMethod]
    public void Generate_WithPlainText_ShouldContainTitle()
    {
        // Arrange
        var chapter = new ChapterInfo
        {
            Index = 0,
            Title = "测试章节标题",
            Content = "内容",
            IsHtml = false,
        };

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        ContainsText(result, "<title>测试章节标题</title>");
        ContainsText(result, "<h1 class=\"chapter-title\">测试章节标题</h1>");
    }

    [TestMethod]
    public void Generate_WithPlainText_ShouldConvertToParagraphs()
    {
        // Arrange
        var chapter = new ChapterInfo
        {
            Index = 0,
            Title = "章节",
            Content = "第一段。\n\n第二段。\n\n第三段。",
            IsHtml = false,
        };

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        ContainsText(result, "<p>第一段。</p>");
        ContainsText(result, "<p>第二段。</p>");
        ContainsText(result, "<p>第三段。</p>");
    }

    [TestMethod]
    public void Generate_WithHtmlContent_ShouldPreserveHtml()
    {
        // Arrange
        var chapter = TestDataFactory.CreateHtmlChapter();

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        ContainsText(result, "<p>这是一个<strong>HTML</strong>段落。</p>");
    }

    [TestMethod]
    public void Generate_WithImages_ShouldIncludeImageTags()
    {
        // Arrange
        var chapter = TestDataFactory.CreateChapterWithImages();

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        ContainsText(result, "<img src=\"../Images/img001.png\"");
        ContainsText(result, "<img src=\"../Images/img002.png\"");
        ContainsText(result, "alt=\"测试图片1\"");
    }

    [TestMethod]
    public void Generate_WithImages_ShouldWrapInContainer()
    {
        // Arrange
        var chapter = TestDataFactory.CreateChapterWithImages();

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        ContainsText(result, "class=\"image-container\"");
    }

    [TestMethod]
    public void Generate_ShouldLinkToStylesheet()
    {
        // Arrange
        var chapter = new ChapterInfo
        {
            Index = 0,
            Title = "章节",
            Content = "内容",
            IsHtml = false,
        };

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        ContainsText(result, "href=\"../Styles/main.css\"");
    }

    [TestMethod]
    public void Generate_WithSpecialCharacters_ShouldEscapeXml()
    {
        // Arrange
        var chapter = new ChapterInfo
        {
            Index = 0,
            Title = "章节 <测试> & \"引用\"",
            Content = "内容包含 <标签> & 符号",
            IsHtml = false,
        };

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        ContainsText(result, "&lt;测试&gt;");
        ContainsText(result, "&amp;");
    }

    [TestMethod]
    public void Generate_WithEmptyContent_ShouldReturnValidXhtml()
    {
        // Arrange
        var chapter = new ChapterInfo
        {
            Index = 0,
            Title = "空章节",
            Content = string.Empty,
            IsHtml = false,
        };

        // Act
        var result = _generator.Generate(chapter);

        // Assert
        Assert.IsNotNull(result);
        ContainsText(result, "<h1 class=\"chapter-title\">空章节</h1>");
    }
}
