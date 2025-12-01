// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Models;

/// <summary>
/// Fb2NavItem 单元测试。
/// </summary>
[TestClass]
public sealed class Fb2NavItemTests
{
    [TestMethod]
    public void Fb2NavItem_DefaultValues_AreCorrect()
    {
        // Act
        var navItem = new Fb2NavItem();

        // Assert
        Assert.AreEqual(string.Empty, navItem.Title);
        Assert.IsNull(navItem.SectionId);
        Assert.AreEqual(0, navItem.Level);
        Assert.IsNotNull(navItem.Children);
        Assert.AreEqual(0, navItem.Children.Count);
        Assert.IsFalse(navItem.HasChildren);
    }

    [TestMethod]
    public void Fb2NavItem_HasChildren_WithChildren_ReturnsTrue()
    {
        // Arrange
        var navItem = new Fb2NavItem();
        navItem.Children.Add(new Fb2NavItem { Title = "Child" });

        // Act & Assert
        Assert.IsTrue(navItem.HasChildren);
    }

    [TestMethod]
    public void Fb2NavItem_ToString_ReturnsTitle()
    {
        // Arrange
        var navItem = new Fb2NavItem
        {
            Title = "Chapter 1",
        };

        // Act
        var result = navItem.ToString();

        // Assert
        Assert.AreEqual("Chapter 1", result);
    }
}
