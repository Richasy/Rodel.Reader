// Copyright (c) Richasy. All rights reserved.

using Richasy.SqliteGenerator;

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 书籍实体（数据库映射）.
/// </summary>
[SqliteTable("Books")]
internal sealed partial class BookEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string Title { get; set; } = string.Empty;

    [SqliteColumn]
    public string? Subtitle { get; set; }

    [SqliteColumn]
    public string? Authors { get; set; }

    [SqliteColumn]
    public string? Translators { get; set; }

    [SqliteColumn]
    public string? Publisher { get; set; }

    [SqliteColumn]
    public string? PublishDate { get; set; }

    [SqliteColumn]
    public string? Language { get; set; }

    [SqliteColumn]
    public string? ISBN { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? Description { get; set; }

    [SqliteColumn]
    public string? Tags { get; set; }

    [SqliteColumn]
    public string? Series { get; set; }

    [SqliteColumn]
    public int? SeriesIndex { get; set; }

    [SqliteColumn]
    public string? Categories { get; set; }

    [SqliteColumn]
    public int Format { get; set; }

    [SqliteColumn]
    public string? LocalPath { get; set; }

    [SqliteColumn]
    public string? CoverPath { get; set; }

    [SqliteColumn]
    public string? CoverUrl { get; set; }

    [SqliteColumn]
    public long? FileSize { get; set; }

    [SqliteColumn]
    public string? FileHash { get; set; }

    [SqliteColumn]
    public int? PageCount { get; set; }

    [SqliteColumn]
    public int? WordCount { get; set; }

    [SqliteColumn]
    public int? ChapterCount { get; set; }

    [SqliteColumn]
    public int SourceType { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? SourceData { get; set; }

    [SqliteColumn]
    public string? WebUrl { get; set; }

    [SqliteColumn]
    public int TrackStatus { get; set; }

    [SqliteColumn]
    public int? UserRating { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? UserReview { get; set; }

    [SqliteColumn]
    public string? UserTags { get; set; }

    [SqliteColumn]
    public bool UseComicReader { get; set; }

    [SqliteColumn]
    public string AddedAt { get; set; } = string.Empty;

    [SqliteColumn]
    public string? LastOpenedAt { get; set; }

    [SqliteColumn]
    public string? FinishedAt { get; set; }

    [SqliteColumn]
    public string UpdatedAt { get; set; } = string.Empty;

    [SqliteColumn(ExcludeFromList = true)]
    public string? ExtraData { get; set; }

    public static BookEntity FromModel(Book book)
    {
        return new BookEntity
        {
            Id = book.Id,
            Title = book.Title,
            Subtitle = book.Subtitle,
            Authors = book.Authors,
            Translators = book.Translators,
            Publisher = book.Publisher,
            PublishDate = book.PublishDate,
            Language = book.Language,
            ISBN = book.ISBN,
            Description = book.Description,
            Tags = book.Tags,
            Series = book.Series,
            SeriesIndex = book.SeriesIndex,
            Categories = book.Categories,
            Format = (int)book.Format,
            LocalPath = book.LocalPath,
            CoverPath = book.CoverPath,
            CoverUrl = book.CoverUrl,
            FileSize = book.FileSize,
            FileHash = book.FileHash,
            PageCount = book.PageCount,
            WordCount = book.WordCount,
            ChapterCount = book.ChapterCount,
            SourceType = (int)book.SourceType,
            SourceData = book.SourceData,
            WebUrl = book.WebUrl,
            TrackStatus = (int)book.TrackStatus,
            UserRating = book.UserRating,
            UserReview = book.UserReview,
            UserTags = book.UserTags,
            UseComicReader = book.UseComicReader,
            AddedAt = book.AddedAt,
            LastOpenedAt = book.LastOpenedAt,
            FinishedAt = book.FinishedAt,
            UpdatedAt = book.UpdatedAt,
            ExtraData = book.ExtraData,
        };
    }

    public Book ToModel()
    {
        return new Book
        {
            Id = Id,
            Title = Title,
            Subtitle = Subtitle,
            Authors = Authors,
            Translators = Translators,
            Publisher = Publisher,
            PublishDate = PublishDate,
            Language = Language,
            ISBN = ISBN,
            Description = Description,
            Tags = Tags,
            Series = Series,
            SeriesIndex = SeriesIndex,
            Categories = Categories,
            Format = (BookFormat)Format,
            LocalPath = LocalPath,
            CoverPath = CoverPath,
            CoverUrl = CoverUrl,
            FileSize = FileSize,
            FileHash = FileHash,
            PageCount = PageCount,
            WordCount = WordCount,
            ChapterCount = ChapterCount,
            SourceType = (BookSourceType)SourceType,
            SourceData = SourceData,
            WebUrl = WebUrl,
            TrackStatus = (BookTrackStatus)TrackStatus,
            UserRating = UserRating,
            UserReview = UserReview,
            UserTags = UserTags,
            UseComicReader = UseComicReader,
            AddedAt = AddedAt,
            LastOpenedAt = LastOpenedAt,
            FinishedAt = FinishedAt,
            UpdatedAt = UpdatedAt,
            ExtraData = ExtraData,
        };
    }
}

/// <summary>
/// 书架实体（数据库映射）.
/// </summary>
[SqliteTable("Shelves")]
internal sealed partial class ShelfEntity
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
    public bool IsDefault { get; set; }

    [SqliteColumn]
    public string CreatedAt { get; set; } = string.Empty;

    [SqliteColumn]
    public string UpdatedAt { get; set; } = string.Empty;

    public static ShelfEntity FromModel(Shelf shelf)
    {
        return new ShelfEntity
        {
            Id = shelf.Id,
            Name = shelf.Name,
            IconEmoji = shelf.IconEmoji,
            SortIndex = shelf.SortIndex,
            IsDefault = shelf.IsDefault,
            CreatedAt = shelf.CreatedAt,
            UpdatedAt = shelf.UpdatedAt,
        };
    }

    public Shelf ToModel()
    {
        return new Shelf
        {
            Id = Id,
            Name = Name,
            IconEmoji = IconEmoji,
            SortIndex = SortIndex,
            IsDefault = IsDefault,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
        };
    }
}

/// <summary>
/// 书籍分组实体（数据库映射）.
/// </summary>
[SqliteTable("BookGroups")]
internal sealed partial class BookGroupEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string ShelfId { get; set; } = string.Empty;

    [SqliteColumn]
    public string Name { get; set; } = string.Empty;

    [SqliteColumn]
    public int SortIndex { get; set; }

    [SqliteColumn]
    public bool IsCollapsed { get; set; }

    [SqliteColumn]
    public string CreatedAt { get; set; } = string.Empty;

    public static BookGroupEntity FromModel(BookGroup group)
    {
        return new BookGroupEntity
        {
            Id = group.Id,
            ShelfId = group.ShelfId,
            Name = group.Name,
            SortIndex = group.SortIndex,
            IsCollapsed = group.IsCollapsed,
            CreatedAt = group.CreatedAt,
        };
    }

    public BookGroup ToModel()
    {
        return new BookGroup
        {
            Id = Id,
            ShelfId = ShelfId,
            Name = Name,
            SortIndex = SortIndex,
            IsCollapsed = IsCollapsed,
            CreatedAt = CreatedAt,
        };
    }
}

