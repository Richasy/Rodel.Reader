// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class NavDocGeneratorTests
{
    private NavDocGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new NavDocGenerator();
    }

    [TestMethod]
    public void Generate_ShouldReturnValidXhtml()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<?xml version=\"1.0\"");
        ContainsText(result, "<!DOCTYPE html>");
        ContainsText(result, "xmlns=\"http://www.w3.org/1999/xhtml\"");
    }

    [TestMethod]
    public void Generate_ShouldContainEpubNamespace()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "xmlns:epub=\"http://www.idpf.org/2007/ops\"");
    }

    [TestMethod]
    public void Generate_ShouldContainNavElement()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<nav epub:type=\"toc\"");
        ContainsText(result, "</nav>");
    }

    [TestMethod]
    public void Generate_ShouldContainTocHeading()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<h1>目录</h1>");
    }

    [TestMethod]
    public void Generate_WithChapters_ShouldContainChapterLinks()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters(2);

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<a href=\"Text/chapter000.xhtml\">");
        ContainsText(result, "<a href=\"Text/chapter001.xhtml\">");
        ContainsText(result, "第1章 测试章节");
        ContainsText(result, "第2章 测试章节");
    }

    [TestMethod]
    public void Generate_ShouldContainOrderedList()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<ol>");
        ContainsText(result, "</ol>");
        ContainsText(result, "<li>");
    }

    [TestMethod]
    public void Generate_ShouldUseCorrectLanguage()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "xml:lang=\"zh\"");
    }

    [TestMethod]
    public void Generate_WithAnchors_ShouldContainNestedList()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content with sections",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "section1", Title = "Section 1.1" },
                    new() { Id = "section2", Title = "Section 1.2" }
                }
            }
        };

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "Section 1.1");
        ContainsText(result, "Section 1.2");
        ContainsText(result, "#section1");
        ContainsText(result, "#section2");
    }

    [TestMethod]
    public void Generate_WithAnchors_ShouldHaveCorrectHrefFormat()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "my-anchor", Title = "My Section" }
                }
            }
        };

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "href=\"Text/chapter000.xhtml#my-anchor\"");
    }

    [TestMethod]
    public void Generate_WithMultipleChaptersAndAnchors_ShouldContainAllLinks()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Introduction",
                Content = "Intro",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "overview", Title = "Overview" }
                }
            },
            new()
            {
                Title = "Main",
                Content = "Main content",
                Index = 1,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "part1", Title = "Part 1" },
                    new() { Id = "part2", Title = "Part 2" }
                }
            }
        };

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "Introduction");
        ContainsText(result, "Overview");
        ContainsText(result, "Main");
        ContainsText(result, "Part 1");
        ContainsText(result, "Part 2");
        ContainsText(result, "chapter000.xhtml#overview");
        ContainsText(result, "chapter001.xhtml#part1");
        ContainsText(result, "chapter001.xhtml#part2");
    }

    [TestMethod]
    public void Generate_WithEmptyAnchors_ShouldNotIncludeNestedList()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Chapter 1",
                Content = "Content",
                Index = 0,
                Anchors = new List<AnchorInfo>() // Empty list
            }
        };

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "Chapter 1");
        // 应该只有一个 ol (顶层)，不应该有嵌套的 ol
        var olCount = System.Text.RegularExpressions.Regex.Matches(result, "<ol>").Count;
        Assert.AreEqual(1, olCount, "Should only have one ol element when no anchors");
    }
}
