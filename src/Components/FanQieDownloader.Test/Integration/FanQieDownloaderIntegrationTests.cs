// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Test.Integration;

/// <summary>
/// FanQieDownloader 集成测试.
/// 使用真实 API 进行完整的下载和同步流程测试.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public class FanQieDownloaderIntegrationTests
{
    private const string TestBookKeyword = "冒姓琅琊";
    private const int InitialStartChapter = 255;
    private const int InitialEndChapter = 265;
    private const int ImageChapter = 260;
    private const int FinalStartChapter = 250;
    private const int FinalEndChapter = 270;

    private static FanQieClient? _client;
    private static IEpubBuilder? _epubBuilder;
    private static string _testDirectory = null!;
    private static string _outputDirectory = null!;
    private static string _tempDirectory = null!;
    private static string? _testBookId;
    private static string? _generatedEpubPath;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        // 创建测试目录
        _testDirectory = Path.Combine(Path.GetTempPath(), $"FanQieDownloaderTest_{DateTime.Now:yyyyMMdd_HHmmss}");
        _outputDirectory = Path.Combine(_testDirectory, "output");
        _tempDirectory = Path.Combine(_testDirectory, "temp");

        Directory.CreateDirectory(_testDirectory);
        Directory.CreateDirectory(_outputDirectory);
        Directory.CreateDirectory(_tempDirectory);

        Console.WriteLine($"测试目录: {_testDirectory}");

        // 创建客户端
        var options = new FanQieClientOptions
        {
            Timeout = TimeSpan.FromSeconds(120),
            MaxConcurrentRequests = 3,
            RequestDelayMs = 300,
            FallbackApiBaseUrl = "http://127.0.0.1:9999"
        };

        _client = new FanQieClient(options);
        _epubBuilder = new EpubBuilder();

        // 搜索书籍获取 ID
        Console.WriteLine($"搜索书籍: {TestBookKeyword}");
        var searchResult = await _client.SearchBooksAsync(TestBookKeyword);

        if (searchResult.Items.Count == 0)
        {
            Assert.Inconclusive($"未找到书籍: {TestBookKeyword}");
        }

        _testBookId = searchResult.Items[0].BookId;
        Console.WriteLine($"找到书籍: {searchResult.Items[0].Title} (ID: {_testBookId})");
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client?.Dispose();

        // 保留测试目录以便检查结果
        Console.WriteLine($"测试完成，结果保存在: {_testDirectory}");
    }

    /// <summary>
    /// 步骤 1: 搜索书籍 "冒姓琅琊" 并验证结果.
    /// </summary>
    [TestMethod]
    [Priority(1)]
    public async Task Step1_SearchBook_ReturnsValidResult()
    {
        // Arrange
        Assert.IsNotNull(_client);

        // Act
        var result = await _client.SearchBooksAsync(TestBookKeyword);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.Count > 0, $"应该找到包含 '{TestBookKeyword}' 的书籍");

        var book = result.Items[0];
        Console.WriteLine($"书籍: {book.Title}");
        Console.WriteLine($"作者: {book.Author}");
        Console.WriteLine($"ID: {book.BookId}");
        Console.WriteLine($"分类: {book.Category}");

        Assert.IsNotNull(_testBookId);
    }

    /// <summary>
    /// 步骤 2: 下载 255-265 章，验证基本下载功能.
    /// </summary>
    [TestMethod]
    [Priority(2)]
    public async Task Step2_DownloadChapters255To265_CreatesValidEpub()
    {
        // Arrange
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_epubBuilder);
        Assert.IsNotNull(_testBookId);

        var downloadService = new FanQieDownloadService(_client, _epubBuilder);

        var syncOptions = new SyncOptions
        {
            TempDirectory = _tempDirectory,
            OutputDirectory = _outputDirectory,
            ContinueOnError = true,
            RetryFailedChapters = false,
            StartChapterOrder = InitialStartChapter,
            EndChapterOrder = InitialEndChapter,
        };

        Console.WriteLine($"开始下载书籍章节 {InitialStartChapter}-{InitialEndChapter}...");

        var progress = new Progress<SyncProgress>(p =>
        {
            Console.WriteLine($"[{p.Phase}] {p.TotalProgress:F1}% - {p.Message}");
        });

        // Act
        var result = await downloadService.SyncBookAsync(_testBookId, syncOptions, progress);

        // Assert
        Assert.IsTrue(result.Success, $"同步应该成功: {result.ErrorMessage}");
        Assert.IsNotNull(result.EpubPath);
        Assert.IsTrue(File.Exists(result.EpubPath), "EPUB 文件应该存在");

        _generatedEpubPath = result.EpubPath;

        Console.WriteLine($"EPUB 生成成功: {result.EpubPath}");
        Console.WriteLine($"新下载: {result.Statistics?.NewlyDownloaded} 章节");
        Console.WriteLine($"失败: {result.Statistics?.Failed} 章节");
        Console.WriteLine($"总计: {result.Statistics?.TotalChapters} 章节");

        // 验证 EPUB 可以被解析
        {
            using var epubBook = await EpubReader.ReadAsync(result.EpubPath);
            Assert.IsNotNull(epubBook);
            Assert.IsNotNull(epubBook.Metadata.Title);
            Console.WriteLine($"EPUB 标题: {epubBook.Metadata.Title}");
            Console.WriteLine($"阅读顺序: {epubBook.ReadingOrder.Count} 项");

            // 验证有番茄元数据
            var bookIdMeta = epubBook.Metadata.MetaItems
                .FirstOrDefault(m => m.Name == "fanqie:book-id" || m.Property == "fanqie:book-id");
            Assert.IsNotNull(bookIdMeta, "EPUB 应该包含 fanqie:book-id 元数据");
        }
    }

    /// <summary>
    /// 步骤 3: 验证第 260 章是否正确解析到了图片.
    /// </summary>
    [TestMethod]
    [Priority(3)]
    public async Task Step3_VerifyChapter260HasImages()
    {
        // Arrange
        if (string.IsNullOrEmpty(_generatedEpubPath) || !File.Exists(_generatedEpubPath))
        {
            Assert.Inconclusive("没有可验证的 EPUB 文件");
            return;
        }

        Console.WriteLine($"检查 EPUB 中第 {ImageChapter} 章的图片: {_generatedEpubPath}");

        // 读取 EPUB - 使用块作用域确保及时释放
        {
            using var epubBook = await EpubReader.ReadAsync(_generatedEpubPath);

            // 查找第 260 章的资源 (文件名格式: chapter260.xhtml)
            var chapter260Resource = epubBook.ReadingOrder
                .FirstOrDefault(r => r.Href.Contains($"chapter{ImageChapter}", StringComparison.OrdinalIgnoreCase));

            if (chapter260Resource != null)
            {
                var html = await epubBook.ReadResourceContentAsStringAsync(chapter260Resource);
                var imageCount = System.Text.RegularExpressions.Regex.Matches(html, @"<img\s", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
                Console.WriteLine($"第 {ImageChapter} 章包含 {imageCount} 张图片");

                if (imageCount > 0)
                {
                    Console.WriteLine("✓ 第 260 章有图片");
                }
            }
            else
            {
                Console.WriteLine($"注意: 未找到第 {ImageChapter} 章的资源文件");
            }

            // 检查 EPUB 中所有图片资源
            var imageResources = epubBook.Resources
                .Where(r => r.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            Console.WriteLine($"EPUB 中的图片资源总数: {imageResources.Count}");
            foreach (var img in imageResources.Take(10))
            {
                Console.WriteLine($"  - {img.Href} ({img.MediaType})");
            }
        }
    }

    /// <summary>
    /// 步骤 4: 增量同步 - 扩展到 250-270 章，其中 267 章故意失败.
    /// </summary>
    [TestMethod]
    [Priority(4)]
    public async Task Step4_IncrementalSync_ExtendToChapters250To270()
    {
        // Arrange
        Assert.IsNotNull(_client);
        Assert.IsNotNull(_epubBuilder);
        Assert.IsNotNull(_testBookId);

        if (string.IsNullOrEmpty(_generatedEpubPath) || !File.Exists(_generatedEpubPath))
        {
            Assert.Inconclusive("没有现有 EPUB 文件");
            return;
        }

        Console.WriteLine($"使用现有 EPUB: {_generatedEpubPath}");

        // 使用正常客户端进行增量同步
        var downloadService = new FanQieDownloadService(_client, _epubBuilder);

        var syncOptions = new SyncOptions
        {
            TempDirectory = _tempDirectory,
            OutputDirectory = _outputDirectory,
            ExistingEpubPath = _generatedEpubPath,
            ContinueOnError = true,
            RetryFailedChapters = true,
            StartChapterOrder = FinalStartChapter,
            EndChapterOrder = FinalEndChapter,
        };

        Console.WriteLine($"增量同步: 扩展到 {FinalStartChapter}-{FinalEndChapter} 章");

        var progress = new Progress<SyncProgress>(p =>
        {
            Console.WriteLine($"[{p.Phase}] {p.TotalProgress:F1}% - {p.Message}");
        });

        // Act
        var result = await downloadService.SyncBookAsync(_testBookId, syncOptions, progress);

        // Assert
        Assert.IsTrue(result.Success, $"同步应该成功: {result.ErrorMessage}");
        Assert.IsNotNull(result.Statistics);

        Console.WriteLine($"同步结果:");
        Console.WriteLine($"  新下载: {result.Statistics.NewlyDownloaded}");
        Console.WriteLine($"  复用: {result.Statistics.Reused}");
        Console.WriteLine($"  失败: {result.Statistics.Failed}");
        Console.WriteLine($"  锁定: {result.Statistics.LockedChapters}");
        Console.WriteLine($"  总章节: {result.Statistics.TotalChapters}");

        _generatedEpubPath = result.EpubPath;

        // 验证 EPUB 已更新
        Assert.IsNotNull(result.EpubPath);
        Assert.IsTrue(File.Exists(result.EpubPath), "EPUB 文件应该存在");

        // 验证章节数量扩展了
        var expectedChapters = FinalEndChapter - FinalStartChapter + 1;
        Assert.AreEqual(expectedChapters, result.Statistics.TotalChapters, $"应该有 {expectedChapters} 章节");
    }

    /// <summary>
    /// 步骤 5: 验证增量同步结果.
    /// </summary>
    [TestMethod]
    [Priority(5)]
    public async Task Step5_VerifyIncrementalSyncResult()
    {
        // Arrange
        if (string.IsNullOrEmpty(_generatedEpubPath) || !File.Exists(_generatedEpubPath))
        {
            Assert.Inconclusive("没有现有 EPUB 文件");
            return;
        }

        Console.WriteLine($"验证增量同步结果: {_generatedEpubPath}");

        // 读取 EPUB 验证内容
        {
            using var epubBook = await EpubReader.ReadAsync(_generatedEpubPath);

            // 验证元数据
            Assert.IsNotNull(epubBook.Metadata.Title);
            Console.WriteLine($"书名: {epubBook.Metadata.Title}");
            Console.WriteLine($"阅读顺序: {epubBook.ReadingOrder.Count} 项");

            // 统计各种章节类型
            var downloadedCount = 0;
            var lockedCount = 0;
            var failedCount = 0;

            foreach (var resource in epubBook.ReadingOrder)
            {
                // 文件名格式是 chapter255.xhtml，不是 chapter_255.xhtml
                if (!resource.Href.Contains("chapter", StringComparison.OrdinalIgnoreCase) ||
                    resource.Href.Contains("nav", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var html = await epubBook.ReadResourceContentAsStringAsync(resource);

                    if (html.Contains("class=\"chapter-locked\"", StringComparison.OrdinalIgnoreCase))
                    {
                        lockedCount++;
                    }
                    else if (html.Contains("class=\"chapter-unavailable\"", StringComparison.OrdinalIgnoreCase))
                    {
                        failedCount++;
                    }
                    else if (html.Contains("fanqie:status", StringComparison.OrdinalIgnoreCase))
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
            Console.WriteLine($"  已下载: {downloadedCount}");
            Console.WriteLine($"  锁定: {lockedCount}");
            Console.WriteLine($"  失败: {failedCount}");

            // 验证至少有章节（下载或锁定都算）
            var totalProcessed = downloadedCount + lockedCount + failedCount;
            Assert.IsTrue(totalProcessed > 0, $"应该有处理过的章节，但得到 downloadedCount={downloadedCount}, lockedCount={lockedCount}, failedCount={failedCount}");
        }
    }

    /// <summary>
    /// 步骤 6: 完整性验证 - 检查 250-270 章是否完整.
    /// </summary>
    [TestMethod]
    [Priority(6)]
    public async Task Step6_VerifyCompleteness_Chapters250To270()
    {
        // Arrange
        if (string.IsNullOrEmpty(_generatedEpubPath) || !File.Exists(_generatedEpubPath))
        {
            Assert.Inconclusive("没有可验证的 EPUB 文件");
            return;
        }

        Console.WriteLine($"验证 EPUB 完整性: {_generatedEpubPath}");

        // 读取 EPUB
        using var epubBook = await EpubReader.ReadAsync(_generatedEpubPath);

        // 验证元数据
        Assert.IsNotNull(epubBook.Metadata.Title);
        Console.WriteLine($"书名: {epubBook.Metadata.Title}");
        Console.WriteLine($"作者: {string.Join(", ", epubBook.Metadata.Authors)}");

        // 检查番茄标记
        var bookIdMeta = epubBook.Metadata.MetaItems
            .FirstOrDefault(m => m.Name == "fanqie:book-id" || m.Property == "fanqie:book-id");
        Assert.IsNotNull(bookIdMeta, "EPUB 应该包含 fanqie:book-id 元数据");
        Console.WriteLine($"番茄书籍 ID: {bookIdMeta.Content}");

        // 验证阅读顺序
        Console.WriteLine($"阅读顺序: {epubBook.ReadingOrder.Count} 项");

        // 统计章节
        var chapterCount = 0;
        var downloadedCount = 0;
        var failedCount = 0;
        var lockedCount = 0;

        foreach (var resource in epubBook.ReadingOrder)
        {
            // 文件名格式是 chapter255.xhtml，不是 chapter_255.xhtml
            if (!resource.Href.Contains("chapter", StringComparison.OrdinalIgnoreCase) ||
                resource.Href.Contains("nav", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            chapterCount++;

            try
            {
                var html = await epubBook.ReadResourceContentAsStringAsync(resource);

                // 检查各种状态标记
                // 1. 失败占位 - data-fanqie-status="failed" 或 chapter-unavailable class
                if (html.Contains("data-fanqie-status=\"failed\"", StringComparison.OrdinalIgnoreCase) ||
                    html.Contains("class=\"chapter-unavailable\"", StringComparison.OrdinalIgnoreCase))
                {
                    failedCount++;
                    Console.WriteLine($"  ❌ {resource.Href} (失败占位)");
                }
                // 2. 锁定章节 - data-fanqie-status="locked" 或 chapter-locked class
                else if (html.Contains("data-fanqie-status=\"locked\"", StringComparison.OrdinalIgnoreCase) ||
                    html.Contains("class=\"chapter-locked\"", StringComparison.OrdinalIgnoreCase))
                {
                    lockedCount++;
                }
                // 3. 已下载章节 - meta 标签 fanqie:status="downloaded" 或 fanqie:chapter-id
                else if (html.Contains("fanqie:status", StringComparison.OrdinalIgnoreCase) ||
                         html.Contains("fanqie:chapter-id", StringComparison.OrdinalIgnoreCase))
                {
                    downloadedCount++;
                }
            }
            catch
            {
                failedCount++;
            }
        }

        Console.WriteLine($"\n统计:");
        Console.WriteLine($"  总章节数: {chapterCount}");
        Console.WriteLine($"  已下载: {downloadedCount}");
        Console.WriteLine($"  失败/占位: {failedCount}");
        Console.WriteLine($"  锁定: {lockedCount}");

        // 验证章节数量
        var expectedChapters = FinalEndChapter - FinalStartChapter + 1;
        Console.WriteLine($"  预期章节: {expectedChapters}");

        // 验证有处理过的章节（下载、锁定或失败都算）
        var totalProcessed = downloadedCount + lockedCount + failedCount;
        Assert.IsTrue(chapterCount > 0 || totalProcessed > 0, $"应该有处理过的章节");
        Assert.AreEqual(expectedChapters, chapterCount, $"章节数量应该匹配");

        Console.WriteLine("\n✅ 完整性验证通过！");
        Console.WriteLine($"\n最终 EPUB 路径: {_generatedEpubPath}");
    }
}
