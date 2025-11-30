// Copyright (c) Reader Copilot. All rights reserved.

using System.Text;
using System.Text.Json;
using VersOne.Epub;

namespace EpubGenerator.Test.Integration;

/// <summary>
/// 预期元数据模型（与 PowerShell 脚本生成的 JSON 对应）.
/// </summary>
public sealed class ExpectedMetadata
{
    public string SourceFile { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string ChapterPattern { get; set; } = string.Empty;
    public bool IsCustomPattern { get; set; }
    public int TotalChapters { get; set; }
    public List<ExpectedChapter> Chapters { get; set; } = [];
    public string GeneratedAt { get; set; } = string.Empty;
}

public sealed class ExpectedChapter
{
    public int Index { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ContentLength { get; set; }
}

/// <summary>
/// EPUB 生成集成测试.
/// 从真实的 TXT 文件生成 EPUB，然后使用 VersOne.Epub 解析验证.
/// </summary>
[TestClass]
public sealed class EpubGenerationIntegrationTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string InputDir = Path.Combine(TestDataDir, "Input");
    private static readonly string ExpectedDir = Path.Combine(TestDataDir, "Expected");
    private static readonly string OutputDir = Path.Combine(TestDataDir, "Output");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private EpubBuilder _builder = null!;
    private RegexTextSplitter _splitter = null!;

