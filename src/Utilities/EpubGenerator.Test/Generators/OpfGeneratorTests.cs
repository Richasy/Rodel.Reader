// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class OpfGeneratorTests
{
    private OpfGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new OpfGenerator();
    }

    [TestMethod]
    public void Generate_WithBasicMetadata_ShouldContainTitle()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var result = _generator.Generate(metadata, chapters, options);

        // Assert
        ContainsText(result, "<dc:title>测试书籍</dc:title>");
    }

    [TestMethod]
    public void Generate_WithBasicMetadata_ShouldContainAuthor()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "测试作者");
    }

    [TestMethod]
    public void Generate_WithEpub2_ShouldHaveVersion2()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { Version = EpubVersion.Epub2 };

        // Act
        var result = _generator.Generate(metadata, chapters, options);

        // Assert
        ContainsText(result, "version=\"2.0\"");
    }

    [TestMethod]
    public void Generate_WithEpub3_ShouldHaveVersion3()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { Version = EpubVersion.Epub3 };

        // Act
        var result = _generator.Generate(metadata, chapters, options);

        // Assert
        ContainsText(result, "version=\"3.0\"");
    }

    [TestMethod]
    public void Generate_WithCover_ShouldIncludeCoverMeta()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCover();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "cover-image");
        ContainsText(result, "Images/cover.jpg");
    }

    [TestMethod]
    public void Generate_WithChapters_ShouldIncludeChapterItems()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters(3);

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "chapter000");
        ContainsText(result, "chapter001");
        ContainsText(result, "chapter002");
    }

    [TestMethod]
    public void Generate_WithTocPage_ShouldIncludeTocPageItem()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = TestDataFactory.CreateChapters();
        var options = new EpubOptions { IncludeTocPage = true };

        // Act
        var result = _generator.Generate(metadata, chapters, options);

        // Assert
        ContainsText(result, "toc-page");
        ContainsText(result, "Text/toc.xhtml");
    }

    [TestMethod]
    public void Generate_WithCustomMetadata_ShouldIncludeCustomMeta()
    {
        // Arrange
        var metadata = TestDataFactory.CreateFullMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "generator");
        ContainsText(result, "EpubGenerator.Test");
    }

    [TestMethod]
    public void Generate_WithSubjects_ShouldIncludeSubjectElements()
    {
        // Arrange
        var metadata = TestDataFactory.CreateFullMetadata();
        var chapters = TestDataFactory.CreateChapters();

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "<dc:subject>小说</dc:subject>");
        ContainsText(result, "<dc:subject>科幻</dc:subject>");
    }

    [TestMethod]
    public void Generate_WithChapterImages_ShouldIncludeImageItems()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();
        var chapters = new List<ChapterInfo> { TestDataFactory.CreateChapterWithImages() };

        // Act
        var result = _generator.Generate(metadata, chapters);

        // Assert
        ContainsText(result, "img001");
        ContainsText(result, "Images/img001.png");
    }
}
