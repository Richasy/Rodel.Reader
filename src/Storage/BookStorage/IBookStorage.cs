// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书籍存储服务接口.
/// </summary>
public interface IBookStorage : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// 初始化数据库.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    #region Books

    /// <summary>
    /// 获取所有书籍.
    /// </summary>
    Task<IReadOnlyList<Book>> GetAllBooksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取书籍.
    /// </summary>
    Task<Book?> GetBookAsync(string bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据文件路径获取书籍.
    /// </summary>
    Task<Book?> GetBookByPathAsync(string localPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据文件哈希获取书籍.
    /// </summary>
    Task<Book?> GetBookByHashAsync(string fileHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// 搜索书籍.
    /// </summary>
    Task<IReadOnlyList<Book>> SearchBooksAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据格式获取书籍.
    /// </summary>
    Task<IReadOnlyList<Book>> GetBooksByFormatAsync(BookFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据追踪状态获取书籍.
    /// </summary>
    Task<IReadOnlyList<Book>> GetBooksByTrackStatusAsync(BookTrackStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据来源类型获取书籍.
    /// </summary>
    Task<IReadOnlyList<Book>> GetBooksBySourceTypeAsync(BookSourceType sourceType, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新书籍.
    /// </summary>
    Task UpsertBookAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加或更新书籍.
    /// </summary>
    Task UpsertBooksAsync(IEnumerable<Book> books, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除书籍（同时删除相关数据）.
    /// </summary>
    Task<bool> DeleteBookAsync(string bookId, CancellationToken cancellationToken = default);

    #endregion

    #region Shelves

    /// <summary>
    /// 获取所有书架.
    /// </summary>
    Task<IReadOnlyList<Shelf>> GetAllShelvesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取书架.
    /// </summary>
    Task<Shelf?> GetShelfAsync(string shelfId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查书架名称是否存在.
    /// </summary>
    Task<bool> IsShelfNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新书架.
    /// </summary>
    Task UpsertShelfAsync(Shelf shelf, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除书架（同时删除关联和分组）.
    /// </summary>
    Task<bool> DeleteShelfAsync(string shelfId, CancellationToken cancellationToken = default);

    #endregion

    #region Book Groups

    /// <summary>
    /// 获取书架下的所有分组.
    /// </summary>
    Task<IReadOnlyList<BookGroup>> GetGroupsByShelfAsync(string shelfId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取分组.
    /// </summary>
    Task<BookGroup?> GetGroupAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新分组.
    /// </summary>
    Task UpsertGroupAsync(BookGroup group, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除分组（书籍移至书架根级别）.
    /// </summary>
    Task<bool> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default);

    #endregion

    #region Shelf-Book Links

    /// <summary>
    /// 获取书架中的书籍（含分组信息）.
    /// </summary>
    Task<IReadOnlyList<(Book Book, ShelfBookLink Link)>> GetBooksInShelfAsync(string shelfId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取分组中的书籍.
    /// </summary>
    Task<IReadOnlyList<Book>> GetBooksInGroupAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加书籍到书架.
    /// </summary>
    Task AddBookToShelfAsync(string bookId, string shelfId, string? groupId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从书架移除书籍.
    /// </summary>
    Task<bool> RemoveBookFromShelfAsync(string bookId, string shelfId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移动书籍到分组.
    /// </summary>
    Task MoveBookToGroupAsync(string bookId, string shelfId, string? groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新书籍在书架中的排序.
    /// </summary>
    Task UpdateBookSortIndexAsync(string bookId, string shelfId, int sortIndex, CancellationToken cancellationToken = default);

    #endregion

    #region Read Progress

    /// <summary>
    /// 获取阅读进度.
    /// </summary>
    Task<ReadProgress?> GetReadProgressAsync(string bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新阅读进度.
    /// </summary>
    Task UpsertReadProgressAsync(ReadProgress progress, CancellationToken cancellationToken = default);

    #endregion

    #region Reading Sessions

    /// <summary>
    /// 获取书籍的阅读时段.
    /// </summary>
    Task<IReadOnlyList<ReadingSession>> GetReadingSessionsAsync(string bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最近的阅读时段.
    /// </summary>
    Task<IReadOnlyList<ReadingSession>> GetRecentSessionsAsync(int days = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取书籍阅读统计.
    /// </summary>
    Task<BookReadingStats> GetReadingStatsAsync(string bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加阅读时段.
    /// </summary>
    Task AddReadingSessionAsync(ReadingSession session, CancellationToken cancellationToken = default);

    #endregion

    #region Bookmarks

    /// <summary>
    /// 获取书籍的书签.
    /// </summary>
    Task<IReadOnlyList<Bookmark>> GetBookmarksAsync(string bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新书签.
    /// </summary>
    Task UpsertBookmarkAsync(Bookmark bookmark, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除书签.
    /// </summary>
    Task<bool> DeleteBookmarkAsync(string bookmarkId, CancellationToken cancellationToken = default);

    #endregion

    #region Annotations

    /// <summary>
    /// 获取书籍的批注.
    /// </summary>
    Task<IReadOnlyList<Annotation>> GetAnnotationsAsync(string bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新批注.
    /// </summary>
    Task UpsertAnnotationAsync(Annotation annotation, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除批注.
    /// </summary>
    Task<bool> DeleteAnnotationAsync(string annotationId, CancellationToken cancellationToken = default);

    #endregion

    #region Cleanup

    /// <summary>
    /// 清理孤立数据.
    /// </summary>
    Task<int> CleanupOrphanedDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 清空所有数据.
    /// </summary>
    Task ClearAllAsync(CancellationToken cancellationToken = default);

    #endregion
}
