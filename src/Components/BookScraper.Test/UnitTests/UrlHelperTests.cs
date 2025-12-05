// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;

namespace BookScraper.Test.UnitTests;

[TestClass]
public class UrlHelperTests
{
    [TestMethod]
    [DataRow("//example.com/image.jpg", "https://example.com/image.jpg")]
    [DataRow("http://example.com/page", "http://example.com/page")]
    [DataRow("https://example.com/page", "https://example.com/page")]
    [DataRow("example.com/page", "https://example.com/page")]
    [DataRow(null, null)]
    [DataRow("", null)]
    [DataRow("  ", null)]
    public void EnsureScheme_ReturnsCorrectUrl(string? input, string? expected)
    {
        var result = UrlHelper.EnsureScheme(input);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void EnsureScheme_WithCustomScheme_ReturnsCorrectUrl()
    {
        var result = UrlHelper.EnsureScheme("//example.com/image.jpg", "http");
        Assert.AreEqual("http://example.com/image.jpg", result);
    }

    [TestMethod]
    [DataRow("https://example.com/books/123", "123")]
    [DataRow("https://example.com/books/abc/", "abc")]
    [DataRow("https://example.com/", "")]
    [DataRow(null, null)]
    [DataRow("", null)]
    public void ExtractLastSegment_ReturnsCorrectSegment(string? input, string? expected)
    {
        var result = UrlHelper.ExtractLastSegment(input);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("https://example.com?id=123&name=test", "id", "123")]
    [DataRow("https://example.com?id=123&name=test", "name", "test")]
    [DataRow("https://example.com?id=123", "missing", null)]
    [DataRow("https://example.com", "id", null)]
    [DataRow(null, "id", null)]
    public void ExtractQueryParam_ReturnsCorrectValue(string? url, string paramName, string? expected)
    {
        var result = UrlHelper.ExtractQueryParam(url, paramName);
        Assert.AreEqual(expected, result);
    }
}
