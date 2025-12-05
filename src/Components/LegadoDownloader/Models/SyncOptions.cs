// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Models;

/// <summary>
/// 同步选项.
/// </summary>
public sealed class SyncOptions
{
    /// <summary>
    /// 临时缓存目录（必填）.
    /// </summary>
    public required string TempDirectory { get; init; }

    /// <summary>
    /// 输出目录（EPUB 将生成到此目录）.
    /// </summary>
    public required string OutputDirectory { get; init; }

    /// <summary>
    /// 现有 EPUB 路径（用于增量同步）.
    /// </summary>
    public string? ExistingEpubPath { get; init; }

    /// <summary>
    /// 是否强制重新下载所有章节.
    /// </summary>
    public bool ForceRedownload { get; init; }

    /// <summary>
    /// 是否重试失败章节.
    /// </summary>
    public bool RetryFailedChapters { get; init; } = true;

    /// <summary>
    /// 下载失败时是否继续.
    /// </summary>
    public bool ContinueOnError { get; init; } = true;

    /// <summary>
    /// EPUB 生成选项.
    /// </summary>
    public EpubOptions? EpubOptions { get; init; }

    /// <summary>
    /// 起始章节索引（从 0 开始，null 表示从第一章开始）.
    /// </summary>
    public int? StartChapterIndex { get; init; }

    /// <summary>
    /// 结束章节索引（包含，null 表示到最后一章）.
    /// </summary>
    public int? EndChapterIndex { get; init; }

    /// <summary>
    /// 最大并发下载数（范围 1-50，默认 3）.
    /// </summary>
    public int MaxConcurrentDownloads { get; init; } = 3;
}