    static EpubGenerationIntegrationTests()
    {
        // 注册 GB2312/GBK 编码支持
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [TestInitialize]
    public void Setup()
    {
        _builder = new EpubBuilder();
        _splitter = new RegexTextSplitter();

        // 确保输出目录存在
        if (!Directory.Exists(OutputDir))
        {
            Directory.CreateDirectory(OutputDir);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        // 可选：清理生成的 EPUB 文件
        // 如果需要保留文件以便调试，可以注释掉下面的代码
        // foreach (var file in Directory.GetFiles(OutputDir, "*.epub"))
        // {
        //     File.Delete(file);
        // }
    }

    /// <summary>
    /// 动态测试：遍历所有 Input 文件夹中的 txt 文件并生成 EPUB.
    /// </summary>
    [TestMethod]
    public async Task GenerateAndValidateAllEpubs()
    {
        // 检查 Input 文件夹是否存在
        if (!Directory.Exists(InputDir))
        {
            Assert.Inconclusive($"Input 目录不存在: {InputDir}");
            return;
        }

        var txtFiles = Directory.GetFiles(InputDir, "*.txt");
        if (txtFiles.Length == 0)
        {
            Assert.Inconclusive("Input 目录中没有 txt 文件，请先添加测试数据");
            return;
        }

        var results = new List<(string FileName, bool Success, string? Error)>();

        foreach (var txtFile in txtFiles)
        {
            var fileName = Path.GetFileName(txtFile);
            try
            {
                await ProcessAndValidateTxtFileAsync(txtFile);
                results.Add((fileName, true, null));
            }
            catch (Exception ex)
            {
                results.Add((fileName, false, ex.Message));
            }
        }

        // 输出结果摘要
        Console.WriteLine("\n========== 测试结果摘要 ==========");
        foreach (var (fileName, success, error) in results)
        {
            if (success)
            {
                Console.WriteLine($"✅ {fileName}");
            }
            else
            {
                Console.WriteLine($"❌ {fileName}: {error}");
            }
        }
        Console.WriteLine("==================================\n");

        // 确保所有测试都通过
        var failures = results.Where(r => !r.Success).ToList();
        if (failures.Count > 0)
        {
            Assert.Fail($"有 {failures.Count} 个文件处理失败:\n" +
                string.Join("\n", failures.Select(f => $"  - {f.FileName}: {f.Error}")));
        }
    }

    private async Task ProcessAndValidateTxtFileAsync(string txtFilePath)
    {
        var fileName = Path.GetFileName(txtFilePath);
        var baseName = Path.GetFileNameWithoutExtension(txtFilePath);
        Console.WriteLine($"\n处理: {fileName}");

        // 1. 读取文件内容（自动检测编码）
        var encoding = DetectFileEncoding(txtFilePath);
        Console.WriteLine($"  检测到编码: {encoding.EncodingName}");
        var rawContent = await File.ReadAllTextAsync(txtFilePath, encoding);
        
        // 清理无效的 XML 控制字符
        var content = SanitizeContent(rawContent);
        
        var lines = content.Split('\n');
        var firstLine = lines[0].Trim();

        // 2. 解析正则表达式
        string chapterPattern;
        string contentToProcess;

        if (IsCustomRegex(firstLine))
        {
            chapterPattern = firstLine[1..^1]; // 去掉首尾的 /
            contentToProcess = string.Join('\n', lines.Skip(1));
            Console.WriteLine($"  使用自定义正则: {chapterPattern}");
        }
        else
        {
            chapterPattern = RegexTextSplitter.DefaultChapterPattern;
            contentToProcess = content;
            Console.WriteLine($"  使用默认正则: {chapterPattern}");
        }

        // 3. 分割章节
        var splitOptions = new SplitOptions { ChapterPattern = chapterPattern };
        var chapters = _splitter.Split(contentToProcess, splitOptions);
        Console.WriteLine($"  分割得到 {chapters.Count} 个章节");

        // 4. 创建元数据
        var metadata = new EpubMetadata
        {
            Title = baseName,
            Author = "测试作者",
            Language = "zh",
            Identifier = $"test-{baseName}-{DateTime.Now:yyyyMMddHHmmss}"
        };

        // 5. 生成 EPUB
        var epubPath = Path.Combine(OutputDir, $"{baseName}.epub");
        await _builder.BuildToFileAsync(metadata, chapters, epubPath);
        Console.WriteLine($"  生成 EPUB: {epubPath}");

        // 6. 验证 EPUB
        await ValidateEpubAsync(epubPath, baseName, chapters);
    }

    /// <summary>
    /// 清理内容中的无效 XML 控制字符.
    /// </summary>
    private static string SanitizeContent(string content)
    {
        // XML 1.0 有效字符: #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD]
        var sb = new StringBuilder(content.Length);
        foreach (var c in content)
        {
            if (c >= 0x20 || c == '\t' || c == '\n' || c == '\r')
            {
                if (c < 0xFFFE) // 排除 0xFFFE 和 0xFFFF
                {
                    sb.Append(c);
                }
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 检测文件编码.
    /// </summary>
    private static Encoding DetectFileEncoding(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);

        // 检查 BOM
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8;
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            return Encoding.Unicode;
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode;
        }

        // 尝试 UTF-8 无 BOM
        try
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            _ = utf8.GetString(bytes);
            return Encoding.UTF8;
        }
        catch
        {
            // 不是有效的 UTF-8，使用 GBK/GB2312
            return Encoding.GetEncoding("gb2312");
        }
    }

    private static async Task ValidateEpubAsync(string epubPath, string baseName, IReadOnlyList<ChapterInfo> expectedChapters)
    {
        // 使用 VersOne.Epub 解析 EPUB
        var epubBook = await EpubReader.ReadBookAsync(epubPath);

        // 验证元数据
        Assert.AreEqual(baseName, epubBook.Title, $"书名不匹配");
        Assert.AreEqual("测试作者", epubBook.Author, $"作者不匹配");

        // 验证章节数量（通过阅读顺序项）
        var readingOrder = epubBook.ReadingOrder;
        Console.WriteLine($"  EPUB 阅读顺序项: {readingOrder.Count}");

        // 加载预期元数据（如果存在）
        var expectedPath = Path.Combine(ExpectedDir, $"{baseName}.json");
        if (File.Exists(expectedPath))
        {
            await ValidateWithExpectedMetadataAsync(epubBook, expectedPath, expectedChapters);
        }
        else
        {
            Console.WriteLine($"  ⚠️ 未找到预期元数据文件: {baseName}.json，跳过详细验证");
            // 基本验证
            Assert.IsTrue(readingOrder.Count > 0, "EPUB 应该有阅读内容");
        }
    }

    private static async Task ValidateWithExpectedMetadataAsync(
        EpubBook epubBook,
        string expectedPath,
        IReadOnlyList<ChapterInfo> actualChapters)
    {
        var json = await File.ReadAllTextAsync(expectedPath);
        var expected = JsonSerializer.Deserialize<ExpectedMetadata>(json, JsonOptions);

        Assert.IsNotNull(expected, "无法解析预期元数据");

        // 验证章节数量
        Assert.AreEqual(expected.TotalChapters, actualChapters.Count,
            $"章节数量不匹配：预期 {expected.TotalChapters}，实际 {actualChapters.Count}");

        // 验证每个章节
        for (var i = 0; i < expected.Chapters.Count && i < actualChapters.Count; i++)
        {
            var expectedChapter = expected.Chapters[i];
            var actualChapter = actualChapters[i];

            Assert.AreEqual(expectedChapter.Title, actualChapter.Title,
                $"第 {i + 1} 章标题不匹配");

            // 内容长度验证：PowerShell 和 C# 使用相同的计算方式
            // 两者都使用 lines.Join("\n").Trim().Length 计算
            // 使用较小容差，因为实际长度应该很接近预期
            var actualLength = actualChapter.Content?.Length ?? 0;
            var lengthDiff = Math.Abs(expectedChapter.ContentLength - actualLength);
            var tolerance = Math.Max(expectedChapter.ContentLength * 0.05, 100); // 至少 100 字符或 5% 容差
            Assert.IsTrue(lengthDiff <= tolerance,
                $"第 {i + 1} 章内容长度差异过大：预期 {expectedChapter.ContentLength}，实际 {actualLength}，差异 {lengthDiff}（容差 {tolerance:F0}）");
        }

        Console.WriteLine($"  ✅ 与预期元数据验证通过");
    }

    private static bool IsCustomRegex(string line)
    {
        return line.StartsWith('/') && line.EndsWith('/') && line.Length > 2;
    }

    /// <summary>
    /// 验证生成的 EPUB 文件结构完整性.
    /// </summary>
    [TestMethod]
    public async Task ValidateEpubStructure_ShouldContainRequiredFiles()
    {
        // 检查是否有输入文件
        if (!Directory.Exists(InputDir))
        {
            Assert.Inconclusive("Input 目录不存在，请添加测试用的 TXT 文件");
            return;
        }

        var txtFiles = Directory.GetFiles(InputDir, "*.txt");
        if (txtFiles.Length == 0)
        {
            Assert.Inconclusive("Input 目录中没有 TXT 文件，请添加测试用的 TXT 文件");
            return;
        }

        // 使用第一个 TXT 文件生成 EPUB 用于结构验证
        var testFile = txtFiles[0];
        var baseName = Path.GetFileNameWithoutExtension(testFile);
        var epubPath = Path.Combine(OutputDir, $"{baseName}_structure_test.epub");

        // 生成 EPUB
        var encoding = DetectFileEncoding(testFile);
        var rawContent = await File.ReadAllTextAsync(testFile, encoding);
        var content = SanitizeContent(rawContent);

        var lines = content.Split('\n');
        var firstLine = lines.Length > 0 ? lines[0].Trim() : string.Empty;
        var pattern = IsCustomRegex(firstLine)
            ? firstLine[1..^1]
            : RegexTextSplitter.DefaultChapterPattern;

        var splitOptions = new SplitOptions { ChapterPattern = pattern };
        var chapters = _splitter.Split(content.AsSpan(), splitOptions);

        var metadata = new EpubMetadata
        {
            Title = baseName,
            Author = "结构测试",
            Language = "zh",
        };

        await _builder.BuildToFileAsync(metadata, chapters, epubPath);

        // 验证结构
        var epubBook = await EpubReader.ReadBookAsync(epubPath);

        Assert.IsNotNull(epubBook.Content, "Content 不应为 null");
        Assert.IsTrue(epubBook.ReadingOrder.Count > 0, "应该有阅读内容");
        Assert.IsTrue(epubBook.Content.Css.Local.Count > 0, "应该有 CSS 样式表");

        Console.WriteLine($"✅ 结构验证通过 (章节: {epubBook.ReadingOrder.Count}, CSS: {epubBook.Content.Css.Local.Count})");

        // 清理测试文件
        if (File.Exists(epubPath))
        {
            File.Delete(epubPath);
        }
    }
}
