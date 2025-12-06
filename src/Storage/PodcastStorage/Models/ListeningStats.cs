// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 收听统计.
/// </summary>
public sealed class ListeningStats
{
    /// <summary>
    /// 播客 ID（可选，全局统计时为空）.
    /// </summary>
    public string? PodcastId { get; set; }

    /// <summary>
    /// 单集 ID（可选，播客或全局统计时为空）.
    /// </summary>
    public string? EpisodeId { get; set; }

    /// <summary>
    /// 总收听时长.
    /// </summary>
    public TimeSpan TotalListeningTime { get; set; }

    /// <summary>
    /// 总收听次数.
    /// </summary>
    public int TotalSessionCount { get; set; }

    /// <summary>
    /// 平均每次收听时长.
    /// </summary>
    public TimeSpan AverageSessionDuration { get; set; }

    /// <summary>
    /// 收听天数.
    /// </summary>
    public int ListeningDays { get; set; }

    /// <summary>
    /// 首次收听日期.
    /// </summary>
    public DateOnly? FirstListenDate { get; set; }

    /// <summary>
    /// 最近收听日期.
    /// </summary>
    public DateOnly? LastListenDate { get; set; }

    /// <summary>
    /// 收听的单集数量.
    /// </summary>
    public int EpisodeCount { get; set; }

    /// <summary>
    /// 收听的播客数量（仅用于全局统计）.
    /// </summary>
    public int PodcastCount { get; set; }
}
