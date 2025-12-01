// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Fb2Parser.Test.Internal;

/// <summary>
/// BinaryParser 单元测试。
/// </summary>
[TestClass]
public sealed class BinaryParserTests
{
    [TestMethod]
    public void Parse_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var binaries = new List<XElement>();

        // Act
        var result = BinaryParser.Parse(binaries);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Parse_ValidBinary_ExtractsInfo()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var base64 = Convert.ToBase64String(data);
        var xml = $@"<binary id=""image1.jpg"" content-type=""image/jpeg"">{base64}</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("image1.jpg", result[0].Id);
        Assert.AreEqual("image/jpeg", result[0].MediaType);
        CollectionAssert.AreEqual(data, result[0].GetBytes());
    }

    [TestMethod]
    public void Parse_WithoutId_SkipsBinary()
    {
        // Arrange
        var xml = @"<binary content-type=""image/jpeg"">AQID</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Parse_MultipleBinaries_ParsesAll()
    {
        // Arrange
        var xml1 = @"<binary id=""img1.jpg"" content-type=""image/jpeg"">AQID</binary>";
        var xml2 = @"<binary id=""img2.png"" content-type=""image/png"">BAUG</binary>";

        // Act
        var result = BinaryParser.Parse([XElement.Parse(xml1), XElement.Parse(xml2)]);

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void Parse_NormalizesMediaType_ImageJpg()
    {
        // Arrange
        var xml = @"<binary id=""test.jpg"" content-type=""image/jpg"">AQID</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        Assert.AreEqual("image/jpeg", result[0].MediaType);
    }

    [TestMethod]
    public void Parse_NormalizesMediaType_JpegShort()
    {
        // Arrange
        var xml = @"<binary id=""test.jpg"" content-type=""jpeg"">AQID</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        Assert.AreEqual("image/jpeg", result[0].MediaType);
    }

    [TestMethod]
    public void Parse_NormalizesMediaType_PngShort()
    {
        // Arrange
        var xml = @"<binary id=""test.png"" content-type=""png"">AQID</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        Assert.AreEqual("image/png", result[0].MediaType);
    }

    [TestMethod]
    public void Parse_WithWhitespace_StripsWhitespace()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var base64 = Convert.ToBase64String(data);
        var xml = $@"<binary id=""test.jpg"" content-type=""image/jpeg"">
            {base64}
        </binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        CollectionAssert.AreEqual(data, result[0].GetBytes());
    }

    [TestMethod]
    public void Parse_WithLineBreaks_StripsLineBreaks()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var base64 = Convert.ToBase64String(data);
        // Simulate base64 with line breaks
        var base64WithBreaks = string.Join("\n", base64.Chunk(4).Select(c => new string(c)));
        var xml = $@"<binary id=""test.jpg"" content-type=""image/jpeg"">{base64WithBreaks}</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        CollectionAssert.AreEqual(data, result[0].GetBytes());
    }

    [TestMethod]
    public void Parse_WithoutContentType_UsesDefault()
    {
        // Arrange
        var xml = @"<binary id=""test.bin"">AQID</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        Assert.AreEqual("application/octet-stream", result[0].MediaType);
    }

    [TestMethod]
    public void Parse_IsImage_TrueForImageTypes()
    {
        // Arrange
        var xml = @"<binary id=""test.jpg"" content-type=""image/jpeg"">AQID</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        Assert.IsTrue(result[0].IsImage);
    }

    [TestMethod]
    public void Parse_IsImage_FalseForNonImageTypes()
    {
        // Arrange
        var xml = @"<binary id=""test.pdf"" content-type=""application/pdf"">AQID</binary>";
        var element = XElement.Parse(xml);

        // Act
        var result = BinaryParser.Parse([element]);

        // Assert
        Assert.IsFalse(result[0].IsImage);
    }
}
