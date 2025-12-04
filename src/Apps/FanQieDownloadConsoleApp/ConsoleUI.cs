// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.FanQie.Models;
using Richasy.RodelReader.Sources.FanQie.Models;
using Spectre.Console;

namespace FanQieDownloadConsoleApp;

/// <summary>
/// 控制台 UI 辅助类.
/// </summary>
internal static class ConsoleUI
{
    /// <summary>
    /// 显示欢迎界面.
    /// </summary>
    public static void ShowWelcome()
    {
        AnsiConsole.Clear();

        var title = new FigletText("FanQie Downloader")
            .LeftJustified()
            .Color(Color.Green);

        AnsiConsole.Write(title);
        AnsiConsole.MarkupLine("[grey]番茄小说下载器 - 将番茄小说下载为 EPUB 格式[/]");
        AnsiConsole.MarkupLine("[grey]默认下载到桌面[/]");
        AnsiConsole.WriteLine();

        var rule = new Rule("[green]开始使用[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 选择书籍.
    /// </summary>
    public static BookItem? SelectBook(IReadOnlyList<BookItem> books)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[yellow]序号[/]").Centered())
            .AddColumn(new TableColumn("[cyan]书名[/]"))
            .AddColumn(new TableColumn("[green]作者[/]"))
            .AddColumn(new TableColumn("[blue]分类[/]"))
            .AddColumn(new TableColumn("[magenta]状态[/]").Centered());

        for (var i = 0; i < books.Count; i++)
        {
            var book = books[i];
            var status = book.CreationStatus == BookCreationStatus.Completed
                ? "[green]完结[/]"
                : "[yellow]连载[/]";

            table.AddRow(
                $"[yellow]{i + 1}[/]",
                Markup.Escape(book.Title),
                Markup.Escape(book.Author ?? "未知"),
                Markup.Escape(book.Category ?? "未知"),
                status);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var selection = AnsiConsole.Prompt(
            new TextPrompt<int>("[green]请输入书籍序号[/] [grey](输入 0 返回)[/]:")
                .PromptStyle("cyan")
                .ValidationErrorMessage("[red]请输入有效的序号[/]")
                .Validate(n => n >= 0 && n <= books.Count
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]请输入 0-{books.Count} 之间的数字[/]")));

        if (selection == 0)
        {
            return null;
        }

        return books[selection - 1];
    }

    /// <summary>
    /// 显示书籍详情.
    /// </summary>
    public static void ShowBookDetail(BookDetail detail, IReadOnlyList<BookVolume> volumes)
    {
        AnsiConsole.WriteLine();

        var panel = new Panel(
            new Markup($"""
                [cyan]书名:[/] {Markup.Escape(detail.Title)}
                [cyan]作者:[/] {Markup.Escape(detail.Author ?? "未知")}
                [cyan]分类:[/] {Markup.Escape(detail.Category ?? "未知")}
                [cyan]状态:[/] {(detail.CreationStatus == BookCreationStatus.Completed ? "[green]已完结[/]" : "[yellow]连载中[/]")}
                [cyan]字数:[/] {FormatWordCount(detail.WordCount)}
                [cyan]章节:[/] {detail.ChapterCount} 章
                [cyan]评分:[/] {detail.Score ?? "暂无"}
                """))
        {
            Header = new PanelHeader("[green] 书籍信息 [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green),
            Padding = new Padding(2, 1, 2, 1),
        };

        AnsiConsole.Write(panel);

        // 显示卷信息
        if (volumes.Count > 1)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]目录结构:[/]");
            foreach (var volume in volumes)
            {
                var freeCount = volume.Chapters.Count(c => !c.IsLocked && !c.NeedPay);
                AnsiConsole.MarkupLine($"  [blue]📁 {Markup.Escape(volume.Name)}[/] [grey]({freeCount}/{volume.Chapters.Count} 章免费)[/]");
            }
        }

        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 选择章节范围.
    /// </summary>
    public static (int Start, int End) SelectChapterRange(IReadOnlyList<ChapterItem> freeChapters)
    {
        var minOrder = freeChapters.Min(c => c.Order);
        var maxOrder = freeChapters.Max(c => c.Order);

        AnsiConsole.MarkupLine($"[grey]可下载范围: 第 {minOrder} 章 - 第 {maxOrder} 章 (共 {freeChapters.Count} 章免费)[/]");
        AnsiConsole.WriteLine();

        var downloadChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]请选择下载方式[/]")
                .AddChoices(["下载全部免费章节", "选择下载范围"]));

        if (downloadChoice == "下载全部免费章节")
        {
            return (minOrder, maxOrder);
        }

        // 选择范围
        var startOrder = AnsiConsole.Prompt(
            new TextPrompt<int>($"[green]起始章节[/] [grey](第 {minOrder}-{maxOrder} 章)[/]:")
                .PromptStyle("cyan")
                .DefaultValue(minOrder)
                .ValidationErrorMessage("[red]请输入有效的章节号[/]")
                .Validate(n => n >= minOrder && n <= maxOrder
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]请输入 {minOrder}-{maxOrder} 之间的数字[/]")));

        var endOrder = AnsiConsole.Prompt(
            new TextPrompt<int>($"[green]结束章节[/] [grey](第 {startOrder}-{maxOrder} 章)[/]:")
                .PromptStyle("cyan")
                .DefaultValue(maxOrder)
                .ValidationErrorMessage("[red]请输入有效的章节号[/]")
                .Validate(n => n >= startOrder && n <= maxOrder
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]请输入 {startOrder}-{maxOrder} 之间的数字[/]")));

        return (startOrder, endOrder);
    }

    /// <summary>
    /// 显示下载结果.
    /// </summary>
    public static void ShowDownloadResult(string epubPath, SyncStatistics stats)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .AddColumn("[green]统计项[/]")
            .AddColumn("[cyan]数值[/]");

        table.AddRow("新下载章节", $"[green]{stats.NewlyDownloaded}[/] 章");
        table.AddRow("缓存恢复", $"[blue]{stats.RestoredFromCache}[/] 章");
        table.AddRow("失败章节", stats.Failed > 0 ? $"[red]{stats.Failed}[/] 章" : "[grey]0 章[/]");
        table.AddRow("下载图片", $"[cyan]{stats.ImagesDownloaded}[/] 张");
        table.AddRow("总耗时", $"[yellow]{stats.Duration:mm\\:ss\\.fff}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[green]✓ EPUB 已保存至:[/] [link]{Markup.Escape(epubPath)}[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 格式化字数.
    /// </summary>
    private static string FormatWordCount(int wordCount)
    {
        return wordCount switch
        {
            >= 10000 => $"{wordCount / 10000.0:F1} 万字",
            >= 1000 => $"{wordCount / 1000.0:F1} 千字",
            _ => $"{wordCount} 字",
        };
    }
}
