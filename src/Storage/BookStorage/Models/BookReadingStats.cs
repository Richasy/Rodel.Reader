// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书籍阅读统计（查询聚合结果，非存储实体）.
/// </summary>
public sealed class BookReadingStats
{
    /// <summary>
    /// 书籍 ID.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// 总阅读时长.
    /// </summary>
    public TimeSpan TotalReadingTime { get; set; }

    /// <summary>
    /// 总阅读次数.
    /// </summary>
    public int TotalSessionCount { get; set; }

    /// <summary>
    /// 平均每次阅读时长.
    /// </summary>
    public TimeSpan AverageSessionDuration { get; set; }

    /// <summary>
    /// 阅读天数.
    /// </summary>
    public int ReadingDays { get; set; }

    /// <summary>
    /// 首次阅读日期.
    /// </summary>
    public DateOnly? FirstReadDate { get; set; }

    /// <summary>
    /// 最近阅读日期.
    /// </summary>
    public DateOnly? LastReadDate { get; set; }

    /// <summary>
    /// 每小时阅读页数.
    /// </summary>
    public double? PagesPerHour { get; set; }

    /// <summary>
    /// 每分钟阅读字数.
    /// </summary>
    public double? WordsPerMinute { get; set; }
}
