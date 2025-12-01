// Copyright (c) Richasy. All rights reserved.

namespace Fb2Parser.Test.Integration;

/// <summary>
/// FB2 解析集成测试。
/// 从真实的 FB2 文件解析并验证结果。
/// </summary>
[TestClass]
public sealed class Fb2ParsingIntegrationTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string InputDir = Path.Combine(TestDataDir, "Input");

    /// <summary>
    /// 获取或设置测试上下文，用于输出测试日志。
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// 动态测试：遍历所有 Input 文件夹中的 fb2 文件并解析。
    /// </summary>
    [TestMethod]
    public async Task ParseAndValidateAllFb2Files()
    {
        // 检查 Input 文件夹是否存在
        if (!Directory.Exists(InputDir))
        {
            Assert.Inconclusive($"Input 目录不存在: {InputDir}，请添加测试用的 FB2 文件");
            return;
        }

        var fb2Files = Directory.GetFiles(InputDir, "*.fb2");
        if (fb2Files.Length == 0)
        {
            Assert.Inconclusive("Input 目录中没有 fb2 文件，请先添加测试数据");
            return;
        }

        var results = new List<(string FileName, bool Success, string? Error)>();

        foreach (var fb2File in fb2Files)
        {
            var fileName = Path.GetFileName(fb2File);
            try
            {
                await ParseAndValidateFb2Async(fb2File);
                results.Add((fileName, true, null));
            }
            catch (Exception ex)
            {
                results.Add((fileName, false, ex.Message));
            }
        }

        // 输出结果摘要
        TestContext.WriteLine("\n========== 测试结果摘要 ==========");
        foreach (var (fileName, success, error) in results)
        {
            if (success)
            {
                TestContext.WriteLine($"✅ {fileName}");
            }
            else
            {
                TestContext.WriteLine($"❌ {fileName}: {error}");
            }
        }
        TestContext.WriteLine("==================================\n");

        // 确保所有测试都通过
        var failures = results.Where(r => !r.Success).ToList();
        if (failures.Count > 0)
        {
            Assert.Fail($"有 {failures.Count} 个文件处理失败:\n" +
                string.Join("\n", failures.Select(f => $"  - {f.FileName}: {f.Error}")));
        }
    }

    private async Task ParseAndValidateFb2Async(string fb2FilePath)
    {
        var fileName = Path.GetFileName(fb2FilePath);
        TestContext.WriteLine($"\n处理: {fileName}");

        // 使用 Fb2Reader 解析
        using var book = await Fb2Reader.ReadAsync(fb2FilePath);

        // 验证基本信息
        Assert.IsNotNull(book, "解析结果不应为 null");
        Assert.IsNotNull(book.Metadata, "元数据不应为 null");

        TestContext.WriteLine($"  标题: {book.Metadata.Title ?? "(无)"}");
        TestContext.WriteLine($"  作者: {string.Join(", ", book.Metadata.Authors.Select(a => a.GetDisplayName()))}");
        TestContext.WriteLine($"  语言: {book.Metadata.Language ?? "(无)"}");
        TestContext.WriteLine($"  类型: {string.Join(", ", book.Metadata.Genres)}");
        TestContext.WriteLine($"  章节数: {book.Sections.Count}");
        TestContext.WriteLine($"  目录项数: {book.Navigation.Count}");
        TestContext.WriteLine($"  二进制资源数: {book.Binaries.Count}");
        TestContext.WriteLine($"  图片数: {book.Images.Count}");
        TestContext.WriteLine($"  有封面: {book.Cover != null}");

        // 验证章节
        if (book.Sections.Count > 0)
        {
            var firstSection = book.Sections[0];
            TestContext.WriteLine($"  第一章标题: {firstSection.Title ?? "(无标题)"}");
            TestContext.WriteLine($"  第一章内容长度: {firstSection.PlainText.Length} 字符");
        }

        // 验证封面
        if (book.Cover != null)
        {
            var coverData = await book.Cover.ReadContentAsync();
            Assert.IsTrue(coverData.Length > 0, "封面数据不应为空");
            TestContext.WriteLine($"  封面大小: {coverData.Length} bytes");
        }

        // 验证图片可读取
        foreach (var image in book.Images.Take(3)) // 只测试前3张图片
        {
            var imageData = await book.ReadBinaryContentAsync(image);
            Assert.IsTrue(imageData.Length > 0, $"图片 {image.Id} 数据不应为空");
        }

        // 验证导航结构
        var totalNavItems = CountNavItems(book.Navigation);
        TestContext.WriteLine($"  总目录项数（含嵌套）: {totalNavItems}");

        // 验证扁平章节列表
        var allSections = book.GetAllSections();
        TestContext.WriteLine($"  总章节数（含嵌套）: {allSections.Count}");
    }

    private static int CountNavItems(IEnumerable<Fb2NavItem> items)
    {
        var count = 0;
        foreach (var item in items)
        {
            count++;
            count += CountNavItems(item.Children);
        }

        return count;
    }

    /// <summary>
    /// 测试从流解析 FB2 文件。
    /// </summary>
    [TestMethod]
    public async Task ParseFromStream_WorksCorrectly()
    {
        // 使用测试数据工厂创建的内容
        var content = TestDataFactory.CreateFb2WithFullMetadata();
        using var stream = TestDataFactory.CreateStreamFromContent(content);

        // Act
        using var book = await Fb2Reader.ReadAsync(stream);

        // Assert
        Assert.IsNotNull(book);
        Assert.AreEqual("The Great Adventure", book.Metadata.Title);
        Assert.IsNull(book.FilePath);
    }

    /// <summary>
    /// 测试解析包含所有功能的 FB2 文件。
    /// </summary>
    [TestMethod]
    public async Task ParseFullFeaturedFb2_AllFeaturesWork()
    {
        // Arrange
        var content = TestDataFactory.CreateFb2WithCoverAndImages();

        // Act
        using var book = await Fb2Reader.ReadFromStringAsync(content);

        // Assert
        Assert.IsNotNull(book);
        Assert.IsNotNull(book.Cover);
        Assert.IsTrue(book.Images.Count > 0);
        Assert.IsTrue(book.Sections.Count > 0);

        // 验证可以读取封面
        var coverData = await book.Cover.ReadContentAsync();
        Assert.IsTrue(coverData.Length > 0);

        // 验证可以读取所有图片
        foreach (var image in book.Images)
        {
            var data = await book.ReadBinaryContentAsync(image);
            Assert.IsTrue(data.Length > 0);
        }
    }

    /// <summary>
    /// 性能测试：解析大量合成 FB2 内容。
    /// </summary>
    [TestMethod]
    public async Task PerformanceTest_ParseMultipleBooks()
    {
        // Arrange
        var contents = new[]
        {
            TestDataFactory.CreateMinimalFb2(),
            TestDataFactory.CreateFb2WithFullMetadata(),
            TestDataFactory.CreateFb2WithNestedSections(),
            TestDataFactory.CreateFb2WithCoverAndImages(),
            TestDataFactory.CreateFb2WithPoemAndCite(),
            TestDataFactory.CreateFb2WithMultipleBodies(),
            TestDataFactory.CreateFb2WithSpecialCharacters(),
        };

        var startTime = DateTime.Now;

        // Act
        for (var i = 0; i < 10; i++)
        {
            foreach (var content in contents)
            {
                using var book = await Fb2Reader.ReadFromStringAsync(content);
                Assert.IsNotNull(book);
            }
        }

        var elapsed = DateTime.Now - startTime;

        // Assert
        TestContext.WriteLine($"解析 {contents.Length * 10} 本书耗时: {elapsed.TotalMilliseconds}ms");
        Assert.IsTrue(elapsed.TotalSeconds < 10, "解析应在 10 秒内完成");
    }

    /// <summary>
    /// 测试各种边缘情况。
    /// </summary>
    [TestMethod]
    public async Task EdgeCases_HandleGracefully()
    {
        // 空书籍
        var emptyContent = TestDataFactory.CreateEmptyFb2();
        using var emptyBook = await Fb2Reader.ReadFromStringAsync(emptyContent);
        Assert.IsNotNull(emptyBook);
        Assert.AreEqual("Empty Book", emptyBook.Metadata.Title);

        // 无命名空间
        var noNsContent = TestDataFactory.CreateFb2WithoutNamespace();
        using var noNsBook = await Fb2Reader.ReadFromStringAsync(noNsContent);
        Assert.IsNotNull(noNsBook);

        // 仅昵称作者
        var nicknameContent = TestDataFactory.CreateFb2WithNicknameAuthor();
        using var nicknameBook = await Fb2Reader.ReadFromStringAsync(nicknameContent);
        Assert.AreEqual("PenName42", nicknameBook.Metadata.Authors[0].GetDisplayName());
    }
}
