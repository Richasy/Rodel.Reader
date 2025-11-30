// Copyright (c) Reader Copilot. All rights reserved.

using System.Text;
using System.Text.RegularExpressions;

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 基于正则表达式的文本分割器实现.
/// </summary>
public sealed partial class RegexTextSplitter : ITextSplitter
{
    /// <summary>
    /// 默认章节匹配正则表达式.
    /// </summary>
    public const string DefaultChapterPattern = @"第[零一二三四五六七八九十百千万\d]+[章节回卷集部篇]";

    /// <summary>
    /// 默认额外章节关键词.
    /// </summary>
    public static readonly string[] DefaultExtraKeywords = ["序", "序章", "序言", "前言", "引子", "楔子", "后记", "後記", "尾声", "尾聲", "附录", "附錄", "番外", "外传", "外傳"];

    /// <inheritdoc/>
    public IReadOnlyList<ChapterInfo> Split(ReadOnlySpan<char> text, SplitOptions? options = null)
    {
        return SplitInternal(text.ToString(), options);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChapterInfo>> SplitFromFileAsync(
        string filePath,
        SplitOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("指定的文件不存在。", filePath);
        }

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return await SplitFromStreamAsync(stream, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChapterInfo>> SplitFromStreamAsync(
        Stream stream,
        SplitOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var encoding = EncodingHelper.DetectEncoding(stream, Encoding.UTF8);

        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        return SplitInternal(text, options);
    }

    private static List<ChapterInfo> SplitInternal(string text, SplitOptions? options)
    {
        options ??= new SplitOptions();
        var pattern = options.ChapterPattern ?? DefaultChapterPattern;
        var extraKeywords = options.ExtraChapterKeywords ?? DefaultExtraKeywords;
        var maxTitleLength = options.MaxTitleLength > 0 ? options.MaxTitleLength : 50;
        var defaultTitle = options.DefaultFirstChapterTitle ?? "前言";

        var regex = new Regex(pattern, RegexOptions.Compiled);
        var chapters = new List<ChapterInfo>();
        var lines = text.Split('\n');

        var currentTitle = string.Empty;
        var contentBuilder = StringBuilderPool.Rent();
        var chapterIndex = 0;

        try
        {
            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                var trimmedLine = options.TrimLines ? line.Trim() : line;

                // 跳过空行（如果配置了移除空行）
                if (options.RemoveEmptyLines && string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }

                // 检查是否是章节标题
                var isChapterTitle = false;
                var newTitle = string.Empty;

                // 检查额外关键词
                if (trimmedLine.Length <= maxTitleLength)
                {
                    foreach (var keyword in extraKeywords)
                    {
                        if (trimmedLine.StartsWith(keyword, StringComparison.Ordinal))
                        {
                            isChapterTitle = true;
                            newTitle = trimmedLine;
                            break;
                        }
                    }
                }

                // 检查正则匹配
                if (!isChapterTitle && regex.IsMatch(trimmedLine))
                {
                    var match = regex.Match(trimmedLine);
                    if (match.Value.Length <= maxTitleLength)
                    {
                        isChapterTitle = true;
                        newTitle = trimmedLine.Length <= maxTitleLength ? trimmedLine : match.Value;
                    }
                }

                if (isChapterTitle)
                {
                    // 保存之前的章节
                    if (contentBuilder.Length > 0 || !string.IsNullOrEmpty(currentTitle))
                    {
                        var title = string.IsNullOrEmpty(currentTitle) ? defaultTitle : currentTitle;
                        chapters.Add(new ChapterInfo
                        {
                            Index = chapterIndex++,
                            Title = title.Trim(),
                            Content = contentBuilder.ToString().Trim(),
                            IsHtml = false,
                        });
                        contentBuilder.Clear();
                    }

                    currentTitle = newTitle;
                }
                else
                {
                    // 添加内容
                    if (!string.IsNullOrWhiteSpace(trimmedLine) || !options.RemoveEmptyLines)
                    {
                        contentBuilder.AppendLine(trimmedLine);
                    }
                }
            }

            // 处理最后一个章节
            if (contentBuilder.Length > 0 || !string.IsNullOrEmpty(currentTitle))
            {
                var title = string.IsNullOrEmpty(currentTitle) ? defaultTitle : currentTitle;
                chapters.Add(new ChapterInfo
                {
                    Index = chapterIndex,
                    Title = title.Trim(),
                    Content = contentBuilder.ToString().Trim(),
                    IsHtml = false,
                });
            }
        }
        finally
        {
            StringBuilderPool.Return(contentBuilder);
        }

        return chapters;
    }
}
