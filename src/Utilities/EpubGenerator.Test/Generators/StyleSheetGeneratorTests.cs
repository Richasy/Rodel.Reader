// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class StyleSheetGeneratorTests
{
    private StyleSheetGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new StyleSheetGenerator();
    }

    [TestMethod]
    public void Generate_WithDefaultOptions_ShouldReturnValidCss()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var result = _generator.Generate(options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result), "CSS output should not be empty");
    }

    [TestMethod]
    public void Generate_ShouldContainBodyStyles()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var result = _generator.Generate(options);

        // Assert - 验证基本布局样式，不验证字体/颜色（让阅读器覆写）
        ContainsText(result, "body");
        ContainsText(result, "line-height");
    }

    [TestMethod]
    public void Generate_ShouldContainChapterStyles()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var result = _generator.Generate(options);

        // Assert
        ContainsText(result, ".chapter");
        ContainsText(result, ".chapter-title");
        ContainsText(result, ".chapter-content");
    }

    [TestMethod]
    public void Generate_ShouldContainImageStyles()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var result = _generator.Generate(options);

        // Assert
        ContainsText(result, "img");
        ContainsText(result, "max-width");
    }

    [TestMethod]
    public void Generate_ShouldContainTocStyles()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var result = _generator.Generate(options);

        // Assert
        ContainsText(result, ".toc");
    }

    [TestMethod]
    public void Generate_ShouldContainTitlePageStyles()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var result = _generator.Generate(options);

        // Assert
        ContainsText(result, ".titlepage");
        ContainsText(result, ".book-title");
    }

    [TestMethod]
    public void Generate_MultipleCalls_ShouldReturnConsistentResult()
    {
        // Arrange
        var options = TestDataFactory.CreateDefaultOptions();

        // Act
        var result1 = _generator.Generate(options);
        var result2 = _generator.Generate(options);

        // Assert
        Assert.AreEqual(result1, result2);
    }
}
