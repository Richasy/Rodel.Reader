// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Test.Unit;

/// <summary>
/// 异常类测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ExceptionTests
{
    [TestMethod]
    public void FanQieException_WithMessage_ContainsMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new FanQieException(message);

        // Assert
        Assert.AreEqual(message, exception.Message);
    }

    [TestMethod]
    public void FanQieException_WithInnerException_ContainsInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var message = "Outer error";

        // Act
        var exception = new FanQieException(message, innerException);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreSame(innerException, exception.InnerException);
    }

    [TestMethod]
    public void FanQieApiException_WithCodeAndMessage_ContainsBoth()
    {
        // Arrange
        var code = 1001;
        var message = "API Error";

        // Act
        var exception = new FanQieApiException(code, message);

        // Assert
        Assert.AreEqual(code, exception.Code);
        Assert.AreEqual(message, exception.Message);
        Assert.IsInstanceOfType<FanQieException>(exception);
    }

    [TestMethod]
    public void FanQieApiException_WithInnerException_ContainsDetails()
    {
        // Arrange
        var innerException = new HttpRequestException("Network error");
        var code = 500;
        var message = "Server error";

        // Act
        var exception = new FanQieApiException(code, message, innerException);

        // Assert
        Assert.AreEqual(code, exception.Code);
        Assert.AreEqual(message, exception.Message);
        Assert.AreSame(innerException, exception.InnerException);
    }

    [TestMethod]
    public void FanQieDecryptException_DefaultMessage_IsNotEmpty()
    {
        // Act
        var exception = new FanQieDecryptException();

        // Assert
        Assert.IsInstanceOfType<FanQieException>(exception);
        Assert.IsFalse(string.IsNullOrEmpty(exception.Message));
    }

    [TestMethod]
    public void FanQieDecryptException_WithMessage_ContainsMessage()
    {
        // Arrange
        var message = "Decryption failed";

        // Act
        var exception = new FanQieDecryptException(message);

        // Assert
        Assert.AreEqual(message, exception.Message);
    }

    [TestMethod]
    public void FanQieDecryptException_WithInnerException_ContainsDetails()
    {
        // Arrange
        var innerException = new CryptographicException("Invalid key");
        var message = "Failed to decrypt content";

        // Act
        var exception = new FanQieDecryptException(message, innerException);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreSame(innerException, exception.InnerException);
    }

    [TestMethod]
    public void FanQieParseException_DefaultMessage_IsNotEmpty()
    {
        // Act
        var exception = new FanQieParseException();

        // Assert
        Assert.IsInstanceOfType<FanQieException>(exception);
        Assert.IsFalse(string.IsNullOrEmpty(exception.Message));
    }

    [TestMethod]
    public void FanQieParseException_WithMessage_ContainsMessage()
    {
        // Arrange
        var message = "Parse error";

        // Act
        var exception = new FanQieParseException(message);

        // Assert
        Assert.AreEqual(message, exception.Message);
    }

    [TestMethod]
    public void FanQieParseException_WithInnerException_ContainsDetails()
    {
        // Arrange
        var innerException = new FormatException("Invalid JSON");
        var message = "Failed to parse response";

        // Act
        var exception = new FanQieParseException(message, innerException);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreSame(innerException, exception.InnerException);
    }
}
