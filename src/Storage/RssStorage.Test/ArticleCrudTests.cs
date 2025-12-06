// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Test;

/// <summary>
/// 文章 CRUD 操作测试.
/// </summary>
[TestClass]
public class ArticleCrudTests
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

        // Add a test feed
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
    public async Task UpsertArticle_NewArticle_ShouldInsert()
    {
        // Arrange
        var article = new RssArticle
        {
            Id = "article1",
            FeedId = "feed1",
            Title = "Test Article",
        };

        // Act
        await _storage.UpsertArticleAsync(article);
        var retrieved = await _storage.GetArticleAsync("article1");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("article1", retrieved.Id);
        Assert.AreEqual("Test Article", retrieved.Title);
    }

    [TestMethod]
    public async Task UpsertArticle_ExistingArticle_ShouldUpdate()
    {
        // Arrange
        var article = new RssArticle
        {
            Id = "article1",
            FeedId = "feed1",
            Title = "Test Article",
        };
        await _storage.UpsertArticleAsync(article);

        // Act
        article.Title = "Updated Article";
        await _storage.UpsertArticleAsync(article);
        var retrieved = await _storage.GetArticleAsync("article1");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Updated Article", retrieved.Title);
    }

    [TestMethod]
    public async Task UpsertArticles_BatchInsert_ShouldInsertAll()
    {
        // Arrange
        var articles = new[]
        {
            new RssArticle { Id = "a1", FeedId = "feed1", Title = "Article 1" },
            new RssArticle { Id = "a2", FeedId = "feed1", Title = "Article 2" },
            new RssArticle { Id = "a3", FeedId = "feed1", Title = "Article 3" },
        };

        // Act
        await _storage.UpsertArticlesAsync(articles);
        var allArticles = await _storage.GetArticlesByFeedAsync("feed1");

        // Assert
        Assert.AreEqual(3, allArticles.Count);
    }

    [TestMethod]
    public async Task GetArticlesByFeed_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var articles = Enumerable.Range(1, 10)
            .Select(i => new RssArticle
            {
                Id = $"a{i}",
                FeedId = "feed1",
                Title = $"Article {i}",
                PublishTime = DateTimeOffset.UtcNow.AddHours(-i).ToString("O"),
            })
            .ToArray();
        await _storage.UpsertArticlesAsync(articles);

        // Act
        var page1 = await _storage.GetArticlesByFeedAsync("feed1", limit: 3, offset: 0);
        var page2 = await _storage.GetArticlesByFeedAsync("feed1", limit: 3, offset: 3);

        // Assert
        Assert.AreEqual(3, page1.Count);
        Assert.AreEqual(3, page2.Count);
        Assert.AreNotEqual(page1[0].Id, page2[0].Id);
    }

    [TestMethod]
    public async Task GetArticleById_NonExistent_ShouldReturnNull()
    {
        // Act
        var article = await _storage.GetArticleAsync("nonexistent");

        // Assert
        Assert.IsNull(article);
    }

    [TestMethod]
    public async Task GetArticleContent_ShouldReturnContent()
    {
        // Arrange
        var article = new RssArticle
        {
            Id = "article1",
            FeedId = "feed1",
            Title = "Test Article",
            Content = "<html><body><p>Full content here</p></body></html>",
        };
        await _storage.UpsertArticleAsync(article);

        // Act
        var content = await _storage.GetArticleContentAsync("article1");

        // Assert
        Assert.AreEqual("<html><body><p>Full content here</p></body></html>", content);
    }

    [TestMethod]
    public async Task DeleteArticle_ExistingArticle_ShouldReturnTrue()
    {
        // Arrange
        await _storage.UpsertArticleAsync(new RssArticle { Id = "a1", FeedId = "feed1", Title = "Article" });

        // Act
        var result = await _storage.DeleteArticleAsync("a1");
        var article = await _storage.GetArticleAsync("a1");

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(article);
    }

    [TestMethod]
    public async Task DeleteArticle_NonExistent_ShouldReturnFalse()
    {
        // Act
        var result = await _storage.DeleteArticleAsync("nonexistent");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task DeleteArticlesByFeed_ShouldDeleteAllArticlesInFeed()
    {
        // Arrange
        await _storage.UpsertArticlesAsync(new[]
        {
            new RssArticle { Id = "a1", FeedId = "feed1", Title = "Article 1" },
            new RssArticle { Id = "a2", FeedId = "feed1", Title = "Article 2" },
        });

        // Act
        var count = await _storage.DeleteArticlesByFeedAsync("feed1");
        var articles = await _storage.GetArticlesByFeedAsync("feed1");

        // Assert
        Assert.AreEqual(2, count);
        Assert.AreEqual(0, articles.Count);
    }

    [TestMethod]
    public async Task Article_AllProperties_ShouldPersist()
    {
        // Arrange
        var article = new RssArticle
        {
            Id = "article1",
            FeedId = "feed1",
            Title = "Test Article",
            Summary = "A summary",
            Content = "<p>Full content</p>",
            CoverUrl = "https://example.com/cover.jpg",
            Url = "https://example.com/article/1",
            Author = "John Doe",
            PublishTime = "2023-01-15T12:00:00Z",
            Tags = "tech,news",
            ExtraData = "{\"key\":\"value\"}",
        };

        // Act
        await _storage.UpsertArticleAsync(article);
        var retrieved = await _storage.GetArticleAsync("article1");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("A summary", retrieved.Summary);
        Assert.AreEqual("<p>Full content</p>", retrieved.Content);
        Assert.AreEqual("https://example.com/cover.jpg", retrieved.CoverUrl);
        Assert.AreEqual("https://example.com/article/1", retrieved.Url);
        Assert.AreEqual("John Doe", retrieved.Author);
        Assert.AreEqual("2023-01-15T12:00:00Z", retrieved.PublishTime);
        Assert.AreEqual("tech,news", retrieved.Tags);
        Assert.AreEqual("{\"key\":\"value\"}", retrieved.ExtraData);
    }
}
