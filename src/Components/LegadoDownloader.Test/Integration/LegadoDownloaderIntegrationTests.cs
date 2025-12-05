// Copyright (c) Richasy. All rights reserved.

using Moq;
using Richasy.RodelReader.Components.Legado.Abstractions;
using Richasy.RodelReader.Components.Legado.Models;
using Richasy.RodelReader.Components.Legado.Services;
using Richasy.RodelReader.Sources.Legado;
using Richasy.RodelReader.Sources.Legado.Models;
using Richasy.RodelReader.Sources.Legado.Models.Enums;
using Richasy.RodelReader.Utilities.EpubParser;
using Richasy.RodelReader.Utilities.EpubGenerator;
using EpubGenMetadata = Richasy.RodelReader.Utilities.EpubGenerator.EpubMetadata;

namespace Richasy.RodelReader.Components.Legado.Test.Integration;

/// <summary>
/// Legado 下载器集成测试.
/// 使用真实 API 进行完整的下载和同步流程测试.
/// 测试按 Priority 顺序执行，以保证同步流程的正确性.
/// </summary>
[TestClass]
[TestCategory("Integration")]
[DoNotParallelize]
public class LegadoDownloaderIntegrationTests
{
    // 测试服务配置
    private const string TestServerUrl = "https://book.richasy.net/";
    private const string TestAccessToken = "richasy:44d1b135eb190a25a28dd7b5310e97cb";

    // 测试章节范围
    private const int InitialStartChapter = 0;
    private const int InitialEndChapter = 9; // 先下载前 10 章
    private const int ExtendedStartChapter = 0;
    private const int ExtendedEndChapter = 19; // 扩展到前 20 章

    private static LegadoClient? _client;
    private static IEpubBuilder? _epubBuilder;
    private static string _testDirectory = null!;
    private static string _outputDirectory = null!;
    private static string _tempDirectory = null!;
    private static Book? _testBook;
    private static string? _generatedEpubPath;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        // 创建测试目录
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LegadoDownloaderTest_{DateTime.Now:yyyyMMdd_HHmmss}");
        _outputDirectory = Path.Combine(_testDirectory, "output");
        _tempDirectory = Path.Combine(_testDirectory, "temp");

        Directory.CreateDirectory(_testDirectory);
        Directory.CreateDirectory(_outputDirectory);
        Directory.CreateDirectory(_tempDirectory);

        Console.WriteLine($"测试目录: {_testDirectory}");

        // 创建客户端
        var options = new LegadoClientOptions
        {
            BaseUrl = TestServerUrl,
            ServerType = ServerType.HectorqinReader,
            AccessToken = TestAccessToken,
            Timeout = TimeSpan.FromSeconds(120),
            IgnoreSslErrors = true,
        };

        _client = new LegadoClient(options);
        _epubBuilder = new EpubBuilder();

        // 获取书架
        Console.WriteLine($"连接服务器: {TestServerUrl}");
        var bookshelf = await _client.GetBookshelfAsync();

        if (bookshelf.Count == 0)
        {
            Assert.Inconclusive("书架为空，无法进行测试");
        }

        // 选择第一本书进行测试
        _testBook = bookshelf[0];
        Console.WriteLine($"测试书籍: {_testBook.Name} by {_testBook.Author}");
        Console.WriteLine($"书架共有 {bookshelf.Count} 本书");
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client?.Dispose();

