// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class CoverPageGeneratorTests
{
    private CoverPageGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new CoverPageGenerator();
    }

    [TestMethod]
    public void Generate_WithValidCover_ShouldReturnValidXhtml()
    {
        // Arrange
        var coverInfo = TestDataFactory.CreateCoverInfo();
        const string title = "测试书籍";

        // Act
        var result = _generator.Generate(coverInfo, title);

        // Assert
        Assert.IsNotNull(result);
        ContainsText(result, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        ContainsText(result, "<html");
        ContainsText(result, "</html>");
    }

    [TestMethod]
    public void Generate_ShouldContainCoverImage()
    {
        // Arrange
        var coverInfo = TestDataFactory.CreateCoverInfo();
        const string title = "测试书籍";

        // Act
        var result = _generator.Generate(coverInfo, title);

        // Assert
        ContainsText(result, "<img");
        ContainsText(result, "cover");
    }

    [TestMethod]
    public void Generate_ShouldContainTitleAsAltText()
    {
        // Arrange
        var coverInfo = TestDataFactory.CreateCoverInfo();
        const string title = "测试书籍标题";

        // Act
        var result = _generator.Generate(coverInfo, title);

        // Assert
        ContainsText(result, title);
    }

    [TestMethod]
    public void Generate_WithJpegMimeType_ShouldUseJpgExtension()
    {
        // Arrange
        var coverInfo = CoverInfo.FromBytes([0xFF, 0xD8, 0xFF], "image/jpeg");
        const string title = "测试书籍";

        // Act
        var result = _generator.Generate(coverInfo, title);

        // Assert
        ContainsText(result, "cover.jpg");
    }

    [TestMethod]
    public void Generate_WithPngMimeType_ShouldUsePngExtension()
    {
        // Arrange
        var coverInfo = CoverInfo.FromBytes([0x89, 0x50, 0x4E, 0x47], "image/png");
        const string title = "测试书籍";

        // Act
        var result = _generator.Generate(coverInfo, title);

        // Assert
        ContainsText(result, "cover.png");
    }

    [TestMethod]
    public void Generate_ShouldContainProperHtmlStructure()
    {
        // Arrange
        var coverInfo = TestDataFactory.CreateCoverInfo();
        const string title = "测试书籍";

        // Act
        var result = _generator.Generate(coverInfo, title);

        // Assert
        ContainsText(result, "<head>");
        ContainsText(result, "</head>");
        ContainsText(result, "<body>");
        ContainsText(result, "</body>");
    }

    [TestMethod]
    public void Generate_ShouldContainStyles()
    {
        // Arrange
        var coverInfo = TestDataFactory.CreateCoverInfo();
        const string title = "测试书籍";

        // Act
        var result = _generator.Generate(coverInfo, title);

        // Assert
        // 封面页使用内联样式
        ContainsText(result, "<style");
    }
}
