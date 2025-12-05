// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics;
using LegadoDownloadConsoleApp;
using Richasy.RodelReader.Components.Legado.Models;
using Richasy.RodelReader.Components.Legado.Services;
using Richasy.RodelReader.Sources.Legado;
using Richasy.RodelReader.Sources.Legado.Models;
using Richasy.RodelReader.Utilities.EpubGenerator;
using Spectre.Console;

// 设置控制台编码
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

// 显示欢迎界面
ConsoleUI.ShowWelcome();

// 加载或创建配置
var config = AppConfig.Load();
if (config == null || !config.IsValid())
{
    config = ConsoleUI.CreateConfig();
}
else
{
    ConsoleUI.ShowCurrentConfig(config);
    AnsiConsole.WriteLine();
}

// 创建客户端
LegadoClient? legadoClient = null;
LegadoDownloadService? downloadService = null;

try
{
    legadoClient = new LegadoClient(config.ToClientOptions());
    var epubBuilder = new EpubBuilder();
    downloadService = new LegadoDownloadService(legadoClient, epubBuilder);

    // 测试连接
    await TestConnectionAsync(legadoClient);

    // 主循环
    while (true)
    {
        try
        {
            var choice = ConsoleUI.ShowMainMenu();

            switch (choice)
            {
                case "📚 浏览书架":
                    await BrowseBookshelfAsync(legadoClient, downloadService, config);
                    break;

                case "🔍 搜索书架":
                    await SearchBookshelfAsync(legadoClient, downloadService, config);
                    break;

                case "⚙️ 编辑配置":
                    var newConfig = ConsoleUI.EditConfig(config);
                    if (newConfig != null && newConfig != config)
                    {
                        config = newConfig;
                        // 重新创建客户端
                        legadoClient.Dispose();
                        legadoClient = new LegadoClient(config.ToClientOptions());
                        downloadService = new LegadoDownloadService(legadoClient, epubBuilder);
                        await TestConnectionAsync(legadoClient);
                    }

                    break;

                case "❌ 退出程序":
                    AnsiConsole.MarkupLine("[yellow]再见！[/]");
                    return;
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowError(ex.Message);
            if (AnsiConsole.Confirm("是否继续？"))
            {
                continue;
            }

            break;
        }
    }
}
finally
{
    legadoClient?.Dispose();
}

// 测试连接
static async Task TestConnectionAsync(LegadoClient client)
{
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("blue"))
        .StartAsync("正在测试服务器连接...", async ctx =>
        {
            try
            {
                var books = await client.GetBookshelfAsync();
                AnsiConsole.MarkupLine($"[green]✓ 连接成功！书架共 {books.Count} 本书籍[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ 连接失败: {Markup.Escape(ex.Message)}[/]");
                throw;
            }
        });

    AnsiConsole.WriteLine();
}

// 浏览书架
static async Task BrowseBookshelfAsync(LegadoClient client, LegadoDownloadService downloadService, AppConfig config)
{
    var books = await GetBookshelfAsync(client);
    if (books == null || books.Count == 0)
    {
        ConsoleUI.ShowWarning("书架为空");
        return;
    }

    var selectedBook = ConsoleUI.ShowBookshelf(books);
    if (selectedBook != null)
    {
        await HandleBookAsync(client, downloadService, selectedBook, config);
    }
}

// 搜索书架
static async Task SearchBookshelfAsync(LegadoClient client, LegadoDownloadService downloadService, AppConfig config)
{
    var keyword = AnsiConsole.Prompt(
        new TextPrompt<string>("[green]请输入搜索关键词[/]:")
            .PromptStyle("cyan"));

    if (string.IsNullOrWhiteSpace(keyword))
    {
        return;
    }

    var books = await GetBookshelfAsync(client);
    if (books == null || books.Count == 0)
    {
        ConsoleUI.ShowWarning("书架为空");
        return;
    }

    // 在本地过滤书籍
    var filteredBooks = books
        .Where(b =>
            (b.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (b.Author?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (b.Kind?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
        .ToList();

    var selectedBook = ConsoleUI.ShowBookshelf(filteredBooks, keyword);
    if (selectedBook != null)
    {
        await HandleBookAsync(client, downloadService, selectedBook, config);
    }
}

// 获取书架
static async Task<IReadOnlyList<Book>?> GetBookshelfAsync(LegadoClient client)
{
    return await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("blue"))
        .StartAsync("正在获取书架...", async _ => await client.GetBookshelfAsync());
}

// 处理书籍
static async Task HandleBookAsync(LegadoClient client, LegadoDownloadService downloadService, Book book, AppConfig config)
{
    // 获取章节列表
    var chapters = await GetChaptersAsync(client, book);
    if (chapters == null || chapters.Count == 0)
    {
        ConsoleUI.ShowError("无法获取章节列表");
        return;
    }

    // 显示书籍详情
    ConsoleUI.ShowBookDetail(book, chapters);

    // 询问用户操作
    var action = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]请选择操作[/]")
            .HighlightStyle(Style.Parse("cyan"))
            .AddChoices(
            [
                "📥 下载书籍",
                "↩️ 返回书架",
            ]));

    if (action != "📥 下载书籍")
    {
        return;
    }

    // 选择下载范围
    var (startIndex, endIndex) = ConsoleUI.SelectChapterRange(chapters);

    // 计算下载章节数
    var contentChapters = chapters.Where(c => !c.IsVolume).ToList();
    var actualStart = startIndex ?? contentChapters.Min(c => c.Index);
    var actualEnd = endIndex ?? contentChapters.Max(c => c.Index);
    var downloadCount = contentChapters.Count(c => c.Index >= actualStart && c.Index <= actualEnd);

    // 确认下载
    var confirmMessage = startIndex.HasValue || endIndex.HasValue
        ? $"确认下载第 [cyan]{actualStart + 1}[/] 章到第 [cyan]{actualEnd + 1}[/] 章，共 [green]{downloadCount}[/] 章？"
        : $"确认下载全部 [green]{downloadCount}[/] 章？";

    if (!AnsiConsole.Confirm(confirmMessage))
    {
        return;
    }

    // 开始下载
    var outputPath = await DownloadBookAsync(downloadService, book, config, startIndex, endIndex);

    if (!string.IsNullOrEmpty(outputPath))
    {
        // 询问是否打开文件夹
        var folder = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(folder) && AnsiConsole.Confirm("下载完成！是否打开文件夹？"))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{outputPath}\"",
                UseShellExecute = true,
            });
        }
    }
}

