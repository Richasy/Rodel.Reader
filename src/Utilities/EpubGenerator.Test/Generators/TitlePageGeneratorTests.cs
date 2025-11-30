// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class TitlePageGeneratorTests
{
    private TitlePageGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new TitlePageGenerator();
    }

    [TestMethod]
    public void Generate_WithBasicMetadata_ShouldReturnValidXhtml()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        Assert.IsNotNull(result);
        ContainsText(result, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        ContainsText(result, "<html");
        ContainsText(result, "</html>");
    }

    [TestMethod]
    public void Generate_ShouldContainBookTitle()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata("我的书名");

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "我的书名");
    }

    [TestMethod]
    public void Generate_WithAuthor_ShouldContainAuthor()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata("测试书籍", "张三");

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "张三");
    }

    [TestMethod]
    public void Generate_WithNullAuthor_ShouldNotFail()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata("测试书籍", null);

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        Assert.IsNotNull(result);
        ContainsText(result, "测试书籍");
    }

    [TestMethod]
    public void Generate_ShouldContainProperHtmlStructure()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "<head>");
        ContainsText(result, "</head>");
        ContainsText(result, "<body");
        ContainsText(result, "</body>");
    }

    [TestMethod]
    public void Generate_ShouldContainTitlePageClass()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "titlepage");
    }

    [TestMethod]
    public void Generate_ShouldContainStylesheetReference()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "stylesheet");
    }

    [TestMethod]
    public void Generate_MultipleCalls_ShouldReturnConsistentResult()
    {
        // Arrange
        var metadata = TestDataFactory.CreateBasicMetadata();

        // Act
        var result1 = _generator.Generate(metadata);
        var result2 = _generator.Generate(metadata);

        // Assert
        Assert.AreEqual(result1, result2);
    }
}