        // 保留测试目录以便检查结果
        Console.WriteLine($"测试完成，结果保存在: {_testDirectory}");
    }

    /// <summary>
    /// 步骤 1: 获取书架并验证结果.
    /// </summary>
    [TestMethod]
    [Priority(1)]
    public async Task Step1_GetBookshelf_ReturnsValidResult()
    {
        // Arrange
        Assert.IsNotNull(_client);

        // Act
        var bookshelf = await _client.GetBookshelfAsync();

        // Assert
        Assert.IsNotNull(bookshelf);
        Assert.IsNotEmpty(bookshelf, "书架应该有书籍");

        Console.WriteLine($"书架书籍列表:");
        foreach (var book in bookshelf.Take(5))
        {
            Console.WriteLine($"  - {book.Name} by {book.Author}");
        }

        if (bookshelf.Count > 5)
        {
            Console.WriteLine($"  ... 共 {bookshelf.Count} 本书");
        }

        Assert.IsNotNull(_testBook);
    }

    /// <summary>
    /// 步骤 2: 获取章节列表并验证.
    /// </summary>
    [TestMethod]
    [Priority(2)]
    public async Task Step2_GetChapterList_ReturnsValidResult()
    {
        // Arrange
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_testBook);

        // Act
        var chapters = await _client.GetChapterListAsync(_testBook.BookUrl);

        // Assert
        Assert.IsNotNull(chapters);
        Assert.IsNotEmpty(chapters, "章节列表不应为空");

        Console.WriteLine($"《{_testBook.Name}》章节信息:");
        Console.WriteLine($"  总章节数: {chapters.Count}");
        Console.WriteLine($"  前 5 章:");
        foreach (var chapter in chapters.Take(5))
        {
            var volumeTag = chapter.IsVolume ? "[卷]" : "";
            Console.WriteLine($"    {chapter.Index}: {volumeTag}{chapter.Title}");
        }
    }

    /// <summary>
    /// 步骤 3: 下载前 10 章，验证基本下载功能.
    /// </summary>
    [TestMethod]
    [Priority(3)]
    public async Task Step3_DownloadChapters0To9_CreatesValidEpub()
    {
        // Arrange
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_epubBuilder);
        Assert.IsNotNull(_testBook);

        var downloadService = new LegadoDownloadService(_client, _epubBuilder);

        var syncOptions = new SyncOptions
        {
            TempDirectory = _tempDirectory,
            OutputDirectory = _outputDirectory,
            ContinueOnError = true,
            RetryFailedChapters = false,
            StartChapterIndex = InitialStartChapter,
            EndChapterIndex = InitialEndChapter,
        };

        Console.WriteLine($"开始下载书籍章节 {InitialStartChapter}-{InitialEndChapter}...");

        var progress = new Progress<SyncProgress>(p =>
        {
            Console.WriteLine($"[{p.Phase}] {p.TotalProgress:F1}% - {p.Message}");
        });

        // Act
        var result = await downloadService.SyncBookAsync(_testBook, syncOptions, progress);

        // Assert
        Assert.IsTrue(result.Success, $"同步应该成功: {result.ErrorMessage}");
        Assert.IsNotNull(result.EpubPath);
        Assert.IsTrue(File.Exists(result.EpubPath), "EPUB 文件应该存在");

        _generatedEpubPath = result.EpubPath;

        Console.WriteLine($"EPUB 生成成功: {result.EpubPath}");
        Console.WriteLine($"文件大小: {new FileInfo(result.EpubPath).Length / 1024.0:F2} KB");
        Console.WriteLine($"新下载: {result.Statistics?.NewlyDownloaded} 章节");
        Console.WriteLine($"失败: {result.Statistics?.Failed} 章节");
        Console.WriteLine($"卷标题: {result.Statistics?.VolumeChapters} 个");
        Console.WriteLine($"总计: {result.Statistics?.TotalChapters} 章节");
        Console.WriteLine($"耗时: {result.Statistics?.Duration.TotalSeconds:F2} 秒");

        // 验证 EPUB 可以被解析
        using var epubBook = await EpubReader.ReadAsync(result.EpubPath);
        Assert.IsNotNull(epubBook);
        Assert.IsNotNull(epubBook.Metadata.Title);
        Console.WriteLine($"EPUB 标题: {epubBook.Metadata.Title}");
        Console.WriteLine($"阅读顺序: {epubBook.ReadingOrder.Count} 项");

        // 验证有 Legado 元数据
        var bookUrlMeta = epubBook.Metadata.MetaItems
            .FirstOrDefault(m => m.Name == "legado:book-url" || m.Property == "legado:book-url");
        Assert.IsNotNull(bookUrlMeta, "EPUB 应该包含 legado:book-url 元数据");
    }

    /// <summary>
    /// 步骤 4: 验证已下载章节的内容.
    /// </summary>
    [TestMethod]
    [Priority(4)]
    public async Task Step4_VerifyDownloadedChapters()
    {
        // Arrange
        if (string.IsNullOrEmpty(_generatedEpubPath) || !File.Exists(_generatedEpubPath))
        {
            Assert.Inconclusive("没有可验证的 EPUB 文件");
            return;
        }

        Console.WriteLine($"验证 EPUB 内容: {_generatedEpubPath}");

        // 读取 EPUB
        using var epubBook = await EpubReader.ReadAsync(_generatedEpubPath);

        // 统计章节状态
        var downloadedCount = 0;
        var volumeCount = 0;
        var failedCount = 0;

        foreach (var resource in epubBook.ReadingOrder)
        {
            if (!resource.Href.Contains("chapter", StringComparison.OrdinalIgnoreCase) ||
                resource.Href.Contains("nav", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                var html = await epubBook.ReadResourceContentAsStringAsync(resource);

                if (html.Contains("legado:status=volume", StringComparison.OrdinalIgnoreCase) ||
                    html.Contains("data-legado-status=\"volume\"", StringComparison.OrdinalIgnoreCase))
                {
                    volumeCount++;
                    Console.WriteLine($"  📁 {resource.Href} (卷标题)");
                }
                else if (html.Contains("legado:status=failed", StringComparison.OrdinalIgnoreCase) ||
                         html.Contains("data-legado-status=\"failed\"", StringComparison.OrdinalIgnoreCase))
                {
                    failedCount++;
                    Console.WriteLine($"  ❌ {resource.Href} (失败)");
                }
                else if (html.Contains("legado:status=downloaded", StringComparison.OrdinalIgnoreCase) ||
                         html.Contains("legado:chapter-index", StringComparison.OrdinalIgnoreCase))
                {
                    downloadedCount++;
                    Console.WriteLine($"  ✅ {resource.Href} (已下载)");
                }
            }
            catch
            {
                failedCount++;
            }
        }

        Console.WriteLine($"\n章节统计:");
        Console.WriteLine($"  已下载: {downloadedCount}");
        Console.WriteLine($"  卷标题: {volumeCount}");
        Console.WriteLine($"  失败: {failedCount}");

        Assert.IsGreaterThan(0, downloadedCount, "应该有成功下载的章节");
    }

    /// <summary>
    /// 步骤 5: 增量同步 - 扩展到前 20 章.
    /// </summary>
    [TestMethod]
    [Priority(5)]
    public async Task Step5_IncrementalSync_ExtendToChapters0To19()
    {
        // Arrange
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_epubBuilder);
        Assert.IsNotNull(_testBook);

        if (string.IsNullOrEmpty(_generatedEpubPath) || !File.Exists(_generatedEpubPath))
        {
            Assert.Inconclusive("没有现有 EPUB 文件");
            return;
        }

        Console.WriteLine($"使用现有 EPUB: {_generatedEpubPath}");

        var downloadService = new LegadoDownloadService(_client, _epubBuilder);

        var syncOptions = new SyncOptions
        {
            TempDirectory = _tempDirectory,
            OutputDirectory = _outputDirectory,
            ExistingEpubPath = _generatedEpubPath,
            ContinueOnError = true,
            RetryFailedChapters = true,
            StartChapterIndex = ExtendedStartChapter,
            EndChapterIndex = ExtendedEndChapter,
        };

        Console.WriteLine($"增量同步: 扩展到 {ExtendedStartChapter}-{ExtendedEndChapter} 章");

        var progress = new Progress<SyncProgress>(p =>
        {
            Console.WriteLine($"[{p.Phase}] {p.TotalProgress:F1}% - {p.Message}");
        });

        // Act
        var result = await downloadService.SyncBookAsync(_testBook, syncOptions, progress);

        // Assert
        Assert.IsTrue(result.Success, $"同步应该成功: {result.ErrorMessage}");
        Assert.IsNotNull(result.Statistics);

        Console.WriteLine($"同步结果:");
        Console.WriteLine($"  新下载: {result.Statistics.NewlyDownloaded}");
        Console.WriteLine($"  复用: {result.Statistics.Reused}");
        Console.WriteLine($"  失败: {result.Statistics.Failed}");
        Console.WriteLine($"  卷标题: {result.Statistics.VolumeChapters}");
        Console.WriteLine($"  总章节: {result.Statistics.TotalChapters}");

        _generatedEpubPath = result.EpubPath;

        // 验证 EPUB 已更新
        Assert.IsNotNull(result.EpubPath);
        Assert.IsTrue(File.Exists(result.EpubPath), "EPUB 文件应该存在");

        // 验证复用了之前下载的章节
        Assert.IsGreaterThan(0, result.Statistics.Reused, "应该复用了之前下载的章节");

        // 验证章节数量扩展了
        var expectedChapters = ExtendedEndChapter - ExtendedStartChapter + 1;
        Assert.AreEqual(expectedChapters, result.Statistics.TotalChapters, $"应该有 {expectedChapters} 章节");
    }

    /// <summary>
    /// 步骤 6: 验证增量同步结果.
    /// </summary>
    [TestMethod]
    [Priority(6)]
    public async Task Step6_VerifyIncrementalSyncResult()
    {
        // Arrange
        if (string.IsNullOrEmpty(_generatedEpubPath) || !File.Exists(_generatedEpubPath))
        {
            Assert.Inconclusive("没有现有 EPUB 文件");
            return;
        }

        Console.WriteLine($"验证增量同步结果: {_generatedEpubPath}");

        // 读取 EPUB 验证内容
        using var epubBook = await EpubReader.ReadAsync(_generatedEpubPath);

        // 验证元数据
        Assert.IsNotNull(epubBook.Metadata.Title);
        Console.WriteLine($"书名: {epubBook.Metadata.Title}");
        Console.WriteLine($"阅读顺序: {epubBook.ReadingOrder.Count} 项");

        // 统计各种章节类型
        var downloadedCount = 0;
        var volumeCount = 0;
        var failedCount = 0;
        var chapterCount = 0;

        foreach (var resource in epubBook.ReadingOrder)
        {
            if (!resource.Href.Contains("chapter", StringComparison.OrdinalIgnoreCase) ||
                resource.Href.Contains("nav", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            chapterCount++;

            try
            {
                var html = await epubBook.ReadResourceContentAsStringAsync(resource);

                if (html.Contains("legado:status=volume", StringComparison.OrdinalIgnoreCase))
                {
                    volumeCount++;
                }
                else if (html.Contains("legado:status=failed", StringComparison.OrdinalIgnoreCase))
                {
                    failedCount++;
                }
                else if (html.Contains("legado:status=downloaded", StringComparison.OrdinalIgnoreCase))
                {
                    downloadedCount++;
                }
            }
            catch
            {
                failedCount++;
            }
        }

        Console.WriteLine($"\n章节统计:");
        Console.WriteLine($"  总章节数: {chapterCount}");
        Console.WriteLine($"  已下载: {downloadedCount}");
        Console.WriteLine($"  卷标题: {volumeCount}");
        Console.WriteLine($"  失败: {failedCount}");

        // 验证至少有章节
        var totalProcessed = downloadedCount + volumeCount + failedCount;
        Assert.IsGreaterThan(0, totalProcessed, "应该有处理过的章节");

        // 验证章节数量
        var expectedChapters = ExtendedEndChapter - ExtendedStartChapter + 1;
        Console.WriteLine($"  预期章节: {expectedChapters}");
        Assert.AreEqual(expectedChapters, chapterCount, "章节数量应该匹配");

        Console.WriteLine("\n✅ 完整性验证通过！");
        Console.WriteLine($"\n最终 EPUB 路径: {_generatedEpubPath}");
    }

    /// <summary>
    /// 步骤 7: 测试从中间范围开始下载.
    /// </summary>
    [TestMethod]
    [Priority(7)]
    public async Task Step7_DownloadMiddleRange_Chapters50To59()
    {
        // Arrange
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_epubBuilder);
        Assert.IsNotNull(_testBook);

        var downloadService = new LegadoDownloadService(_client, _epubBuilder);

        // 下载中间 10 章
        const int middleStart = 50;
        const int middleEnd = 59;

        var syncOptions = new SyncOptions
        {
            TempDirectory = _tempDirectory,
            OutputDirectory = _outputDirectory,
            ContinueOnError = true,
            StartChapterIndex = middleStart,
            EndChapterIndex = middleEnd,
        };

        Console.WriteLine($"下载中间范围: 章节 {middleStart}-{middleEnd}...");

        var progress = new Progress<SyncProgress>(p =>
        {
            Console.WriteLine($"[{p.Phase}] {p.TotalProgress:F1}% - {p.Message}");
        });

        // Act
        var result = await downloadService.SyncBookAsync(_testBook, syncOptions, progress);

        // Assert
        Assert.IsTrue(result.Success, $"同步应该成功: {result.ErrorMessage}");
        Assert.IsNotNull(result.EpubPath);
        Assert.IsTrue(File.Exists(result.EpubPath), "EPUB 文件应该存在");

        Console.WriteLine($"EPUB 生成成功: {result.EpubPath}");
        Console.WriteLine($"文件大小: {new FileInfo(result.EpubPath).Length / 1024.0:F2} KB");
        Console.WriteLine($"总计: {result.Statistics?.TotalChapters} 章节");

        // 验证章节数量
        var expectedChapters = middleEnd - middleStart + 1;
        Assert.AreEqual(expectedChapters, result.Statistics?.TotalChapters, $"应该有 {expectedChapters} 章节");
    }

    /// <summary>
    /// 步骤 8: 测试缓存状态和清理.
    /// </summary>
    [TestMethod]
    [Priority(8)]
    public async Task Step8_CacheStateAndCleanup()
    {
        // Arrange
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_epubBuilder);
        Assert.IsNotNull(_testBook);

        var downloadService = new LegadoDownloadService(_client, _epubBuilder);

        // 获取缓存状态
        var cacheState = await downloadService.GetCacheStateAsync(_testBook.BookUrl, _tempDirectory);

        if (cacheState != null)
        {
            Console.WriteLine($"缓存状态:");
            Console.WriteLine($"  书籍 URL: {cacheState.BookUrl}");
            Console.WriteLine($"  目录哈希: {cacheState.TocHash}");
            Console.WriteLine($"  标题: {cacheState.Title}");

            // 清理缓存
            await downloadService.CleanupCacheAsync(_testBook.BookUrl, _tempDirectory);
            Console.WriteLine("缓存已清理");

            // 验证缓存已清理
            var stateAfterCleanup = await downloadService.GetCacheStateAsync(_testBook.BookUrl, _tempDirectory);
            Assert.IsNull(stateAfterCleanup, "清理后缓存应该不存在");
        }
        else
        {
            Console.WriteLine("没有找到缓存（可能已被清理）");
        }

        Console.WriteLine("✅ 缓存测试通过！");
    }
}

