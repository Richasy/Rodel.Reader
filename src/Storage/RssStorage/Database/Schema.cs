// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 数据库 Schema 定义.
/// </summary>
internal static class Schema
{
    /// <summary>
    /// 创建所有表的 SQL.
    /// </summary>
    public const string CreateTablesSql = """
        -- 订阅源分组表
        CREATE TABLE IF NOT EXISTS Groups (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL
        );

        -- 订阅源表
        CREATE TABLE IF NOT EXISTS Feeds (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            Url TEXT NOT NULL,
            Website TEXT,
            Description TEXT,
            IconUrl TEXT,
            GroupIds TEXT,
            Comment TEXT,
            IsFullContentRequired INTEGER DEFAULT 0
        );

        -- 文章表
        CREATE TABLE IF NOT EXISTS Articles (
            Id TEXT PRIMARY KEY NOT NULL,
            FeedId TEXT NOT NULL,
            Title TEXT NOT NULL,
            Summary TEXT,
            Content TEXT,
            CoverUrl TEXT,
            Url TEXT,
            Author TEXT,
            PublishTime TEXT,
            Tags TEXT,
            ExtraData TEXT,
            CachedAt TEXT NOT NULL,
            FOREIGN KEY (FeedId) REFERENCES Feeds(Id) ON DELETE CASCADE
        );

        -- 阅读状态表
        CREATE TABLE IF NOT EXISTS ReadStatus (
            ArticleId TEXT PRIMARY KEY NOT NULL,
            ReadAt TEXT NOT NULL,
            FOREIGN KEY (ArticleId) REFERENCES Articles(Id) ON DELETE CASCADE
        );

        -- 收藏表
        CREATE TABLE IF NOT EXISTS Favorites (
            ArticleId TEXT PRIMARY KEY NOT NULL,
            FavoritedAt TEXT NOT NULL,
            FOREIGN KEY (ArticleId) REFERENCES Articles(Id) ON DELETE CASCADE
        );

        -- 索引
        CREATE INDEX IF NOT EXISTS idx_articles_feedid ON Articles(FeedId);
        CREATE INDEX IF NOT EXISTS idx_articles_publishtime ON Articles(PublishTime);
        CREATE INDEX IF NOT EXISTS idx_articles_cachedat ON Articles(CachedAt);
        """;

    /// <summary>
    /// 删除所有表的 SQL.
    /// </summary>
    public const string DropTablesSql = """
        DROP TABLE IF EXISTS Favorites;
        DROP TABLE IF EXISTS ReadStatus;
        DROP TABLE IF EXISTS Articles;
        DROP TABLE IF EXISTS Feeds;
        DROP TABLE IF EXISTS Groups;
        """;
}
