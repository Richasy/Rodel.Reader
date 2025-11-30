// Copyright (c) Reader Copilot. All rights reserved.

using static EpubGenerator.Test.AssertExtensions;

namespace EpubGenerator.Test.Generators;

[TestClass]
public sealed class ContainerGeneratorTests
{
    private ContainerGenerator _generator = null!;

    [TestInitialize]
    public void Setup()
    {
        _generator = new ContainerGenerator();
    }

    [TestMethod]
    public void Generate_ShouldReturnValidXml()
    {
        // Act
        var result = _generator.Generate();

        // Assert
        Assert.IsNotNull(result);
        ContainsText(result, "<?xml version=\"1.0\"");
        ContainsText(result, "<container");
        ContainsText(result, "xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\"");
    }

    [TestMethod]
    public void Generate_ShouldContainRootfileElement()
    {
        // Act
        var result = _generator.Generate();

        // Assert
        ContainsText(result, "<rootfile");
        ContainsText(result, "full-path=\"OEBPS/content.opf\"");
        ContainsText(result, "media-type=\"application/oebps-package+xml\"");
    }

    [TestMethod]
    public void Generate_ShouldContainRootfilesElement()
    {
        // Act
        var result = _generator.Generate();

        // Assert
        ContainsText(result, "<rootfiles>");
        ContainsText(result, "</rootfiles>");
    }

    [TestMethod]
    public void Generate_MultipleCalls_ShouldReturnConsistentResult()
    {
        // Act
        var result1 = _generator.Generate();
        var result2 = _generator.Generate();

        // Assert
        Assert.AreEqual(result1, result2);
    }
}
