// Copyright (c) Reader Copilot. All rights reserved.

using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Richasy.ReaderKernel.Models.Rss;

/// <summary>
/// RSS 单集.
/// </summary>
public class RssEpisodeBase
{
    /// <summary>
    /// 标识符.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    [Key]
    public string Id { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 概述.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 作者.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    public string? PublishDate { get; set; }

    /// <summary>
    /// 音频链接.
    /// </summary>
    public string MediaUrl { get; set; }

    /// <summary>
    /// 时长.
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 封面.
    /// </summary>
    public string? Cover { get; set; }

    /// <summary>
    /// 最近一次收听的时间.
    /// </summary>
    public long? LastListenTime { get; set; }

    /// <summary>
    /// 播放进度.
    /// </summary>
    public int? Progress { get; set; }

    /// <summary>
    /// 网址.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 所属播客 id.
    /// </summary>
    public string? PodcastId { get; set; }

    /// <summary>
    /// 内部排序 ID.
    /// </summary>
    public int? InternalSortID { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RssEpisodeBase episode && Id == episode.Id;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// 转换为缓存的单集.
    /// </summary>
    /// <returns></returns>
    public RssCacheEpisode ToCache()
    {
        return new RssCacheEpisode
        {
            Id = Id,
            Title = Title,
            Summary = Summary,
            Description = Description,
            Author = Author,
            PublishDate = PublishDate,
            MediaUrl = MediaUrl,
            Duration = Duration,
            Cover = Cover,
            LastListenTime = LastListenTime,
            Progress = Progress,
            Url = Url,
            PodcastId = PodcastId,
            InternalSortID = InternalSortID,
        };
    }

    /// <summary>
    /// 转换为最近播放的单集.
    /// </summary>
    /// <returns></returns>
    public RssRecentEpisode ToRecent()
    {
        return new RssRecentEpisode
        {
            Id = Id,
            Title = Title,
            Summary = Summary,
            Description = Description,
            Author = Author,
            PublishDate = PublishDate,
            MediaUrl = MediaUrl,
            Duration = Duration,
            Cover = Cover,
            LastListenTime = LastListenTime,
            Progress = Progress,
            Url = Url,
            PodcastId = PodcastId,
            InternalSortID = InternalSortID,
        };
    }
}

/// <summary>
/// 缓存的 RSS 单集.
/// </summary>
[SugarTable("Episodes")]
[Table("Episodes")]
public sealed class RssCacheEpisode : RssEpisodeBase;

/// <summary>
/// 最近播放的 RSS 单集.
/// </summary>
[SugarTable("RecentPlay")]
[Table("RecentPlay")]
public sealed class RssRecentEpisode : RssEpisodeBase;