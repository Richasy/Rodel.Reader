// Copyright (c) Reader Copilot. All rights reserved.

using Richasy.ReaderKernel.Models.Config;
using Richasy.ReaderKernel.Models.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace Richasy.ReaderKernel.Toolkits.WinUI;

/// <summary>
/// 内核文本文件工具箱.
/// </summary>
public sealed partial class KernelTxtToolkit : IKernelTxtToolkit
{
    private static Dictionary<string, string>? _chapterNames;
    private readonly IKernelFileToolkit _fileToolkit;
    private string[]? _files;
    private EpubGeneratorConfig? _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="KernelTxtToolkit"/> class.
    /// </summary>
    public KernelTxtToolkit(IKernelFileToolkit fileToolkit) => _fileToolkit = fileToolkit;

    /// <inheritdoc/>
    public void ClearCache()
    {
        if (Directory.Exists(TXT_FOLDER_PATH))
        {
            Directory.Delete(TXT_FOLDER_PATH, true);
        }

        if (Directory.Exists(TEMP_FOLDER_PATH))
        {
            Directory.Delete(TEMP_FOLDER_PATH, true);
        }
    }

    /// <inheritdoc/>
    public void ClearGeneratedFiles()
    {
        if (Directory.Exists(GENERATE_FOLDER_PATH))
        {
            Directory.Delete(GENERATE_FOLDER_PATH, true);
        }
    }

    /// <inheritdoc/>
    public async Task CreateAsync(EpubGeneratorConfig config, CancellationToken cancellationToken = default)
    {
        _config = config;
        GetFiles();
        CreateTempFolder();
        await CopyStyleSheetAsync().ConfigureAwait(false);
        await CopyContainerFileAsync().ConfigureAwait(false);
        await Task.WhenAll(
            Task.Run(CreateMimeTypeFileAsync, cancellationToken),
            Task.Run(CreateOpfFileAsync, cancellationToken),
            Task.Run(CreateNcxFileAsync, cancellationToken)).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        CreateZipFile();
        ClearCache();
    }

