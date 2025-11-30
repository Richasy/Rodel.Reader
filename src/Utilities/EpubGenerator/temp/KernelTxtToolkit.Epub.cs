// Copyright (c) Reader Copilot. All rights reserved.

using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Richasy.ReaderKernel.Toolkits.WinUI;

/// <summary>
/// 内核文本文件工具箱.
/// </summary>
public sealed partial class KernelTxtToolkit
{
    private static void CreateTempFolder()
    {
        if (!Directory.Exists(TEMP_FOLDER_PATH))
        {
            _ = Directory.CreateDirectory(TEMP_FOLDER_PATH);
        }
    }

    private async Task CopyStyleSheetAsync()
    {
        if (!Directory.Exists(TEMP_CSS_FOLDER_PATH))
        {
            _ = Directory.CreateDirectory(TEMP_CSS_FOLDER_PATH);
        }

        await _fileToolkit.CopyAsync(ASSETS_CSS_FILE_PATH, TEMP_CSS_FILE_PATH).ConfigureAwait(false);
    }

    private async Task CopyContainerFileAsync()
    {
        if (!Directory.Exists(TEMP_META_FOLDER_PATH))
        {
            _ = Directory.CreateDirectory(TEMP_META_FOLDER_PATH);
        }

        await _fileToolkit.CopyAsync(ASSETS_CONTAINER_FILE_PATH, TEMP_CONTAINER_FILE_PATH).ConfigureAwait(false);
    }

    private Task CreateMimeTypeFileAsync()
        => WriteFileContentWithLongPathAsync(TEMP_MIME_TYPE_FILE_PATH, MIME_TYPE);

    private void GetFiles()
    {
        var files = Directory.GetFiles(_config!.SourceFolderPath!, "*.html");
        // 按照 _config.ChapterNames 的顺序排序.
        _files = [.. files.OrderBy(p =>
        {
            var fileName = Path.GetFileNameWithoutExtension(p);
            return _config.ChapterNames!.Keys.ToList().IndexOf(fileName);
        })];
    }