/// <summary>
/// 书架-书籍关联实体（数据库映射）.
/// </summary>
[SqliteTable("ShelfBookLinks")]
internal sealed partial class ShelfBookLinkEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string BookId { get; set; } = string.Empty;

    [SqliteColumn]
    public string ShelfId { get; set; } = string.Empty;

    [SqliteColumn]
    public string? GroupId { get; set; }

    [SqliteColumn]
    public int SortIndex { get; set; }

    [SqliteColumn]
    public string AddedAt { get; set; } = string.Empty;

    public static ShelfBookLinkEntity FromModel(ShelfBookLink link)
    {
        return new ShelfBookLinkEntity
        {
            Id = link.Id,
            BookId = link.BookId,
            ShelfId = link.ShelfId,
            GroupId = link.GroupId,
            SortIndex = link.SortIndex,
            AddedAt = link.AddedAt,
        };
    }

    public ShelfBookLink ToModel()
    {
        return new ShelfBookLink
        {
            Id = Id,
            BookId = BookId,
            ShelfId = ShelfId,
            GroupId = GroupId,
            SortIndex = SortIndex,
            AddedAt = AddedAt,
        };
    }
}

/// <summary>
/// 阅读进度实体（数据库映射）.
/// </summary>
[SqliteTable("ReadProgress")]
internal sealed partial class ReadProgressEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string BookId { get; set; } = string.Empty;

    [SqliteColumn]
    public double Progress { get; set; }

    [SqliteColumn]
    public string? Position { get; set; }

    [SqliteColumn]
    public string? ChapterId { get; set; }

    [SqliteColumn]
    public string? ChapterTitle { get; set; }

    [SqliteColumn]
    public int? CurrentPage { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? Locations { get; set; }

    [SqliteColumn]
    public string UpdatedAt { get; set; } = string.Empty;

    public static ReadProgressEntity FromModel(ReadProgress progress)
    {
        return new ReadProgressEntity
        {
            BookId = progress.BookId,
            Progress = progress.Progress,
            Position = progress.Position,
            ChapterId = progress.ChapterId,
            ChapterTitle = progress.ChapterTitle,
            CurrentPage = progress.CurrentPage,
            Locations = progress.Locations,
            UpdatedAt = progress.UpdatedAt,
        };
    }

    public ReadProgress ToModel()
    {
        return new ReadProgress
        {
            BookId = BookId,
            Progress = Progress,
            Position = Position,
            ChapterId = ChapterId,
            ChapterTitle = ChapterTitle,
            CurrentPage = CurrentPage,
            Locations = Locations,
            UpdatedAt = UpdatedAt,
        };
    }
}

