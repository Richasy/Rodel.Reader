// Copyright (c) Richasy. All rights reserved.

namespace EpubParser.Test.Reader;

/// <summary>
/// EpubReader 单元测试。
/// </summary>
[TestClass]
public sealed class EpubReaderTests
{
    [TestMethod]
    public async Task ReadAsync_MinimalEpub_ParsesSuccessfully()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        Assert.IsNotNull(book);
        Assert.IsNotNull(book.Metadata);
        Assert.AreEqual("测试书籍", book.Metadata.Title);
        AssertExtensions.HasItems(book.Metadata.Authors);
        Assert.AreEqual("测试作者", book.Metadata.Authors[0]);
        Assert.AreEqual("zh", book.Metadata.Language);
    }

    [TestMethod]
    public async Task ReadAsync_HasCover_ReturnsCover()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        Assert.IsNotNull(book.Cover);
        Assert.AreEqual("image/jpeg", book.Cover.Resource.MediaType);

        var coverData = await book.Cover.ReadContentAsync();
        Assert.IsTrue(coverData.Length > 0, "封面数据应该不为空");
    }

    [TestMethod]
    public async Task ReadAsync_HasNavigation_ReturnsNavItems()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        AssertExtensions.HasItems(book.Navigation);
        Assert.AreEqual("第一章", book.Navigation[0].Title);
    }

    [TestMethod]
    public async Task ReadAsync_HasReadingOrder_ReturnsSpineItems()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        AssertExtensions.HasItems(book.ReadingOrder);
        Assert.AreEqual("application/xhtml+xml", book.ReadingOrder[0].MediaType);
    }

    [TestMethod]
    public async Task ReadAsync_HasResources_ReturnsAllManifestItems()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        AssertExtensions.HasItems(book.Resources);
        // 至少应该有：nav, ncx, chapter1, cover-image
        Assert.IsTrue(book.Resources.Count >= 4, $"资源数量应该 >= 4，实际: {book.Resources.Count}");
    }

    [TestMethod]
    public async Task ReadAsync_WithCustomMetadata_ParsesMetaItems()
    {
        using var stream = TestDataFactory.CreateEpubWithCustomMetadata();
        var book = await EpubReader.ReadAsync(stream);

        Assert.IsNotNull(book.Metadata.MetaItems);
        Assert.IsTrue(book.Metadata.MetaItems.Count > 0, "应该有 meta 元素");

        // 检查自定义元数据
        Assert.IsTrue(book.Metadata.CustomMetadata.ContainsKey("generator"), "应该包含 generator");
        Assert.AreEqual("EpubParser.Test", book.Metadata.CustomMetadata["generator"]);
    }

    [TestMethod]
    public async Task ReadAsync_WithContributors_ParsesContributors()
    {
        using var stream = TestDataFactory.CreateEpubWithCustomMetadata();
        var book = await EpubReader.ReadAsync(stream);

        AssertExtensions.HasItems(book.Metadata.Contributors);
        CollectionAssert.Contains(book.Metadata.Contributors.ToList(), "贡献者B");
        CollectionAssert.Contains(book.Metadata.Contributors.ToList(), "贡献者C");
    }

    [TestMethod]
    public async Task ReadAsync_WithSubjects_ParsesSubjects()
    {
        using var stream = TestDataFactory.CreateEpubWithCustomMetadata();
        var book = await EpubReader.ReadAsync(stream);

        AssertExtensions.HasItems(book.Metadata.Subjects);
        CollectionAssert.Contains(book.Metadata.Subjects.ToList(), "科幻");
        CollectionAssert.Contains(book.Metadata.Subjects.ToList(), "冒险");
    }

    [TestMethod]
    public async Task ReadAsync_NestedToc_ParsesHierarchy()
    {
        using var stream = TestDataFactory.CreateEpubWithNestedToc();
        var book = await EpubReader.ReadAsync(stream);

        AssertExtensions.HasItems(book.Navigation);
        var firstItem = book.Navigation[0];
        Assert.AreEqual("第一部分", firstItem.Title);
        AssertExtensions.HasItems(firstItem.Children);
        Assert.AreEqual(2, firstItem.Children.Count, "子项数量应该为 2");
        Assert.AreEqual("第一章", firstItem.Children[0].Title);
        Assert.AreEqual("第二章", firstItem.Children[1].Title);
    }

    [TestMethod]
    public async Task ReadAsync_Epub2_ParsesNcxNavigation()
    {
        using var stream = TestDataFactory.CreateEpub2Stream();
        var book = await EpubReader.ReadAsync(stream);

        AssertExtensions.HasItems(book.Navigation);
        Assert.AreEqual("第一章", book.Navigation[0].Title);
    }

    [TestMethod]
    public async Task ReadAsync_Epub2_FindsCoverFromMeta()
    {
        using var stream = TestDataFactory.CreateEpub2Stream();
        var book = await EpubReader.ReadAsync(stream);

        Assert.IsNotNull(book.Cover);
        Assert.AreEqual("image/jpeg", book.Cover.Resource.MediaType);
    }

    [TestMethod]
    public async Task ReadAsync_MangaEpub_ParsesImageResources()
    {
        using var stream = TestDataFactory.CreateMangaEpub();
        var book = await EpubReader.ReadAsync(stream);

        // 检查图片资源
        var images = book.Resources.Where(r => r.MediaType.StartsWith("image/", StringComparison.Ordinal)).ToList();
        Assert.IsTrue(images.Count >= 3, $"图片数量应该 >= 3，实际: {images.Count}"); // cover + 2 pages

        // 检查语言
        Assert.AreEqual("ja", book.Metadata.Language);
    }

    [TestMethod]
    public async Task ReadAsync_ReadResourceContentAsString_ReturnsContent()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        var chapter = book.ReadingOrder[0];
        var content = await book.ReadResourceContentAsStringAsync(chapter);

        Assert.IsNotNull(content);
        AssertExtensions.ContainsText(content, "第一章");
    }

    [TestMethod]
    public async Task ReadAsync_ReadResourceContent_ReturnsBytes()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        var cover = book.Resources.First(r => r.MediaType == "image/jpeg");
        var bytes = await book.ReadResourceContentAsync(cover);

        Assert.IsNotNull(bytes);
        Assert.IsTrue(bytes.Length > 0, "字节数组应该不为空");
    }

    [TestMethod]
    public void Read_SyncVersion_Works()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = EpubReader.Read(stream);

        Assert.IsNotNull(book);
        Assert.AreEqual("测试书籍", book.Metadata.Title);
    }

    [TestMethod]
    public async Task ReadAsync_DisposesCorrectly()
    {
        EpubBook book;
        using (var stream = TestDataFactory.CreateMinimalEpubStream())
        {
            book = await EpubReader.ReadAsync(stream);
        }

        // 书籍应该仍然持有 archive
        Assert.IsNotNull(book);

        // Dispose 书籍
        book.Dispose();

        // Dispose 后再次调用应该不会抛异常
        book.Dispose();
    }

    [TestMethod]
    public async Task ReadAsync_FindResourceByHref_Works()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        var resource = book.FindResourceByHref("Text/chapter1.xhtml");
        Assert.IsNotNull(resource);
        Assert.AreEqual("chapter1", resource.Id);
    }

    [TestMethod]
    public async Task ReadAsync_FindResourceById_Works()
    {
        using var stream = TestDataFactory.CreateMinimalEpubStream();
        var book = await EpubReader.ReadAsync(stream);

        var resource = book.FindResourceById("chapter1");
        Assert.IsNotNull(resource);
        AssertExtensions.ContainsText(resource.Href, "chapter1.xhtml");
    }

    [TestMethod]
    public async Task ReadAsync_ImagesProperty_FiltersImages()
    {
        using var stream = TestDataFactory.CreateMangaEpub();
        var book = await EpubReader.ReadAsync(stream);

        AssertExtensions.HasItems(book.Images);
        foreach (var image in book.Images)
        {
            Assert.IsTrue(image.IsImage);
            Assert.IsTrue(image.MediaType.StartsWith("image/", StringComparison.Ordinal));
        }
    }
}
