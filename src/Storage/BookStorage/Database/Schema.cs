// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 数据库 Schema 定义.
/// </summary>
internal static class Schema
{
    /// <summary>
    /// 创建所有表的 SQL.
    /// </summary>
    public const string CreateTablesSql = """
        -- 书籍表
        CREATE TABLE IF NOT EXISTS Books (
            Id TEXT PRIMARY KEY NOT NULL,
            Title TEXT NOT NULL,
            Subtitle TEXT,
            Authors TEXT,
            Translators TEXT,
            Publisher TEXT,
            PublishDate TEXT,
            Language TEXT,
            ISBN TEXT,
            Description TEXT,
            Tags TEXT,
            Series TEXT,
            SeriesIndex INTEGER,
            Categories TEXT,
            Format INTEGER NOT NULL,
            LocalPath TEXT,
            CoverPath TEXT,
            CoverUrl TEXT,
            FileSize INTEGER,
            FileHash TEXT,
            PageCount INTEGER,
            WordCount INTEGER,
            ChapterCount INTEGER,
            SourceType INTEGER NOT NULL DEFAULT 0,
            SourceData TEXT,
            WebUrl TEXT,
            TrackStatus INTEGER DEFAULT 0,
            UserRating INTEGER,
            UserReview TEXT,
            UserTags TEXT,
            UseComicReader INTEGER DEFAULT 0,
            AddedAt TEXT NOT NULL,
            LastOpenedAt TEXT,
            FinishedAt TEXT,
            UpdatedAt TEXT NOT NULL,
            ExtraData TEXT
        );

        -- 书架表
        CREATE TABLE IF NOT EXISTS Shelves (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            IconEmoji TEXT,
            SortIndex INTEGER DEFAULT 0,
            IsDefault INTEGER DEFAULT 0,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );

        -- 书籍分组表
        CREATE TABLE IF NOT EXISTS BookGroups (
            Id TEXT PRIMARY KEY NOT NULL,
            ShelfId TEXT NOT NULL,
            Name TEXT NOT NULL,
            SortIndex INTEGER DEFAULT 0,
            IsCollapsed INTEGER DEFAULT 0,
            CreatedAt TEXT NOT NULL,
            FOREIGN KEY (ShelfId) REFERENCES Shelves(Id) ON DELETE CASCADE
        );

        -- 书架-书籍关联表
        CREATE TABLE IF NOT EXISTS ShelfBookLinks (
            Id TEXT PRIMARY KEY NOT NULL,
            BookId TEXT NOT NULL,
            ShelfId TEXT NOT NULL,
            GroupId TEXT,
            SortIndex INTEGER DEFAULT 0,
            AddedAt TEXT NOT NULL,
            FOREIGN KEY (BookId) REFERENCES Books(Id) ON DELETE CASCADE,
            FOREIGN KEY (ShelfId) REFERENCES Shelves(Id) ON DELETE CASCADE,
            FOREIGN KEY (GroupId) REFERENCES BookGroups(Id) ON DELETE SET NULL
        );

        -- 阅读进度表
        CREATE TABLE IF NOT EXISTS ReadProgress (
            BookId TEXT PRIMARY KEY NOT NULL,
            Progress REAL DEFAULT 0,
            Position TEXT,
            ChapterId TEXT,
            ChapterTitle TEXT,
            CurrentPage INTEGER,
            Locations TEXT,
            UpdatedAt TEXT NOT NULL,
            FOREIGN KEY (BookId) REFERENCES Books(Id) ON DELETE CASCADE
        );

        -- 阅读时段记录表
        CREATE TABLE IF NOT EXISTS ReadingSessions (
            Id TEXT PRIMARY KEY NOT NULL,
            BookId TEXT NOT NULL,
            StartedAt TEXT NOT NULL,
            EndedAt TEXT NOT NULL,
            DurationSeconds INTEGER NOT NULL,
            StartProgress REAL,
            EndProgress REAL,
            PagesRead INTEGER,
            DeviceId TEXT,
            DeviceName TEXT,
            FOREIGN KEY (BookId) REFERENCES Books(Id) ON DELETE CASCADE
        );

        -- 书签表
        CREATE TABLE IF NOT EXISTS Bookmarks (
            Id TEXT PRIMARY KEY NOT NULL,
            BookId TEXT NOT NULL,
            Title TEXT,
            Note TEXT,
            Position TEXT NOT NULL,
            ChapterId TEXT,
            ChapterTitle TEXT,
            PageNumber INTEGER,
            Color TEXT,
            CreatedAt TEXT NOT NULL,
            FOREIGN KEY (BookId) REFERENCES Books(Id) ON DELETE CASCADE
        );

        -- 批注表
        CREATE TABLE IF NOT EXISTS Annotations (
            Id TEXT PRIMARY KEY NOT NULL,
            BookId TEXT NOT NULL,
            Position TEXT NOT NULL,
            EndPosition TEXT,
            ChapterId TEXT,
            ChapterTitle TEXT,
            PageNumber INTEGER,
            SelectedText TEXT,
            Note TEXT,
            Type INTEGER NOT NULL,
            Color TEXT,
            Style TEXT,
            RectJson TEXT,
            SvgPath TEXT,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            FOREIGN KEY (BookId) REFERENCES Books(Id) ON DELETE CASCADE
        );

        -- 索引
        CREATE INDEX IF NOT EXISTS idx_books_format ON Books(Format);
        CREATE INDEX IF NOT EXISTS idx_books_track ON Books(TrackStatus);
        CREATE INDEX IF NOT EXISTS idx_books_source ON Books(SourceType);
        CREATE INDEX IF NOT EXISTS idx_books_addedat ON Books(AddedAt);
        CREATE INDEX IF NOT EXISTS idx_books_lastopened ON Books(LastOpenedAt);
        CREATE INDEX IF NOT EXISTS idx_books_localpath ON Books(LocalPath);
        CREATE INDEX IF NOT EXISTS idx_books_filehash ON Books(FileHash);
        CREATE INDEX IF NOT EXISTS idx_shelflinks_bookid ON ShelfBookLinks(BookId);
        CREATE INDEX IF NOT EXISTS idx_shelflinks_shelfid ON ShelfBookLinks(ShelfId);
        CREATE INDEX IF NOT EXISTS idx_shelflinks_groupid ON ShelfBookLinks(GroupId);
        CREATE INDEX IF NOT EXISTS idx_groups_shelfid ON BookGroups(ShelfId);
        CREATE INDEX IF NOT EXISTS idx_sessions_bookid ON ReadingSessions(BookId);
        CREATE INDEX IF NOT EXISTS idx_sessions_startedat ON ReadingSessions(StartedAt);
        CREATE INDEX IF NOT EXISTS idx_bookmarks_bookid ON Bookmarks(BookId);
        CREATE INDEX IF NOT EXISTS idx_annotations_bookid ON Annotations(BookId);
        """;

    /// <summary>
    /// 删除所有表的 SQL.
    /// </summary>
    public const string DropTablesSql = """
        DROP TABLE IF EXISTS Annotations;
        DROP TABLE IF EXISTS Bookmarks;
        DROP TABLE IF EXISTS ReadingSessions;
        DROP TABLE IF EXISTS ReadProgress;
        DROP TABLE IF EXISTS ShelfBookLinks;
        DROP TABLE IF EXISTS BookGroups;
        DROP TABLE IF EXISTS Shelves;
        DROP TABLE IF EXISTS Books;
        """;
}
