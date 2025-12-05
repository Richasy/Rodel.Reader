// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.Legado.Internal;

namespace Richasy.RodelReader.Components.Legado.Test.UnitTests;

/// <summary>
/// 占位符生成器测试.
/// </summary>
[TestClass]
public class PlaceholderGeneratorTests
{
    [TestMethod]
    public void GenerateFailedPlaceholder_ContainsChapterInfo()
    {
        // Arrange
        var chapterIndex = 5;
        var title = "第六章 测试章节";
        var reason = "网络错误";

        // Act
        var html = PlaceholderGenerator.GenerateFailedPlaceholder(chapterIndex, title, reason);

        // Assert
        Assert.IsTrue(html.Contains(reason, StringComparison.Ordinal));
        Assert.IsTrue(html.Contains($"legado:chapter-index={chapterIndex}", StringComparison.Ordinal));
        Assert.IsTrue(html.Contains("legado:status=failed", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateFailedPlaceholder_ContainsChapterIndexMarker()
    {
        // Arrange
        var chapterIndex = 10;
        var title = "测试章节";
        var reason = "超时";

        // Act
        var html = PlaceholderGenerator.GenerateFailedPlaceholder(chapterIndex, title, reason);

        // Assert
        Assert.IsTrue(html.Contains("legado:chapter-index=10", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateVolumeContent_ContainsVolumeInfo()
    {
        // Arrange
        var chapterIndex = 0;
        var volumeTitle = "卷一 序章";

        // Act
        var html = PlaceholderGenerator.GenerateVolumeContent(chapterIndex, volumeTitle);

        // Assert
        Assert.IsTrue(html.Contains(volumeTitle, StringComparison.Ordinal));
        Assert.IsTrue(html.Contains($"legado:chapter-index={chapterIndex}", StringComparison.Ordinal));
        Assert.IsTrue(html.Contains("legado:status=volume", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateVolumeContent_ContainsChapterIndexMarker()
    {
        // Arrange
        var chapterIndex = 3;
        var volumeTitle = "卷二";

        // Act
        var html = PlaceholderGenerator.GenerateVolumeContent(chapterIndex, volumeTitle);

        // Assert
        Assert.IsTrue(html.Contains("legado:chapter-index=3", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateFailedPlaceholder_HtmlEscapesReason()
    {
        // Arrange
        var chapterIndex = 0;
        var title = "第一章";
        var reason = "<script>alert('xss')</script>";

        // Act
        var html = PlaceholderGenerator.GenerateFailedPlaceholder(chapterIndex, title, reason);

        // Assert
        Assert.IsFalse(html.Contains("<script>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateVolumeContent_HtmlEscapesTitle()
    {
        // Arrange
        var chapterIndex = 0;
        var volumeTitle = "<div onclick=\"attack()\">卷一</div>";

        // Act
        var html = PlaceholderGenerator.GenerateVolumeContent(chapterIndex, volumeTitle);

        // Assert - HTML 特殊字符应该被编码
        Assert.IsFalse(html.Contains("<div onclick", StringComparison.Ordinal)); // < 应该被编码为 &lt;
        Assert.IsTrue(html.Contains("&lt;div", StringComparison.Ordinal) || html.Contains("&gt;", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GenerateFailedPlaceholder_HandlesNullReason()
    {
        // Arrange
        var chapterIndex = 0;
        var title = "第一章";

        // Act
        var html = PlaceholderGenerator.GenerateFailedPlaceholder(chapterIndex, title, reason: null);

        // Assert
        Assert.IsTrue(html.Contains(title, StringComparison.Ordinal) || html.Contains("网络原因", StringComparison.Ordinal)); // 默认原因
    }

    [TestMethod]
    public void WrapChapterContent_AddsMetadata()
    {
        // Arrange
        var chapterIndex = 5;
        var content = "<p>这是正文内容</p>";

        // Act
        var html = PlaceholderGenerator.WrapChapterContent(chapterIndex, content);

        // Assert
        Assert.IsTrue(html.Contains($"legado:chapter-index={chapterIndex}", StringComparison.Ordinal));
        Assert.IsTrue(html.Contains("legado:status=downloaded", StringComparison.Ordinal));
        Assert.IsTrue(html.Contains(content, StringComparison.Ordinal));
    }
}
