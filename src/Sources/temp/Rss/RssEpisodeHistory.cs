// Copyright (c) Reader Copilot. All rights reserved.

using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 播客历史.
/// </summary>
[SugarTable("ListenHistory")]
[Table("ListenHistory")]
public sealed class RssEpisodeHistory
{
    /// <summary>
    /// 媒体地址.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    [Key]
    public string Id { get; set; }

    /// <summary>
    /// 最近播放时间.
    /// </summary>
    public string LastListenTime { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RssEpisodeHistory history && Id == history.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// 设置最后播放时间.
    /// </summary>
    /// <param name="dateTime"></param>
    public void SetLastListenTime(DateTimeOffset dateTime)
        => LastListenTime = dateTime.ToString("yyyy/M/d HH:mm:ss zzz");
}
