// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Test.UnitTests;

/// <summary>
/// 缓存管理器测试.
/// </summary>
[TestClass]
public class CacheManagerTests
{
    private string _testDirectory = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LegadoTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [TestMethod]
    public async Task Initialize_CreatesDirectoryStructure()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "https://example.com/book/123");

        // Act
        await cacheManager.InitializeAsync("https://example.com/book/123", "tochash123", "测试书籍", "https://source.com", "http://192.168.1.1:1234");

        // Assert
        Assert.IsTrue(Directory.Exists(cacheManager.CacheRoot));
        Assert.IsTrue(Directory.Exists(cacheManager.ChaptersDirectory));
        Assert.IsTrue(Directory.Exists(cacheManager.ImagesDirectory));
        Assert.IsTrue(cacheManager.Exists());
    }

    [TestMethod]
    public async Task SaveAndLoadManifest_RoundTrips()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123", "测试书籍", "https://source.com", "http://192.168.1.1:1234");

        // Act
        var manifest = await cacheManager.LoadManifestAsync();

        // Assert
        Assert.IsNotNull(manifest);
        Assert.AreEqual(bookUrl, manifest.BookUrl);
        Assert.AreEqual("tochash123", manifest.TocHash);
        Assert.AreEqual("测试书籍", manifest.Title);
        Assert.AreEqual("https://source.com", manifest.BookSource);
        Assert.AreEqual("http://192.168.1.1:1234", manifest.ServerUrl);
    }

    [TestMethod]
    public async Task SaveAndLoadChapter_RoundTrips()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123");

        var chapter = new CachedChapter
        {
            ChapterIndex = 0,
            ChapterUrl = "https://example.com/chapter/1",
            Title = "第一章",
            Status = ChapterStatus.Downloaded,
            HtmlContent = "<p>内容</p>",
        };

        // Act
        await cacheManager.SaveChapterAsync(chapter);
        var loaded = await cacheManager.LoadChapterAsync(0);

        // Assert
        Assert.IsNotNull(loaded);
        Assert.AreEqual(0, loaded.ChapterIndex);
        Assert.AreEqual("第一章", loaded.Title);
        Assert.AreEqual(ChapterStatus.Downloaded, loaded.Status);
        Assert.AreEqual("<p>内容</p>", loaded.HtmlContent);
    }

    [TestMethod]
    public async Task SaveChapter_UpdatesManifest()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123");

        var chapter = new CachedChapter
        {
            ChapterIndex = 0,
            ChapterUrl = "https://example.com/chapter/1",
            Title = "第一章",
            Status = ChapterStatus.Downloaded,
        };

        // Act
        await cacheManager.SaveChapterAsync(chapter);
        var manifest = await cacheManager.LoadManifestAsync();

        // Assert
        Assert.IsNotNull(manifest);
        Assert.Contains(0, manifest.CachedChapterIndexes);
    }

    [TestMethod]
    public async Task SaveFailedChapter_UpdatesManifestFailedList()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123");

        var chapter = new CachedChapter
        {
            ChapterIndex = 0,
            ChapterUrl = "https://example.com/chapter/1",
            Title = "第一章",
            Status = ChapterStatus.Failed,
            FailureReason = "网络错误",
        };

        // Act
        await cacheManager.SaveChapterAsync(chapter);
        var manifest = await cacheManager.LoadManifestAsync();

        // Assert
        Assert.IsNotNull(manifest);
        Assert.Contains(0, manifest.FailedChapterIndexes);
        Assert.DoesNotContain(0, manifest.CachedChapterIndexes);
    }

    [TestMethod]
    public async Task SaveVolumeChapter_UpdatesManifestCachedList()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123");

        var chapter = new CachedChapter
        {
            ChapterIndex = 0,
            ChapterUrl = "https://example.com/chapter/1",
            Title = "卷一",
            IsVolume = true,
            Status = ChapterStatus.Volume,
        };

        // Act
        await cacheManager.SaveChapterAsync(chapter);
        var manifest = await cacheManager.LoadManifestAsync();

        // Assert
        Assert.IsNotNull(manifest);
        Assert.Contains(0, manifest.CachedChapterIndexes);
    }

    [TestMethod]
    public async Task SaveAndLoadImage_RoundTrips()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123");
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header

        // Act
        await cacheManager.SaveImageAsync("cover", imageData);
        var loaded = await cacheManager.LoadImageAsync("cover");

        // Assert
        Assert.IsNotNull(loaded);
        CollectionAssert.AreEqual(imageData, loaded);
    }

    [TestMethod]
    public async Task ImageExists_ReturnsTrueForExistingImage()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123");
        await cacheManager.SaveImageAsync("cover", [0xFF, 0xD8]);

        // Act & Assert
        Assert.IsTrue(cacheManager.ImageExists("cover"));
        Assert.IsFalse(cacheManager.ImageExists("nonexistent"));
    }

    [TestMethod]
    public async Task Cleanup_RemovesCacheDirectory()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123");
        Assert.IsTrue(cacheManager.Exists());

        // Act
        cacheManager.Cleanup();

        // Assert
        Assert.IsFalse(cacheManager.Exists());
    }

    [TestMethod]
    public async Task GetState_ReturnsCacheState()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123", "测试书籍", "https://source.com", "http://192.168.1.1:1234");

        // Act
        var state = await cacheManager.GetStateAsync();

        // Assert
        Assert.IsNotNull(state);
        Assert.AreEqual(bookUrl, state.BookUrl);
        Assert.AreEqual("tochash123", state.TocHash);
        Assert.AreEqual("测试书籍", state.Title);
        Assert.IsTrue(state.IsValid("tochash123"));
        Assert.IsFalse(state.IsValid("differenthash"));
    }

    [TestMethod]
    public async Task LoadAllChapters_ReturnsAllCachedChapters()
    {
        // Arrange
        var bookUrl = "https://example.com/book/123";
        var cacheManager = new CacheManager(_testDirectory, bookUrl);
        await cacheManager.InitializeAsync(bookUrl, "tochash123");

        await cacheManager.SaveChapterAsync(new CachedChapter
        {
            ChapterIndex = 0,
            ChapterUrl = "url1",
            Title = "第一章",
            Status = ChapterStatus.Downloaded,
        });

        await cacheManager.SaveChapterAsync(new CachedChapter
        {
            ChapterIndex = 1,
            ChapterUrl = "url2",
            Title = "第二章",
            Status = ChapterStatus.Downloaded,
        });

        // Act
        var chapters = await cacheManager.LoadAllChaptersAsync();

        // Assert
        Assert.HasCount(2, chapters);
    }

    [TestMethod]
    public void DifferentBookUrls_CreateDifferentCacheDirectories()
    {
        // Arrange
        var cacheManager1 = new CacheManager(_testDirectory, "https://example.com/book/123");
        var cacheManager2 = new CacheManager(_testDirectory, "https://example.com/book/456");

        // Assert
        Assert.AreNotEqual(cacheManager1.CacheRoot, cacheManager2.CacheRoot);
    }
}
