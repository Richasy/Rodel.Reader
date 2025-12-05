// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Rss.Abstractions.Test.Unit;

/// <summary>
/// 模型类单元测试.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class ModelTests
{
    [TestMethod]
    public void RssFeed_Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new RssFeed
        {
            Id = "feed-1",
            Name = "Test Feed",
            Url = "https://example.com/feed.xml",
            Website = "https://example.com",
            Description = "A test feed",
            GroupIds = "group-1,group-2",
        };

        // Act
        var clone = original.Clone();
        clone.Name = "Modified Feed";

        // Assert
        Assert.AreNotSame(original, clone);
        Assert.AreEqual("Test Feed", original.Name);
        Assert.AreEqual("Modified Feed", clone.Name);
    }

    [TestMethod]
    public void RssFeed_GetGroupIdList_ReturnsCorrectList()
    {
        // Arrange
        var feed = new RssFeed { GroupIds = "group-1,group-2,group-3" };

        // Act
        var groupIds = feed.GetGroupIdList();

        // Assert
        Assert.AreEqual(3, groupIds.Count);
        Assert.AreEqual("group-1", groupIds[0]);
        Assert.AreEqual("group-2", groupIds[1]);
        Assert.AreEqual("group-3", groupIds[2]);
    }

    [TestMethod]
    public void RssFeed_GetGroupIdList_WithEmptyString_ReturnsEmptyList()
    {
        // Arrange
        var feed = new RssFeed { GroupIds = string.Empty };

        // Act
        var groupIds = feed.GetGroupIdList();

        // Assert
        Assert.AreEqual(0, groupIds.Count);
    }

    [TestMethod]
    public void RssFeed_SetGroupIdList_SetsCorrectString()
    {
        // Arrange
        var feed = new RssFeed();
        var ids = new[] { "group-1", "group-2" };

        // Act
        feed.SetGroupIdList(ids);

        // Assert
        Assert.AreEqual("group-1,group-2", feed.GroupIds);
    }

    [TestMethod]
    public void RssFeed_Equals_WithSameId_ReturnsTrue()
    {
        // Arrange
        var feed1 = new RssFeed { Id = "feed-1", Name = "Feed 1" };
        var feed2 = new RssFeed { Id = "feed-1", Name = "Feed 2" };

        // Act & Assert
        Assert.AreEqual(feed1, feed2);
    }

    [TestMethod]
    public void RssFeed_Equals_WithDifferentId_ReturnsFalse()
    {
        // Arrange
        var feed1 = new RssFeed { Id = "feed-1" };
        var feed2 = new RssFeed { Id = "feed-2" };

        // Act & Assert
        Assert.AreNotEqual(feed1, feed2);
    }

    [TestMethod]
    public void RssFeedGroup_Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new RssFeedGroup { Id = "group-1", Name = "Test Group" };

        // Act
        var clone = original.Clone();
        clone.Name = "Modified Group";

        // Assert
        Assert.AreNotSame(original, clone);
        Assert.AreEqual("Test Group", original.Name);
        Assert.AreEqual("Modified Group", clone.Name);
    }

    [TestMethod]
    public void RssArticle_GetTagList_ReturnsCorrectList()
    {
        // Arrange
        var article = new RssArticle { Tags = "tech,news,programming" };

        // Act
        var tags = article.GetTagList();

        // Assert
        Assert.AreEqual(3, tags.Count);
        Assert.AreEqual("tech", tags[0]);
        Assert.AreEqual("news", tags[1]);
        Assert.AreEqual("programming", tags[2]);
    }

    [TestMethod]
    public void RssArticle_SetTagList_SetsCorrectString()
    {
        // Arrange
        var article = new RssArticle();
        var tags = new[] { "tech", "news" };

        // Act
        article.SetTagList(tags);

        // Assert
        Assert.AreEqual("tech,news", article.Tags);
    }

    [TestMethod]
    public void RssArticle_GetPublishTime_WithValidTime_ReturnsDateTimeOffset()
    {
        // Arrange
        var expectedTime = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var article = new RssArticle();
        article.SetPublishTime(expectedTime);

        // Act
        var actualTime = article.GetPublishTime();

        // Assert
        Assert.IsNotNull(actualTime);
        Assert.AreEqual(expectedTime, actualTime.Value);
    }

    [TestMethod]
    public void RssArticle_GetPublishTime_WithInvalidTime_ReturnsNull()
    {
        // Arrange
        var article = new RssArticle { PublishTime = "invalid-date" };

        // Act
        var time = article.GetPublishTime();

        // Assert
        Assert.IsNull(time);
    }

    [TestMethod]
    public void RssClientOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new RssClientOptions();

        // Assert
        Assert.AreEqual(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.AreEqual(10, options.MaxConcurrentRequests);
    }
}
