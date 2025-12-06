// Copyright (c) Richasy. All rights reserved.

using Richasy.SqliteGenerator;

namespace Richasy.RodelReader.Storage.Podcast.Database;

/// <summary>
/// 播客实体（数据库映射）.
/// </summary>
[SqliteTable("Podcasts")]
internal sealed partial class PodcastEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string Title { get; set; } = string.Empty;

    [SqliteColumn(ExcludeFromList = true)]
    public string? Description { get; set; }

    [SqliteColumn]
    public string? Author { get; set; }

    [SqliteColumn]
    public string? FeedUrl { get; set; }

    [SqliteColumn]
    public string? Website { get; set; }

    [SqliteColumn]
    public string? CoverUrl { get; set; }

    [SqliteColumn]
    public string? CoverPath { get; set; }

    [SqliteColumn]
    public string? Language { get; set; }

    [SqliteColumn]
    public string? Categories { get; set; }

    [SqliteColumn]
    public int SourceType { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? SourceData { get; set; }

    [SqliteColumn]
    public string? GroupIds { get; set; }

    [SqliteColumn]
    public int? EpisodeCount { get; set; }

    [SqliteColumn]
    public long? LatestEpisodeDate { get; set; }

    [SqliteColumn]
    public bool IsSubscribed { get; set; }

    [SqliteColumn]
    public int? SortIndex { get; set; }

    [SqliteColumn]
    public long AddedAt { get; set; }

    [SqliteColumn]
    public long? LastRefreshedAt { get; set; }

    [SqliteColumn]
    public long UpdatedAt { get; set; }

    public static PodcastEntity FromModel(Podcast podcast)
    {
        return new PodcastEntity
        {
            Id = podcast.Id,
            Title = podcast.Title,
            Description = podcast.Description,
            Author = podcast.Author,
            FeedUrl = podcast.FeedUrl,
            Website = podcast.Website,
            CoverUrl = podcast.CoverUrl,
            CoverPath = podcast.CoverPath,
            Language = podcast.Language,
            Categories = podcast.Categories,
            SourceType = (int)podcast.SourceType,
            SourceData = podcast.SourceData,
            GroupIds = podcast.GroupIds,
            EpisodeCount = podcast.EpisodeCount,
            LatestEpisodeDate = podcast.LatestEpisodeDate?.ToUnixTimeSeconds(),
            IsSubscribed = podcast.IsSubscribed,
            SortIndex = podcast.SortIndex,
            AddedAt = podcast.AddedAt.ToUnixTimeSeconds(),
            LastRefreshedAt = podcast.LastRefreshedAt?.ToUnixTimeSeconds(),
            UpdatedAt = podcast.UpdatedAt.ToUnixTimeSeconds(),
        };
    }

    public Podcast ToModel()
    {
        return new Podcast
        {
            Id = Id,
            Title = Title,
            Description = Description,
            Author = Author,
            FeedUrl = FeedUrl,
            Website = Website,
            CoverUrl = CoverUrl,
            CoverPath = CoverPath,
            Language = Language,
            Categories = Categories,
            SourceType = (PodcastSourceType)SourceType,
            SourceData = SourceData,
            GroupIds = GroupIds,
            EpisodeCount = EpisodeCount,
            LatestEpisodeDate = LatestEpisodeDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(LatestEpisodeDate.Value) : null,
            IsSubscribed = IsSubscribed,
            SortIndex = SortIndex,
            AddedAt = DateTimeOffset.FromUnixTimeSeconds(AddedAt),
            LastRefreshedAt = LastRefreshedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(LastRefreshedAt.Value) : null,
            UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(UpdatedAt),
        };
    }
}

/// <summary>
/// 播客分组实体（数据库映射）.
/// </summary>
[SqliteTable("PodcastGroups")]
internal sealed partial class PodcastGroupEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string Name { get; set; } = string.Empty;

    [SqliteColumn]
    public string? IconEmoji { get; set; }

    [SqliteColumn]
    public int SortIndex { get; set; }

    [SqliteColumn]
    public long CreatedAt { get; set; }

    public static PodcastGroupEntity FromModel(PodcastGroup group)
    {
        return new PodcastGroupEntity
        {
            Id = group.Id,
            Name = group.Name,
            IconEmoji = group.IconEmoji,
            SortIndex = group.SortIndex,
            CreatedAt = group.CreatedAt.ToUnixTimeSeconds(),
        };
    }

    public PodcastGroup ToModel()
    {
        return new PodcastGroup
        {
            Id = Id,
            Name = Name,
            IconEmoji = IconEmoji,
            SortIndex = SortIndex,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(CreatedAt),
        };
    }
}

