// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;

namespace BookScraper.Test.UnitTests;

[TestClass]
public class EncodingHelperTests
{
    [TestMethod]
    public void GetGbkEncoding_ReturnsEncoding()
    {
        var encoding = EncodingHelper.GetGbkEncoding();
        Assert.IsNotNull(encoding);
        Assert.AreEqual("gb2312", encoding.WebName);
    }

    [TestMethod]
    public void EncodeAsGbkUrl_EncodesChineseCorrectly()
    {
        var result = EncodingHelper.EncodeAsGbkUrl("中文");
        Assert.IsNotNull(result);
        Assert.AreEqual("%D6%D0%CE%C4", result);
    }

    [TestMethod]
    public void EncodeAsGbkUrl_HandlesEmptyString()
    {
        var result = EncodingHelper.EncodeAsGbkUrl(string.Empty);
        Assert.AreEqual(string.Empty, result);
    }
}
