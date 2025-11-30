// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class CopyrightPageGeneratorTests
{
    private CopyrightPageGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new CopyrightPageGenerator();
    }

    [TestMethod]
    public void Generate_WithMetadataContainingCopyright_ShouldReturnValidXhtml()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCopyright();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        Assert.IsNotNull(result);
        ContainsText(result, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        ContainsText(result, "<html");
        ContainsText(result, "</html>");
    }

    [TestMethod]
    public void Generate_ShouldContainCopyrightInfo()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCopyright();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "© 2024 测试作者");
    }

    [TestMethod]
    public void Generate_ShouldContainIsbn()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCopyright();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "978-7-1234-5678-9");
    }

    [TestMethod]
    public void Generate_ShouldContainEdition()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCopyright();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "第一版");
    }

    [TestMethod]
    public void Generate_ShouldContainRights()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCopyright();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "保留所有权利");
    }

    [TestMethod]
    public void Generate_ShouldContainProperHtmlStructure()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCopyright();

        // Act
        var result = _generator.Generate(metadata);

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
        var metadata = TestDataFactory.CreateMetadataWithCopyright();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "stylesheet");
    }

    [TestMethod]
    public void Generate_WithFullMetadata_ShouldContainPublisher()
    {
        // Arrange
        var metadata = TestDataFactory.CreateFullMetadata();

        // Act
        var result = _generator.Generate(metadata);

        // Assert
        ContainsText(result, "测试出版社");
    }

    [TestMethod]
    public void Generate_MultipleCalls_ShouldReturnConsistentResult()
    {
        // Arrange
        var metadata = TestDataFactory.CreateMetadataWithCopyright();

        // Act
        var result1 = _generator.Generate(metadata);
        var result2 = _generator.Generate(metadata);

        // Assert
        Assert.AreEqual(result1, result2);
    }
}
