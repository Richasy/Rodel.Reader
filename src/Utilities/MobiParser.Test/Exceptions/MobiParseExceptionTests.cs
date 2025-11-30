// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Exceptions;

/// <summary>
/// MobiParseException 测试。
/// </summary>
[TestClass]
public sealed class MobiParseExceptionTests
{
    /// <summary>
    /// 测试默认构造函数。
    /// </summary>
    [TestMethod]
    public void DefaultConstructor_ShouldCreateException()
    {
        // Act
        var exception = new MobiParseException();

        // Assert
        Assert.IsNotNull(exception);
        Assert.IsNull(exception.InnerException);
    }

    /// <summary>
    /// 测试带消息的构造函数。
    /// </summary>
    [TestMethod]
    public void MessageConstructor_ShouldSetMessage()
    {
        // Arrange
        var message = "测试错误消息";

        // Act
        var exception = new MobiParseException(message);

        // Assert
        Assert.AreEqual(message, exception.Message);
    }

    /// <summary>
    /// 测试带消息和内部异常的构造函数。
    /// </summary>
    [TestMethod]
    public void MessageAndInnerExceptionConstructor_ShouldSetBoth()
    {
        // Arrange
        var message = "外部错误";
        var innerException = new InvalidOperationException("内部错误");

        // Act
        var exception = new MobiParseException(message, innerException);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreEqual(innerException, exception.InnerException);
    }
}
