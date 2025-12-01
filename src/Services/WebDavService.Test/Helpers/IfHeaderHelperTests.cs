// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav.Test.Helpers;

[TestClass]
public class IfHeaderHelperTests
{
    [TestMethod]
    public void GetHeaderValue_ValidLockToken_ReturnsFormattedValue()
    {
        // Arrange
        var lockToken = "urn:uuid:a515cfa4-5d2a-11d1-8f23-00aa00bd5301";

        // Act
        var result = IfHeaderHelper.GetHeaderValue(lockToken);

        // Assert
        Assert.AreEqual($"(<{lockToken}>)", result);
    }

    [TestMethod]
    public void GetHeaderValue_AlreadyFormatted_DoubleWraps()
    {
        // Arrange
        var lockToken = "<urn:uuid:a515cfa4-5d2a-11d1-8f23-00aa00bd5301>";

        // Act
        var result = IfHeaderHelper.GetHeaderValue(lockToken);

        // Assert
        // Note: Current implementation double-wraps, which may need review
        Assert.AreEqual($"(<{lockToken}>)", result);
    }
}
