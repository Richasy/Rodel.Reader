// Copyright (c) Richasy. All rights reserved.

namespace EpubParser.Test.Integration;

/// <summary>
/// EPUB 解析集成测试。
/// 从真实的 EPUB 文件解析并验证结果。
/// </summary>
[TestClass]
public sealed class EpubParsingIntegrationTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string InputDir = Path.Combine(TestDataDir, "Input");

    /// <summary>
    /// 获取或设置测试上下文，用于输出测试日志。
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// 动态测试：遍历所有 Input 文件夹中的 epub 文件并解析。
    /// </summary>
    [TestMethod]
    public async Task ParseAndValidateAllEpubs()
    {
        // 检查 Input 文件夹是否存在
        if (!Directory.Exists(InputDir))
        {
            Assert.Inconclusive($"Input 目录不存在: {InputDir}，请添加测试用的 EPUB 文件");
            return;
        }

        var epubFiles = Directory.GetFiles(InputDir, "*.epub");
        if (epubFiles.Length == 0)
        {
            Assert.Inconclusive("Input 目录中没有 epub 文件，请先添加测试数据");
            return;
        }

        var results = new List<(string FileName, bool Success, string? Error)>();

        foreach (var epubFile in epubFiles)
        {
            var fileName = Path.GetFileName(epubFile);
            try
            {
                await ParseAndValidateEpubAsync(epubFile);
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

    private async Task ParseAndValidateEpubAsync(string epubFilePath)
    {
        var fileName = Path.GetFileName(epubFilePath);
        TestContext.WriteLine($"\n处理: {fileName}");

        // 使用 EpubReader 解析
        using var book = await EpubReader.ReadAsync(epubFilePath);

        // 验证基本信息
        Assert.IsNotNull(book, "解析结果不应为 null");
        Assert.IsNotNull(book.Metadata, "元数据不应为 null");

        TestContext.WriteLine($"  标题: {book.Metadata.Title ?? "(无)"}");
        TestContext.WriteLine($"  作者: {string.Join(", ", book.Metadata.Authors)}");
        TestContext.WriteLine($"  语言: {book.Metadata.Language ?? "(无)"}");
        TestContext.WriteLine($"  资源数: {book.Resources.Count}");
        TestContext.WriteLine($"  阅读顺序项数: {book.ReadingOrder.Count}");
        TestContext.WriteLine($"  目录项数: {book.Navigation.Count}");
        TestContext.WriteLine($"  图片数: {book.Images.Count}");
        TestContext.WriteLine($"  有封面: {book.Cover != null}");

        // 验证资源
        Assert.IsTrue(book.Resources.Count > 0, "应该有资源");

        // 验证阅读顺序
        Assert.IsTrue(book.ReadingOrder.Count > 0, "应该有阅读顺序");

        // 如果有封面，验证能读取封面数据
        if (book.Cover != null)
        {
            var coverData = await book.Cover.ReadContentAsync();
            Assert.IsTrue(coverData.Length > 0, "封面数据不应为空");
            TestContext.WriteLine($"  封面大小: {coverData.Length:N0} 字节");
        }

        // 验证能读取第一个阅读项的内容
        var firstItem = book.ReadingOrder[0];
        var content = await book.ReadResourceContentAsStringAsync(firstItem);
        Assert.IsNotNull(content, "内容不应为 null");
        Assert.IsTrue(content.Length > 0, "内容不应为空");
        TestContext.WriteLine($"  首项内容长度: {content.Length:N0} 字符");

        // 验证目录结构
        if (book.Navigation.Count > 0)
        {
            PrintNavigation(book.Navigation, "  ");
        }

        TestContext.WriteLine($"  ✅ 解析成功");
    }

    private void PrintNavigation(IReadOnlyList<EpubNavItem> items, string indent)
    {
        foreach (var item in items)
        {
            TestContext.WriteLine($"{indent}📖 {item.Title}");
            if (item.Children.Count > 0)
            {
                PrintNavigation(item.Children, indent + "  ");
            }
        }
    }

    /// <summary>
    /// 测试从流解析 EPUB。
    /// </summary>
    [TestMethod]
    public async Task ParseFromStream_ShouldSucceed()
    {
        var epubFiles = Directory.Exists(InputDir)
            ? Directory.GetFiles(InputDir, "*.epub")
            : [];

        if (epubFiles.Length == 0)
        {
            Assert.Inconclusive("没有可用的 EPUB 测试文件");
            return;
        }

        var testFile = epubFiles[0];
        TestContext.WriteLine($"测试文件: {Path.GetFileName(testFile)}");

        using var fileStream = File.OpenRead(testFile);
        using var book = await EpubReader.ReadAsync(fileStream);

        Assert.IsNotNull(book);
        Assert.IsNull(book.FilePath, "从流加载时 FilePath 应为 null");
        Assert.IsNotNull(book.Metadata);

        TestContext.WriteLine($"  标题: {book.Metadata.Title}");
        TestContext.WriteLine($"  ✅ 从流解析成功");
    }

    /// <summary>
    /// 测试验证元数据完整性。
    /// </summary>
    [TestMethod]
    public async Task ValidateMetadataCompleteness()
    {
        var epubFiles = Directory.Exists(InputDir)
            ? Directory.GetFiles(InputDir, "*.epub")
            : [];

        if (epubFiles.Length == 0)
        {
            Assert.Inconclusive("没有可用的 EPUB 测试文件");
            return;
        }

        foreach (var epubFile in epubFiles)
        {
            using var book = await EpubReader.ReadAsync(epubFile);

            // 记录元数据完整性
            var hasTitle = !string.IsNullOrEmpty(book.Metadata.Title);
            var hasAuthor = book.Metadata.Authors.Count > 0;
            var hasLanguage = !string.IsNullOrEmpty(book.Metadata.Language);
            var hasIdentifier = !string.IsNullOrEmpty(book.Metadata.Identifier);

            TestContext.WriteLine($"\n{Path.GetFileName(epubFile)}:");
            TestContext.WriteLine($"  标题: {(hasTitle ? "✓" : "✗")} {book.Metadata.Title}");
            TestContext.WriteLine($"  作者: {(hasAuthor ? "✓" : "✗")} {string.Join(", ", book.Metadata.Authors)}");
            TestContext.WriteLine($"  语言: {(hasLanguage ? "✓" : "✗")} {book.Metadata.Language}");
            TestContext.WriteLine($"  标识符: {(hasIdentifier ? "✓" : "✗")} {book.Metadata.Identifier}");
            TestContext.WriteLine($"  主题数: {book.Metadata.Subjects.Count}");
            TestContext.WriteLine($"  贡献者数: {book.Metadata.Contributors.Count}");
            TestContext.WriteLine($"  自定义元数据数: {book.Metadata.CustomMetadata.Count}");
            TestContext.WriteLine($"  Meta 元素数: {book.Metadata.MetaItems.Count}");
        }
    }

    /// <summary>
    /// 测试漫画/图片 EPUB 的解析。
    /// </summary>
    [TestMethod]
    public async Task ParseMangaEpub_ShouldHaveImages()
    {
        var epubFiles = Directory.Exists(InputDir)
            ? Directory.GetFiles(InputDir, "*.epub")
            : [];

        if (epubFiles.Length == 0)
        {
            Assert.Inconclusive("没有可用的 EPUB 测试文件");
            return;
        }

        foreach (var epubFile in epubFiles)
        {
            using var book = await EpubReader.ReadAsync(epubFile);

            TestContext.WriteLine($"\n{Path.GetFileName(epubFile)}:");
            TestContext.WriteLine($"  图片资源数: {book.Images.Count}");

            if (book.Images.Count > 0)
            {
                foreach (var image in book.Images.Take(5))
                {
                    TestContext.WriteLine($"    - {image.Href} ({image.MediaType})");
                }

                if (book.Images.Count > 5)
                {
                    TestContext.WriteLine($"    ... 还有 {book.Images.Count - 5} 个图片");
                }
            }
        }
    }
}
