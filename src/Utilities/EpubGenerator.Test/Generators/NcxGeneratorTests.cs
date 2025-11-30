// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class NcxGeneratorTests
{
    private NcxGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new NcxGenerator();
    }

    [TestMethod]
    public void Generate_ShouldReturnValidNcxXml()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<?xml version=\"1.0\"");
        ContainsText(result, "<ncx");
        ContainsText(result, "xmlns=\"http://www.daisy.org/z3986/2005/ncx/\"");
    }

    [TestMethod]
    public void Generate_ShouldContainDocTitle()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<docTitle>");
        ContainsText(result, "<text>测试书籍</text>");
    }

    [TestMethod]
    public void Generate_ShouldContainDocAuthor()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<docAuthor>");
        ContainsText(result, "<text>测试作者</text>");
    }

    [TestMethod]
    public void Generate_ShouldContainNavMap()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<navMap>");
        ContainsText(result, "</navMap>");
    }

    [TestMethod]
    public void Generate_WithChapters_ShouldContainNavPoints()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters(3);

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<navPoint");
        ContainsText(result, "playOrder=\"1\"");
        ContainsText(result, "playOrder=\"2\"");
        ContainsText(result, "playOrder=\"3\"");
    }

    [TestMethod]
    public void Generate_WithChapters_ShouldContainChapterTitles()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters(2);

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "第1章 测试章节");
        ContainsText(result, "第2章 测试章节");
    }

    [TestMethod]
    public void Generate_WithChapters_ShouldContainContentSrc()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters(1);

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<content src=\"Text/chapter000.xhtml\"/>");
    }

    [TestMethod]
    public void Generate_ShouldContainMetaElements()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "dtb:uid");
        ContainsText(result, "dtb:depth");
    }

    [TestMethod]
    public void Generate_WithAnchors_ShouldContainNestedNavPoints()
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
    public void Generate_WithAnchors_ShouldHaveCorrectPlayOrder()
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
                    new() { Id = "anchor1", Title = "Anchor 1" }
                }
            },
            new()
            {
                Title = "Chapter 2",
                Content = "Content",
                Index = 1
            }
        };

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        // Chapter 1 -> playOrder 1, Anchor 1 -> playOrder 2, Chapter 2 -> playOrder 3
        ContainsText(result, "playOrder=\"1\"");
        ContainsText(result, "playOrder=\"2\"");
        ContainsText(result, "playOrder=\"3\"");
    }

    [TestMethod]
    public void Generate_WithMultipleChaptersAndAnchors_ShouldNestCorrectly()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = new List<ChapterInfo>
        {
            new()
            {
                Title = "Introduction",
                Content = "Intro content",
                Index = 0,
                Anchors = new List<AnchorInfo>
                {
                    new() { Id = "overview", Title = "Overview" }
                }
            },
            new()
            {
                Title = "Main Content",
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
        ContainsText(result, "Main Content");
        ContainsText(result, "Part 1");
        ContainsText(result, "Part 2");
    }
}
