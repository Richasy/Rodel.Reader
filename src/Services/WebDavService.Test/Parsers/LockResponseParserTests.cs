// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Parsers;

[TestClass]
public class LockResponseParserTests
{
    private LockResponseParser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new LockResponseParser();
    }

    [TestMethod]
    public void Parse_ValidLockResponse_ReturnsLockInfo()
    {
        // Arrange
        var xml = TestDataFactory.LockResponse;

        // Act
        var result = _parser.Parse(xml, 200, "OK");

        // Assert
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsNotNull(result.ActiveLock);
    }

    [TestMethod]
    public void Parse_ValidLockResponse_ParsesLockToken()
    {
        // Arrange
        var xml = TestDataFactory.LockResponse;

        // Act
        var result = _parser.Parse(xml, 200, "OK");

        // Assert
        Assert.IsNotNull(result.ActiveLock);
        Assert.AreEqual("urn:uuid:a515cfa4-5d2a-11d1-8f23-00aa00bd5301", result.ActiveLock.LockToken);
    }

    [TestMethod]
    public void Parse_ValidLockResponse_ParsesLockScope()
    {
        // Arrange
        var xml = TestDataFactory.LockResponse;

        // Act
        var result = _parser.Parse(xml, 200, "OK");

        // Assert
        Assert.IsNotNull(result.ActiveLock);
        Assert.AreEqual(LockScope.Exclusive, result.ActiveLock.LockScope);
    }

    [TestMethod]
    public void Parse_ValidLockResponse_ParsesOwner()
    {
        // Arrange
        var xml = TestDataFactory.LockResponse;

        // Act
        var result = _parser.Parse(xml, 200, "OK");

        // Assert
        Assert.IsNotNull(result.ActiveLock?.Owner);
        Assert.IsInstanceOfType<UriLockOwner>(result.ActiveLock.Owner);
    }

    [TestMethod]
    public void Parse_ValidLockResponse_ParsesLockRoot()
    {
        // Arrange
        var xml = TestDataFactory.LockResponse;

        // Act
        var result = _parser.Parse(xml, 200, "OK");

        // Assert
        Assert.IsNotNull(result.ActiveLock);
        Assert.AreEqual("/documents/file.txt", result.ActiveLock.LockRoot);
    }

    [TestMethod]
    public void Parse_InvalidXml_ReturnsNullActiveLock()
    {
        // Arrange
        var xml = "not valid xml";

        // Act
        var result = _parser.Parse(xml, 200, "OK");

        // Assert
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsNull(result.ActiveLock);
    }

    [TestMethod]
    public void Parse_SharedLock_ParsesCorrectly()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <D:prop xmlns:D="DAV:">
                <D:lockdiscovery>
                    <D:activelock>
                        <D:locktype><D:write/></D:locktype>
                        <D:lockscope><D:shared/></D:lockscope>
                        <D:locktoken>
                            <D:href>urn:uuid:shared-token</D:href>
                        </D:locktoken>
                    </D:activelock>
                </D:lockdiscovery>
            </D:prop>
            """;

        // Act
        var result = _parser.Parse(xml, 200, "OK");

        // Assert
        Assert.IsNotNull(result.ActiveLock);
        Assert.AreEqual(LockScope.Shared, result.ActiveLock.LockScope);
    }
}
