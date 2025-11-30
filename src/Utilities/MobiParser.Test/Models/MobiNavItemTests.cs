// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Models;

/// <summary>
/// MobiNavItem 测试。
/// </summary>
[TestClass]
public sealed class MobiNavItemTests
{
    /// <summary>
    /// 测试默认值。
    /// </summary>
    [TestMethod]
    public void DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var navItem = new MobiNavItem();

        // Assert
        Assert.AreEqual(string.Empty, navItem.Title);
        Assert.AreEqual(0, navItem.Position);
        Assert.IsNull(navItem.Anchor);
        Assert.IsNotNull(navItem.Children);
        Assert.AreEqual(0, navItem.Children.Count);
        Assert.IsFalse(navItem.HasChildren);
    }

    /// <summary>
    /// 测试 HasChildren 属性。
    /// </summary>
    [TestMethod]
    public void HasChildren_WithChildren_ShouldReturnTrue()
    {
        // Arrange
        var navItem = new MobiNavItem
        {
            Title = "父项",
            Children =
            [
                new MobiNavItem { Title = "子项" }
            ],
        };

        // Assert
        Assert.IsTrue(navItem.HasChildren);
    }

    /// <summary>
    /// 测试 ToString 方法。
    /// </summary>
    [TestMethod]
    public void ToString_ShouldReturnTitle()
    {
        // Arrange
        var navItem = new MobiNavItem { Title = "第一章" };

        // Act
        var result = navItem.ToString();

        // Assert
        Assert.AreEqual("第一章", result);
    }
}