#region Mock 测试

/// <summary>
/// 使用 Mock 的单元测试.
/// </summary>
[TestClass]
public class LegadoDownloaderMockTests
{
    private string _testDirectory = null!;
    private string _outputDirectory = null!;
    private string _tempDirectory = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"LegadoMockTest_{Guid.NewGuid():N}");
        _outputDirectory = Path.Combine(_testDirectory, "output");
        _tempDirectory = Path.Combine(_testDirectory, "temp");
        Directory.CreateDirectory(_outputDirectory);
        Directory.CreateDirectory(_tempDirectory);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // 忽略清理错误
            }
        }
    }

    [TestMethod]
    public async Task SyncBookAsync_EmptyChapterList_ReturnsFailure()
    {
        // Arrange
        var mockClient = new Mock<ILegadoClient>(MockBehavior.Strict);
        var mockEpubBuilder = new Mock<IEpubBuilder>(MockBehavior.Loose);

        mockClient
            .Setup(c => c.GetChapterListAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Chapter>());

        var service = new LegadoDownloadService(mockClient.Object, mockEpubBuilder.Object);

        var book = new Book
        {
            BookUrl = "https://example.com/book/123",
            Origin = "https://source.com",
            Name = "测试书籍",
            Author = "测试作者",
        };

        var options = new SyncOptions
        {
            TempDirectory = _tempDirectory,
            OutputDirectory = _outputDirectory,
        };

        // Act
        var result = await service.SyncBookAsync(book, options);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task AnalyzeEpubAsync_NonExistentFile_ReturnsNull()
    {
        // Arrange
        var mockClient = new Mock<ILegadoClient>(MockBehavior.Loose);
        var mockEpubBuilder = new Mock<IEpubBuilder>(MockBehavior.Loose);
        var service = new LegadoDownloadService(mockClient.Object, mockEpubBuilder.Object);

        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.epub");

        // Act
        var result = await service.AnalyzeEpubAsync(nonExistentPath);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetCacheStateAsync_NoCacheExists_ReturnsNull()
    {
        // Arrange
        var mockClient = new Mock<ILegadoClient>(MockBehavior.Loose);
        var mockEpubBuilder = new Mock<IEpubBuilder>(MockBehavior.Loose);
        var service = new LegadoDownloadService(mockClient.Object, mockEpubBuilder.Object);

        // Act
        var result = await service.GetCacheStateAsync("https://example.com/book/999", _tempDirectory);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CleanupCacheAsync_NoCacheExists_DoesNotThrow()
    {
        // Arrange
        var mockClient = new Mock<ILegadoClient>(MockBehavior.Loose);
        var mockEpubBuilder = new Mock<IEpubBuilder>(MockBehavior.Loose);
        var service = new LegadoDownloadService(mockClient.Object, mockEpubBuilder.Object);

        // Act & Assert - 不应抛出异常
        await service.CleanupCacheAsync("https://example.com/book/999", _tempDirectory);
    }
}

#endregion
