// Copyright (c) Reader Copilot. All rights reserved.

namespace FeedParser.Test.Exceptions;

/// <summary>
/// 异常类测试.
/// </summary>
[TestClass]
public sealed class FeedExceptionTests
{
    #region FeedParseException 测试

    [TestMethod]
    public void FeedParseException_DefaultConstructor_ShouldCreateInstance()
    {
        // Act
        var ex = new FeedParseException();

        // Assert
        Assert.IsNotNull(ex);
        Assert.IsNull(ex.ElementName);
        Assert.IsNull(ex.LineNumber);
    }

    [TestMethod]
    public void FeedParseException_WithMessage_ShouldSetMessage()
    {
        // Arrange
        const string message = "解析错误";

        // Act
        var ex = new FeedParseException(message);

        // Assert
        Assert.AreEqual(message, ex.Message);
    }

    [TestMethod]
    public void FeedParseException_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        const string message = "解析错误";
        var innerException = new InvalidOperationException("内部错误");

        // Act
        var ex = new FeedParseException(message, innerException);

        // Assert
        Assert.AreEqual(message, ex.Message);
        Assert.AreEqual(innerException, ex.InnerException);
    }

    [TestMethod]
    public void FeedParseException_ElementName_ShouldBeSettable()
    {
        // Arrange
        var ex = new FeedParseException("错误")
        {
            ElementName = "item"
        };

        // Assert
        Assert.AreEqual("item", ex.ElementName);
    }

    [TestMethod]
    public void FeedParseException_LineNumber_ShouldBeSettable()
    {
        // Arrange
        var ex = new FeedParseException("错误")
        {
            LineNumber = 42
        };

        // Assert
        Assert.AreEqual(42, ex.LineNumber);
    }

    #endregion

    #region InvalidFeedFormatException 测试

    [TestMethod]
    public void InvalidFeedFormatException_DefaultConstructor_ShouldCreateInstance()
    {
        // Act
        var ex = new InvalidFeedFormatException();

        // Assert
        Assert.IsNotNull(ex);
    }

    [TestMethod]
    public void InvalidFeedFormatException_WithMessage_ShouldSetMessage()
    {
        // Arrange
        const string message = "无效的 Feed 格式";

        // Act
        var ex = new InvalidFeedFormatException(message);

        // Assert
        Assert.AreEqual(message, ex.Message);
    }

    #endregion

    #region UnsupportedFeedFormatException 测试

    [TestMethod]
    public void UnsupportedFeedFormatException_DefaultConstructor_ShouldCreateInstance()
    {
        // Act
        var ex = new UnsupportedFeedFormatException();

        // Assert
        Assert.IsNotNull(ex);
    }

    [TestMethod]
    public void UnsupportedFeedFormatException_WithMessage_ShouldSetMessage()
    {
        // Arrange
        const string message = "不支持的 Feed 格式";

        // Act
        var ex = new UnsupportedFeedFormatException(message);

        // Assert
        Assert.AreEqual(message, ex.Message);
    }

    [TestMethod]
    public void UnsupportedFeedFormatException_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        const string message = "不支持的格式";
        var innerException = new InvalidOperationException("内部错误");

        // Act
        var ex = new UnsupportedFeedFormatException(message, innerException);

        // Assert
        Assert.AreEqual(message, ex.Message);
        Assert.AreEqual(innerException, ex.InnerException);
    }

    #endregion
}
