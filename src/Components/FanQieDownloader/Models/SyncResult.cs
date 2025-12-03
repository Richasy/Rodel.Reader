// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.FanQie.Models;

/// <summary>
/// 同步结果.
/// </summary>
public sealed record SyncResult
{
    /// <summary>
    /// 是否成功.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// 生成的 EPUB 路径.
    /// </summary>
    public string? EpubPath { get; init; }

    /// <summary>
    /// 书籍信息.
    /// </summary>
    public FanQieBookInfo? BookInfo { get; init; }

    /// <summary>
    /// 统计信息.
    /// </summary>
    public SyncStatistics? Statistics { get; init; }

    /// <summary>
    /// 错误信息.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 是否被取消.
    /// </summary>
    public bool IsCancelled { get; init; }

    /// <summary>
    /// 创建成功结果.
    /// </summary>
    public static SyncResult CreateSuccess(string epubPath, FanQieBookInfo bookInfo, SyncStatistics statistics)
        => new()
        {
            Success = true,
            EpubPath = epubPath,
            BookInfo = bookInfo,
            Statistics = statistics,
        };

    /// <summary>
    /// 创建失败结果.
    /// </summary>
    public static SyncResult CreateFailure(string errorMessage, FanQieBookInfo? bookInfo = null)
        => new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            BookInfo = bookInfo,
        };

    /// <summary>
    /// 创建取消结果.
    /// </summary>
    public static SyncResult CreateCancelled(FanQieBookInfo? bookInfo = null)
        => new()
        {
            Success = false,
            IsCancelled = true,
            BookInfo = bookInfo,
        };
}
