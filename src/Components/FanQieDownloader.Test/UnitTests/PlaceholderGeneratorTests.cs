// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Test.UnitTests;

/// <summary>
/// 占位内容生成器测试.
/// </summary>
[TestClass]
public class PlaceholderGeneratorTests
{
    [TestMethod]
    public void GenerateFailedPlaceholder_ContainsChapterId()
    {
        // Arrange
        var chapterId = "12345678";
        var title = "第一章 测试";
        var order = 1;

        // Act
        var html = PlaceholderGenerator.GenerateFailedPlaceholder(chapterId, title, order);

        // Assert - 新格式使用 HTML 注释
        Assert.IsTrue(html.Contains($"fanqie:chapter-id={chapterId}", StringComparison.Ordinal));
        Assert.IsTrue(html.Contains("data-fanqie-status=\"failed\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateFailedPlaceholder_ContainsStatus()
    {
        // Arrange
        var chapterId = "12345678";
        var title = "第一章";
        var order = 1;

        // Act
        var html = PlaceholderGenerator.GenerateFailedPlaceholder(chapterId, title, order);

        // Assert - 新格式使用 HTML 注释
        Assert.IsTrue(html.Contains("fanqie:status=failed", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateFailedPlaceholder_ContainsReason()
    {
        // Arrange
        var chapterId = "12345678";
        var title = "第一章";
        var order = 1;
        var reason = "网络超时";

        // Act
        var html = PlaceholderGenerator.GenerateFailedPlaceholder(chapterId, title, order, reason);

        // Assert
        Assert.IsTrue(html.Contains("网络超时", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateLockedPlaceholder_ContainsLockedStatus()
    {
        // Arrange
        var chapterId = "12345678";
        var title = "VIP章节";
        var order = 100;

        // Act
        var html = PlaceholderGenerator.GenerateLockedPlaceholder(chapterId, title, order);

        // Assert - 新格式使用 HTML 注释和 data-* 属性
        Assert.IsTrue(html.Contains("data-fanqie-status=\"locked\"", StringComparison.Ordinal));
        Assert.IsTrue(html.Contains("fanqie:status=locked", StringComparison.Ordinal));
        Assert.IsTrue(html.Contains("付费阅读", StringComparison.Ordinal));
    }

    [TestMethod]
    public void WrapChapterContent_ContainsDownloadedStatus()
    {
        // Arrange
        var chapterId = "12345678";
        var title = "第一章";
        var order = 1;
        var body = "<p>章节内容</p>";

        // Act
        var html = PlaceholderGenerator.WrapChapterContent(chapterId, title, order, body);

        // Assert - 新格式使用 HTML 注释
        Assert.IsTrue(html.Contains("fanqie:status=downloaded", StringComparison.Ordinal));
        Assert.IsTrue(html.Contains(body, StringComparison.Ordinal));
    }

    [TestMethod]
    public void WrapChapterContent_ContainsChapterOrder()
    {
        // Arrange
        var chapterId = "12345678";
        var title = "第一章";
        var order = 42;
        var body = "<p>内容</p>";

        // Act
        var html = PlaceholderGenerator.WrapChapterContent(chapterId, title, order, body);

        // Assert - 新格式使用 HTML 注释
        Assert.IsTrue(html.Contains("fanqie:chapter-order=42", StringComparison.Ordinal));
    }

    [TestMethod]
    public void AllPlaceholders_AreHtmlFragments()
    {
        // Arrange & Act
        var failed = PlaceholderGenerator.GenerateFailedPlaceholder("1", "Test", 1);
        var locked = PlaceholderGenerator.GenerateLockedPlaceholder("1", "Test", 1);
        var wrapped = PlaceholderGenerator.WrapChapterContent("1", "Test", 1, "<p>Content</p>");

        // Assert - 新格式不包含完整的 XHTML 结构，只是 body 内部的片段
        foreach (var html in new[] { failed, locked, wrapped })
        {
            // 不应该包含完整 XHTML 结构
            Assert.IsFalse(html.Contains("<?xml version", StringComparison.Ordinal));
            Assert.IsFalse(html.Contains("<!DOCTYPE", StringComparison.Ordinal));
            Assert.IsFalse(html.Contains("<html", StringComparison.Ordinal));
            Assert.IsFalse(html.Contains("</html>", StringComparison.Ordinal));

            // 应该包含元数据注释
            Assert.IsTrue(html.Contains("fanqie:chapter-id=", StringComparison.Ordinal));
            Assert.IsTrue(html.Contains("fanqie:chapter-order=", StringComparison.Ordinal));
            Assert.IsTrue(html.Contains("fanqie:status=", StringComparison.Ordinal));
        }
    }
}
