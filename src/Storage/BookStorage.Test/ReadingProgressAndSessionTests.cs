// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Book.Test;

/// <summary>
/// 阅读进度和阅读会话测试.
/// </summary>
[TestClass]
public class ReadingProgressAndSessionTests
{
    private string _testDbPath = null!;
    private BookStorage _storage = null!;
    private Book _testBook = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"book_test_{Guid.NewGuid()}.db");
        var options = new BookStorageOptions
        {
            DatabasePath = _testDbPath,
            CreateTablesOnInit = true,
        };
        _storage = new BookStorage(options);
        await _storage.InitializeAsync();

        // 创建测试书籍
        _testBook = new Book
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "Test Book",
            Format = BookFormat.Epub,
            SourceType = BookSourceType.Local,
            AddedAt = DateTimeOffset.UtcNow,
        };
        await _storage.UpsertBookAsync(_testBook);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _storage.DisposeAsync();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    #region ReadProgress Tests

    [TestMethod]
    public async Task UpsertReadProgress_Insert_ShouldSucceed()
    {
        // Arrange
        var progress = new ReadProgress
        {
            BookId = _testBook.Id,
            Progress = 0.5,
            Position = "chapter-1",
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        // Act
        await _storage.UpsertReadProgressAsync(progress);
        var retrieved = await _storage.GetReadProgressAsync(_testBook.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(0.5, retrieved.Progress);
        Assert.AreEqual("chapter-1", retrieved.Position);
    }

    [TestMethod]
    public async Task UpsertReadProgress_Update_ShouldSucceed()
    {
        // Arrange
        var progress = new ReadProgress
        {
            BookId = _testBook.Id,
            Progress = 0.3,
            Position = "chapter-1",
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        await _storage.UpsertReadProgressAsync(progress);

        // Act
        progress.Progress = 0.7;
        progress.Position = "chapter-3";
        await _storage.UpsertReadProgressAsync(progress);
        var retrieved = await _storage.GetReadProgressAsync(_testBook.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(0.7, retrieved.Progress);
        Assert.AreEqual("chapter-3", retrieved.Position);
    }

    [TestMethod]
    public async Task GetReadProgress_NonExistent_ShouldReturnNull()
    {
        // Act
        var progress = await _storage.GetReadProgressAsync("non-existent-book-id");

        // Assert
        Assert.IsNull(progress);
    }

    #endregion

    #region ReadingSession Tests

    [TestMethod]
    public async Task AddReadingSession_Insert_ShouldSucceed()
    {
        // Arrange
        var session = CreateTestSession();

        // Act
        await _storage.AddReadingSessionAsync(session);
        var sessions = await _storage.GetReadingSessionsAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(1, sessions.Count);
        Assert.AreEqual(session.Id, sessions[0].Id);
        Assert.AreEqual(300, sessions[0].DurationSeconds);
    }

    [TestMethod]
    public async Task GetReadingSessions_ShouldReturnOrderedByStartedAtDesc()
    {
        // Arrange
        var session1 = CreateTestSession("session1");
        session1.StartedAt = DateTimeOffset.UtcNow.AddHours(-2);

        var session2 = CreateTestSession("session2");
        session2.StartedAt = DateTimeOffset.UtcNow.AddHours(-1);

        var session3 = CreateTestSession("session3");
        session3.StartedAt = DateTimeOffset.UtcNow;

        await _storage.AddReadingSessionAsync(session1);
        await _storage.AddReadingSessionAsync(session2);
        await _storage.AddReadingSessionAsync(session3);

        // Act
        var sessions = await _storage.GetReadingSessionsAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(3, sessions.Count);
        Assert.AreEqual(session3.Id, sessions[0].Id); // 最新的在前
        Assert.AreEqual(session2.Id, sessions[1].Id);
        Assert.AreEqual(session1.Id, sessions[2].Id);
    }

    [TestMethod]
    public async Task GetReadingStats_ShouldCalculateCorrectly()
    {
        // Arrange
        var session1 = CreateTestSession("session1");
        session1.DurationSeconds = 3600; // 1 hour
        session1.PagesRead = 30;
        session1.StartedAt = DateTimeOffset.UtcNow.AddDays(-2);

        var session2 = CreateTestSession("session2");
        session2.DurationSeconds = 1800; // 30 minutes
        session2.PagesRead = 15;
        session2.StartedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var session3 = CreateTestSession("session3");
        session3.DurationSeconds = 2700; // 45 minutes
        session3.PagesRead = 20;
        session3.StartedAt = DateTimeOffset.UtcNow;

        await _storage.AddReadingSessionAsync(session1);
        await _storage.AddReadingSessionAsync(session2);
        await _storage.AddReadingSessionAsync(session3);

        // Act
        var stats = await _storage.GetReadingStatsAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(_testBook.Id, stats.BookId);
        Assert.AreEqual(TimeSpan.FromSeconds(8100), stats.TotalReadingTime); // 3600 + 1800 + 2700
        Assert.AreEqual(3, stats.TotalSessionCount);
        Assert.AreEqual(3, stats.ReadingDays);
    }

    [TestMethod]
    public async Task GetReadingStats_NoSessions_ShouldReturnEmptyStats()
    {
        // Act
        var stats = await _storage.GetReadingStatsAsync(_testBook.Id);

        // Assert
        Assert.AreEqual(_testBook.Id, stats.BookId);
        Assert.AreEqual(TimeSpan.Zero, stats.TotalReadingTime);
        Assert.AreEqual(0, stats.TotalSessionCount);
    }

    #endregion

    private ReadingSession CreateTestSession(string? id = null)
    {
        return new ReadingSession
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            BookId = _testBook.Id,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            EndedAt = DateTimeOffset.UtcNow,
            DurationSeconds = 300,
        };
    }
}
