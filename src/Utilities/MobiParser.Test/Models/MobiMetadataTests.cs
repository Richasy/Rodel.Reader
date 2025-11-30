// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Models;

/// <summary>
/// MobiMetadata 测试。
/// </summary>
[TestClass]
public sealed class MobiMetadataTests
{
    /// <summary>
    /// 测试默认值。
    /// </summary>
    [TestMethod]
    public void DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var metadata = new MobiMetadata();

        // Assert
        Assert.IsNull(metadata.Title);
        Assert.IsNotNull(metadata.Authors);
        Assert.AreEqual(0, metadata.Authors.Count);
        Assert.IsNull(metadata.Description);
        Assert.IsNull(metadata.Publisher);
        Assert.IsNull(metadata.Language);
        Assert.IsNull(metadata.PublishDate);
        Assert.IsNull(metadata.Identifier);
        Assert.IsNotNull(metadata.Subjects);
        Assert.AreEqual(0, metadata.Subjects.Count);
        Assert.IsNull(metadata.Rights);
        Assert.IsNotNull(metadata.Contributors);
        Assert.AreEqual(0, metadata.Contributors.Count);
        Assert.IsNull(metadata.Asin);
        Assert.IsNull(metadata.Isbn);
        Assert.AreEqual(0, metadata.MobiVersion);
        Assert.IsNotNull(metadata.CustomMetadata);
        Assert.AreEqual(0, metadata.CustomMetadata.Count);
    }

    /// <summary>
    /// 测试设置属性。
    /// </summary>
    [TestMethod]
    public void SetProperties_ShouldStoreValues()
    {
        // Arrange
        var metadata = new MobiMetadata
        {
            Title = "测试书籍",
            Description = "这是描述",
            Publisher = "测试出版社",
            Language = "zh-CN",
            PublishDate = "2024-01-01",
            Identifier = "978-7-123456-78-9",
            Rights = "版权所有",
            Asin = "B01234567",
            Isbn = "978-7-123456-78-9",
            MobiVersion = 6,
        };

        metadata.Authors.Add("作者一");
        metadata.Authors.Add("作者二");
        metadata.Subjects.Add("科幻");
        metadata.Contributors.Add("贡献者");
        metadata.CustomMetadata["key"] = "value";

        // Assert
        Assert.AreEqual("测试书籍", metadata.Title);
        Assert.AreEqual("这是描述", metadata.Description);
        Assert.AreEqual("测试出版社", metadata.Publisher);
        Assert.AreEqual("zh-CN", metadata.Language);
        Assert.AreEqual("2024-01-01", metadata.PublishDate);
        Assert.AreEqual("978-7-123456-78-9", metadata.Identifier);
        Assert.AreEqual("版权所有", metadata.Rights);
        Assert.AreEqual("B01234567", metadata.Asin);
        Assert.AreEqual("978-7-123456-78-9", metadata.Isbn);
        Assert.AreEqual(6, metadata.MobiVersion);
        Assert.AreEqual(2, metadata.Authors.Count);
        Assert.AreEqual("作者一", metadata.Authors[0]);
        Assert.AreEqual(1, metadata.Subjects.Count);
        Assert.AreEqual(1, metadata.Contributors.Count);
        Assert.AreEqual("value", metadata.CustomMetadata["key"]);
    }
}
