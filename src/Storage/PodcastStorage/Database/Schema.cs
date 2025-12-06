// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Podcast.Database;

/// <summary>
/// 数据库 Schema 定义.
/// </summary>
internal static class Schema
{
    /// <summary>
    /// 创建所有表的 SQL.
    /// </summary>
    public const string CreateTablesSql = """
        -- 播客分组表
        CREATE TABLE IF NOT EXISTS PodcastGroups (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            IconEmoji TEXT,
            SortIndex INTEGER DEFAULT 0,
            CreatedAt INTEGER NOT NULL
        );

        -- 播客表
        CREATE TABLE IF NOT EXISTS Podcasts (
            Id TEXT PRIMARY KEY NOT NULL,
            Title TEXT NOT NULL,
            Description TEXT,
            Author TEXT,
            FeedUrl TEXT,
            Website TEXT,
            CoverUrl TEXT,
            CoverPath TEXT,
            Language TEXT,
            Categories TEXT,
            SourceType INTEGER NOT NULL DEFAULT 0,
            SourceData TEXT,
            GroupIds TEXT,
            EpisodeCount INTEGER,
            LatestEpisodeDate INTEGER,
            IsSubscribed INTEGER DEFAULT 1,
            SortIndex INTEGER,
            AddedAt INTEGER NOT NULL,
            LastRefreshedAt INTEGER,
            UpdatedAt INTEGER NOT NULL
        );

        -- 单集表
        CREATE TABLE IF NOT EXISTS Episodes (
            Id TEXT PRIMARY KEY NOT NULL,
            PodcastId TEXT NOT NULL,
            Title TEXT NOT NULL,
            Description TEXT,
            Summary TEXT,
            Author TEXT,
            MediaUrl TEXT NOT NULL,
            MediaType TEXT,
            MediaSize INTEGER,
            Duration INTEGER,
            CoverUrl TEXT,
            WebUrl TEXT,
            PublishDate INTEGER,
            Season INTEGER,
            EpisodeNumber INTEGER,
            SortIndex INTEGER,
            CachedAt INTEGER NOT NULL,
            FOREIGN KEY (PodcastId) REFERENCES Podcasts(Id) ON DELETE CASCADE
        );

        -- 收听进度表
        CREATE TABLE IF NOT EXISTS ListeningProgress (
            EpisodeId TEXT PRIMARY KEY NOT NULL,
            Position INTEGER DEFAULT 0,
            Duration INTEGER,
            Progress REAL DEFAULT 0,
            PlaybackRate REAL,
            UpdatedAt INTEGER NOT NULL,
            FOREIGN KEY (EpisodeId) REFERENCES Episodes(Id) ON DELETE CASCADE
        );

        -- 收听时段表
        CREATE TABLE IF NOT EXISTS ListeningSessions (
            Id TEXT PRIMARY KEY NOT NULL,
            EpisodeId TEXT NOT NULL,
            PodcastId TEXT NOT NULL,
            StartedAt INTEGER NOT NULL,
            EndedAt INTEGER NOT NULL,
            DurationSeconds INTEGER NOT NULL,
            StartPosition INTEGER,
            EndPosition INTEGER,
            DeviceId TEXT,
            DeviceName TEXT,
            FOREIGN KEY (EpisodeId) REFERENCES Episodes(Id) ON DELETE CASCADE,
            FOREIGN KEY (PodcastId) REFERENCES Podcasts(Id) ON DELETE CASCADE
        );

        -- 索引
        CREATE INDEX IF NOT EXISTS idx_episodes_podcastid ON Episodes(PodcastId);
        CREATE INDEX IF NOT EXISTS idx_episodes_publishdate ON Episodes(PublishDate);
        CREATE INDEX IF NOT EXISTS idx_sessions_episodeid ON ListeningSessions(EpisodeId);
        CREATE INDEX IF NOT EXISTS idx_sessions_podcastid ON ListeningSessions(PodcastId);
        CREATE INDEX IF NOT EXISTS idx_sessions_startedat ON ListeningSessions(StartedAt);
        CREATE INDEX IF NOT EXISTS idx_podcasts_sourcetype ON Podcasts(SourceType);
        """;

    /// <summary>
    /// 删除所有表的 SQL.
    /// </summary>
    public const string DropTablesSql = """
        DROP TABLE IF EXISTS ListeningSessions;
        DROP TABLE IF EXISTS ListeningProgress;
        DROP TABLE IF EXISTS Episodes;
        DROP TABLE IF EXISTS Podcasts;
        DROP TABLE IF EXISTS PodcastGroups;
        """;
}