// 获取章节列表
static async Task<IReadOnlyList<Chapter>?> GetChaptersAsync(LegadoClient client, Book book)
{
    return await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("blue"))
        .StartAsync("正在获取章节列表...", async _ => await client.GetChapterListAsync(book.BookUrl));
}

// 下载书籍
static async Task<string?> DownloadBookAsync(
    LegadoDownloadService downloadService,
    Book book,
    AppConfig config,
    int? startIndex,
    int? endIndex)
{
    var outputPath = config.OutputDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    var tempPath = Path.Combine(Path.GetTempPath(), "LegadoDownloader");

    var options = new SyncOptions
    {
        TempDirectory = tempPath,
        OutputDirectory = outputPath,
        RetryFailedChapters = true,
        ContinueOnError = true,
        StartChapterIndex = startIndex,
        EndChapterIndex = endIndex,
        MaxConcurrentDownloads = config.MaxConcurrentDownloads,
    };

    string? resultPath = null;
    SyncStatistics? stats = null;

    await AnsiConsole.Progress()
        .AutoRefresh(true)
        .AutoClear(false)
        .HideCompleted(false)
        .Columns(
        [
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new RemainingTimeColumn(),
            new SpinnerColumn(),
        ])
        .StartAsync(async ctx =>
        {
            var downloadTask = ctx.AddTask("[blue]下载进度[/]", maxValue: 100);
            downloadTask.StartTask();

            var progress = new Progress<SyncProgress>(p =>
            {
                downloadTask.Value = p.TotalProgress;

                var description = p.Phase switch
                {
                    SyncPhase.Analyzing => "[grey]分析现有文件...[/]",
                    SyncPhase.FetchingToc => "[blue]获取目录...[/]",
                    SyncPhase.CheckingCache => "[blue]检查缓存...[/]",
                    SyncPhase.DownloadingChapters => p.DownloadDetail != null
                        ? $"[green]下载章节[/] [grey]({p.DownloadDetail.Completed}/{p.DownloadDetail.Total})[/] {(p.DownloadDetail.Failed > 0 ? $"[red]失败 {p.DownloadDetail.Failed}[/]" : string.Empty)}"
                        : "[green]下载中...[/]",
                    SyncPhase.DownloadingImages => "[cyan]下载图片...[/]",
                    SyncPhase.GeneratingEpub => p.GenerateDetail != null
                        ? $"[yellow]生成 EPUB[/] [grey]({p.GenerateDetail.ProcessedChapters}/{p.GenerateDetail.TotalChapters})[/]"
                        : "[yellow]生成 EPUB...[/]",
                    SyncPhase.CleaningUp => "[grey]清理缓存...[/]",
                    SyncPhase.Completed => "[green]✓ 完成！[/]",
                    SyncPhase.Failed => $"[red]✗ 失败: {Markup.Escape(p.Message ?? string.Empty)}[/]",
                    
                    _ => p.Message ?? string.Empty,
                };

                downloadTask.Description = description;
            });

            var result = await downloadService.SyncBookAsync(book, options, progress);

            if (result.Success)
            {
                resultPath = result.EpubPath;
                stats = result.Statistics;
                downloadTask.Value = 100;
                downloadTask.Description = "[green]✓ 下载完成[/]";
            }
            else if (result.IsCancelled)
            {
                downloadTask.Description = "[yellow]已取消[/]";
            }
            else
            {
                downloadTask.Description = $"[red]✗ {Markup.Escape(result.ErrorMessage ?? "下载失败")}[/]";
            }
        });

    // 显示统计信息
    if (stats != null && !string.IsNullOrEmpty(resultPath))
    {
        ConsoleUI.ShowDownloadResult(resultPath, stats);
    }

    return resultPath;
}
