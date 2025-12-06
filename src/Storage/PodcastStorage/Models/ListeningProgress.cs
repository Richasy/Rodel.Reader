// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 收听进度.
/// </summary>
public sealed class ListeningProgress
{
    /// <summary>
    /// 单集 ID（主键）.
    /// </summary>
    public string EpisodeId { get; set; } = string.Empty;

    /// <summary>
    /// 当前位置（秒）.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 总时长（秒）.
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 进度 (0.0 - 1.0).
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// 播放速率.
    /// </summary>
    public double? PlaybackRate { get; set; }

    /// <summary>
    /// 更新时间.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ListeningProgress progress && EpisodeId == progress.EpisodeId;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(EpisodeId);
}
