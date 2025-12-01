// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Fb2Parser.Test.Internal;

/// <summary>
/// DescriptionParser 单元测试。
/// </summary>
[TestClass]
public sealed class DescriptionParserTests
{
    private static readonly XNamespace FbNs = "http://www.gribuser.ru/xml/fictionbook/2.0";

    [TestMethod]
    public void Parse_NullElement_ReturnsEmptyMetadata()
    {
        // Act
        var metadata = DescriptionParser.Parse(null);

        // Assert
        Assert.IsNotNull(metadata);
        Assert.IsNull(metadata.Title);
        Assert.AreEqual(0, metadata.Authors.Count);
    }

    [TestMethod]
    public void Parse_WithTitleInfo_ExtractsTitle()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <book-title>Test Title</book-title>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual("Test Title", metadata.Title);
    }

    [TestMethod]
    public void Parse_WithAuthor_ExtractsAuthorInfo()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <author>
                        <first-name>John</first-name>
                        <middle-name>William</middle-name>
                        <last-name>Doe</last-name>
                        <nickname>JD</nickname>
                        <home-page>https://example.com</home-page>
                        <email>john@example.com</email>
                    </author>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual(1, metadata.Authors.Count);
        var author = metadata.Authors[0];
        Assert.AreEqual("John", author.FirstName);
        Assert.AreEqual("William", author.MiddleName);
        Assert.AreEqual("Doe", author.LastName);
        Assert.AreEqual("JD", author.Nickname);
        Assert.AreEqual("https://example.com", author.HomePage);
        Assert.AreEqual("john@example.com", author.Email);
    }

    [TestMethod]
    public void Parse_WithMultipleAuthors_ExtractsAll()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <author><first-name>John</first-name><last-name>Doe</last-name></author>
                    <author><first-name>Jane</first-name><last-name>Smith</last-name></author>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual(2, metadata.Authors.Count);
    }

    [TestMethod]
    public void Parse_WithGenres_ExtractsAll()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <genre>science_fiction</genre>
                    <genre>adventure</genre>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual(2, metadata.Genres.Count);
        Assert.IsTrue(metadata.Genres.Contains("science_fiction"));
        Assert.IsTrue(metadata.Genres.Contains("adventure"));
    }

    [TestMethod]
    public void Parse_WithAnnotation_ExtractsDescription()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <annotation>
                        <p>First paragraph.</p>
                        <p>Second paragraph.</p>
                    </annotation>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.IsNotNull(metadata.Description);
        Assert.IsTrue(metadata.Description.Contains("First paragraph"));
        Assert.IsTrue(metadata.Description.Contains("Second paragraph"));
    }

    [TestMethod]
    public void Parse_WithSequence_ExtractsSequenceInfo()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <sequence name=""Space Saga"" number=""3""/>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.IsNotNull(metadata.Sequence);
        Assert.AreEqual("Space Saga", metadata.Sequence.Name);
        Assert.AreEqual(3, metadata.Sequence.Number);
    }

    [TestMethod]
    public void Parse_WithCoverpage_ExtractsCoverImageId()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"" xmlns:l=""http://www.w3.org/1999/xlink"">
                <title-info>
                    <coverpage>
                        <image l:href=""#cover.jpg""/>
                    </coverpage>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual("cover.jpg", metadata.CoverpageImageId);
    }

    [TestMethod]
    public void Parse_WithDocumentInfo_ExtractsDocInfo()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <document-info>
                    <author><nickname>converter</nickname></author>
                    <program-used>FB2 Creator</program-used>
                    <date value=""2023-01-01"">January 2023</date>
                    <src-url>https://source.example.com</src-url>
                    <id>doc-12345</id>
                    <version>1.0</version>
                </document-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.IsNotNull(metadata.DocumentInfo);
        Assert.AreEqual("FB2 Creator", metadata.DocumentInfo.ProgramUsed);
        Assert.AreEqual("2023-01-01", metadata.DocumentInfo.Date);
        Assert.AreEqual("doc-12345", metadata.DocumentInfo.Id);
        Assert.AreEqual("1.0", metadata.DocumentInfo.Version);
        Assert.AreEqual(1, metadata.DocumentInfo.SourceUrls.Count);
    }

    [TestMethod]
    public void Parse_WithPublishInfo_ExtractsPubInfo()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <publish-info>
                    <book-name>Published Title</book-name>
                    <publisher>Great Publisher</publisher>
                    <city>New York</city>
                    <year>2023</year>
                    <isbn>978-1234567890</isbn>
                </publish-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.IsNotNull(metadata.PublishInfo);
        Assert.AreEqual("Published Title", metadata.PublishInfo.BookName);
        Assert.AreEqual("Great Publisher", metadata.PublishInfo.Publisher);
        Assert.AreEqual("New York", metadata.PublishInfo.City);
        Assert.AreEqual("2023", metadata.PublishInfo.Year);
        Assert.AreEqual("978-1234567890", metadata.PublishInfo.Isbn);
    }

    [TestMethod]
    public void Parse_PublishInfoFallback_SetsPublisherFromPublishInfo()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <book-title>Test</book-title>
                </title-info>
                <publish-info>
                    <publisher>Test Publisher</publisher>
                    <year>2023</year>
                    <isbn>123-456</isbn>
                </publish-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual("Test Publisher", metadata.Publisher);
        Assert.AreEqual("2023", metadata.PublishDate);
        Assert.AreEqual("123-456", metadata.Identifier);
    }

    [TestMethod]
    public void Parse_WithTranslators_ExtractsTranslators()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <translator>
                        <first-name>Alex</first-name>
                        <last-name>Translator</last-name>
                    </translator>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual(1, metadata.Translators.Count);
        Assert.AreEqual("Alex Translator", metadata.Translators[0].GetDisplayName());
    }

    [TestMethod]
    public void Parse_WithKeywords_ExtractsKeywords()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <keywords>adventure, space, sci-fi</keywords>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual(3, metadata.Keywords.Count);
        Assert.IsTrue(metadata.Keywords.Contains("adventure"));
        Assert.IsTrue(metadata.Keywords.Contains("space"));
        Assert.IsTrue(metadata.Keywords.Contains("sci-fi"));
    }

    [TestMethod]
    public void Parse_WithDateValue_UsesValueAttribute()
    {
        // Arrange
        var xml = @"
            <description xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <title-info>
                    <date value=""2023-06-15"">June 15, 2023</date>
                </title-info>
            </description>";
        var element = XElement.Parse(xml);

        // Act
        var metadata = DescriptionParser.Parse(element);

        // Assert
        Assert.AreEqual("2023-06-15", metadata.PublishDate);
    }
}