/// <summary>
/// 单集实体（数据库映射）.
/// </summary>
[SqliteTable("Episodes")]
internal sealed partial class EpisodeEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string PodcastId { get; set; } = string.Empty;

    [SqliteColumn]
    public string Title { get; set; } = string.Empty;

    [SqliteColumn(ExcludeFromList = true)]
    public string? Description { get; set; }

    [SqliteColumn]
    public string? Summary { get; set; }

    [SqliteColumn]
    public string? Author { get; set; }

    [SqliteColumn]
    public string MediaUrl { get; set; } = string.Empty;

    [SqliteColumn]
    public string? MediaType { get; set; }

    [SqliteColumn]
    public long? MediaSize { get; set; }

    [SqliteColumn]
    public int? Duration { get; set; }

    [SqliteColumn]
    public string? CoverUrl { get; set; }

    [SqliteColumn]
    public string? WebUrl { get; set; }

    [SqliteColumn]
    public long? PublishDate { get; set; }

    [SqliteColumn]
    public int? Season { get; set; }

    [SqliteColumn]
    public int? EpisodeNumber { get; set; }

    [SqliteColumn]
    public int? SortIndex { get; set; }

    [SqliteColumn]
    public long CachedAt { get; set; }

    public static EpisodeEntity FromModel(Episode episode)
    {
        return new EpisodeEntity
        {
            Id = episode.Id,
            PodcastId = episode.PodcastId,
            Title = episode.Title,
            Description = episode.Description,
            Summary = episode.Summary,
            Author = episode.Author,
            MediaUrl = episode.MediaUrl,
            MediaType = episode.MediaType,
            MediaSize = episode.MediaSize,
            Duration = episode.Duration,
            CoverUrl = episode.CoverUrl,
            WebUrl = episode.WebUrl,
            PublishDate = episode.PublishDate?.ToUnixTimeSeconds(),
            Season = episode.Season,
            EpisodeNumber = episode.EpisodeNumber,
            SortIndex = episode.SortIndex,
            CachedAt = episode.CachedAt.ToUnixTimeSeconds(),
        };
    }

    public Episode ToModel()
    {
        return new Episode
        {
            Id = Id,
            PodcastId = PodcastId,
            Title = Title,
            Description = Description,
            Summary = Summary,
            Author = Author,
            MediaUrl = MediaUrl,
            MediaType = MediaType,
            MediaSize = MediaSize,
            Duration = Duration,
            CoverUrl = CoverUrl,
            WebUrl = WebUrl,
            PublishDate = PublishDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(PublishDate.Value) : null,
            Season = Season,
            EpisodeNumber = EpisodeNumber,
            SortIndex = SortIndex,
            CachedAt = DateTimeOffset.FromUnixTimeSeconds(CachedAt),
        };
    }
}

/// <summary>
/// 收听进度实体（数据库映射）.
/// </summary>
[SqliteTable("ListeningProgress")]
internal sealed partial class ListeningProgressEntity
{
    [SqliteColumn("EpisodeId", IsPrimaryKey = true)]
    public string EpisodeId { get; set; } = string.Empty;

    [SqliteColumn]
    public int Position { get; set; }

    [SqliteColumn]
    public int? Duration { get; set; }

    [SqliteColumn]
    public double Progress { get; set; }

    [SqliteColumn]
    public double? PlaybackRate { get; set; }

    [SqliteColumn]
    public long UpdatedAt { get; set; }

    public static ListeningProgressEntity FromModel(ListeningProgress progress)
    {
        return new ListeningProgressEntity
        {
            EpisodeId = progress.EpisodeId,
            Position = progress.Position,
            Duration = progress.Duration,
            Progress = progress.Progress,
            PlaybackRate = progress.PlaybackRate,
            UpdatedAt = progress.UpdatedAt.ToUnixTimeSeconds(),
        };
    }

    public ListeningProgress ToModel()
    {
        return new ListeningProgress
        {
            EpisodeId = EpisodeId,
            Position = Position,
            Duration = Duration,
            Progress = Progress,
            PlaybackRate = PlaybackRate,
            UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(UpdatedAt),
        };
    }
}

/// <summary>
/// 收听时段实体（数据库映射）.
/// </summary>
[SqliteTable("ListeningSessions")]
internal sealed partial class ListeningSessionEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string EpisodeId { get; set; } = string.Empty;

    [SqliteColumn]
    public string PodcastId { get; set; } = string.Empty;

    [SqliteColumn]
    public long StartedAt { get; set; }

    [SqliteColumn]
    public long EndedAt { get; set; }

    [SqliteColumn]
    public int DurationSeconds { get; set; }

    [SqliteColumn]
    public int? StartPosition { get; set; }

    [SqliteColumn]
    public int? EndPosition { get; set; }

    [SqliteColumn]
    public string? DeviceId { get; set; }

    [SqliteColumn]
    public string? DeviceName { get; set; }

    public static ListeningSessionEntity FromModel(ListeningSession session)
    {
        return new ListeningSessionEntity
        {
            Id = session.Id,
            EpisodeId = session.EpisodeId,
            PodcastId = session.PodcastId,
            StartedAt = session.StartedAt.ToUnixTimeSeconds(),
            EndedAt = session.EndedAt.ToUnixTimeSeconds(),
            DurationSeconds = session.DurationSeconds,
            StartPosition = session.StartPosition,
            EndPosition = session.EndPosition,
            DeviceId = session.DeviceId,
            DeviceName = session.DeviceName,
        };
    }

    public ListeningSession ToModel()
    {
        return new ListeningSession
        {
            Id = Id,
            EpisodeId = EpisodeId,
            PodcastId = PodcastId,
            StartedAt = DateTimeOffset.FromUnixTimeSeconds(StartedAt),
            EndedAt = DateTimeOffset.FromUnixTimeSeconds(EndedAt),
            DurationSeconds = DurationSeconds,
            StartPosition = StartPosition,
            EndPosition = EndPosition,
            DeviceId = DeviceId,
            DeviceName = DeviceName,
        };
    }
}
