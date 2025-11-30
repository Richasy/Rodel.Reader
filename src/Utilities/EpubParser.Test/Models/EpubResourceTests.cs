// Copyright (c) Richasy. All rights reserved.

namespace EpubParser.Test.Models;

/// <summary>
/// EpubResource 单元测试。
/// </summary>
[TestClass]
public sealed class EpubResourceTests
{
    [TestMethod]
    public void IsImage_ImageMediaType_ReturnsTrue()
    {
        var resource = new EpubResource
        {
            MediaType = "image/jpeg"
        };

        Assert.IsTrue(resource.IsImage);
    }

    [TestMethod]
    public void IsImage_NonImageMediaType_ReturnsFalse()
    {
        var resource = new EpubResource
        {
            MediaType = "application/xhtml+xml"
        };

        Assert.IsFalse(resource.IsImage);
    }

    [TestMethod]
    public void IsImage_VariousImageTypes_ReturnsTrue()
    {
        var types = new[] { "image/png", "image/gif", "image/svg+xml", "image/webp" };

        foreach (var type in types)
        {
            var resource = new EpubResource { MediaType = type };
            Assert.IsTrue(resource.IsImage, $"MediaType {type} 应该被识别为图片");
        }
    }

    [TestMethod]
    public void DefaultValues_AreCorrect()
    {
        var resource = new EpubResource();

        Assert.AreEqual(string.Empty, resource.Id);
        Assert.AreEqual(string.Empty, resource.Href);
        Assert.AreEqual(string.Empty, resource.FullPath);
        Assert.AreEqual(string.Empty, resource.MediaType);
        Assert.IsNotNull(resource.Properties);
        Assert.AreEqual(0, resource.Properties.Count);
    }

    [TestMethod]
    public void Properties_CanBeModified()
    {
        var resource = new EpubResource();
        resource.Properties.Add("nav");
        resource.Properties.Add("cover-image");

        Assert.AreEqual(2, resource.Properties.Count);
        CollectionAssert.Contains(resource.Properties.ToList(), "nav");
    }
}

/// <summary>
/// EpubNavItem 单元测试。
/// </summary>
[TestClass]
public sealed class EpubNavItemTests
{
    [TestMethod]
    public void DefaultValues_AreCorrect()
    {
        var navItem = new EpubNavItem();

        Assert.AreEqual(string.Empty, navItem.Title);
        Assert.IsNull(navItem.Href);
        Assert.IsNull(navItem.Anchor);
        Assert.IsNull(navItem.FullPath);
        Assert.IsNotNull(navItem.Children);
        Assert.AreEqual(0, navItem.Children.Count);
    }

    [TestMethod]
    public void Children_CanBeModified()
    {
        var parent = new EpubNavItem { Title = "父级" };
        parent.Children.Add(new EpubNavItem { Title = "子级1" });
        parent.Children.Add(new EpubNavItem { Title = "子级2" });

        Assert.AreEqual(2, parent.Children.Count);
        Assert.AreEqual("子级1", parent.Children[0].Title);
    }

    [TestMethod]
    public void NestedChildren_WorksCorrectly()
    {
        var root = new EpubNavItem { Title = "根" };
        var level1 = new EpubNavItem { Title = "一级" };
        var level2 = new EpubNavItem { Title = "二级" };

        level1.Children.Add(level2);
        root.Children.Add(level1);

        Assert.AreEqual("二级", root.Children[0].Children[0].Title);
    }
}

/// <summary>
/// EpubCover 单元测试。
/// </summary>
[TestClass]
public sealed class EpubCoverTests
{
    [TestMethod]
    public async Task ReadContentAsync_ReturnsData()
    {
        var testData = TestDataFactory.CreateMinimalJpeg();
        var resource = new EpubResource
        {
            Id = "cover",
            MediaType = "image/jpeg",
            Href = "Images/cover.jpg",
            FullPath = "OEBPS/Images/cover.jpg"
        };

        var cover = new EpubCover(resource, () => Task.FromResult(testData));

        var data = await cover.ReadContentAsync();

        Assert.IsNotNull(data);
        Assert.AreEqual(testData.Length, data.Length);
    }

    [TestMethod]
    public void ReadContent_SyncVersion_Works()
    {
        var testData = TestDataFactory.CreateMinimalJpeg();
        var resource = new EpubResource
        {
            Id = "cover",
            MediaType = "image/jpeg"
        };

        var cover = new EpubCover(resource, () => Task.FromResult(testData));

        var data = cover.ReadContent();

        Assert.IsNotNull(data);
        Assert.AreEqual(testData.Length, data.Length);
    }

    [TestMethod]
    public void Resource_ReturnsOriginalResource()
    {
        var resource = new EpubResource
        {
            Id = "cover",
            MediaType = "image/jpeg",
            Href = "cover.jpg"
        };

        var cover = new EpubCover(resource, () => Task.FromResult(Array.Empty<byte>()));

        Assert.AreSame(resource, cover.Resource);
        Assert.AreEqual("image/jpeg", cover.Resource.MediaType);
    }
}
