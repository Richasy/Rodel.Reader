// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Models;

/// <summary>
/// Fb2Section 单元测试。
/// </summary>
[TestClass]
public sealed class Fb2SectionTests
{
    [TestMethod]
    public void Fb2Section_DefaultValues_AreCorrect()
    {
        // Act
        var section = new Fb2Section();

        // Assert
        Assert.IsNull(section.Id);
        Assert.IsNull(section.Title);
        Assert.AreEqual(string.Empty, section.Content);
        Assert.AreEqual(string.Empty, section.PlainText);
        Assert.AreEqual(0, section.Level);
        Assert.IsNotNull(section.Children);
        Assert.AreEqual(0, section.Children.Count);
        Assert.IsFalse(section.HasChildren);
        Assert.IsNotNull(section.ImageIds);
        Assert.AreEqual(0, section.ImageIds.Count);
    }

    [TestMethod]
    public void Fb2Section_HasChildren_WithChildren_ReturnsTrue()
    {
        // Arrange
        var section = new Fb2Section();
        section.Children.Add(new Fb2Section { Title = "Child" });

        // Act & Assert
        Assert.IsTrue(section.HasChildren);
    }

    [TestMethod]
    public void Fb2Section_ToString_WithTitle_ReturnsTitle()
    {
        // Arrange
        var section = new Fb2Section
        {
            Title = "Chapter 1",
            Id = "ch1",
        };

        // Act
        var result = section.ToString();

        // Assert
        Assert.AreEqual("Chapter 1", result);
    }

    [TestMethod]
    public void Fb2Section_ToString_WithoutTitle_ReturnsSectionId()
    {
        // Arrange
        var section = new Fb2Section
        {
            Id = "ch1",
        };

        // Act
        var result = section.ToString();

        // Assert
        Assert.AreEqual("Section ch1", result);
    }
}
