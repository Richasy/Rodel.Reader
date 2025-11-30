// Copyright (c) Richasy. All rights reserved.

namespace MobiParser.Test.Integration;

/// <summary>
/// Mobi 解析集成测试。
/// 从真实的 Mobi 文件解析并验证结果。
/// </summary>
[TestClass]
public sealed class MobiParsingIntegrationTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string InputDir = Path.Combine(TestDataDir, "Input");

    /// <summary>
    /// 获取或设置测试上下文，用于输出测试日志。
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// 动态测试：遍历所有 Input 文件夹中的 mobi/azw 文件并解析。
    /// </summary>
    [TestMethod]
    public async Task ParseAndValidateAllMobiFiles()
    {
        // 检查 Input 文件夹是否存在
        if (!Directory.Exists(InputDir))
        {
            Assert.Inconclusive($"Input 目录不存在: {InputDir}，请添加测试用的 Mobi 文件");
            return;
        }

        var mobiFiles = Directory.GetFiles(InputDir, "*.mobi")
            .Concat(Directory.GetFiles(InputDir, "*.azw"))
            .Concat(Directory.GetFiles(InputDir, "*.azw3"))
            .ToArray();

        if (mobiFiles.Length == 0)
        {
            Assert.Inconclusive("Input 目录中没有 mobi/azw 文件，请先添加测试数据");
            return;
        }

        var results = new List<(string FileName, bool Success, string? Error)>();

        foreach (var mobiFile in mobiFiles)
        {
            var fileName = Path.GetFileName(mobiFile);
            try
            {
                await ParseAndValidateMobiAsync(mobiFile);
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

    private async Task ParseAndValidateMobiAsync(string mobiFilePath)
    {
        var fileName = Path.GetFileName(mobiFilePath);
        TestContext.WriteLine($"\n处理: {fileName}");

        // 使用 MobiReader 解析
        using var book = await MobiReader.ReadAsync(mobiFilePath);

        // 验证基本信息
        Assert.IsNotNull(book, "解析结果不应为 null");
        Assert.IsNotNull(book.Metadata, "元数据不应为 null");

        TestContext.WriteLine($"  标题: {book.Metadata.Title ?? "(无)"}");
        TestContext.WriteLine($"  作者: {string.Join(", ", book.Metadata.Authors)}");
        TestContext.WriteLine($"  语言: {book.Metadata.Language ?? "(无)"}");
        TestContext.WriteLine($"  ASIN: {book.Metadata.Asin ?? "(无)"}");
        TestContext.WriteLine($"  ISBN: {book.Metadata.Isbn ?? "(无)"}");
        TestContext.WriteLine($"  Mobi 版本: {book.Metadata.MobiVersion}");
        TestContext.WriteLine($"  目录项数: {book.Navigation.Count}");
        TestContext.WriteLine($"  图片数: {book.Images.Count}");
        TestContext.WriteLine($"  有封面: {book.Cover != null}");

        // 如果有封面，验证能读取封面数据
        if (book.Cover != null)
        {
            var coverData = await book.Cover.ReadContentAsync();
            Assert.IsTrue(coverData.Length > 0, "封面数据不应为空");
            TestContext.WriteLine($"  封面大小: {coverData.Length:N0} 字节");
            TestContext.WriteLine($"  封面类型: {book.Cover.MediaType}");
        }

        // 验证目录结构
        if (book.Navigation.Count > 0)
        {
            PrintNavigation(book.Navigation, "  ");
        }

        // 验证图片
        if (book.Images.Count > 0)
        {
            TestContext.WriteLine($"  图片列表:");
            foreach (var image in book.Images.Take(5))
            {
                TestContext.WriteLine($"    - 索引 {image.Index}: {image.MediaType} ({image.Size:N0} bytes)");
            }
            if (book.Images.Count > 5)
            {
                TestContext.WriteLine($"    ... 还有 {book.Images.Count - 5} 个图片");
            }

            // 尝试读取第一张图片
            var firstImage = book.Images[0];
            var imageData = await book.ReadImageContentAsync(firstImage);
            Assert.IsTrue(imageData.Length > 0, "图片数据不应为空");
        }

        TestContext.WriteLine($"  ✅ 解析成功");
    }

    private void PrintNavigation(IReadOnlyList<MobiNavItem> items, string indent)
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
    /// 测试从流解析 Mobi。
    /// </summary>
    [TestMethod]
    public async Task ParseFromStream_ShouldSucceed()
    {
        var mobiFiles = Directory.Exists(InputDir)
            ? Directory.GetFiles(InputDir, "*.mobi")
                .Concat(Directory.GetFiles(InputDir, "*.azw"))
                .Concat(Directory.GetFiles(InputDir, "*.azw3"))
                .ToArray()
            : [];

        if (mobiFiles.Length == 0)
        {
            Assert.Inconclusive("没有可用的 Mobi 测试文件");
            return;
        }

        var testFile = mobiFiles[0];
        TestContext.WriteLine($"测试文件: {Path.GetFileName(testFile)}");

        using var fileStream = File.OpenRead(testFile);
        using var book = await MobiReader.ReadAsync(fileStream);

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
        var mobiFiles = Directory.Exists(InputDir)
            ? Directory.GetFiles(InputDir, "*.mobi")
                .Concat(Directory.GetFiles(InputDir, "*.azw"))
                .Concat(Directory.GetFiles(InputDir, "*.azw3"))
                .ToArray()
            : [];

        if (mobiFiles.Length == 0)
        {
            Assert.Inconclusive("没有可用的 Mobi 测试文件");
            return;
        }

        foreach (var mobiFile in mobiFiles)
        {
            using var book = await MobiReader.ReadAsync(mobiFile);

            // 记录元数据完整性
            var hasTitle = !string.IsNullOrEmpty(book.Metadata.Title);
            var hasAuthor = book.Metadata.Authors.Count > 0;
            var hasLanguage = !string.IsNullOrEmpty(book.Metadata.Language);
            var hasIdentifier = !string.IsNullOrEmpty(book.Metadata.Identifier);

            TestContext.WriteLine($"\n{Path.GetFileName(mobiFile)}:");
            TestContext.WriteLine($"  标题: {(hasTitle ? "✓" : "✗")} {book.Metadata.Title}");
            TestContext.WriteLine($"  作者: {(hasAuthor ? "✓" : "✗")} {string.Join(", ", book.Metadata.Authors)}");
            TestContext.WriteLine($"  语言: {(hasLanguage ? "✓" : "✗")} {book.Metadata.Language}");
            TestContext.WriteLine($"  标识符: {(hasIdentifier ? "✓" : "✗")} {book.Metadata.Identifier}");
            TestContext.WriteLine($"  主题数: {book.Metadata.Subjects.Count}");
            TestContext.WriteLine($"  贡献者数: {book.Metadata.Contributors.Count}");
            TestContext.WriteLine($"  自定义元数据数: {book.Metadata.CustomMetadata.Count}");
        }
    }

    /// <summary>
    /// 测试漫画/图片 Mobi 的解析。
    /// </summary>
    [TestMethod]
    public async Task ParseMangaMobi_ShouldHaveImages()
    {
        var mobiFiles = Directory.Exists(InputDir)
            ? Directory.GetFiles(InputDir, "*.mobi")
                .Concat(Directory.GetFiles(InputDir, "*.azw"))
                .Concat(Directory.GetFiles(InputDir, "*.azw3"))
                .ToArray()
            : [];

        if (mobiFiles.Length == 0)
        {
            Assert.Inconclusive("没有可用的 Mobi 测试文件");
            return;
        }

        foreach (var mobiFile in mobiFiles)
        {
            using var book = await MobiReader.ReadAsync(mobiFile);

            TestContext.WriteLine($"\n{Path.GetFileName(mobiFile)}:");
            TestContext.WriteLine($"  图片资源数: {book.Images.Count}");

            if (book.Images.Count > 0)
            {
                // 按索引排序显示图片顺序
                var sortedImages = book.Images.OrderBy(i => i.Index).ToList();
                foreach (var image in sortedImages.Take(5))
                {
                    TestContext.WriteLine($"    - 索引 {image.Index}: {image.MediaType} ({image.Size:N0} bytes)");
                }

                if (sortedImages.Count > 5)
                {
                    TestContext.WriteLine($"    ... 还有 {sortedImages.Count - 5} 个图片");
                }

                // 验证图片顺序
                var indices = sortedImages.Select(i => i.Index).ToList();
                for (var i = 1; i < indices.Count; i++)
                {
                    Assert.IsTrue(indices[i] > indices[i - 1], "图片索引应该是递增的");
                }
            }
        }
    }
}
