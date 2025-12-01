// Copyright (c) Reader Copilot. All rights reserved.

namespace FeedParser.Test.Models;

/// <summary>
/// FeedContent 模型测试.
/// </summary>
[TestClass]
public sealed class FeedContentTests
{
    [TestMethod]
    public void FeedContent_Constructor_ShouldSetProperties()
    {
        // Arrange & Act
        var content = new FeedContent(
            "title",
            "http://example.com/ns",
            "测试值",
            [new FeedAttribute("attr", "value")],
            [new FeedContent("child", null, "子元素", null, null)]);

        // Assert
        Assert.AreEqual("title", content.Name);
        Assert.AreEqual("http://example.com/ns", content.Namespace);
        Assert.AreEqual("测试值", content.Value);
        Assert.IsNotNull(content.Attributes);
        Assert.AreEqual(1, content.Attributes.Count);
        Assert.IsNotNull(content.Children);
        Assert.AreEqual(1, content.Children.Count);
    }

    [TestMethod]
    public void FeedContent_GetAttributeValue_ExistingAttribute_ShouldReturnValue()
    {
        // Arrange
        var content = new FeedContent(
            "element",
            null,
            null,
            [
                new FeedAttribute("href", "https://example.com"),
                new FeedAttribute("type", "text/html"),
            ],
            null);

        // Act
        var href = content.GetAttributeValue("href");
        var type = content.GetAttributeValue("type");

        // Assert
        Assert.AreEqual("https://example.com", href);
        Assert.AreEqual("text/html", type);
    }

    [TestMethod]
    public void FeedContent_GetAttributeValue_NonExistingAttribute_ShouldReturnNull()
    {
        // Arrange
        var content = new FeedContent("element", null, null, null, null);

        // Act
        var result = content.GetAttributeValue("nonexistent");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FeedContent_GetAttributeValue_NullAttributes_ShouldReturnNull()
    {
        // Arrange
        var content = new FeedContent("element", null, "value", null, null);

        // Act
        var result = content.GetAttributeValue("any");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FeedContent_GetChild_ExistingChild_ShouldReturnChild()
    {
        // Arrange
        var content = new FeedContent(
            "parent",
            null,
            null,
            null,
            [
                new FeedContent("title", null, "标题", null, null),
                new FeedContent("link", null, "https://example.com", null, null),
            ]);

        // Act
        var title = content.GetChild("title");
        var link = content.GetChild("link");

        // Assert
        Assert.IsNotNull(title);
        Assert.AreEqual("标题", title.Value);
        Assert.IsNotNull(link);
        Assert.AreEqual("https://example.com", link.Value);
    }

    [TestMethod]
    public void FeedContent_GetChild_NonExistingChild_ShouldReturnNull()
    {
        // Arrange
        var content = new FeedContent("parent", null, null, null, null);

        // Act
        var result = content.GetChild("nonexistent");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FeedContent_GetChild_NullChildren_ShouldReturnNull()
    {
        // Arrange
        var content = new FeedContent("item", null, null, null, null);

        // Act
        var result = content.GetChild("missing");

        // Assert
        Assert.IsNull(result);
    }
}

