// Copyright (c) Richasy. All rights reserved.

namespace EpubParser.Test.Models;

/// <summary>
/// EpubMetadata 单元测试。
/// </summary>
[TestClass]
public sealed class EpubMetadataTests
{
    [TestMethod]
    public void DefaultValues_AreCorrect()
    {
        var metadata = new EpubMetadata();

        Assert.IsNull(metadata.Title);
        Assert.IsNotNull(metadata.Authors);
        Assert.AreEqual(0, metadata.Authors.Count);
        Assert.IsNull(metadata.Language);
        Assert.IsNotNull(metadata.Subjects);
        Assert.AreEqual(0, metadata.Subjects.Count);
        Assert.IsNotNull(metadata.Contributors);
        Assert.AreEqual(0, metadata.Contributors.Count);
        Assert.IsNotNull(metadata.CustomMetadata);
        Assert.AreEqual(0, metadata.CustomMetadata.Count);
        Assert.IsNotNull(metadata.MetaItems);
        Assert.AreEqual(0, metadata.MetaItems.Count);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        var metadata = new EpubMetadata
        {
            Title = "测试标题",
            Language = "zh",
            Identifier = "urn:uuid:test",
            Publisher = "测试出版社",
            Description = "测试描述",
            PublishDate = "2024-01-01"
        };
        metadata.Authors.Add("测试作者");

        Assert.AreEqual("测试标题", metadata.Title);
        Assert.AreEqual("测试作者", metadata.Authors[0]);
        Assert.AreEqual("zh", metadata.Language);
        Assert.AreEqual("urn:uuid:test", metadata.Identifier);
        Assert.AreEqual("测试出版社", metadata.Publisher);
        Assert.AreEqual("测试描述", metadata.Description);
        Assert.AreEqual("2024-01-01", metadata.PublishDate);
    }

    [TestMethod]
    public void Subjects_CanBeModified()
    {
        var metadata = new EpubMetadata();
        metadata.Subjects.Add("科幻");
        metadata.Subjects.Add("冒险");

        Assert.AreEqual(2, metadata.Subjects.Count);
        CollectionAssert.Contains(metadata.Subjects.ToList(), "科幻");
    }

    [TestMethod]
    public void Contributors_CanBeModified()
    {
        var metadata = new EpubMetadata();
        metadata.Contributors.Add("编辑A");
        metadata.Contributors.Add("译者B");

        Assert.AreEqual(2, metadata.Contributors.Count);
        CollectionAssert.Contains(metadata.Contributors.ToList(), "译者B");
    }

    [TestMethod]
    public void CustomMetadata_CanBeModified()
    {
        var metadata = new EpubMetadata();
        metadata.CustomMetadata["generator"] = "TestGenerator";
        metadata.CustomMetadata["source"] = "https://example.com";

        Assert.AreEqual(2, metadata.CustomMetadata.Count);
        Assert.AreEqual("TestGenerator", metadata.CustomMetadata["generator"]);
    }

    [TestMethod]
    public void MetaItems_CanBeModified()
    {
        var metadata = new EpubMetadata();
        metadata.MetaItems.Add(new EpubMetaItem
        {
            Name = "generator",
            Content = "TestGenerator",
            Property = null
        });

        Assert.AreEqual(1, metadata.MetaItems.Count);
        Assert.AreEqual("generator", metadata.MetaItems[0].Name);
    }
}

/// <summary>
/// EpubMetaItem 单元测试。
/// </summary>
[TestClass]
public sealed class EpubMetaItemTests
{
    [TestMethod]
    public void DefaultValues_AreNull()
    {
        var item = new EpubMetaItem();

        Assert.IsNull(item.Name);
        Assert.IsNull(item.Content);
        Assert.IsNull(item.Property);
        Assert.IsNull(item.Refines);
        Assert.IsNull(item.Scheme);
        Assert.IsNull(item.Id);
    }

    [TestMethod]
    public void Epub2Style_NameContent()
    {
        var item = new EpubMetaItem
        {
            Name = "cover",
            Content = "cover-image"
        };

        Assert.AreEqual("cover", item.Name);
        Assert.AreEqual("cover-image", item.Content);
    }

    [TestMethod]
    public void Epub3Style_PropertyContent()
    {
        var item = new EpubMetaItem
        {
            Property = "dcterms:modified",
            Content = "2024-01-01T00:00:00Z"
        };

        Assert.AreEqual("dcterms:modified", item.Property);
        Assert.AreEqual("2024-01-01T00:00:00Z", item.Content);
    }

    [TestMethod]
    public void ToString_ReturnsFormattedString()
    {
        var item = new EpubMetaItem
        {
            Name = "generator",
            Content = "TestGen"
        };

        Assert.AreEqual("generator: TestGen", item.ToString());
    }
}
