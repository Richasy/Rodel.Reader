// Copyright (c) Richasy. All rights reserved.

namespace RssSource.Local.Test.Integration;

/// <summary>
/// 文章已读状态和收藏管理集成测试.
/// </summary>
[TestClass]
[TestCategory("Network")]
public sealed class ArticleStatusTests : IntegrationTestBase
{
    [TestInitialize]
    public async Task Setup()
    {
        await InitializeAsync();
    }

    [TestCleanup]
    public new void Dispose()
    {
        base.Dispose();
    }

    [TestMethod]
    public async Task MarkArticlesAsRead_ShouldUpdateReadStatus()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(IthomeFeed);
        var detail = await Client.GetFeedDetailAsync(feed!);
        Assert.IsNotNull(detail);
        Assert.IsTrue(detail.Articles.Count > 0);

        // 保存文章到存储
        foreach (var article in detail.Articles.Take(3))
        {
            await Storage.UpsertArticleAsync(article);
        }

        var articleIds = detail.Articles.Take(3).Select(a => a.Id).ToList();

        // Act
        var result = await Client.MarkArticlesAsReadAsync(articleIds);

        // Assert
        Assert.IsTrue(result);

        // 验证文章已标记为已读
        foreach (var articleId in articleIds)
        {
            var isRead = await Storage.IsArticleReadAsync(articleId);
            Assert.IsTrue(isRead, $"文章 {articleId} 应已被标记为已读");
        }
    }

    [TestMethod]
    public async Task MarkFeedAsRead_ShouldMarkAllFeedArticlesAsRead()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(IthomeFeed);
        var detail = await Client.GetFeedDetailAsync(feed!);
        Assert.IsNotNull(detail);
        Assert.IsTrue(detail.Articles.Count > 0);

        // 保存文章到存储
        foreach (var article in detail.Articles)
        {
            await Storage.UpsertArticleAsync(article);
        }

        // Act
        var result = await Client.MarkFeedAsReadAsync(feed!);

        // Assert
        Assert.IsTrue(result);

        // 验证所有文章已标记为已读
        var unreadArticles = await Storage.GetUnreadArticlesAsync(feed!.Id);
        Assert.AreEqual(0, unreadArticles.Count, "订阅源下不应有未读文章");
    }

    [TestMethod]
    public async Task MarkGroupAsRead_ShouldMarkAllGroupArticlesAsRead()
    {
        // Arrange
        var group = await Client.AddGroupAsync(new RssFeedGroup { Name = "测试分组" });

        var feed1 = new RssFeed
        {
            Name = "IT之家",
            Url = "https://www.ithome.com/rss",
            GroupIds = group!.Id,
        };
        var feed2 = new RssFeed
        {
            Name = ".NET Blog",
            Url = "https://devblogs.microsoft.com/dotnet/feed/",
            GroupIds = group.Id,
        };

        var addedFeed1 = await Client.AddFeedAsync(feed1);
        var addedFeed2 = await Client.AddFeedAsync(feed2);

        // 获取文章并保存
        var detail1 = await Client.GetFeedDetailAsync(addedFeed1!);
        var detail2 = await Client.GetFeedDetailAsync(addedFeed2!);

        if (detail1 != null)
        {
            foreach (var article in detail1.Articles.Take(5))
            {
                await Storage.UpsertArticleAsync(article);
            }
        }

        if (detail2 != null)
        {
            foreach (var article in detail2.Articles.Take(5))
            {
                await Storage.UpsertArticleAsync(article);
            }
        }

        // Act
        var result = await Client.MarkGroupAsReadAsync(group);

        // Assert
        Assert.IsTrue(result);

        // 验证两个订阅源的文章都已标记为已读
        var unread1 = await Storage.GetUnreadArticlesAsync(addedFeed1!.Id);
        var unread2 = await Storage.GetUnreadArticlesAsync(addedFeed2!.Id);

        Assert.AreEqual(0, unread1.Count, "IT之家不应有未读文章");
        Assert.AreEqual(0, unread2.Count, ".NET Blog 不应有未读文章");
    }

    [TestMethod]
    public async Task StorageFavorite_AddAndRemove_ShouldWork()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(IthomeFeed);
        var detail = await Client.GetFeedDetailAsync(feed!);
        Assert.IsNotNull(detail);
        Assert.IsTrue(detail.Articles.Count > 0);

        var article = detail.Articles[0];
        await Storage.UpsertArticleAsync(article);

        // Act - 添加收藏
        await Storage.AddFavoriteAsync(article.Id);
        var isFavorite1 = await Storage.IsArticleFavoriteAsync(article.Id);

        // Assert
        Assert.IsTrue(isFavorite1, "文章应已收藏");

        // Act - 移除收藏
        await Storage.RemoveFavoriteAsync(article.Id);
        var isFavorite2 = await Storage.IsArticleFavoriteAsync(article.Id);

        // Assert
        Assert.IsFalse(isFavorite2, "文章收藏应已移除");
    }

    [TestMethod]
    public async Task GetFavoriteArticles_ShouldReturnOnlyFavorites()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(IthomeFeed);
        var detail = await Client.GetFeedDetailAsync(feed!);
        Assert.IsNotNull(detail);
        Assert.IsTrue(detail.Articles.Count >= 3);

        // 保存前3篇文章
        for (var i = 0; i < 3; i++)
        {
            await Storage.UpsertArticleAsync(detail.Articles[i]);
        }

        // 只收藏第1和第3篇
        await Storage.AddFavoriteAsync(detail.Articles[0].Id);
        await Storage.AddFavoriteAsync(detail.Articles[2].Id);

        // Act
        var favorites = await Storage.GetFavoriteArticlesAsync();

        // Assert
        Assert.AreEqual(2, favorites.Count, "应有2篇收藏文章");
    }

    [TestMethod]
    public async Task GetUnreadArticles_ShouldExcludeReadArticles()
    {
        // Arrange
        var feed = await Client.AddFeedAsync(IthomeFeed);
        var detail = await Client.GetFeedDetailAsync(feed!);
        Assert.IsNotNull(detail);
        Assert.IsTrue(detail.Articles.Count >= 5);

        // 保存前5篇文章
        for (var i = 0; i < 5; i++)
        {
            await Storage.UpsertArticleAsync(detail.Articles[i]);
        }

        // 标记前2篇为已读
        await Storage.MarkAsReadAsync(detail.Articles.Take(2).Select(a => a.Id));

        // Act
        var unread = await Storage.GetUnreadArticlesAsync(feed!.Id);

        // Assert
        Assert.AreEqual(3, unread.Count, "应有3篇未读文章");
    }
}
