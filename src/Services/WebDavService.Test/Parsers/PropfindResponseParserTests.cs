// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Parsers;

[TestClass]
public class PropfindResponseParserTests
{
    private PropfindResponseParser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new PropfindResponseParser();
    }

    [TestMethod]
    public void Parse_ValidResponse_ReturnsResources()
    {
        // Arrange
        var xml = TestDataFactory.PropfindResponse;

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        Assert.AreEqual(207, result.StatusCode);
        Assert.AreEqual(2, result.Resources.Count);
    }

    [TestMethod]
    public void Parse_ValidResponse_ParsesCollectionCorrectly()
    {
        // Arrange
        var xml = TestDataFactory.PropfindResponse;

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        var folder = result.Resources.First(r => r.Uri.EndsWith('/'));
        Assert.IsTrue(folder.IsCollection);
        Assert.AreEqual("documents", folder.DisplayName);
    }

    [TestMethod]
    public void Parse_ValidResponse_ParsesFileCorrectly()
    {
        // Arrange
        var xml = TestDataFactory.PropfindResponse;

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        var file = result.Resources.First(r => r.Uri.EndsWith(".txt", StringComparison.Ordinal));
        Assert.IsFalse(file.IsCollection);
        Assert.AreEqual("file.txt", file.DisplayName);
        Assert.AreEqual(1024, file.ContentLength);
        Assert.AreEqual("text/plain", file.ContentType);
        Assert.AreEqual("\"abc123\"", file.ETag);
    }

    [TestMethod]
    public void Parse_InvalidXml_ReturnsEmptyResources()
    {
        // Arrange
        var xml = "not valid xml";

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        Assert.AreEqual(207, result.StatusCode);
        Assert.AreEqual(0, result.Resources.Count);
    }

    [TestMethod]
    public void Parse_EmptyResponse_ReturnsEmptyResources()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <D:multistatus xmlns:D="DAV:">
            </D:multistatus>
            """;

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        Assert.AreEqual(0, result.Resources.Count);
    }

    [TestMethod]
    public void Parse_ResponseWithDates_ParsesDatesCorrectly()
    {
        // Arrange
        var xml = TestDataFactory.PropfindResponse;

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        var folder = result.Resources.First(r => r.IsCollection);
        Assert.IsNotNull(folder.CreationDate);
        Assert.IsNotNull(folder.LastModifiedDate);
    }
}
