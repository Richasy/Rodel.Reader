// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics;
using FanQieDownloadConsoleApp;
using Richasy.RodelReader.Utilities.EpubGenerator;
using Richasy.RodelReader.Components.FanQie.Models;
using Richasy.RodelReader.Components.FanQie.Services;
using Richasy.RodelReader.Sources.FanQie;
using Richasy.RodelReader.Sources.FanQie.Models;
using Spectre.Console;

// 设置控制台编码
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

// 显示欢迎界面
ConsoleUI.ShowWelcome();

// 创建依赖
using var fanQieClient = new FanQieClient();
var epubBuilder = new EpubBuilder();
var downloadService = new FanQieDownloadService(fanQieClient, epubBuilder);

// 主循环
while (true)
{
    try
    {
        // 1. 搜索书籍
        var keyword = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]请输入搜索关键词[/] [grey](输入 q 退出)[/]:")
                .PromptStyle("cyan"));

        if (keyword.Equals("q", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine("[yellow]再见！[/]");
            break;
        }

        // 搜索书籍
        var searchResult = await SearchBooksAsync(fanQieClient, keyword);
        if (searchResult == null || searchResult.Items.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]未找到相关书籍，请尝试其他关键词。[/]");
            continue;
        }

        // 2. 选择书籍
        var selectedBook = ConsoleUI.SelectBook(searchResult.Items);
        if (selectedBook == null)
        {
            continue;
        }

        // 3. 获取书籍详情和目录
        var (bookDetail, volumes) = await GetBookInfoAsync(fanQieClient, selectedBook.BookId);
        if (bookDetail == null || volumes == null)
        {
            AnsiConsole.MarkupLine("[red]获取书籍信息失败。[/]");
            continue;
        }

        // 显示书籍详情
        ConsoleUI.ShowBookDetail(bookDetail, volumes);

        // 4. 选择下载范围
        var allChapters = volumes.SelectMany(v => v.Chapters).ToList();
        var freeChapters = allChapters.Where(c => !c.IsLocked && !c.NeedPay).ToList();

        if (freeChapters.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]该书籍没有免费章节可下载。[/]");
            continue;
        }

        var (startOrder, endOrder) = ConsoleUI.SelectChapterRange(freeChapters);

        // 5. 确认下载
        var downloadCount = endOrder - startOrder + 1;
        if (!AnsiConsole.Confirm($"确认下载第 [cyan]{startOrder}[/] 章到第 [cyan]{endOrder}[/] 章，共 [green]{downloadCount}[/] 章？"))
        {
            continue;
        }

        // 6. 开始下载
        var outputPath = await DownloadBookAsync(
            downloadService,
            selectedBook.BookId,
            startOrder,
            endOrder);

        if (!string.IsNullOrEmpty(outputPath))
        {
            // 7. 打开文件夹
            var folder = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(folder))
            {
                if (AnsiConsole.Confirm("下载完成！是否打开文件夹？"))
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

        // 询问是否继续
        if (!AnsiConsole.Confirm("是否继续下载其他书籍？"))
        {
            AnsiConsole.MarkupLine("[yellow]再见！[/]");
            break;
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]发生错误: {ex.Message}[/]");
        AnsiConsole.MarkupLine("[grey]按任意键继续...[/]");
        Console.ReadKey(true);
    }
}

// 搜索书籍
static async Task<SearchResult<BookItem>?> SearchBooksAsync(FanQieClient client, string keyword)
{
    return await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("green"))
        .StartAsync($"正在搜索 [cyan]{keyword}[/] ...", async ctx => await client.SearchBooksAsync(keyword));
}

// 获取书籍信息
static async Task<(BookDetail?, IReadOnlyList<BookVolume>?)> GetBookInfoAsync(FanQieClient client, string bookId)
{
    return await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("green"))
        .StartAsync("正在获取书籍信息...", async ctx =>
        {
            var detail = await client.GetBookDetailAsync(bookId);
            ctx.Status("正在获取目录...");
            var volumes = await client.GetBookTocAsync(bookId);
            return (detail, volumes);
        });
}

// 下载书籍
static async Task<string?> DownloadBookAsync(
    FanQieDownloadService downloadService,
    string bookId,
    int startOrder,
    int endOrder)
{
    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    var tempPath = Path.Combine(Path.GetTempPath(), "FanQieDownloader");

    var options = new SyncOptions
    {
        TempDirectory = tempPath,
        OutputDirectory = desktopPath,
        RetryFailedChapters = true,
        ContinueOnError = true,
        StartChapterOrder = startOrder,
        EndChapterOrder = endOrder,
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
            var downloadTask = ctx.AddTask("[green]下载进度[/]", maxValue: 100);
            downloadTask.StartTask();

            var progress = new Progress<SyncProgress>(p =>
            {
                downloadTask.Value = p.TotalProgress;

                var description = p.Phase switch
                {
                    SyncPhase.Analyzing => "[blue]分析中...[/]",
                    SyncPhase.FetchingToc => "[blue]获取目录...[/]",
                    SyncPhase.CheckingCache => "[blue]检查缓存...[/]",
                    SyncPhase.DownloadingChapters => p.DownloadDetail != null
                        ? $"[green]下载章节[/] [grey]({p.DownloadDetail.Completed}/{p.DownloadDetail.Total})[/]"
                        : "[green]下载中...[/]",
                    SyncPhase.DownloadingImages => "[cyan]下载图片...[/]",
                    SyncPhase.GeneratingEpub => "[yellow]生成 EPUB...[/]",
                    SyncPhase.CleaningUp => "[grey]清理缓存...[/]",
                    SyncPhase.Completed => "[green]完成！[/]",
                    SyncPhase.Failed => "[red]失败[/]",
                    SyncPhase.Cancelled => "[yellow]已取消[/]",
                    _ => p.Message ?? string.Empty,
                };

                downloadTask.Description = description;
            });

            var result = await downloadService.SyncBookAsync(bookId, options, progress);

            if (result.Success)
            {
                resultPath = result.EpubPath;
                stats = result.Statistics;
                downloadTask.Value = 100;
                downloadTask.Description = "[green]✓ 下载完成[/]";
            }
            else
            {
                downloadTask.Description = $"[red]✗ {result.ErrorMessage}[/]";
            }
        });

    // 显示统计信息
    if (stats != null && !string.IsNullOrEmpty(resultPath))
    {
        ConsoleUI.ShowDownloadResult(resultPath, stats);
    }

    return resultPath;
}