/// <summary>
/// 阅读时段实体（数据库映射）.
/// </summary>
[SqliteTable("ReadingSessions")]
internal sealed partial class ReadingSessionEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string BookId { get; set; } = string.Empty;

    [SqliteColumn]
    public string StartedAt { get; set; } = string.Empty;

    [SqliteColumn]
    public string EndedAt { get; set; } = string.Empty;

    [SqliteColumn]
    public int DurationSeconds { get; set; }

    [SqliteColumn]
    public double? StartProgress { get; set; }

    [SqliteColumn]
    public double? EndProgress { get; set; }

    [SqliteColumn]
    public int? PagesRead { get; set; }

    [SqliteColumn]
    public string? DeviceId { get; set; }

    [SqliteColumn]
    public string? DeviceName { get; set; }

    public static ReadingSessionEntity FromModel(ReadingSession session)
    {
        return new ReadingSessionEntity
        {
            Id = session.Id,
            BookId = session.BookId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            DurationSeconds = session.DurationSeconds,
            StartProgress = session.StartProgress,
            EndProgress = session.EndProgress,
            PagesRead = session.PagesRead,
            DeviceId = session.DeviceId,
            DeviceName = session.DeviceName,
        };
    }

    public ReadingSession ToModel()
    {
        return new ReadingSession
        {
            Id = Id,
            BookId = BookId,
            StartedAt = StartedAt,
            EndedAt = EndedAt,
            DurationSeconds = DurationSeconds,
            StartProgress = StartProgress,
            EndProgress = EndProgress,
            PagesRead = PagesRead,
            DeviceId = DeviceId,
            DeviceName = DeviceName,
        };
    }
}