    private async Task CreateOpfFileAsync()
    {
        var opfTempleate = await ReadFileContentWithLongPathAsync(ASSETS_OPF_FILE_PATH).ConfigureAwait(false);
        opfTempleate = opfTempleate.Replace("{{title}}", WebUtility.HtmlEncode(_config!.Name), StringComparison.InvariantCultureIgnoreCase)
                                   .Replace("{{language}}", _config.Language, StringComparison.InvariantCultureIgnoreCase)
                                   .Replace("{{author}}", WebUtility.HtmlEncode(_config.Author), StringComparison.InvariantCultureIgnoreCase)
                                   .Replace("{{date}}", DateTimeOffset.Now.ToString(CultureInfo.CurrentCulture), StringComparison.InvariantCultureIgnoreCase)
                                   .Replace("{{titlePage}}", $"Pages/{Path.GetFileName(_config.TitlePagePath)}", StringComparison.InvariantCultureIgnoreCase)
                                   .Replace("{{others}}", _config.Others ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);

        var manifestBuilder = new StringBuilder();
        var spineBuilder = new StringBuilder();
        foreach (var file in _files!.Where(p => !p.Equals(_config.TitlePagePath, StringComparison.OrdinalIgnoreCase)))
        {
            var pathName = Path.GetFileNameWithoutExtension(file);
            if (!_config.ChapterNames!.ContainsKey(pathName))
            {
                continue;
            }

            var title = _config.ChapterNames![pathName];
            var customFileName = pathName;
            if (title.Contains("{!}", StringComparison.OrdinalIgnoreCase))
            {
                var parts = title.Split("{!}");
                title = parts[0];
                customFileName = parts[1];
            }

            _ = manifestBuilder.AppendLine(new string(' ', 12) + $"<item id=\"ch{customFileName}\" href=\"Pages/{pathName}.html\" media-type=\"application/xhtml+xml\"/>");
            _ = spineBuilder.AppendLine(new string(' ', 12) + $"<itemref idref=\"ch{customFileName}\"/>");
        }

        if (string.IsNullOrEmpty(spineBuilder.ToString().Trim()))
        {
            throw new InvalidOperationException("No valid chapters found to include in the EPUB spine.");
        }

        opfTempleate = opfTempleate.Replace("{{manifest}}", manifestBuilder.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                   .Replace("{{spine}}", spineBuilder.ToString(), StringComparison.InvariantCultureIgnoreCase);
        _ = manifestBuilder.Clear();
        _ = spineBuilder.Clear();

        await WriteFileContentWithLongPathAsync(TEMP_CONTENT_OPF_PATH, opfTempleate).ConfigureAwait(false);
    }

    private async Task CreateNcxFileAsync()
    {
        var ncxTemplate = await ReadFileContentWithLongPathAsync(ASSETS_NCX_FILE_PATH).ConfigureAwait(false);
        ncxTemplate = ncxTemplate.Replace("{{title}}", WebUtility.HtmlEncode(_config!.Name), StringComparison.InvariantCultureIgnoreCase)
                                 .Replace("{{author}}", WebUtility.HtmlEncode(_config!.Author), StringComparison.InvariantCultureIgnoreCase);

        var mapBuilder = new StringBuilder();
        var tocFiles = _files!.Where(p => !p.Equals(_config.TitlePagePath, StringComparison.OrdinalIgnoreCase));
        var index = 1;
        foreach (var file in tocFiles)
        {
            var pathName = Path.GetFileNameWithoutExtension(file);
            if (!_config.ChapterNames!.ContainsKey(pathName))
            {
                continue;
            }

            var tempTitle = _config.ChapterNames![pathName];
            var customPath = pathName;
            if (tempTitle.Contains("{!}", StringComparison.OrdinalIgnoreCase))
            {
                var sp = tempTitle.Split("{!}");
                tempTitle = sp[0];
                customPath = sp[1];
            }

            var fileName = WebUtility.HtmlEncode(tempTitle);

            try
            {
                _ = mapBuilder.AppendLine(new string(' ', 8) + $"<navPoint class=\"chapter\" id=\"ch{customPath}\" playOrder=\"{index}\">");
                _ = mapBuilder.AppendLine(new string(' ', 12) + "<navLabel>");
                _ = mapBuilder.AppendLine(new string(' ', 16) + $"<text>{fileName}</text>");
                _ = mapBuilder.AppendLine(new string(' ', 12) + "</navLabel>");
                _ = mapBuilder.AppendLine(new string(' ', 12) + $"<content src=\"Pages/{pathName}.html\" />");
                _ = mapBuilder.AppendLine(new string(' ', 8) + "</navPoint>");
                index++;
            }
            catch (Exception)
            {
                continue;
            }
        }

        if (string.IsNullOrWhiteSpace(mapBuilder.ToString()))
        {
            throw new InvalidOperationException("No valid chapters found to include in the EPUB TOC.");
        }

        ncxTemplate = ncxTemplate.Replace("{{navMap}}", mapBuilder.ToString(), StringComparison.InvariantCultureIgnoreCase);
        _ = mapBuilder.Clear();

        await WriteFileContentWithLongPathAsync(TEMP_TOC_NCX_PATH, ncxTemplate).ConfigureAwait(false);
    }

    private void CreateZipFile()
    {
        var outputPath = Path.Combine(_config!.OutputFolderPath!, _config.OutputFileName!);
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        if (!File.Exists(TEMP_CONTENT_OPF_PATH) || !File.Exists(TEMP_TOC_NCX_PATH))
        {
            return;
        }

        using var archive = ZipFile.Open(outputPath, ZipArchiveMode.Create);
        archive.CreateEntryFromFile(TEMP_MIME_TYPE_FILE_PATH, Path.GetFileName(TEMP_MIME_TYPE_FILE_PATH));
        archive.CreateEntryFromFile(TEMP_CONTENT_OPF_PATH, "OEBPS/" + Path.GetFileName(TEMP_CONTENT_OPF_PATH));
        archive.CreateEntryFromFile(TEMP_CSS_FILE_PATH, "OEBPS/Style/" + Path.GetFileName(TEMP_CSS_FILE_PATH));
        archive.CreateEntryFromFile(TEMP_TOC_NCX_PATH, "OEBPS/" + Path.GetFileName(TEMP_TOC_NCX_PATH));
        archive.CreateEntryFromFile(_config.TitlePagePath!, "OEBPS/Pages/" + Path.GetFileName(_config.TitlePagePath));
        archive.CreateEntryFromFile(TEMP_CONTAINER_FILE_PATH, "META-INF/" + Path.GetFileName(TEMP_CONTAINER_FILE_PATH));
        if (File.Exists(Path.Combine(_config.SourceFolderPath!, "crn.json")))
        {
            archive.CreateEntryFromFile(Path.Combine(_config.SourceFolderPath!, "crn.json"), "OEBPS/Pages/crn.json");
        }

        foreach (var file in _files!.Where(p => Path.GetFileName(p) != Path.GetFileName(_config.TitlePagePath)))
        {
            archive.CreateEntryFromFile(file, "OEBPS/Pages/" + Path.GetFileName(file));
        }
    }
}
