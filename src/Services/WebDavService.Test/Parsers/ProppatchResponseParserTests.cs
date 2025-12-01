// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Parsers;

[TestClass]
public class ProppatchResponseParserTests
{
    private ProppatchResponseParser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new ProppatchResponseParser();
    }

    [TestMethod]
    public void Parse_ValidResponse_ReturnsPropertyStatuses()
    {
        // Arrange
        var xml = TestDataFactory.ProppatchResponse;

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        Assert.AreEqual(207, result.StatusCode);
        Assert.IsTrue(result.PropertyStatuses.Count > 0);
    }

    [TestMethod]
    public void Parse_SuccessfulUpdate_ReturnsOkStatus()
    {
        // Arrange
        var xml = TestDataFactory.ProppatchResponse;

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        var status = result.PropertyStatuses.FirstOrDefault();
        Assert.IsNotNull(status);
        Assert.AreEqual(200, status.StatusCode);
    }

    [TestMethod]
    public void Parse_InvalidXml_ReturnsEmptyStatuses()
    {
        // Arrange
        var xml = "not valid xml";

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        Assert.AreEqual(207, result.StatusCode);
        Assert.AreEqual(0, result.PropertyStatuses.Count);
    }

    [TestMethod]
    public void Parse_FailedProperty_ReturnsForbiddenStatus()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <D:multistatus xmlns:D="DAV:">
                <D:response>
                    <D:href>/documents/file.txt</D:href>
                    <D:propstat>
                        <D:prop>
                            <D:getetag/>
                        </D:prop>
                        <D:status>HTTP/1.1 403 Forbidden</D:status>
                    </D:propstat>
                </D:response>
            </D:multistatus>
            """;

        // Act
        var result = _parser.Parse(xml, 207, "Multi-Status");

        // Assert
        var status = result.PropertyStatuses.FirstOrDefault();
        Assert.IsNotNull(status);
        Assert.AreEqual(403, status.StatusCode);
    }
}