    /// <inheritdoc/>
    public async Task<EpubGeneratorConfig> CreateGeneratorConfigFromTxtFileAsync(string filePath, string? splitRegex = null, CancellationToken cancellationToken = default)
    {
        ClearCache();
        splitRegex ??= CHAPTER_DIVISION_REGEX;

        var file = new FileInfo(filePath);
        if (file.Exists)
        {
            if (!Directory.Exists(TXT_FOLDER_PATH))
            {
                _ = Directory.CreateDirectory(TXT_FOLDER_PATH);
            }

            _chapterNames?.Clear();
            var title = Path.GetFileNameWithoutExtension(filePath);
            var encoding = EncodingHelper.DetectFileEncoding(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8);
            var lines = await File.ReadAllLinesAsync(filePath, encoding, cancellationToken).ConfigureAwait(false);
            var filterLines = new List<string>();
            var lastTitle = title;
            var chapterTitle = string.Empty;
            var chapterIndex = 0;

            await GenerateTitlePageFromTxtAsync(title).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var regex = new Regex(splitRegex, RegexOptions.Compiled);
            foreach (var line in lines)
            {
                var l = line.Trim();
                if (string.IsNullOrEmpty(l))
                {
                    filterLines.Add(l);
                    continue;
                }

                if (IsExtra(l))
                {
                    chapterTitle = NormalizeTitle(l);
                }
                else if (IsTitle(l, regex))
                {
                    chapterTitle = NormalizeTitle(regex.Match(l).Value).Trim();
                }

                if (!string.IsNullOrEmpty(chapterTitle))
                {
                    if (!string.IsNullOrEmpty(lastTitle))
                    {
                        var t = lastTitle;
                        var arr = filterLines.ToArray();
                        await GenerateHtmlFromTxtAsync(t, arr, chapterIndex).ConfigureAwait(false);
                        chapterIndex++;
                        filterLines.Clear();
                    }

                    lastTitle = chapterTitle;
                    chapterTitle = string.Empty;
                }
                else
                {
                    filterLines.Add(l);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (filterLines.Count > 0)
            {
                if (string.IsNullOrEmpty(lastTitle))
                {
                    lastTitle = "Untitled";
                }

                await GenerateHtmlFromTxtAsync(lastTitle, filterLines.ToArray(), chapterIndex).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(GENERATE_FOLDER_PATH))
            {
                _ = Directory.CreateDirectory(GENERATE_FOLDER_PATH);
            }

            return new EpubGeneratorConfig()
            {
                Name = title,
                Author = string.Empty,
                SourceFolderPath = TXT_FOLDER_PATH,
                Language = "zh",
                OutputFileName = $"{title}.epub",
                OutputFolderPath = GENERATE_FOLDER_PATH,
                TitlePagePath = $"{TXT_FOLDER_PATH}titlepage.html",
                ChapterNames = _chapterNames ?? [],
            };
        }
        else
        {
            throw new ArgumentException($"指定文件 {filePath} 不存在.");
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TxtSplitChapter>> GenerateChaptersAsync(string filePath, string? splitRegex = null)
    {
        splitRegex ??= CHAPTER_DIVISION_REGEX;

        var file = new FileInfo(filePath);
        if (file.Exists)
        {
            var title = Path.GetFileNameWithoutExtension(filePath);
            var encoding = EncodingHelper.DetectFileEncoding(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8);
            var lines = await File.ReadAllLinesAsync(filePath, encoding).ConfigureAwait(false);
            var filterLines = new List<string>();
            var lastTitle = title;
            var chapterTitle = string.Empty;
            var chapterIndex = 0;
            var result = new List<TxtSplitChapter>();
            var regex = new Regex(splitRegex, RegexOptions.Compiled);

            foreach (var line in lines)
            {
                var l = line.Trim();
                if (string.IsNullOrEmpty(l))
                {
                    filterLines.Add(l);
                    continue;
                }

                if (IsExtra(l))
                {
                    chapterTitle = NormalizeTitle(l);
                }
                else if (IsTitle(l, regex))
                {
                    chapterTitle = NormalizeTitle(regex.Match(l).Value).Trim();
                }

                if (!string.IsNullOrEmpty(chapterTitle))
                {
                    if (!string.IsNullOrEmpty(lastTitle))
                    {
                        var t = lastTitle;
                        var arr = filterLines.ToArray();
                        var index = chapterIndex;
                        var content = string.Join(Environment.NewLine, arr);
                        result.Add(new TxtSplitChapter { Content = content, Index = index, Title = t, WordCount = content.Length });
                        chapterIndex++;
                        filterLines.Clear();
                    }

                    lastTitle = chapterTitle;
                    chapterTitle = string.Empty;
                }
                else
                {
                    filterLines.Add(l);
                }
            }

            if (filterLines.Count > 0)
            {
                if (string.IsNullOrEmpty(lastTitle))
                {
                    lastTitle = "Untitled";
                }

                var content = string.Join(Environment.NewLine, filterLines);
                result.Add(new TxtSplitChapter { Content = content, Index = chapterIndex, Title = lastTitle, WordCount = content.Length });
            }

            return result;
        }
        else
        {
            throw new ArgumentException($"指定文件 {filePath} 不存在.");
        }
    }

    private static ActionBlock<Task> GetActionBlock(CancellationToken cancellationToken = default)
    {
        var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 12, BoundedCapacity = DataflowBlockOptions.Unbounded, CancellationToken = cancellationToken };

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
        return new ActionBlock<Task>(t => t);
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
    }

    private static Task WriteFileContentWithLongPathAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        path = $"\\\\?\\{path}";
        return File.WriteAllTextAsync(path, content, cancellationToken);
    }

    private static Task<string> ReadFileContentWithLongPathAsync(string path, CancellationToken cancellationToken = default)
    {
        path = $"\\\\?\\{path}";
        return File.ReadAllTextAsync(path, cancellationToken);
    }
}
