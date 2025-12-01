// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Exceptions;

/// <summary>
/// Fb2ParseException 单元测试。
/// </summary>
[TestClass]
public sealed class Fb2ParseExceptionTests
{
    [TestMethod]
    public void Constructor_Default_CreatesException()
    {
        // Act
        var exception = new Fb2ParseException();

        // Assert
        Assert.IsNotNull(exception);
        Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new Fb2ParseException(message);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        // Arrange
        var message = "Outer exception";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new Fb2ParseException(message, innerException);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreSame(innerException, exception.InnerException);
    }

    [TestMethod]
    public void Exception_IsException_True()
    {
        // Act
        var exception = new Fb2ParseException();

        // Assert
        Assert.IsInstanceOfType<Exception>(exception);
    }
}
