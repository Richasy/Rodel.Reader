// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Test.Unit;

/// <summary>
/// 异常类测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ExceptionTests
{
    [TestMethod]
    public void ZLibraryException_WithMessage_ContainsMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new ZLibraryException(message);

        // Assert
        Assert.AreEqual(message, exception.Message);
    }

    [TestMethod]
    public void ZLibraryException_WithInnerException_ContainsInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var message = "Outer error";

        // Act
        var exception = new ZLibraryException(message, innerException);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreSame(innerException, exception.InnerException);
    }

    [TestMethod]
    public void LoginFailedException_InheritsFromZLibraryException()
    {
        // Arrange & Act
        var exception = new LoginFailedException("Login failed");

        // Assert
        Assert.IsInstanceOfType<ZLibraryException>(exception);
        Assert.AreEqual("Login failed", exception.Message);
    }

    [TestMethod]
    public void NotAuthenticatedException_InheritsFromZLibraryException()
    {
        // Arrange & Act
        var exception = new NotAuthenticatedException();

        // Assert
        Assert.IsInstanceOfType<ZLibraryException>(exception);
    }

    [TestMethod]
    public void NotAuthenticatedException_WithMessage_ContainsMessage()
    {
        // Arrange & Act
        var exception = new NotAuthenticatedException("Please login first");

        // Assert
        Assert.AreEqual("Please login first", exception.Message);
    }

    [TestMethod]
    public void BookNotFoundException_InheritsFromZLibraryException()
    {
        // Arrange & Act
        var exception = new BookNotFoundException("12345");

        // Assert
        Assert.IsInstanceOfType<ZLibraryException>(exception);
        Assert.IsTrue(exception.Message.Contains("12345", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ParseException_InheritsFromZLibraryException()
    {
        // Arrange & Act
        var exception = new ParseException("Failed to parse HTML");

        // Assert
        Assert.IsInstanceOfType<ZLibraryException>(exception);
    }

    [TestMethod]
    public void ParseException_WithInnerException_ContainsDetails()
    {
        // Arrange
        var innerException = new FormatException("Invalid format");

        // Act
        var exception = new ParseException("Parse failed", innerException);

        // Assert
        Assert.AreEqual("Parse failed", exception.Message);
        Assert.AreSame(innerException, exception.InnerException);
    }

    [TestMethod]
    public void DownloadLimitExceededException_InheritsFromZLibraryException()
    {
        // Arrange & Act
        var exception = new DownloadLimitExceededException("Daily limit reached");

        // Assert
        Assert.IsInstanceOfType<ZLibraryException>(exception);
    }

    [TestMethod]
    public void EmptyQueryException_InheritsFromZLibraryException()
    {
        // Arrange & Act
        var exception = new EmptyQueryException();

        // Assert
        Assert.IsInstanceOfType<ZLibraryException>(exception);
    }

    [TestMethod]
    public void EmptyQueryException_WithMessage_ContainsMessage()
    {
        // Arrange & Act
        var exception = new EmptyQueryException("Search query cannot be empty");

        // Assert
        Assert.AreEqual("Search query cannot be empty", exception.Message);
    }
}
