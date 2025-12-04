// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Test.UnitTests;

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
        _testDirectory = Path.Combine(Path.GetTempPath(), $"FanQieTest_{Guid.NewGuid():N}");
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
        var cacheManager = new CacheManager(_testDirectory, "12345");

        // Act
        await cacheManager.InitializeAsync("tochash123", "测试书籍");

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
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123", "测试书籍");

        // Act
        var manifest = await cacheManager.LoadManifestAsync();

        // Assert
        Assert.IsNotNull(manifest);
        Assert.AreEqual("12345", manifest.BookId);
        Assert.AreEqual("tochash123", manifest.TocHash);
        Assert.AreEqual("测试书籍", manifest.Title);
    }

    [TestMethod]
    public async Task SaveAndLoadChapter_RoundTrips()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123");

        var chapter = new CachedChapter
        {
            ChapterId = "ch001",
            Title = "第一章",
            Order = 1,
            Status = ChapterStatus.Downloaded,
            HtmlContent = "<p>内容</p>",
            WordCount = 100,
        };

        // Act
        await cacheManager.SaveChapterAsync(chapter);
        var loaded = await cacheManager.LoadChapterAsync("ch001");

        // Assert
        Assert.IsNotNull(loaded);
        Assert.AreEqual("ch001", loaded.ChapterId);
        Assert.AreEqual("第一章", loaded.Title);
        Assert.AreEqual(ChapterStatus.Downloaded, loaded.Status);
        Assert.AreEqual("<p>内容</p>", loaded.HtmlContent);
    }

    [TestMethod]
    public async Task SaveChapter_UpdatesManifest()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123");

        var chapter = new CachedChapter
        {
            ChapterId = "ch001",
            Title = "第一章",
            Order = 1,
            Status = ChapterStatus.Downloaded,
        };

        // Act
        await cacheManager.SaveChapterAsync(chapter);
        var manifest = await cacheManager.LoadManifestAsync();

        // Assert
        Assert.IsNotNull(manifest);
        Assert.IsTrue(manifest.CachedChapterIds.Contains("ch001"));
    }

    [TestMethod]
    public async Task SaveFailedChapter_UpdatesManifestFailedList()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123");

        var chapter = new CachedChapter
        {
            ChapterId = "ch001",
            Title = "第一章",
            Order = 1,
            Status = ChapterStatus.Failed,
            FailureReason = "网络错误",
        };

        // Act
        await cacheManager.SaveChapterAsync(chapter);
        var manifest = await cacheManager.LoadManifestAsync();

        // Assert
        Assert.IsNotNull(manifest);
        Assert.IsTrue(manifest.FailedChapterIds.Contains("ch001"));
        Assert.IsFalse(manifest.CachedChapterIds.Contains("ch001"));
    }

    [TestMethod]
    public async Task SaveAndLoadImage_RoundTrips()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123");
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header

        // Act
        await cacheManager.SaveImageAsync("cover", imageData);
        var loaded = await cacheManager.LoadImageAsync("cover");

        // Assert
        Assert.IsNotNull(loaded);
        CollectionAssert.AreEqual(imageData, loaded);
    }

    [TestMethod]
    public async Task ImageExists_ReturnsTrueForExisting()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123");
        await cacheManager.SaveImageAsync("cover", [0xFF, 0xD8]);

        // Act & Assert
        Assert.IsTrue(cacheManager.ImageExists("cover"));
        Assert.IsFalse(cacheManager.ImageExists("nonexistent"));
    }

    [TestMethod]
    public async Task LoadAllChapters_ReturnsAllSaved()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123");

        for (var i = 1; i <= 5; i++)
        {
            await cacheManager.SaveChapterAsync(new CachedChapter
            {
                ChapterId = $"ch{i:D3}",
                Title = $"第{i}章",
                Order = i,
                Status = ChapterStatus.Downloaded,
            });
        }

        // Act
        var chapters = await cacheManager.LoadAllChaptersAsync();

        // Assert
        Assert.AreEqual(5, chapters.Count);
    }

    [TestMethod]
    public async Task Cleanup_RemovesAllFiles()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123");
        await cacheManager.SaveChapterAsync(new CachedChapter
        {
            ChapterId = "ch001",
            Title = "第一章",
            Order = 1,
            Status = ChapterStatus.Downloaded,
        });

        // Act
        cacheManager.Cleanup();

        // Assert
        Assert.IsFalse(Directory.Exists(cacheManager.CacheRoot));
        Assert.IsFalse(cacheManager.Exists());
    }

    [TestMethod]
    public async Task GetState_ReturnsCorrectState()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123", "测试");

        await cacheManager.SaveChapterAsync(new CachedChapter
        {
            ChapterId = "ch001",
            Title = "第一章",
            Order = 1,
            Status = ChapterStatus.Downloaded,
        });

        await cacheManager.SaveChapterAsync(new CachedChapter
        {
            ChapterId = "ch002",
            Title = "第二章",
            Order = 2,
            Status = ChapterStatus.Failed,
        });

        // Act
        var state = await cacheManager.GetStateAsync();

        // Assert
        Assert.IsNotNull(state);
        Assert.AreEqual("12345", state.BookId);
        Assert.AreEqual("tochash123", state.TocHash);
        Assert.IsTrue(state.CachedChapterIds.Contains("ch001"));
        Assert.IsTrue(state.FailedChapterIds.Contains("ch002"));
    }

    [TestMethod]
    public async Task CacheState_IsValid_ReturnsTrueForMatchingHash()
    {
        // Arrange
        var cacheManager = new CacheManager(_testDirectory, "12345");
        await cacheManager.InitializeAsync("tochash123");

        // Act
        var state = await cacheManager.GetStateAsync();

        // Assert
        Assert.IsNotNull(state);
        Assert.IsTrue(state.IsValid("tochash123"));
        Assert.IsFalse(state.IsValid("differenthash"));
    }
}
