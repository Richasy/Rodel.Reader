// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class TocPageGeneratorTests
{
    private TocPageGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new TocPageGenerator();
    }

    [TestMethod]
    public void Generate_WithChapters_ShouldReturnValidXhtml()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(3);

        // Act
        var result = _generator.Generate(chapters);

        // Assert
        Assert.IsNotNull(result);
        ContainsText(result, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        ContainsText(result, "<html");
        ContainsText(result, "</html>");
    }

    [TestMethod]
    public void Generate_ShouldContainDefaultTocTitle()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(2);

        // Act
        var result = _generator.Generate(chapters);

        // Assert
        ContainsText(result, "目录");
    }

    [TestMethod]
    public void Generate_WithCustomTitle_ShouldContainCustomTitle()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(2);
        const string customTitle = "章节列表";

        // Act
        var result = _generator.Generate(chapters, customTitle);

        // Assert
        ContainsText(result, customTitle);
    }

    [TestMethod]
    public void Generate_ShouldContainAllChapterTitles()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(3);

        // Act
        var result = _generator.Generate(chapters);

        // Assert
        ContainsText(result, "第1章 测试章节");
        ContainsText(result, "第2章 测试章节");
        ContainsText(result, "第3章 测试章节");
    }

    [TestMethod]
    public void Generate_ShouldContainChapterLinks()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(2);

        // Act
        var result = _generator.Generate(chapters);

        // Assert
        ContainsText(result, "<a");
        ContainsText(result, "href=");
    }

    [TestMethod]
    public void Generate_ShouldContainTocClass()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(1);

        // Act
        var result = _generator.Generate(chapters);

        // Assert
        ContainsText(result, "toc");
    }

    [TestMethod]
    public void Generate_WithEmptyChapters_ShouldStillReturnValidXhtml()
    {
        // Arrange
        var chapters = new List<ChapterInfo>();

        // Act
        var result = _generator.Generate(chapters);

        // Assert
        Assert.IsNotNull(result);
        ContainsText(result, "<html");
        ContainsText(result, "</html>");
    }

    [TestMethod]
    public void Generate_ShouldContainProperHtmlStructure()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(2);

        // Act
        var result = _generator.Generate(chapters);

        // Assert
        ContainsText(result, "<head>");
        ContainsText(result, "</head>");
        ContainsText(result, "<body");
        ContainsText(result, "</body>");
    }

    [TestMethod]
    public void Generate_ShouldContainStylesheetReference()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(1);

        // Act
        var result = _generator.Generate(chapters);

        // Assert
        ContainsText(result, "stylesheet");
    }

    [TestMethod]
    public void Generate_MultipleCalls_ShouldReturnConsistentResult()
    {
        // Arrange
        var chapters = TestDataFactory.CreateChapters(2);

        // Act
        var result1 = _generator.Generate(chapters);
        var result2 = _generator.Generate(chapters);

        // Assert
        Assert.AreEqual(result1, result2);
    }
}
