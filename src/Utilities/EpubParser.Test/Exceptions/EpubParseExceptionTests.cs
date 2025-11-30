// Copyright (c) Richasy. All rights reserved.

namespace EpubParser.Test.Exceptions;

/// <summary>
/// EpubParseException 单元测试。
/// </summary>
[TestClass]
public sealed class EpubParseExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_SetsMessage()
    {
        var exception = new EpubParseException("测试错误消息");

        Assert.AreEqual("测试错误消息", exception.Message);
    }

    [TestMethod]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("内部错误");
        var exception = new EpubParseException("外部错误", inner);

        Assert.AreEqual("外部错误", exception.Message);
        Assert.AreSame(inner, exception.InnerException);
    }

    [TestMethod]
    public void Constructor_Default_HasDefaultMessage()
    {
        var exception = new EpubParseException();

        Assert.IsNotNull(exception.Message);
    }
}
