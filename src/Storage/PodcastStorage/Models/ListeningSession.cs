// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast;

/// <summary>
/// 收听时段记录.
/// </summary>
public sealed class ListeningSession
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 单集 ID.
    /// </summary>
    public string EpisodeId { get; set; } = string.Empty;

    /// <summary>
    /// 播客 ID.
    /// </summary>
    public string PodcastId { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    public DateTimeOffset EndedAt { get; set; }

    /// <summary>
    /// 收听时长（秒）.
    /// </summary>
    public int DurationSeconds { get; set; }

    /// <summary>
    /// 开始位置（秒）.
    /// </summary>
    public int? StartPosition { get; set; }

    /// <summary>
    /// 结束位置（秒）.
    /// </summary>
    public int? EndPosition { get; set; }

    /// <summary>
    /// 设备 ID.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 设备名称.
    /// </summary>
    public string? DeviceName { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ListeningSession session && Id == session.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
