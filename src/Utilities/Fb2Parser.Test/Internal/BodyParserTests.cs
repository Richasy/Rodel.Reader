// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Fb2Parser.Test.Internal;

/// <summary>
/// BodyParser 单元测试。
/// </summary>
[TestClass]
public sealed class BodyParserTests
{
    private static readonly XNamespace FbNs = "http://www.gribuser.ru/xml/fictionbook/2.0";

    [TestMethod]
    public void Parse_EmptyBodyList_ReturnsEmptyList()
    {
        // Arrange
        var bodies = new List<XElement>();

        // Act
        var sections = BodyParser.Parse(bodies);

        // Assert
        Assert.AreEqual(0, sections.Count);
    }

    [TestMethod]
    public void Parse_SingleSection_ExtractsSection()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section id=""ch1"">
                    <title><p>Chapter 1</p></title>
                    <p>Content here.</p>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.AreEqual(1, sections.Count);
        Assert.AreEqual("ch1", sections[0].Id);
        Assert.AreEqual("Chapter 1", sections[0].Title);
        Assert.AreEqual(0, sections[0].Level);
    }

    [TestMethod]
    public void Parse_NestedSections_ExtractsHierarchy()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section id=""part1"">
                    <title><p>Part 1</p></title>
                    <section id=""ch1"">
                        <title><p>Chapter 1</p></title>
                        <p>Content.</p>
                    </section>
                    <section id=""ch2"">
                        <title><p>Chapter 2</p></title>
                        <p>More content.</p>
                    </section>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.AreEqual(1, sections.Count);
        Assert.AreEqual(2, sections[0].Children.Count);
        Assert.AreEqual("Chapter 1", sections[0].Children[0].Title);
        Assert.AreEqual(1, sections[0].Children[0].Level);
    }

    [TestMethod]
    public void Parse_MultipleBodies_ParsesAll()
    {
        // Arrange
        var xml1 = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section><title><p>Main</p></title></section>
            </body>";
        var xml2 = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"" name=""notes"">
                <section><title><p>Notes</p></title></section>
            </body>";

        // Act
        var sections = BodyParser.Parse([XElement.Parse(xml1), XElement.Parse(xml2)]);

        // Assert
        Assert.AreEqual(2, sections.Count);
    }

    [TestMethod]
    public void Parse_MultiLineTitleP_JoinsWithSpace()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section>
                    <title>
                        <p>Part One:</p>
                        <p>The Beginning</p>
                    </title>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.AreEqual("Part One: The Beginning", sections[0].Title);
    }

    [TestMethod]
    public void Parse_PlainText_ExtractedCorrectly()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section>
                    <title><p>Title</p></title>
                    <p>First paragraph.</p>
                    <p>Second paragraph.</p>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.IsTrue(sections[0].PlainText.Contains("First paragraph"));
        Assert.IsTrue(sections[0].PlainText.Contains("Second paragraph"));
        // Title should not be in plain text (handled separately)
        Assert.IsFalse(sections[0].PlainText.Contains("Title"));
    }

    [TestMethod]
    public void Parse_WithImages_ExtractsImageIds()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"" xmlns:l=""http://www.w3.org/1999/xlink"">
                <section>
                    <title><p>Chapter</p></title>
                    <image l:href=""#image1.jpg""/>
                    <p>Text with image.</p>
                    <image l:href=""#image2.png""/>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.AreEqual(2, sections[0].ImageIds.Count);
        Assert.IsTrue(sections[0].ImageIds.Contains("image1.jpg"));
        Assert.IsTrue(sections[0].ImageIds.Contains("image2.png"));
    }

    [TestMethod]
    public void Parse_ContentPreserved_AsXml()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section id=""test"">
                    <title><p>Test</p></title>
                    <p>Content.</p>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.IsTrue(sections[0].Content.Contains("<section"));
        Assert.IsTrue(sections[0].Content.Contains("id=\"test\""));
    }

    [TestMethod]
    public void Parse_Poem_ExtractsText()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section>
                    <title><p>Poetry</p></title>
                    <poem>
                        <stanza>
                            <v>First line,</v>
                            <v>Second line.</v>
                        </stanza>
                    </poem>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.IsTrue(sections[0].PlainText.Contains("First line"));
        Assert.IsTrue(sections[0].PlainText.Contains("Second line"));
    }

    [TestMethod]
    public void Parse_Cite_ExtractsText()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section>
                    <title><p>With Quote</p></title>
                    <cite>
                        <p>Famous quote here.</p>
                    </cite>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.IsTrue(sections[0].PlainText.Contains("Famous quote"));
    }

    [TestMethod]
    public void Parse_EmptyLine_Handled()
    {
        // Arrange
        var xml = @"
            <body xmlns=""http://www.gribuser.ru/xml/fictionbook/2.0"">
                <section>
                    <p>Before.</p>
                    <empty-line/>
                    <p>After.</p>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.IsTrue(sections[0].PlainText.Contains("Before"));
        Assert.IsTrue(sections[0].PlainText.Contains("After"));
    }

    [TestMethod]
    public void Parse_WithoutNamespace_StillWorks()
    {
        // Arrange
        var xml = @"
            <body>
                <section id=""ch1"">
                    <title><p>Chapter 1</p></title>
                    <p>Content.</p>
                </section>
            </body>";
        var body = XElement.Parse(xml);

        // Act
        var sections = BodyParser.Parse([body]);

        // Assert
        Assert.AreEqual(1, sections.Count);
        Assert.AreEqual("Chapter 1", sections[0].Title);
    }
}
