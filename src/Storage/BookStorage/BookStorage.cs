// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Storage.Book.Database;

namespace Richasy.RodelReader.Storage.Book;

/// <summary>
/// 书籍存储服务实现.
/// </summary>
public sealed class BookStorage : IBookStorage
{
    private readonly BookStorageOptions _options;
    private readonly ILogger<BookStorage>? _logger;

    private BookDatabase? _database;
    private BookRepository? _bookRepository;
    private ShelfRepository? _shelfRepository;
    private GroupRepository? _groupRepository;
    private LinkRepository? _linkRepository;
    private ProgressRepository? _progressRepository;
    private SessionRepository? _sessionRepository;
    private BookmarkRepository? _bookmarkRepository;
    private AnnotationRepository? _annotationRepository;
    private bool _initialized;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookStorage"/> class.
    /// </summary>
    public BookStorage(BookStorageOptions options, ILogger<BookStorage>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_initialized)
        {
            _logger?.LogDebug("Storage already initialized.");
            return;
        }

        _logger?.LogInformation("Initializing Book storage at {DatabasePath}...", _options.DatabasePath);

        var directory = Path.GetDirectoryName(_options.DatabasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _database = new BookDatabase(_options.DatabasePath, _logger as ILogger<BookDatabase>);

        if (_options.CreateTablesOnInit)
        {
            await _database.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        _bookRepository = new BookRepository(_database, _logger);
        _shelfRepository = new ShelfRepository(_database, _logger);
        _groupRepository = new GroupRepository(_database, _logger);
        _linkRepository = new LinkRepository(_database, _logger);
        _progressRepository = new ProgressRepository(_database, _logger);
        _sessionRepository = new SessionRepository(_database, _logger);
        _bookmarkRepository = new BookmarkRepository(_database, _logger);
        _annotationRepository = new AnnotationRepository(_database, _logger);

        _initialized = true;
        _logger?.LogInformation("Book storage initialized successfully.");
    }

    #region Books

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> GetAllBooksAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Book?> GetBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.GetByIdAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Book?> GetBookByPathAsync(string localPath, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.GetByPathAsync(localPath, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Book?> GetBookByHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.GetByHashAsync(fileHash, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> SearchBooksAsync(string keyword, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.SearchAsync(keyword, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> GetBooksByFormatAsync(BookFormat format, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.GetByFormatAsync(format, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> GetBooksByTrackStatusAsync(BookTrackStatus status, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.GetByTrackStatusAsync(status, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> GetBooksBySourceTypeAsync(BookSourceType sourceType, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.GetBySourceTypeAsync(sourceType, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _bookRepository!.UpsertAsync(book, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertBooksAsync(IEnumerable<Book> books, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _bookRepository!.UpsertManyAsync(books, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookRepository!.DeleteAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Shelves

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Shelf>> GetAllShelvesAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _shelfRepository!.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Shelf?> GetShelfAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _shelfRepository!.GetByIdAsync(shelfId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsShelfNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _shelfRepository!.IsNameExistsAsync(name, excludeId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertShelfAsync(Shelf shelf, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _shelfRepository!.UpsertAsync(shelf, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteShelfAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _shelfRepository!.DeleteAsync(shelfId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Book Groups

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BookGroup>> GetGroupsByShelfAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _groupRepository!.GetByShelfAsync(shelfId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<BookGroup?> GetGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _groupRepository!.GetByIdAsync(groupId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertGroupAsync(BookGroup group, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _groupRepository!.UpsertAsync(group, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _linkRepository!.ClearGroupAsync(groupId, cancellationToken).ConfigureAwait(false);
        return await _groupRepository!.DeleteAsync(groupId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Shelf-Book Links

    /// <inheritdoc/>
    public async Task<IReadOnlyList<(Book Book, ShelfBookLink Link)>> GetBooksInShelfAsync(string shelfId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var links = await _linkRepository!.GetByShelfAsync(shelfId, cancellationToken).ConfigureAwait(false);
        var results = new List<(Book, ShelfBookLink)>();

        foreach (var link in links)
        {
            var book = await _bookRepository!.GetByIdAsync(link.BookId, cancellationToken).ConfigureAwait(false);
            if (book is not null)
            {
                results.Add((book, link));
            }
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> GetBooksInGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var links = await _linkRepository!.GetByGroupAsync(groupId, cancellationToken).ConfigureAwait(false);
        var results = new List<Book>();

        foreach (var link in links)
        {
            var book = await _bookRepository!.GetByIdAsync(link.BookId, cancellationToken).ConfigureAwait(false);
            if (book is not null)
            {
                results.Add(book);
            }
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddBookToShelfAsync(string bookId, string shelfId, string? groupId = null, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var link = new ShelfBookLink
        {
            Id = $"{bookId}_{shelfId}",
            BookId = bookId,
            ShelfId = shelfId,
            GroupId = groupId,
            SortIndex = 0,
            AddedAt = DateTimeOffset.UtcNow.ToString("O"),
        };
        await _linkRepository!.UpsertAsync(link, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveBookFromShelfAsync(string bookId, string shelfId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _linkRepository!.DeleteAsync(bookId, shelfId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task MoveBookToGroupAsync(string bookId, string shelfId, string? groupId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var link = await _linkRepository!.GetLinkAsync(bookId, shelfId, cancellationToken).ConfigureAwait(false);
        if (link is not null)
        {
            link.GroupId = groupId;
            await _linkRepository.UpsertAsync(link, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task UpdateBookSortIndexAsync(string bookId, string shelfId, int sortIndex, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var link = await _linkRepository!.GetLinkAsync(bookId, shelfId, cancellationToken).ConfigureAwait(false);
        if (link is not null)
        {
            link.SortIndex = sortIndex;
            await _linkRepository.UpsertAsync(link, cancellationToken).ConfigureAwait(false);
        }
    }

    #endregion

    #region Read Progress

    /// <inheritdoc/>
    public async Task<ReadProgress?> GetReadProgressAsync(string bookId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _progressRepository!.GetByBookIdAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertReadProgressAsync(ReadProgress progress, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _progressRepository!.UpsertAsync(progress, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Reading Sessions

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReadingSession>> GetReadingSessionsAsync(string bookId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetByBookAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReadingSession>> GetRecentSessionsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetRecentAsync(days, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<BookReadingStats> GetReadingStatsAsync(string bookId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _sessionRepository!.GetStatsAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddReadingSessionAsync(ReadingSession session, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _sessionRepository!.AddAsync(session, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Bookmarks

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Bookmark>> GetBookmarksAsync(string bookId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookmarkRepository!.GetByBookAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertBookmarkAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _bookmarkRepository!.UpsertAsync(bookmark, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteBookmarkAsync(string bookmarkId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _bookmarkRepository!.DeleteAsync(bookmarkId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Annotations

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Annotation>> GetAnnotationsAsync(string bookId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _annotationRepository!.GetByBookAsync(bookId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task UpsertAnnotationAsync(Annotation annotation, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _annotationRepository!.UpsertAsync(annotation, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAnnotationAsync(string annotationId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _annotationRepository!.DeleteAsync(annotationId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Cleanup

    /// <inheritdoc/>
    public async Task<int> CleanupOrphanedDataAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        // 清理孤立的书架-书籍关联（书籍不存在）
        const string cleanupLinksSql = """
            DELETE FROM ShelfBookLinks
            WHERE BookId NOT IN (SELECT Id FROM Books)
            """;

        // 清理孤立的阅读进度
        const string cleanupProgressSql = """
            DELETE FROM ReadProgress
            WHERE BookId NOT IN (SELECT Id FROM Books)
            """;

        // 清理孤立的阅读时段
        const string cleanupSessionsSql = """
            DELETE FROM ReadingSessions
            WHERE BookId NOT IN (SELECT Id FROM Books)
            """;

        // 清理孤立的书签
        const string cleanupBookmarksSql = """
            DELETE FROM Bookmarks
            WHERE BookId NOT IN (SELECT Id FROM Books)
            """;

        // 清理孤立的批注
        const string cleanupAnnotationsSql = """
            DELETE FROM Annotations
            WHERE BookId NOT IN (SELECT Id FROM Books)
            """;

        var totalAffected = 0;

        await using (var cmd = _database!.CreateCommand(cleanupLinksSql))
        {
            totalAffected += await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var cmd = _database.CreateCommand(cleanupProgressSql))
        {
            totalAffected += await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var cmd = _database.CreateCommand(cleanupSessionsSql))
        {
            totalAffected += await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var cmd = _database.CreateCommand(cleanupBookmarksSql))
        {
            totalAffected += await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var cmd = _database.CreateCommand(cleanupAnnotationsSql))
        {
            totalAffected += await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        _logger?.LogInformation("Cleaned up {Count} orphaned records.", totalAffected);
        return totalAffected;
    }

    /// <inheritdoc/>
    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        await using var cmd = _database!.CreateCommand(Schema.DropTablesSql);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        await _database.InitializeAsync(cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("All data cleared.");
    }

    #endregion

    #region Dispose

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _database?.Dispose();
        _database = null;
        _disposed = true;
        _logger?.LogDebug("BookStorage disposed.");
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_database is not null)
        {
            await _database.DisposeAsync().ConfigureAwait(false);
            _database = null;
        }

        _disposed = true;
        _logger?.LogDebug("BookStorage disposed asynchronously.");
    }

    #endregion

    private void EnsureInitialized()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_initialized)
        {
            throw new InvalidOperationException("Storage has not been initialized. Call InitializeAsync first.");
        }
    }
}
