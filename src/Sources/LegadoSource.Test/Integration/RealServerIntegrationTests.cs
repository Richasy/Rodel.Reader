// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Test.Integration;

/// <summary>
/// 真实服务器集成测试.
/// </summary>
/// <remarks>
/// 使用 hectorqin/reader 服务器进行测试.
/// 需要本地服务器运行，CI 环境中跳过.
/// </remarks>
[TestClass]
[TestCategory("Integration")]
[Ignore("需要本地服务器运行")]
public class RealServerIntegrationTests
{
    // 服务器配置
    private const string BaseUrl = "http://192.168.100.104:4396";
    private const string AccessToken = "richasy:12eef2feb08e077b6635b7c5317dff77";

    private static LegadoClient CreateClient()
    {
        var options = new LegadoClientOptions
        {
            BaseUrl = BaseUrl,
            ServerType = ServerType.HectorqinReader,
            AccessToken = AccessToken,
        };
        return new LegadoClient(options);
    }

    [TestMethod]
    public async Task GetBookshelf_ReturnsBooks()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var books = await client.GetBookshelfAsync();

        // Assert
        Assert.IsNotNull(books);
        Assert.IsTrue(books.Count > 0, "书架为空");

        Console.WriteLine($"书架共有 {books.Count} 本书：");
        foreach (var book in books)
        {
            Console.WriteLine($"  - 《{book.Name}》 作者: {book.Author}");
            Console.WriteLine($"    BookUrl: {book.BookUrl}");
        }
    }

    [TestMethod]
    public async Task GetBookshelf_ContainsDouPoCangQiong()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var books = await client.GetBookshelfAsync();

        // Assert
        var douPo = books.FirstOrDefault(b =>
            b.Name?.Contains("斗破苍穹", StringComparison.Ordinal) == true);

        Assert.IsNotNull(douPo, "书架中未找到《斗破苍穹》");
        Console.WriteLine($"找到书籍：《{douPo.Name}》");
        Console.WriteLine($"  作者: {douPo.Author}");
        Console.WriteLine($"  BookUrl: {douPo.BookUrl}");
    }

    [TestMethod]
    public async Task GetChapterList_ForDouPoCangQiong_ReturnsChapters()
    {
        // Arrange
        using var client = CreateClient();
        var books = await client.GetBookshelfAsync();

        var douPo = books.FirstOrDefault(b =>
            b.Name?.Contains("斗破苍穹", StringComparison.Ordinal) == true);
        Assert.IsNotNull(douPo, "书架中未找到《斗破苍穹》");

        // Act
        var chapters = await client.GetChapterListAsync(douPo.BookUrl);

        // Assert
        Assert.IsNotNull(chapters);
        Assert.IsTrue(chapters.Count > 0, "章节列表为空");

        Console.WriteLine($"《{douPo.Name}》共有 {chapters.Count} 章");
        Console.WriteLine("前10章：");
        foreach (var chapter in chapters.Take(10))
        {
            Console.WriteLine($"  第{chapter.Index + 1}章: {chapter.Title}");
        }
    }

    [TestMethod]
    public async Task GetFirstChapterContent_ForDouPoCangQiong_ReturnsContent()
    {
        // Arrange
        using var client = CreateClient();
        var books = await client.GetBookshelfAsync();

        var douPo = books.FirstOrDefault(b =>
            b.Name?.Contains("斗破苍穹", StringComparison.Ordinal) == true);
        Assert.IsNotNull(douPo, "书架中未找到《斗破苍穹》");

        // 先获取章节列表以获取第一章标题
        var chapters = await client.GetChapterListAsync(douPo.BookUrl);
        Assert.IsTrue(chapters.Count > 0, "章节列表为空");

        var firstChapter = chapters[0];

        // Act
        var content = await client.GetChapterContentAsync(douPo.BookUrl, 0);

        // Assert
        Assert.IsNotNull(content);
        Assert.IsFalse(string.IsNullOrWhiteSpace(content.Content), "章节内容为空");

        Console.WriteLine($"《{douPo.Name}》第一章内容：");
        Console.WriteLine($"  章节标题: {firstChapter.Title}");
        Console.WriteLine($"  内容长度: {content.Content.Length} 字符");
        Console.WriteLine($"  内容预览 (前500字):");
        Console.WriteLine("  " + new string('-', 50));

        var preview = content.Content.Length > 500
            ? content.Content[..500] + "..."
            : content.Content;
        Console.WriteLine($"  {preview}");
    }

    [TestMethod]
    public async Task FullWorkflow_GetBookshelf_GetChapters_GetFirstChapterContent()
    {
        // 完整工作流测试：获取书架 -> 找到斗破苍穹 -> 获取章节列表 -> 获取第一章内容

        Console.WriteLine("=== 开始完整工作流测试 ===\n");

        // Step 1: 获取书架
        Console.WriteLine("Step 1: 获取书架...");
        using var client = CreateClient();
        var books = await client.GetBookshelfAsync();
        Assert.IsNotNull(books);
        Console.WriteLine($"  ✓ 书架共有 {books.Count} 本书\n");

        // Step 2: 查找斗破苍穹
        Console.WriteLine("Step 2: 查找《斗破苍穹》...");
        var douPo = books.FirstOrDefault(b =>
            b.Name?.Contains("斗破苍穹", StringComparison.Ordinal) == true);
        Assert.IsNotNull(douPo, "书架中未找到《斗破苍穹》");
        Console.WriteLine($"  ✓ 找到《{douPo.Name}》- 作者: {douPo.Author}\n");

        // Step 3: 获取章节列表
        Console.WriteLine("Step 3: 获取章节列表...");
        var chapters = await client.GetChapterListAsync(douPo.BookUrl);
        Assert.IsNotNull(chapters);
        Assert.IsTrue(chapters.Count > 0, "章节列表为空");
        Console.WriteLine($"  ✓ 共有 {chapters.Count} 章");
        Console.WriteLine($"  第一章: {chapters[0].Title}\n");

        // Step 4: 获取第一章内容
        Console.WriteLine("Step 4: 获取第一章内容...");
        var content = await client.GetChapterContentAsync(douPo.BookUrl, 0);
        Assert.IsNotNull(content);
        Assert.IsFalse(string.IsNullOrWhiteSpace(content.Content), "章节内容为空");
        Console.WriteLine($"  ✓ 内容长度: {content.Content.Length} 字符");
        Console.WriteLine($"  内容预览: {content.Content[..Math.Min(200, content.Content.Length)]}...\n");

        Console.WriteLine("=== 完整工作流测试通过 ===");
    }
}
