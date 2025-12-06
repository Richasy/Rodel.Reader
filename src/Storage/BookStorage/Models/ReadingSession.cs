// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 阅读时段记录.
/// </summary>
public sealed class ReadingSession
{
    /// <summary>
    /// 标识符.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间 (ISO 8601).
    /// </summary>
    public string StartedAt { get; set; } = string.Empty;

    /// <summary>
    /// 结束时间 (ISO 8601).
    /// </summary>
    public string EndedAt { get; set; } = string.Empty;

    /// <summary>
    /// 阅读时长（秒）.
    /// </summary>
    public int DurationSeconds { get; set; }

    /// <summary>
    /// 开始时的进度.
    /// </summary>
    public double? StartProgress { get; set; }

    /// <summary>
    /// 结束时的进度.
    /// </summary>
    public double? EndProgress { get; set; }

    /// <summary>
    /// 阅读页数.
    /// </summary>
    public int? PagesRead { get; set; }

    /// <summary>
    /// 设备标识.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 设备名称.
    /// </summary>
    public string? DeviceName { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ReadingSession session && Id == session.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);
}
