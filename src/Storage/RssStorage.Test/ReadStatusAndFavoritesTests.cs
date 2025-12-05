// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Test;

/// <summary>
/// 阅读状态和收藏功能测试.
/// </summary>
[TestClass]
public class ReadStatusAndFavoritesTests
{
    private string _testDbPath = null!;
    private RssStorage _storage = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"rss_test_{Guid.NewGuid()}.db");
        var options = new RssStorageOptions { DatabasePath = _testDbPath };
        _storage = new RssStorage(options);
        await _storage.InitializeAsync();

        // Add test feed and articles
        await _storage.UpsertFeedAsync(new RssFeed { Id = "feed1", Name = "Test Feed", Url = "https://example.com" });
        await _storage.UpsertArticlesAsync(new[]
        {
            new RssArticle { Id = "a1", FeedId = "feed1", Title = "Article 1", PublishTime = DateTimeOffset.UtcNow.AddHours(-1).ToString("O") },
            new RssArticle { Id = "a2", FeedId = "feed1", Title = "Article 2", PublishTime = DateTimeOffset.UtcNow.AddHours(-2).ToString("O") },
            new RssArticle { Id = "a3", FeedId = "feed1", Title = "Article 3", PublishTime = DateTimeOffset.UtcNow.AddHours(-3).ToString("O") },
        });
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

    #region Read Status

    [TestMethod]
    public async Task MarkAsRead_ShouldMarkArticleAsRead()
    {
        // Act
        await _storage.MarkAsReadAsync(new[] { "a1" });
        var isRead = await _storage.IsArticleReadAsync("a1");

        // Assert
        Assert.IsTrue(isRead);
    }

    [TestMethod]
    public async Task MarkAsUnread_ShouldMarkArticleAsUnread()
    {
        // Arrange
        await _storage.MarkAsReadAsync(new[] { "a1" });

        // Act
        await _storage.MarkAsUnreadAsync(new[] { "a1" });
        var isRead = await _storage.IsArticleReadAsync("a1");

        // Assert
        Assert.IsFalse(isRead);
    }

    [TestMethod]
    public async Task IsArticleRead_NewArticle_ShouldReturnFalse()
    {
        // Act
        var isRead = await _storage.IsArticleReadAsync("a1");

        // Assert
        Assert.IsFalse(isRead);
    }

    [TestMethod]
    public async Task MarkFeedAsRead_ShouldMarkAllArticlesInFeed()
    {
        // Act
        await _storage.MarkFeedAsReadAsync("feed1");

        // Assert
        Assert.IsTrue(await _storage.IsArticleReadAsync("a1"));
        Assert.IsTrue(await _storage.IsArticleReadAsync("a2"));
        Assert.IsTrue(await _storage.IsArticleReadAsync("a3"));
    }

    [TestMethod]
    public async Task MarkAllAsRead_ShouldMarkAllArticles()
    {
        // Arrange
        await _storage.UpsertFeedAsync(new RssFeed { Id = "feed2", Name = "Feed 2", Url = "https://b.com" });
        await _storage.UpsertArticleAsync(new RssArticle { Id = "a4", FeedId = "feed2", Title = "Article 4" });

        // Act
        await _storage.MarkAllAsReadAsync();

        // Assert
        Assert.IsTrue(await _storage.IsArticleReadAsync("a1"));
        Assert.IsTrue(await _storage.IsArticleReadAsync("a4"));
    }

    [TestMethod]
    public async Task GetUnreadArticles_ShouldReturnOnlyUnread()
    {
        // Arrange
        await _storage.MarkAsReadAsync(new[] { "a1" });

        // Act
        var unread = await _storage.GetUnreadArticlesAsync();

        // Assert
        Assert.AreEqual(2, unread.Count);
        Assert.IsFalse(unread.Any(a => a.Id == "a1"));
    }

    [TestMethod]
    public async Task GetUnreadArticles_WithFeedFilter_ShouldReturnUnreadInFeed()
    {
        // Arrange
        await _storage.UpsertFeedAsync(new RssFeed { Id = "feed2", Name = "Feed 2", Url = "https://b.com" });
        await _storage.UpsertArticleAsync(new RssArticle { Id = "a4", FeedId = "feed2", Title = "Article 4" });
        await _storage.MarkAsReadAsync(new[] { "a1" });

        // Act
        var unread = await _storage.GetUnreadArticlesAsync(feedId: "feed1");

        // Assert
        Assert.AreEqual(2, unread.Count);
        Assert.IsTrue(unread.All(a => a.FeedId == "feed1"));
    }

    #endregion

    #region Favorites

    [TestMethod]
    public async Task AddFavorite_ShouldAddToFavorites()
    {
        // Act
        await _storage.AddFavoriteAsync("a1");
        var isFavorite = await _storage.IsArticleFavoriteAsync("a1");

        // Assert
        Assert.IsTrue(isFavorite);
    }

    [TestMethod]
    public async Task RemoveFavorite_ShouldRemoveFromFavorites()
    {
        // Arrange
        await _storage.AddFavoriteAsync("a1");

        // Act
        await _storage.RemoveFavoriteAsync("a1");
        var isFavorite = await _storage.IsArticleFavoriteAsync("a1");

        // Assert
        Assert.IsFalse(isFavorite);
    }

    [TestMethod]
    public async Task IsFavorite_NewArticle_ShouldReturnFalse()
    {
        // Act
        var isFavorite = await _storage.IsArticleFavoriteAsync("a1");

        // Assert
        Assert.IsFalse(isFavorite);
    }

    [TestMethod]
    public async Task GetFavoriteArticles_ShouldReturnOnlyFavorites()
    {
        // Arrange
        await _storage.AddFavoriteAsync("a1");
        await _storage.AddFavoriteAsync("a3");

        // Act
        var favorites = await _storage.GetFavoriteArticlesAsync();

        // Assert
        Assert.AreEqual(2, favorites.Count);
        Assert.IsTrue(favorites.Any(a => a.Id == "a1"));
        Assert.IsTrue(favorites.Any(a => a.Id == "a3"));
    }

    [TestMethod]
    public async Task AddFavorite_Duplicate_ShouldNotThrow()
    {
        // Arrange
        await _storage.AddFavoriteAsync("a1");

        // Act & Assert - should not throw
        await _storage.AddFavoriteAsync("a1");
        var isFavorite = await _storage.IsArticleFavoriteAsync("a1");
        Assert.IsTrue(isFavorite);
    }

    #endregion
}
