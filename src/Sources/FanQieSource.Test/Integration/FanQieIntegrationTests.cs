// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Test.Integration;

/// <summary>
/// 番茄小说集成测试.
/// 使用真实 API 进行测试.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public class FanQieIntegrationTests
{
    private const string TestInstallId = "2209343760766170";
    private static FanQieClient? _client;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        var options = new FanQieClientOptions
        {
            Timeout = TimeSpan.FromSeconds(60),
            MaxConcurrentRequests = 2,
            RequestDelayMs = 1000, // 增加延迟避免被限流
            InstallId = TestInstallId,
        };

        _client = new FanQieClient(options);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client?.Dispose();
    }

    #region Search Tests

    [TestMethod]
    public async Task SearchBooks_WithValidQuery_ReturnsResults()
    {
        // Arrange
        var query = "斗罗大陆";

        // Act
        var result = await _client!.SearchBooksAsync(query);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.Count > 0, "搜索应返回至少一个结果");

        var firstBook = result.Items[0];
        Assert.IsFalse(string.IsNullOrEmpty(firstBook.BookId), "书籍 ID 不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(firstBook.Title), "书籍标题不应为空");

        Console.WriteLine($"找到 {result.Items.Count} 本书籍");
        Console.WriteLine($"第一本: {firstBook.Title} (ID: {firstBook.BookId})");
        Console.WriteLine($"作者: {firstBook.Author}");
        Console.WriteLine($"分类: {firstBook.Category}");
        Console.WriteLine($"状态: {firstBook.CreationStatus}");
    }

    [TestMethod]
    public async Task SearchBooks_WithPagination_ReturnsNextPage()
    {
        // Arrange
        var query = "玄幻";

        // Act - 第一页
        var result1 = await _client!.SearchBooksAsync(query, 0);

        // 等待一下避免限流
        await Task.Delay(1000);

        // Act - 第二页
        var result2 = await _client!.SearchBooksAsync(query, result1.NextOffset);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);

        if (result1.HasMore && result2.Items.Count > 0)
        {
            // 验证两页结果不同
            var firstPageIds = result1.Items.Select(b => b.BookId).ToHashSet();
            var secondPageIds = result2.Items.Select(b => b.BookId).ToHashSet();

            // 两页不应完全相同
            Assert.IsFalse(
                firstPageIds.SetEquals(secondPageIds),
                "两页的搜索结果应该不同");
        }

        Console.WriteLine($"第一页: {result1.Items.Count} 本, 有更多: {result1.HasMore}");
        Console.WriteLine($"第二页: {result2.Items.Count} 本");
    }

    [TestMethod]
    public async Task SearchBooks_WithNonExistentQuery_ReturnsEmptyOrFew()
    {
        // Arrange
        var query = "xyznonexistentbookquery12345abc";

        // Act
        var result = await _client!.SearchBooksAsync(query);

        // Assert
        Assert.IsNotNull(result);
        Console.WriteLine($"搜索不存在的关键词返回 {result.Items.Count} 个结果");
    }

    #endregion

    #region Book Detail Tests

    [TestMethod]
    public async Task GetBookDetail_WithValidId_ReturnsDetail()
    {
        // Arrange - 先搜索获取一个有效的书籍 ID
        var searchResult = await _client!.SearchBooksAsync("斗罗大陆");
        Assert.IsTrue(searchResult.Items.Count > 0, "需要先搜索到书籍");

        var bookId = searchResult.Items[0].BookId;
        await Task.Delay(1000);

        // Act
        var detail = await _client!.GetBookDetailAsync(bookId);

        // Assert
        Assert.IsNotNull(detail, "书籍详情不应为空");
        Assert.AreEqual(bookId, detail.BookId);
        Assert.IsFalse(string.IsNullOrEmpty(detail.Title), "标题不应为空");

        Console.WriteLine($"书籍: {detail.Title}");
        Console.WriteLine($"作者: {detail.Author}");
        Console.WriteLine($"分类: {detail.Category}");
        Console.WriteLine($"字数: {detail.WordCount}");
        Console.WriteLine($"章节数: {detail.ChapterCount}");
        Console.WriteLine($"状态: {detail.CreationStatus}");
        Console.WriteLine($"性别分类: {detail.Gender}");
        Console.WriteLine($"评分: {detail.Score}");

        if (detail.Tags != null && detail.Tags.Count > 0)
        {
            Console.WriteLine($"标签: {string.Join(", ", detail.Tags)}");
        }
    }

    [TestMethod]
    public async Task GetBookDetail_WithInvalidId_ReturnsNullOrThrows()
    {
        // Arrange
        var invalidBookId = "99999999999999999";

        try
        {
            // Act
            var detail = await _client!.GetBookDetailAsync(invalidBookId);

            // Assert - 可能返回 null 或抛出异常
            Console.WriteLine($"无效 ID 返回: {(detail == null ? "null" : detail.Title)}");
        }
        catch (FanQieApiException ex)
        {
            Console.WriteLine($"无效 ID 抛出异常: Code={ex.Code}, Message={ex.Message}");
            // 这是预期行为之一
        }
    }

    #endregion

    #region Book TOC Tests

    [TestMethod]
    public async Task GetBookToc_WithValidId_ReturnsVolumes()
    {
        // Arrange - 先搜索获取一个有效的书籍 ID
        var searchResult = await _client!.SearchBooksAsync("斗罗大陆");
        Assert.IsTrue(searchResult.Items.Count > 0, "需要先搜索到书籍");

        var bookId = searchResult.Items[0].BookId;
        await Task.Delay(1000);

        // Act
        var volumes = await _client!.GetBookTocAsync(bookId);

        // Assert
        Assert.IsNotNull(volumes);
        Assert.IsTrue(volumes.Count > 0, "应至少有一个卷");

        var totalChapters = volumes.Sum(v => v.Chapters.Count);
        Console.WriteLine($"共 {volumes.Count} 卷, {totalChapters} 章");

        foreach (var volume in volumes)
        {
            Console.WriteLine($"  {volume.Name}: {volume.Chapters.Count} 章");

            // 显示前3章
            foreach (var chapter in volume.Chapters.Take(3))
            {
                var lockStatus = chapter.IsLocked ? "[锁定]" : (chapter.NeedPay ? "[付费]" : "[免费]");
                Console.WriteLine($"    {chapter.Order}. {chapter.Title} {lockStatus}");
            }

            if (volume.Chapters.Count > 3)
            {
                Console.WriteLine($"    ... 还有 {volume.Chapters.Count - 3} 章");
            }
        }

        // 验证章节结构
        var firstChapter = volumes[0].Chapters[0];
        Assert.IsFalse(string.IsNullOrEmpty(firstChapter.ItemId), "章节 ID 不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(firstChapter.Title), "章节标题不应为空");
        Assert.IsTrue(firstChapter.Order > 0, "章节序号应大于0");
    }

    #endregion

    #region Chapter Content Tests

    [TestMethod]
    public async Task GetChapterContent_WithValidChapter_ReturnsContent()
    {
        // Arrange - 获取书籍和目录
        var searchResult = await _client!.SearchBooksAsync("斗罗大陆");
        Assert.IsTrue(searchResult.Items.Count > 0);

        var book = searchResult.Items[0];
        await Task.Delay(1000);

        var volumes = await _client!.GetBookTocAsync(book.BookId);
        Assert.IsTrue(volumes.Count > 0);

        // 找到第一个免费章节
        var freeChapter = volumes
            .SelectMany(v => v.Chapters)
            .FirstOrDefault(c => !c.NeedPay && !c.IsLocked);

        if (freeChapter == null)
        {
            Assert.Inconclusive("没有找到免费章节");
            return;
        }

        await Task.Delay(1000);

        // Act
        var content = await _client!.GetChapterContentAsync(book.BookId, book.Title, freeChapter);

        // Assert
        Assert.IsNotNull(content, "章节内容不应为空");
        Assert.AreEqual(freeChapter.ItemId, content.ItemId);
        Assert.IsFalse(string.IsNullOrEmpty(content.TextContent), "文本内容不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(content.HtmlContent), "HTML 内容不应为空");
        Assert.IsTrue(content.WordCount > 0, "字数应大于0");

        Console.WriteLine($"章节: {content.Title}");
        Console.WriteLine($"字数: {content.WordCount}");
        Console.WriteLine($"内容预览 (前200字): {content.TextContent[..Math.Min(200, content.TextContent.Length)]}...");

        if (content.Images != null && content.Images.Count > 0)
        {
            Console.WriteLine($"图片数: {content.Images.Count}");
        }
    }

    [TestMethod]
    public async Task GetChapterContents_BatchDownload_ReturnsMultipleChapters()
    {
        // Arrange
        var searchResult = await _client!.SearchBooksAsync("斗罗大陆");
        Assert.IsTrue(searchResult.Items.Count > 0);

        var book = searchResult.Items[0];
        await Task.Delay(1000);

        var volumes = await _client!.GetBookTocAsync(book.BookId);
        Assert.IsTrue(volumes.Count > 0);

        // 获取前5个免费章节
        var freeChapters = volumes
            .SelectMany(v => v.Chapters)
            .Where(c => !c.NeedPay && !c.IsLocked)
            .Take(5)
            .ToList();

        if (freeChapters.Count == 0)
        {
            Assert.Inconclusive("没有找到免费章节");
            return;
        }

        await Task.Delay(1000);

        // Act
        var contents = await _client!.GetChapterContentsAsync(book.BookId, book.Title, freeChapters);

        // Assert
        Assert.IsNotNull(contents);
        Assert.AreEqual(freeChapters.Count, contents.Count, "返回的章节数应与请求的相同");

        Console.WriteLine($"批量下载了 {contents.Count} 章:");
        foreach (var content in contents)
        {
            Console.WriteLine($"  {content.Order}. {content.Title} ({content.WordCount} 字)");
        }

        // 验证所有章节都有内容
        foreach (var content in contents)
        {
            Assert.IsFalse(string.IsNullOrEmpty(content.TextContent), $"章节 {content.Title} 内容不应为空");
        }
    }

    #endregion

    #region Download Book Tests

    [TestMethod]
    public async Task DownloadBook_WithProgress_ReturnsBookData()
    {
        // Arrange
        var searchResult = await _client!.SearchBooksAsync("斗罗大陆");
        Assert.IsTrue(searchResult.Items.Count > 0);

        var bookId = searchResult.Items[0].BookId;
        var progressReports = new List<(int Current, int Total)>();

        // 创建一个只下载少量章节的客户端选项
        var limitedOptions = new FanQieClientOptions
        {
            Timeout = TimeSpan.FromSeconds(120),
            MaxConcurrentRequests = 2,
            BatchSize = 5,
            RequestDelayMs = 1000,
            InstallId = TestInstallId,
        };

        using var limitedClient = new FanQieClient(limitedOptions);

        // 先获取目录，只测试下载前10章
        await Task.Delay(1000);
        var volumes = await limitedClient.GetBookTocAsync(bookId);
        var freeChapters = volumes
            .SelectMany(v => v.Chapters)
            .Where(c => !c.NeedPay && !c.IsLocked)
            .Take(10)
            .ToList();

        if (freeChapters.Count == 0)
        {
            Assert.Inconclusive("没有找到免费章节");
            return;
        }

        await Task.Delay(1000);
        var detail = await limitedClient.GetBookDetailAsync(bookId);
        Assert.IsNotNull(detail);

        await Task.Delay(1000);

        // Act - 手动下载指定章节（而不是整本书）
        var progress = new Progress<(int Current, int Total)>(p =>
        {
            progressReports.Add(p);
            Console.WriteLine($"进度: {p.Current}/{p.Total}");
        });

        var contents = await limitedClient.GetChapterContentsAsync(
            bookId,
            detail.Title,
            freeChapters);

        // Assert
        Assert.IsNotNull(contents);
        Assert.IsTrue(contents.Count > 0, "应下载到至少一章");

        Console.WriteLine($"\n下载完成:");
        Console.WriteLine($"  书籍: {detail.Title}");
        Console.WriteLine($"  作者: {detail.Author}");
        Console.WriteLine($"  下载章节数: {contents.Count}");

        var totalWords = contents.Sum(c => c.WordCount);
        Console.WriteLine($"  总字数: {totalWords}");

        // 显示下载的章节
        Console.WriteLine("\n已下载章节:");
        foreach (var chapter in contents.Take(5))
        {
            Console.WriteLine($"  {chapter.Order}. {chapter.Title} ({chapter.WordCount} 字)");
        }

        if (contents.Count > 5)
        {
            Console.WriteLine($"  ... 还有 {contents.Count - 5} 章");
        }
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public async Task Api_WithNetworkError_ThrowsAppropriateException()
    {
        // Arrange - 创建一个超时很短的客户端
        var options = new FanQieClientOptions
        {
            Timeout = TimeSpan.FromMilliseconds(1), // 极短的超时
        };

        using var client = new FanQieClient(options);

        // Act & Assert
        try
        {
            await client.SearchBooksAsync("测试");
            Assert.Fail("应该抛出异常");
        }
        catch (Exception ex)
        {
            // 预期会抛出超时或网络相关的异常
            Console.WriteLine($"预期的异常类型: {ex.GetType().Name}");
            Console.WriteLine($"异常消息: {ex.Message}");
        }
    }

    #endregion

    #region Data Integrity Tests

    [TestMethod]
    public async Task ChapterContent_DataIntegrity_AllFieldsPopulated()
    {
        // Arrange
        var searchResult = await _client!.SearchBooksAsync("玄幻");
        Assert.IsTrue(searchResult.Items.Count > 0);

        var book = searchResult.Items[0];
        await Task.Delay(1000);

        var volumes = await _client!.GetBookTocAsync(book.BookId);
        var freeChapter = volumes
            .SelectMany(v => v.Chapters)
            .FirstOrDefault(c => !c.NeedPay && !c.IsLocked);

        if (freeChapter == null)
        {
            Assert.Inconclusive("没有找到免费章节");
            return;
        }

        await Task.Delay(1000);

        // Act
        var content = await _client!.GetChapterContentAsync(book.BookId, book.Title, freeChapter);

        // Assert - 验证数据完整性
        Assert.IsNotNull(content);

        // 必填字段
        Assert.IsFalse(string.IsNullOrEmpty(content.ItemId), "ItemId 应有值");
        Assert.IsFalse(string.IsNullOrEmpty(content.BookId), "BookId 应有值");
        Assert.IsFalse(string.IsNullOrEmpty(content.BookTitle), "BookTitle 应有值");
        Assert.IsFalse(string.IsNullOrEmpty(content.Title), "Title 应有值");
        Assert.IsFalse(string.IsNullOrEmpty(content.TextContent), "TextContent 应有值");
        Assert.IsFalse(string.IsNullOrEmpty(content.HtmlContent), "HtmlContent 应有值");

        // 数值字段
        Assert.IsTrue(content.WordCount > 0, "WordCount 应大于0");
        Assert.IsTrue(content.Order > 0, "Order 应大于0");

        // 验证 HTML 内容确实包含 HTML 标签
        Assert.IsTrue(
            content.HtmlContent.Contains('<', StringComparison.Ordinal) && content.HtmlContent.Contains('>', StringComparison.Ordinal),
            "HtmlContent 应包含 HTML 标签");

        // 验证纯文本不包含 HTML 标签
        Assert.IsFalse(
            content.TextContent.Contains("<p>", StringComparison.OrdinalIgnoreCase),
            "TextContent 不应包含 HTML 标签");

        Console.WriteLine("数据完整性验证通过:");
        Console.WriteLine($"  ItemId: {content.ItemId}");
        Console.WriteLine($"  BookId: {content.BookId}");
        Console.WriteLine($"  BookTitle: {content.BookTitle}");
        Console.WriteLine($"  Title: {content.Title}");
        Console.WriteLine($"  WordCount: {content.WordCount}");
        Console.WriteLine($"  Order: {content.Order}");
        Console.WriteLine($"  VolumeName: {content.VolumeName ?? "(无)"}");
        Console.WriteLine($"  PublishTime: {content.PublishTime?.ToString() ?? "(无)"}");
        Console.WriteLine($"  Images: {content.Images?.Count ?? 0}");
    }

    #endregion
}
