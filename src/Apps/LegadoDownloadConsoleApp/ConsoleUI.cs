// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.Legado.Models;
using Richasy.RodelReader.Sources.Legado.Models;
using Richasy.RodelReader.Sources.Legado.Models.Enums;
using Spectre.Console;

namespace LegadoDownloadConsoleApp;

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

        var title = new FigletText("Legado Downloader")
            .LeftJustified()
            .Color(Color.Blue);

        AnsiConsole.Write(title);
        AnsiConsole.MarkupLine("[grey]开源阅读下载器 - 将书架书籍下载为 EPUB 格式[/]");
        AnsiConsole.MarkupLine("[grey]支持手机端开源阅读和 hectorqin/reader 服务器[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 显示主菜单.
    /// </summary>
    public static string ShowMainMenu()
    {
        var rule = new Rule("[blue]主菜单[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]请选择操作[/]")
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices(
                [
                    "📚 浏览书架",
                    "🔍 搜索书架",
                    "⚙️ 编辑配置",
                    "❌ 退出程序",
                ]));
    }

    /// <summary>
    /// 创建初始配置.
    /// </summary>
    public static AppConfig CreateConfig()
    {
        AnsiConsole.WriteLine();
        var rule = new Rule("[yellow]初始化配置[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[grey]检测到尚未配置服务器信息，请先进行初始化设置。[/]");
        AnsiConsole.WriteLine();

        var config = new AppConfig();

        // 选择服务器类型
        var serverTypeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]请选择服务器类型[/]")
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices(
                [
                    "📱 手机端开源阅读 (Legado)",
                    "🖥️ hectorqin/reader 服务器",
                ]));

        config.ServerType = serverTypeChoice.Contains("hectorqin", StringComparison.OrdinalIgnoreCase)
            ? ServerType.HectorqinReader
            : ServerType.Legado;

        // 输入服务器地址
        AnsiConsole.WriteLine();
        if (config.ServerType == ServerType.Legado)
        {
            AnsiConsole.MarkupLine("[grey]请确保手机端开源阅读已开启 Web 服务。[/]");
            AnsiConsole.MarkupLine("[grey]格式示例: http://192.168.1.100:1122[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]格式示例: http://your-server.com:4396[/]");
        }

        config.ServerUrl = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]请输入服务器地址[/]:")
                .PromptStyle("cyan")
                .ValidationErrorMessage("[red]请输入有效的 URL[/]")
                .Validate(url =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        return ValidationResult.Error("[red]服务器地址不能为空[/]");
                    }

                    if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        return ValidationResult.Error("[red]服务器地址必须以 http:// 或 https:// 开头[/]");
                    }

                    return ValidationResult.Success();
                }));

        // 如果是 hectorqin/reader，询问访问令牌
        if (config.ServerType == ServerType.HectorqinReader)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]如果服务器启用了多用户模式，请输入访问令牌。[/]");
            AnsiConsole.MarkupLine("[grey]如果无需认证，可直接按回车跳过。[/]");

            var token = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]访问令牌[/] [grey](可选)[/]:")
                    .PromptStyle("cyan")
                    .AllowEmpty());

            config.AccessToken = string.IsNullOrWhiteSpace(token) ? null : token;
        }

        // 设置输出目录
        AnsiConsole.WriteLine();
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        AnsiConsole.MarkupLine($"[grey]默认输出目录为桌面: {Markup.Escape(desktopPath)}[/]");

        if (AnsiConsole.Confirm("是否使用自定义输出目录？", defaultValue: false))
        {
            config.OutputDirectory = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]请输入输出目录路径[/]:")
                    .PromptStyle("cyan")
                    .ValidationErrorMessage("[red]请输入有效的目录路径[/]")
                    .Validate(path =>
                    {
                        if (string.IsNullOrWhiteSpace(path))
                        {
                            return ValidationResult.Error("[red]目录路径不能为空[/]");
                        }

                        try
                        {
                            Directory.CreateDirectory(path);
                            return ValidationResult.Success();
                        }
                        catch
                        {
                            return ValidationResult.Error("[red]无法创建或访问该目录[/]");
                        }
                    }));
        }
        else
        {
            config.OutputDirectory = desktopPath;
        }

        // 设置并发数
        AnsiConsole.WriteLine();
        config.MaxConcurrentDownloads = AnsiConsole.Prompt(
            new TextPrompt<int>("[green]最大并发下载数[/] [grey](1-20, 默认 3)[/]:")
                .PromptStyle("cyan")
                .DefaultValue(3)
                .ValidationErrorMessage("[red]请输入 1-20 之间的数字[/]")
                .Validate(n => n is >= 1 and <= 20
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]请输入 1-20 之间的数字[/]")));

        // 保存配置
        config.Save();

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]✓ 配置已保存！[/]");
        AnsiConsole.MarkupLine($"[grey]配置文件路径: {Markup.Escape(AppConfig.GetConfigPath())}[/]");
        AnsiConsole.WriteLine();

        return config;
    }

    /// <summary>
    /// 编辑配置.
    /// </summary>
    public static AppConfig? EditConfig(AppConfig currentConfig)
    {
        AnsiConsole.WriteLine();
        var rule = new Rule("[yellow]编辑配置[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // 显示当前配置
        ShowCurrentConfig(currentConfig);

        var editChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]请选择要编辑的项目[/]")
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices(
                [
                    "🔗 修改服务器地址",
                    "🔄 修改服务器类型",
                    "🔑 修改访问令牌",
                    "📁 修改输出目录",
                    "⚡ 修改并发数",
                    "🗑️ 删除配置并重新创建",
                    "↩️ 返回主菜单",
                ]));

        switch (editChoice)
        {
            case "🔗 修改服务器地址":
                currentConfig.ServerUrl = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]请输入新的服务器地址[/]:")
                        .PromptStyle("cyan")
                        .DefaultValue(currentConfig.ServerUrl ?? string.Empty)
                        .ValidationErrorMessage("[red]请输入有效的 URL[/]")
                        .Validate(url =>
                        {
                            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                return ValidationResult.Error("[red]服务器地址必须以 http:// 或 https:// 开头[/]");
                            }

                            return ValidationResult.Success();
                        }));
                currentConfig.Save();
                AnsiConsole.MarkupLine("[green]✓ 服务器地址已更新！[/]");
                break;

            case "🔄 修改服务器类型":
                var serverTypeChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]请选择服务器类型[/]")
                        .HighlightStyle(Style.Parse("cyan"))
                        .AddChoices(
                        [
                            "📱 手机端开源阅读 (Legado)",
                            "🖥️ hectorqin/reader 服务器",
                        ]));
                currentConfig.ServerType = serverTypeChoice.Contains("hectorqin", StringComparison.OrdinalIgnoreCase)
                    ? ServerType.HectorqinReader
                    : ServerType.Legado;
                currentConfig.Save();
                AnsiConsole.MarkupLine("[green]✓ 服务器类型已更新！[/]");
                break;

            case "🔑 修改访问令牌":
                var token = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]请输入新的访问令牌[/] [grey](留空则清除)[/]:")
                        .PromptStyle("cyan")
                        .DefaultValue(currentConfig.AccessToken ?? string.Empty)
                        .AllowEmpty());
                currentConfig.AccessToken = string.IsNullOrWhiteSpace(token) ? null : token;
                currentConfig.Save();
                AnsiConsole.MarkupLine("[green]✓ 访问令牌已更新！[/]");
                break;

            case "📁 修改输出目录":
                currentConfig.OutputDirectory = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]请输入新的输出目录[/]:")
                        .PromptStyle("cyan")
                        .DefaultValue(currentConfig.OutputDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                        .ValidationErrorMessage("[red]请输入有效的目录路径[/]")
                        .Validate(path =>
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                return ValidationResult.Success();
                            }
                            catch
                            {
                                return ValidationResult.Error("[red]无法创建或访问该目录[/]");
                            }
                        }));
                currentConfig.Save();
                AnsiConsole.MarkupLine("[green]✓ 输出目录已更新！[/]");
                break;

            case "⚡ 修改并发数":
                currentConfig.MaxConcurrentDownloads = AnsiConsole.Prompt(
                    new TextPrompt<int>("[green]最大并发下载数[/] [grey](1-20)[/]:")
                        .PromptStyle("cyan")
                        .DefaultValue(currentConfig.MaxConcurrentDownloads)
                        .ValidationErrorMessage("[red]请输入 1-20 之间的数字[/]")
                        .Validate(n => n is >= 1 and <= 20
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]请输入 1-20 之间的数字[/]")));
                currentConfig.Save();
                AnsiConsole.MarkupLine("[green]✓ 并发数已更新！[/]");
                break;

            case "🗑️ 删除配置并重新创建":
                if (AnsiConsole.Confirm("[yellow]确定要删除当前配置吗？[/]", defaultValue: false))
                {
                    AppConfig.Delete();
                    AnsiConsole.MarkupLine("[yellow]配置已删除，将重新创建...[/]");
                    return CreateConfig();
                }

                break;

            case "↩️ 返回主菜单":
                break;
        }

        AnsiConsole.WriteLine();
        return currentConfig;
    }

    /// <summary>
    /// 显示当前配置.
    /// </summary>
    public static void ShowCurrentConfig(AppConfig config)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[cyan]配置项[/]"))
            .AddColumn(new TableColumn("[white]值[/]"));

        table.AddRow("[cyan]服务器地址[/]", Markup.Escape(config.ServerUrl ?? "(未设置)"));
        table.AddRow("[cyan]服务器类型[/]", config.ServerType == ServerType.Legado ? "手机端开源阅读" : "hectorqin/reader");
        table.AddRow("[cyan]访问令牌[/]", string.IsNullOrEmpty(config.AccessToken) ? "[grey](未设置)[/]" : "[green]已设置[/]");
        table.AddRow("[cyan]输出目录[/]", Markup.Escape(config.OutputDirectory ?? "(默认桌面)"));
        table.AddRow("[cyan]并发数[/]", config.MaxConcurrentDownloads.ToString());

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 显示书架.
    /// </summary>
    public static Book? ShowBookshelf(IReadOnlyList<Book> books, string? searchKeyword = null)
    {
        AnsiConsole.WriteLine();

        if (books.Count == 0)
        {
            if (string.IsNullOrEmpty(searchKeyword))
            {
                AnsiConsole.MarkupLine("[yellow]书架为空，请先在开源阅读中添加书籍。[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]未找到包含 \"{Markup.Escape(searchKeyword)}\" 的书籍。[/]");
            }

            AnsiConsole.WriteLine();
            return null;
        }

        var title = string.IsNullOrEmpty(searchKeyword)
            ? $"[blue]书架[/] [grey]({books.Count} 本书)[/]"
            : $"[blue]搜索结果[/] [grey](\"{Markup.Escape(searchKeyword)}\" - {books.Count} 本书)[/]";

        var rule = new Rule(title);
        rule.LeftJustified();
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("[yellow]序号[/]").Centered())
            .AddColumn(new TableColumn("[cyan]书名[/]"))
            .AddColumn(new TableColumn("[green]作者[/]"))
            .AddColumn(new TableColumn("[blue]分类[/]"))
            .AddColumn(new TableColumn("[magenta]来源[/]"))
            .AddColumn(new TableColumn("[grey]最新章节[/]"));

        for (var i = 0; i < books.Count; i++)
        {
            var book = books[i];
            var latestChapter = TruncateText(book.LatestChapterTitle ?? "未知", 20);
            var category = TruncateText(book.Kind ?? "未知", 10);
            var source = TruncateText(book.OriginName ?? "未知", 15);

            table.AddRow(
                $"[yellow]{i + 1}[/]",
                Markup.Escape(TruncateText(book.Name, 25)),
                Markup.Escape(TruncateText(book.Author ?? "未知", 12)),
                Markup.Escape(category),
                Markup.Escape(source),
                Markup.Escape(latestChapter));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var selection = AnsiConsole.Prompt(
            new TextPrompt<int>("[green]请输入书籍序号查看详情[/] [grey](输入 0 返回)[/]:")
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
    public static void ShowBookDetail(Book book, IReadOnlyList<Chapter> chapters)
    {
        AnsiConsole.WriteLine();

        var contentChapters = chapters.Where(c => !c.IsVolume).ToList();
        var volumeCount = chapters.Count(c => c.IsVolume);

        var panel = new Panel(
            new Markup($"""
                [cyan]书名:[/] {Markup.Escape(book.Name)}
                [cyan]作者:[/] {Markup.Escape(book.Author ?? "未知")}
                [cyan]分类:[/] {Markup.Escape(book.Kind ?? "未知")}
                [cyan]来源:[/] {Markup.Escape(book.OriginName ?? "未知")}
                [cyan]章节:[/] {contentChapters.Count} 章{(volumeCount > 0 ? $" (含 {volumeCount} 个卷标题)" : string.Empty)}
                [cyan]最新:[/] {Markup.Escape(book.LatestChapterTitle ?? "未知")}
                """))
        {
            Header = new PanelHeader("[blue] 书籍信息 [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue),
            Padding = new Padding(2, 1, 2, 1),
        };

        AnsiConsole.Write(panel);

        // 显示简介
        if (!string.IsNullOrEmpty(book.Intro))
        {
            AnsiConsole.WriteLine();
            var intro = TruncateText(book.Intro.Trim(), 500);
            AnsiConsole.MarkupLine("[grey]简介:[/]");
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(intro)}[/]");
        }

        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 选择章节范围.
    /// </summary>
    public static (int? StartIndex, int? EndIndex) SelectChapterRange(IReadOnlyList<Chapter> chapters)
    {
        var contentChapters = chapters.Where(c => !c.IsVolume).ToList();

        if (contentChapters.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]该书籍没有可下载的章节。[/]");
            return (null, null);
        }

        var minIndex = contentChapters.Min(c => c.Index);
        var maxIndex = contentChapters.Max(c => c.Index);

        AnsiConsole.MarkupLine($"[grey]可下载范围: 第 {minIndex + 1} 章 - 第 {maxIndex + 1} 章 (共 {contentChapters.Count} 章内容)[/]");
        AnsiConsole.WriteLine();

        var downloadChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]请选择下载方式[/]")
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices(["下载全部章节", "选择下载范围"]));

        if (downloadChoice == "下载全部章节")
        {
            return (null, null);
        }

        // 选择范围
        var startChapter = AnsiConsole.Prompt(
            new TextPrompt<int>($"[green]起始章节[/] [grey](第 {minIndex + 1}-{maxIndex + 1} 章)[/]:")
                .PromptStyle("cyan")
                .DefaultValue(minIndex + 1)
                .ValidationErrorMessage("[red]请输入有效的章节号[/]")
                .Validate(n => n >= minIndex + 1 && n <= maxIndex + 1
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]请输入 {minIndex + 1}-{maxIndex + 1} 之间的数字[/]")));

        var endChapter = AnsiConsole.Prompt(
            new TextPrompt<int>($"[green]结束章节[/] [grey](第 {startChapter}-{maxIndex + 1} 章)[/]:")
                .PromptStyle("cyan")
                .DefaultValue(maxIndex + 1)
                .ValidationErrorMessage("[red]请输入有效的章节号[/]")
                .Validate(n => n >= startChapter && n <= maxIndex + 1
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]请输入 {startChapter}-{maxIndex + 1} 之间的数字[/]")));

        return (startChapter - 1, endChapter - 1);
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

        table.AddRow("总章节数", $"[white]{stats.TotalChapters}[/] 章");
        table.AddRow("新下载章节", $"[green]{stats.NewlyDownloaded}[/] 章");
        table.AddRow("复用章节", $"[blue]{stats.Reused}[/] 章");
        table.AddRow("缓存恢复", $"[cyan]{stats.RestoredFromCache}[/] 章");
        table.AddRow("失败章节", stats.Failed > 0 ? $"[red]{stats.Failed}[/] 章" : "[grey]0 章[/]");
        table.AddRow("下载图片", $"[magenta]{stats.ImagesDownloaded}[/] 张");
        table.AddRow("卷标题", $"[grey]{stats.VolumeChapters}[/] 个");
        table.AddRow("总耗时", $"[yellow]{stats.Duration:mm\\:ss\\.fff}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[green]✓ EPUB 已保存至:[/] [link]{Markup.Escape(epubPath)}[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 显示错误信息.
    /// </summary>
    public static void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]✗ 错误: {Markup.Escape(message)}[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// 显示成功信息.
    /// </summary>
    public static void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓ {Markup.Escape(message)}[/]");
    }

    /// <summary>
    /// 显示警告信息.
    /// </summary>
    public static void ShowWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠ {Markup.Escape(message)}[/]");
    }

    /// <summary>
    /// 截断文本.
    /// </summary>
    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // 移除换行符
        text = text.Replace("\n", " ", StringComparison.Ordinal).Replace("\r", string.Empty, StringComparison.Ordinal);

        if (text.Length <= maxLength)
        {
            return text;
        }

        return text[..(maxLength - 3)] + "...";
    }
}
