// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace EpubParser.Test.Helpers;

/// <summary>
/// XmlHelper 单元测试。
/// </summary>
[TestClass]
public sealed class XmlHelperTests
{
    [TestMethod]
    public async Task LoadDocumentAsync_ValidXml_ReturnsDocument()
    {
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <root xmlns="http://example.com">
                <child>content</child>
            </root>
            """;

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
        var doc = await XmlHelper.LoadDocumentAsync(stream);

        Assert.IsNotNull(doc);
        Assert.IsNotNull(doc.Root);
        Assert.IsTrue(doc.Root.HasLocalName("root"));
    }

    [TestMethod]
    public async Task LoadDocumentAsync_MalformedXml_ReturnsNull()
    {
        var malformedXml = "<root><unclosed>";

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(malformedXml));
        var doc = await XmlHelper.LoadDocumentAsync(stream);

        Assert.IsNull(doc);
    }

    [TestMethod]
    public void GetElement_WithNamespace_FindsElement()
    {
        var xml = """
            <root xmlns="http://example.com">
                <child>content</child>
            </root>
            """;

        var doc = XDocument.Parse(xml);
        var child = doc.Root!.GetElement("child");

        Assert.IsNotNull(child);
        Assert.AreEqual("content", child.Value);
    }

    [TestMethod]
    public void GetElement_DifferentNamespaces_FindsElement()
    {
        var xml = """
            <root xmlns="http://example1.com">
                <child xmlns="http://example2.com">content</child>
            </root>
            """;

        var doc = XDocument.Parse(xml);
        var child = doc.Root!.GetElement("child");

        Assert.IsNotNull(child);
    }

    [TestMethod]
    public void GetElements_ReturnsAllMatchingElements()
    {
        var xml = """
            <root xmlns="http://example.com">
                <item>1</item>
                <item>2</item>
                <item>3</item>
            </root>
            """;

        var doc = XDocument.Parse(xml);
        var items = doc.Root!.GetElements("item").ToList();

        Assert.AreEqual(3, items.Count, "应该找到 3 个元素");
    }

    [TestMethod]
    public void GetAttributeValue_ReturnsValue()
    {
        var xml = """<root attr="value"/>""";
        var doc = XDocument.Parse(xml);
        var value = doc.Root!.GetAttributeValue("attr");

        Assert.AreEqual("value", value);
    }

    [TestMethod]
    public void GetAttributeValue_MissingAttribute_ReturnsNull()
    {
        var xml = """<root/>""";
        var doc = XDocument.Parse(xml);
        var value = doc.Root!.GetAttributeValue("missing");

        Assert.IsNull(value);
    }

    [TestMethod]
    public void HasLocalName_MatchesIgnoringNamespace()
    {
        var xml = """<root xmlns="http://example.com"/>""";
        var doc = XDocument.Parse(xml);

        Assert.IsTrue(doc.Root!.HasLocalName("root"));
        Assert.IsFalse(doc.Root!.HasLocalName("other"));
    }
}
