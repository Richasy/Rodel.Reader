// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Test.UnitTests;

/// <summary>
/// 章节内容标记器测试.
/// </summary>
[TestClass]
public class ChapterContentMarkerTests
{
    [TestMethod]
    public void AddMarkers_AddsFanQieAttributes()
    {
        // Arrange
        var html = "<p>第一段内容</p><p>第二段内容</p>";
        var chapterId = "12345678";

        // Act
        var result = ChapterContentMarker.AddMarkers(html, chapterId);

        // Assert
        Assert.IsTrue(result.Contains("data-fanqie-index=\"0\"", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("data-fanqie-index=\"1\"", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains($"data-fanqie-chapter-id=\"{chapterId}\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void AddMarkers_PreservesExistingMarkers()
    {
        // Arrange
        var html = "<p data-fanqie-index=\"0\" data-fanqie-chapter-id=\"111\">已标记段落</p><p>未标记段落</p>";
        var chapterId = "222";

        // Act
        var result = ChapterContentMarker.AddMarkers(html, chapterId);

        // Assert
        // 已有标记的段落不应被重复标记
        Assert.IsTrue(result.Contains("data-fanqie-chapter-id=\"111\"", StringComparison.Ordinal));
        // 新段落应该使用新的 chapterId
        Assert.IsTrue(result.Contains($"data-fanqie-chapter-id=\"{chapterId}\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void AddMarkers_HandlesEmptyContent()
    {
        // Arrange
        var html = "";
        var chapterId = "12345678";

        // Act
        var result = ChapterContentMarker.AddMarkers(html, chapterId);

        // Assert
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void AddMarkers_HandlesParagraphsWithAttributes()
    {
        // Arrange
        var html = "<p class=\"content\">带属性的段落</p>";
        var chapterId = "12345678";

        // Act
        var result = ChapterContentMarker.AddMarkers(html, chapterId);

        // Assert
        Assert.IsTrue(result.Contains("data-fanqie-index=\"0\"", StringComparison.Ordinal));
        Assert.IsTrue(result.Contains("class=\"content\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ExtractChapterId_FromMetaTag()
    {
        // Arrange
        var html = """
            <html>
            <head>
                <meta name="fanqie:chapter-id" content="7046844484302144036"/>
            </head>
            <body></body>
            </html>
            """;

        // Act
        var chapterId = ChapterContentMarker.ExtractChapterId(html);

        // Assert
        Assert.AreEqual("7046844484302144036", chapterId);
    }

    [TestMethod]
    public void ExtractChapterId_FromParagraphAttribute()
    {
        // Arrange
        var html = """
            <p data-fanqie-index="0" data-fanqie-chapter-id="7046844484302144036">内容</p>
            """;

        // Act
        var chapterId = ChapterContentMarker.ExtractChapterId(html);

        // Assert
        Assert.AreEqual("7046844484302144036", chapterId);
    }

    [TestMethod]
    public void ExtractChapterId_ReturnsNull_WhenNoMarker()
    {
        // Arrange
        var html = "<p>普通内容</p>";

        // Act
        var chapterId = ChapterContentMarker.ExtractChapterId(html);

        // Assert
        Assert.IsNull(chapterId);
    }

    [TestMethod]
    public void ExtractStatus_ReturnsDownloaded_WhenHasParagraphMarker()
    {
        // Arrange
        var html = "<p data-fanqie-chapter-id=\"123\">内容</p>";

        // Act
        var status = ChapterContentMarker.ExtractStatus(html);

        // Assert
        Assert.AreEqual(ChapterStatus.Downloaded, status);
    }

    [TestMethod]
    public void ExtractStatus_ReturnsFailed_WhenHasUnavailableClass()
    {
        // Arrange
        var html = "<div class=\"chapter-unavailable\">章节不可用</div>";

        // Act
        var status = ChapterContentMarker.ExtractStatus(html);

        // Assert
        Assert.AreEqual(ChapterStatus.Failed, status);
    }

    [TestMethod]
    public void ExtractStatus_ReturnsFailed_WhenMetaStatusFailed()
    {
        // Arrange
        var html = """
            <meta name="fanqie:status" content="failed"/>
            """;

        // Act
        var status = ChapterContentMarker.ExtractStatus(html);

        // Assert
        Assert.AreEqual(ChapterStatus.Failed, status);
    }
}
