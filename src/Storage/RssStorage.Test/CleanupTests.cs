// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Test;

/// <summary>
/// 清理功能测试.
/// </summary>
[TestClass]
public class CleanupTests
{
    private string _testDbPath = null!;
    private RssStorage _storage = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"rss_test_{Guid.NewGuid()}.db");
        var options = new RssStorageOptions { DatabasePath = _testDbPath, DefaultArticleRetentionDays = 7 };
        _storage = new RssStorage(options);
        await _storage.InitializeAsync();

        await _storage.UpsertFeedAsync(new RssFeed { Id = "feed1", Name = "Test Feed", Url = "https://example.com" });
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

    [TestMethod]
    public async Task CleanupOldArticles_ShouldDeleteOldArticles()
    {
        // Arrange - insert articles with old cached dates
        // Note: The CachedAt is set automatically on upsert, so we need to test with the current time
        await _storage.UpsertArticlesAsync(new[]
        {
            new RssArticle { Id = "a1", FeedId = "feed1", Title = "Article 1" },
            new RssArticle { Id = "a2", FeedId = "feed1", Title = "Article 2" },
        });

        // Act - cleanup with a future date (will delete all)
        var deleted = await _storage.CleanupOldArticlesAsync(DateTimeOffset.UtcNow.AddDays(1));

        // Assert
        Assert.AreEqual(2, deleted);
        var articles = await _storage.GetArticlesByFeedAsync("feed1");
        Assert.AreEqual(0, articles.Count);
    }

    [TestMethod]
    public async Task CleanupOldArticles_WithKeepFavorites_ShouldKeepFavorites()
    {
        // Arrange
        await _storage.UpsertArticlesAsync(new[]
        {
            new RssArticle { Id = "a1", FeedId = "feed1", Title = "Article 1" },
            new RssArticle { Id = "a2", FeedId = "feed1", Title = "Article 2" },
        });
        await _storage.AddFavoriteAsync("a1");

        // Act
        var deleted = await _storage.CleanupOldArticlesAsync(DateTimeOffset.UtcNow.AddDays(1), keepFavorites: true);

        // Assert
        Assert.AreEqual(1, deleted);
        var article = await _storage.GetArticleAsync("a1");
        Assert.IsNotNull(article);
    }

    [TestMethod]
    public async Task CleanupOldArticles_WithoutKeepFavorites_ShouldDeleteFavorites()
    {
        // Arrange
        await _storage.UpsertArticlesAsync(new[]
        {
            new RssArticle { Id = "a1", FeedId = "feed1", Title = "Article 1" },
        });
        await _storage.AddFavoriteAsync("a1");

        // Act
        var deleted = await _storage.CleanupOldArticlesAsync(DateTimeOffset.UtcNow.AddDays(1), keepFavorites: false);

        // Assert
        Assert.AreEqual(1, deleted);
        var article = await _storage.GetArticleAsync("a1");
        Assert.IsNull(article);
    }
}
