// Copyright (c) Richasy. All rights reserved.

namespace EpubParser.Test.Helpers;

/// <summary>
/// PathHelper 单元测试。
/// </summary>
[TestClass]
public sealed class PathHelperTests
{
    [TestMethod]
    public void Combine_SimpleRelativePath_ReturnsCombined()
    {
        var result = PathHelper.Combine("OEBPS", "Text/chapter1.xhtml");
        Assert.AreEqual("OEBPS/Text/chapter1.xhtml", result);
    }

    [TestMethod]
    public void Combine_WithParentReference_ResolvesCorrectly()
    {
        var result = PathHelper.Combine("OEBPS/Text", "../Images/cover.jpg");
        Assert.AreEqual("OEBPS/Images/cover.jpg", result);
    }

    [TestMethod]
    public void Combine_MultipleParentReferences_ResolvesCorrectly()
    {
        var result = PathHelper.Combine("OEBPS/Text/Sub", "../../Images/cover.jpg");
        Assert.AreEqual("OEBPS/Images/cover.jpg", result);
    }

    [TestMethod]
    public void Combine_EmptyBase_ReturnsRelativePath()
    {
        var result = PathHelper.Combine("", "Text/chapter1.xhtml");
        Assert.AreEqual("Text/chapter1.xhtml", result);
    }

    [TestMethod]
    public void Combine_WithBackslashes_NormalizesToForwardSlashes()
    {
        var result = PathHelper.Combine("OEBPS\\Text", "chapter1.xhtml");
        Assert.AreEqual("OEBPS/Text/chapter1.xhtml", result);
    }

    [TestMethod]
    public void SplitAnchor_WithAnchor_SplitsCorrectly()
    {
        var (path, anchor) = PathHelper.SplitAnchor("chapter1.xhtml#section1");
        Assert.AreEqual("chapter1.xhtml", path);
        Assert.AreEqual("section1", anchor);
    }

    [TestMethod]
    public void SplitAnchor_WithoutAnchor_ReturnsNullAnchor()
    {
        var (path, anchor) = PathHelper.SplitAnchor("chapter1.xhtml");
        Assert.AreEqual("chapter1.xhtml", path);
        Assert.IsNull(anchor);
    }

    [TestMethod]
    public void SplitAnchor_EmptyAnchor_ReturnsEmptyAnchor()
    {
        var (path, anchor) = PathHelper.SplitAnchor("chapter1.xhtml#");
        Assert.AreEqual("chapter1.xhtml", path);
        Assert.AreEqual("", anchor);
    }

    [TestMethod]
    public void NormalizePath_WithParentReferences_Normalizes()
    {
        var result = PathHelper.NormalizePath("OEBPS/Text/../Images/cover.jpg");
        Assert.AreEqual("OEBPS/Images/cover.jpg", result);
    }

    [TestMethod]
    public void NormalizePath_WithCurrentDirectory_Removes()
    {
        var result = PathHelper.NormalizePath("OEBPS/./Text/chapter1.xhtml");
        Assert.AreEqual("OEBPS/Text/chapter1.xhtml", result);
    }

    [TestMethod]
    public void GetDirectoryPath_ReturnsDirectory()
    {
        var result = PathHelper.GetDirectoryPath("OEBPS/Text/chapter1.xhtml");
        Assert.AreEqual("OEBPS/Text", result);
    }

    [TestMethod]
    public void GetDirectoryPath_RootFile_ReturnsEmpty()
    {
        var result = PathHelper.GetDirectoryPath("content.opf");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void IsRemoteUrl_HttpUrl_ReturnsTrue()
    {
        Assert.IsTrue(PathHelper.IsRemoteUrl("http://example.com/image.jpg"));
        Assert.IsTrue(PathHelper.IsRemoteUrl("https://example.com/image.jpg"));
    }

    [TestMethod]
    public void IsRemoteUrl_LocalPath_ReturnsFalse()
    {
        Assert.IsFalse(PathHelper.IsRemoteUrl("Images/cover.jpg"));
        Assert.IsFalse(PathHelper.IsRemoteUrl("../Images/cover.jpg"));
    }
}
