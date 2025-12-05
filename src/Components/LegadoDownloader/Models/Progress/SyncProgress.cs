// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Components.Legado.Models;

/// <summary>
/// 同步进度.
/// </summary>
public sealed record SyncProgress
{
    /// <summary>
    /// 当前阶段.
    /// </summary>
    public required SyncPhase Phase { get; init; }

    /// <summary>
    /// 总进度（0-100）.
    /// </summary>
    public double TotalProgress { get; init; }

    /// <summary>
    /// 当前阶段进度（0-100）.
    /// </summary>
    public double PhaseProgress { get; init; }

    /// <summary>
    /// 进度消息.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// 下载详情（仅在下载阶段有效）.
    /// </summary>
    public DownloadProgressDetail? DownloadDetail { get; init; }

    /// <summary>
    /// 生成详情（仅在生成阶段有效）.
    /// </summary>
    public GenerateProgressDetail? GenerateDetail { get; init; }

    /// <summary>
    /// 创建分析阶段进度.
    /// </summary>
    public static SyncProgress Analyzing(string? message = null)
        => new() { Phase = SyncPhase.Analyzing, TotalProgress = 2, PhaseProgress = 50, Message = message ?? "正在分析现有 EPUB..." };

    /// <summary>
    /// 创建获取目录阶段进度.
    /// </summary>
    public static SyncProgress FetchingToc(string? message = null)
        => new() { Phase = SyncPhase.FetchingToc, TotalProgress = 5, PhaseProgress = 50, Message = message ?? "正在获取在线目录..." };

    /// <summary>
    /// 创建检查缓存阶段进度.
    /// </summary>
    public static SyncProgress CheckingCache(string? message = null)
        => new() { Phase = SyncPhase.CheckingCache, TotalProgress = 8, PhaseProgress = 50, Message = message ?? "正在检查缓存..." };

    /// <summary>
    /// 创建下载章节阶段进度.
    /// </summary>
    public static SyncProgress DownloadingChapters(DownloadProgressDetail detail)
        => new()
        {
            Phase = SyncPhase.DownloadingChapters,
            TotalProgress = 10 + (detail.Percentage * 0.5), // 10% - 60%
            PhaseProgress = detail.Percentage,
            Message = $"正在下载: {detail.CurrentChapter}",
            DownloadDetail = detail,
        };

    /// <summary>
    /// 创建下载图片阶段进度.
    /// </summary>
    public static SyncProgress DownloadingImages(double percentage, string? message = null)
        => new()
        {
            Phase = SyncPhase.DownloadingImages,
            TotalProgress = 60 + (percentage * 0.15), // 60% - 75%
            PhaseProgress = percentage,
            Message = message ?? "正在下载图片...",
        };

    /// <summary>
    /// 创建生成 EPUB 阶段进度.
    /// </summary>
    public static SyncProgress GeneratingEpub(GenerateProgressDetail detail)
        => new()
        {
            Phase = SyncPhase.GeneratingEpub,
            TotalProgress = 75 + (detail.Percentage * 0.2), // 75% - 95%
            PhaseProgress = detail.Percentage,
            Message = detail.Step ?? "正在生成 EPUB...",
            GenerateDetail = detail,
        };

    /// <summary>
    /// 创建清理缓存阶段进度.
    /// </summary>
    public static SyncProgress CleaningUp(string? message = null)
        => new() { Phase = SyncPhase.CleaningUp, TotalProgress = 98, PhaseProgress = 50, Message = message ?? "正在清理缓存..." };

    /// <summary>
    /// 创建完成阶段进度.
    /// </summary>
    public static SyncProgress Completed(string? message = null)
        => new() { Phase = SyncPhase.Completed, TotalProgress = 100, PhaseProgress = 100, Message = message ?? "同步完成" };

    /// <summary>
    /// 创建失败阶段进度.
    /// </summary>
    public static SyncProgress Failed(string? message = null)
        => new() { Phase = SyncPhase.Failed, TotalProgress = 0, PhaseProgress = 0, Message = message ?? "同步失败" };
}
