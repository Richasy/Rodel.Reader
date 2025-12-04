// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Test.Unit;

/// <summary>
/// 异常类单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ExceptionTests
{
    [TestMethod]
    public void LegadoException_DefaultConstructor_CreatesException()
    {
        // Act
        var ex = new LegadoException();

        // Assert
        Assert.IsNotNull(ex);
    }

    [TestMethod]
    public void LegadoException_WithMessage_ContainsMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var ex = new LegadoException(message);

        // Assert
        Assert.AreEqual(message, ex.Message);
    }

    [TestMethod]
    public void LegadoException_WithInnerException_ContainsInnerException()
    {
        // Arrange
        var inner = new InvalidOperationException("Inner");
        var message = "Outer";

        // Act
        var ex = new LegadoException(message, inner);

        // Assert
        Assert.AreEqual(message, ex.Message);
        Assert.AreSame(inner, ex.InnerException);
    }

    [TestMethod]
    public void LegadoApiException_WithStatusCode_ContainsStatusCode()
    {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;
        var message = "Not found";

        // Act
        var ex = new LegadoApiException(statusCode, message);

        // Assert
        Assert.AreEqual(statusCode, ex.StatusCode);
        Assert.AreEqual(message, ex.Message);
    }

    [TestMethod]
    public void LegadoApiException_WithErrorCode_ContainsErrorCode()
    {
        // Arrange
        var errorCode = "ERR001";
        var message = "API error";

        // Act
        var ex = new LegadoApiException(errorCode, message, isApiError: true);

        // Assert
        Assert.AreEqual(errorCode, ex.ErrorCode);
        Assert.AreEqual(message, ex.Message);
    }

    [TestMethod]
    public void LegadoAuthException_DefaultConstructor_HasDefaultMessage()
    {
        // Act
        var ex = new LegadoAuthException();

        // Assert
        Assert.IsNotNull(ex.Message);
        Assert.IsTrue(ex.Message.Contains("Authentication", StringComparison.Ordinal));
    }

    [TestMethod]
    public void LegadoAuthException_WithMessage_ContainsMessage()
    {
        // Arrange
        var message = "Invalid token";

        // Act
        var ex = new LegadoAuthException(message);

        // Assert
        Assert.AreEqual(message, ex.Message);
    }
}
