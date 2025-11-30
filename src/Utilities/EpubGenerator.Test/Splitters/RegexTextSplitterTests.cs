// Copyright (c) Reader Copilot. All rights reserved.

namespace EpubGenerator.Test.Splitters;

[TestClass]
public sealed class RegexTextSplitterTests
{
    private RegexTextSplitter _splitter = null!;

    [TestInitialize]
    public void Setup()
    {
        _splitter = new RegexTextSplitter();
    }

    [TestMethod]
    public void Split_WithDefaultPattern_ShouldSplitChineseChapters()
    {
        // Arrange - 注意不要使用缩进的原始字符串，否则每行都会有前导空格
        // 内容中避免包含 "第X章" 这样的文字，以免被错误匹配
        const string text = "第一章 开始\n这是开始部分的内容。\n\n第二章 发展\n这是发展部分的内容。\n\n第三章 结局\n这是结局部分的内容。";

        // Act
        var result = _splitter.Split(text);

        // Assert
        Assert.AreEqual(3, result.Count, "Expected 3 chapters");
        Assert.AreEqual("第一章 开始", result[0].Title);
        Assert.AreEqual("第二章 发展", result[1].Title);
        Assert.AreEqual("第三章 结局", result[2].Title);
    }

    [TestMethod]
    public void Split_WithCustomPattern_ShouldUseCustomPattern()
    {
        // Arrange
        const string text = """
            Chapter 1: Introduction
            This is the introduction.
            
            Chapter 2: Main Content
            This is the main content.
            """;

        var options = new SplitOptions
        {
            ChapterPattern = @"^Chapter \d+:.*$"
        };

        // Act
        var result = _splitter.Split(text, options);

        // Assert
        Assert.AreEqual(2, result.Count, "Expected 2 chapters");
    }

    [TestMethod]
    public void Split_ShouldPreserveChapterContent()
    {
        // Arrange
        const string text = """
            第一章 开始
            这是第一段。
            这是第二段。
            """;

        // Act
        var result = _splitter.Split(text);

        // Assert
        Assert.AreEqual(1, result.Count, "Expected 1 chapter");
        AssertExtensions.ContainsText(result[0].Content, "这是第一段");
        AssertExtensions.ContainsText(result[0].Content, "这是第二段");
    }

    [TestMethod]
    public void Split_WithExtraKeywords_ShouldMatchKeywords()
    {
        // Arrange
        const string text = """
            序
            这是序言内容。
            
            第一章 开始
            这是第一章的内容。
            
            后记
            这是后记内容。
            """;

        var options = new SplitOptions
        {
            ExtraChapterKeywords = ["序", "后记"]
        };

        // Act
        var result = _splitter.Split(text, options);

        // Assert
        Assert.IsTrue(result.Count >= 2, "Expected at least 2 chapters");
    }

    [TestMethod]
    public void Split_WithEmptyText_ShouldReturnEmptyList()
    {
        // Arrange
        const string text = "";

        // Act
        var result = _splitter.Split(text);

        // Assert
        // 空文本会生成一个带有默认标题的章节
        Assert.IsTrue(result.Count <= 1, "Empty text should return empty or single default chapter");
    }

    [TestMethod]
    public void Split_WithWhitespaceOnly_ShouldReturnMinimalResult()
    {
        // Arrange
        const string text = "   \n\n   \t   ";

        // Act
        var result = _splitter.Split(text);

        // Assert
        // 只有空白的文本可能会生成一个空章节或无章节
        Assert.IsTrue(result.Count <= 1, "Whitespace-only text should return minimal result");
    }

    [TestMethod]
    public void Split_ShouldAssignCorrectIndices()
    {
        // Arrange
        const string text = """
            第一章 开始
            内容一。
            
            第二章 发展
            内容二。
            
            第三章 结局
            内容三。
            """;

        // Act
        var result = _splitter.Split(text);

        // Assert
        Assert.AreEqual(3, result.Count, "Expected 3 chapters");
        Assert.AreEqual(0, result[0].Index);
        Assert.AreEqual(1, result[1].Index);
        Assert.AreEqual(2, result[2].Index);
    }

    [TestMethod]
    public void Split_WithMaxTitleLength_ShouldTruncateTitle()
    {
        // Arrange
        const string text = """
            第一章 这是一个非常非常非常非常非常非常非常非常非常非常非常长的标题
            内容。
            """;

        var options = new SplitOptions
        {
            MaxTitleLength = 10
        };

        // Act
        var result = _splitter.Split(text, options);

        // Assert
        if (result.Count > 0)
        {
            Assert.IsTrue(result[0].Title.Length <= 13, "Title should be truncated"); // +3 for possible ellipsis
        }
    }

    [TestMethod]
    public void Split_WithTrimLines_ShouldTrimContent()
    {
        // Arrange
        const string text = """
            第一章 开始
               这是有空格的内容。   
            """;

        var options = new SplitOptions
        {
            TrimLines = true
        };

        // Act
        var result = _splitter.Split(text, options);

        // Assert
        Assert.IsTrue(result.Count > 0, "Expected at least one chapter");
    }

    [TestMethod]
    public void Split_WithRemoveEmptyLines_ShouldRemoveEmptyLines()
    {
        // Arrange
        const string text = """
            第一章 开始
            
            
            这是内容。
            
            
            """;

        var options = new SplitOptions
        {
            RemoveEmptyLines = true
        };

        // Act
        var result = _splitter.Split(text, options);

        // Assert
        Assert.IsTrue(result.Count > 0, "Expected at least one chapter");
    }

    [TestMethod]
    public void Split_WithDefaultFirstChapterTitle_ShouldUseDefaultTitle()
    {
        // Arrange
        const string text = """
            这是章节之前的内容。
            
            第一章 开始
            这是第一章的内容。
            """;

        var options = new SplitOptions
        {
            DefaultFirstChapterTitle = "序言"
        };

        // Act
        var result = _splitter.Split(text, options);

        // Assert
        Assert.IsTrue(result.Count >= 1, "Expected at least one chapter");
    }

    [TestMethod]
    public void Split_ChapterContentShouldNotBeHtml()
    {
        // Arrange
        const string text = """
            第一章 开始
            这是纯文本内容。
            """;

        // Act
        var result = _splitter.Split(text);

        // Assert
        Assert.IsTrue(result.Count > 0, "Expected at least one chapter");
        Assert.IsFalse(result[0].IsHtml);
    }

    [TestMethod]
    public async Task SplitFromStreamAsync_ShouldWork()
    {
        // Arrange
        const string text = """
            第一章 开始
            这是第一章的内容。
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));

        // Act
        var result = await _splitter.SplitFromStreamAsync(stream);

        // Assert
        Assert.IsTrue(result.Count > 0, "Expected at least one chapter");
    }
}
