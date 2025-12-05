// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions.Test.Unit;

/// <summary>
/// 异常类单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ExceptionTests
{
    [TestMethod]
    public void RssClientException_WithMessage_SetsMessageCorrectly()
    {
        // Arrange
        const string message = "Test error message";

        // Act
        var exception = new RssClientException(message);

        // Assert
        Assert.AreEqual(message, exception.Message);
    }

    [TestMethod]
    public void RssClientException_WithInnerException_SetsInnerExceptionCorrectly()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        const string message = "Outer error";

        // Act
        var exception = new RssClientException(message, innerException);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreSame(innerException, exception.InnerException);
    }

    [TestMethod]
    public void RssAuthenticationException_DefaultMessage_IsCorrect()
    {
        // Arrange & Act
        var exception = new RssAuthenticationException();

        // Assert
        Assert.AreEqual("Authentication failed.", exception.Message);
    }

    [TestMethod]
    public void RssNetworkException_DefaultMessage_IsCorrect()
    {
        // Arrange & Act
        var exception = new RssNetworkException();

        // Assert
        Assert.AreEqual("Network error occurred.", exception.Message);
    }

    [TestMethod]
    public void RssFeedParseException_DefaultMessage_IsCorrect()
    {
        // Arrange & Act
        var exception = new RssFeedParseException();

        // Assert
        Assert.AreEqual("Failed to parse RSS feed.", exception.Message);
    }

    [TestMethod]
    public void RssAuthenticationException_IsRssClientException()
    {
        // Arrange & Act
        var exception = new RssAuthenticationException();

        // Assert
        Assert.IsInstanceOfType<RssClientException>(exception);
    }

    [TestMethod]
    public void RssNetworkException_IsRssClientException()
    {
        // Arrange & Act
        var exception = new RssNetworkException();

        // Assert
        Assert.IsInstanceOfType<RssClientException>(exception);
    }

    [TestMethod]
    public void RssFeedParseException_IsRssClientException()
    {
        // Arrange & Act
        var exception = new RssFeedParseException();

        // Assert
        Assert.IsInstanceOfType<RssClientException>(exception);
    }
}