/// <summary>
/// 书签实体（数据库映射）.
/// </summary>
[SqliteTable("Bookmarks")]
internal sealed partial class BookmarkEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string BookId { get; set; } = string.Empty;

    [SqliteColumn]
    public string? Title { get; set; }

    [SqliteColumn]
    public string? Note { get; set; }

    [SqliteColumn]
    public string Position { get; set; } = string.Empty;

    [SqliteColumn]
    public string? ChapterId { get; set; }

    [SqliteColumn]
    public string? ChapterTitle { get; set; }

    [SqliteColumn]
    public int? PageNumber { get; set; }

    [SqliteColumn]
    public string? Color { get; set; }

    [SqliteColumn]
    public string CreatedAt { get; set; } = string.Empty;

    public static BookmarkEntity FromModel(Bookmark bookmark)
    {
        return new BookmarkEntity
        {
            Id = bookmark.Id,
            BookId = bookmark.BookId,
            Title = bookmark.Title,
            Note = bookmark.Note,
            Position = bookmark.Position,
            ChapterId = bookmark.ChapterId,
            ChapterTitle = bookmark.ChapterTitle,
            PageNumber = bookmark.PageNumber,
            Color = bookmark.Color,
            CreatedAt = bookmark.CreatedAt,
        };
    }

    public Bookmark ToModel()
    {
        return new Bookmark
        {
            Id = Id,
            BookId = BookId,
            Title = Title,
            Note = Note,
            Position = Position,
            ChapterId = ChapterId,
            ChapterTitle = ChapterTitle,
            PageNumber = PageNumber,
            Color = Color,
            CreatedAt = CreatedAt,
        };
    }
}

/// <summary>
/// 批注实体（数据库映射）.
/// </summary>
[SqliteTable("Annotations")]
internal sealed partial class AnnotationEntity
{
    [SqliteColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    [SqliteColumn]
    public string BookId { get; set; } = string.Empty;

    [SqliteColumn]
    public string Position { get; set; } = string.Empty;

    [SqliteColumn]
    public string? EndPosition { get; set; }

    [SqliteColumn]
    public string? ChapterId { get; set; }

    [SqliteColumn]
    public string? ChapterTitle { get; set; }

    [SqliteColumn]
    public int? PageNumber { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? SelectedText { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? Note { get; set; }

    [SqliteColumn]
    public int Type { get; set; }

    [SqliteColumn]
    public string? Color { get; set; }

    [SqliteColumn]
    public string? Style { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? RectJson { get; set; }

    [SqliteColumn(ExcludeFromList = true)]
    public string? SvgPath { get; set; }

    [SqliteColumn]
    public string CreatedAt { get; set; } = string.Empty;

    [SqliteColumn]
    public string UpdatedAt { get; set; } = string.Empty;

    public static AnnotationEntity FromModel(Annotation annotation)
    {
        return new AnnotationEntity
        {
            Id = annotation.Id,
            BookId = annotation.BookId,
            Position = annotation.Position,
            EndPosition = annotation.EndPosition,
            ChapterId = annotation.ChapterId,
            ChapterTitle = annotation.ChapterTitle,
            PageNumber = annotation.PageNumber,
            SelectedText = annotation.SelectedText,
            Note = annotation.Note,
            Type = (int)annotation.Type,
            Color = annotation.Color,
            Style = annotation.Style,
            RectJson = annotation.RectJson,
            SvgPath = annotation.SvgPath,
            CreatedAt = annotation.CreatedAt,
            UpdatedAt = annotation.UpdatedAt,
        };
    }

    public Annotation ToModel()
    {
        return new Annotation
        {
            Id = Id,
            BookId = BookId,
            Position = Position,
            EndPosition = EndPosition,
            ChapterId = ChapterId,
            ChapterTitle = ChapterTitle,
            PageNumber = PageNumber,
            SelectedText = SelectedText,
            Note = Note,
            Type = (AnnotationType)Type,
            Color = Color,
            Style = Style,
            RectJson = RectJson,
            SvgPath = SvgPath,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
        };
    }
}
