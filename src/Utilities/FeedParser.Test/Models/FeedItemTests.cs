// Copyright (c) Reader Copilot. All rights reserved.

namespace FeedParser.Test.Models;

/// <summary>
/// FeedItem 模型测试.
/// </summary>
[TestClass]
public sealed class FeedItemTests
{
    [TestMethod]
    public void FeedItem_RequiredProperties_ShouldBeSet()
    {
        // Arrange & Act
        var item = new FeedItem
        {
            Title = "文章标题",
            Description = "文章描述",
        };

        // Assert
        Assert.AreEqual("文章标题", item.Title);
        Assert.AreEqual("文章描述", item.Description);
    }

    [TestMethod]
    public void FeedItem_OptionalProperties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var item = new FeedItem { Title = "标题" };

        // Assert
        Assert.AreEqual("标题", item.Title);
        Assert.IsNull(item.Id);
        Assert.IsNull(item.Description);
        Assert.IsNull(item.Content);
        Assert.IsNull(item.ImageUrl);
        Assert.IsNull(item.PublishedAt);
        Assert.IsNull(item.UpdatedAt);
        Assert.IsNull(item.Duration);
        Assert.IsNotNull(item.Links);
        Assert.AreEqual(0, item.Links.Count);
        Assert.IsNotNull(item.Categories);
        Assert.AreEqual(0, item.Categories.Count);
        Assert.IsNotNull(item.Contributors);
        Assert.AreEqual(0, item.Contributors.Count);
    }

    [TestMethod]
    public void FeedItem_WithDuration_ShouldStoreDuration()
    {
        // Arrange
        var durationSeconds = 2700; // 45 分钟

        // Act
        var item = new FeedItem
        {
            Title = "播客节目",
            Duration = durationSeconds,
        };

        // Assert
        Assert.AreEqual(durationSeconds, item.Duration);
    }

    [TestMethod]
    public void FeedItem_WithEnclosure_ShouldStoreEnclosure()
    {
        // Arrange & Act
        var item = new FeedItem
        {
            Title = "播客节目",
            Links =
            [
                new FeedLink(
                    new Uri("https://example.com/episode.mp3"),
                    FeedLinkType.Enclosure,
                    null,
                    "audio/mpeg",
                    12345678),
            ],
        };

        // Assert
        var enclosure = item.Links.FirstOrDefault(l => l.LinkType == FeedLinkType.Enclosure);
        Assert.IsNotNull(enclosure);
        Assert.AreEqual("audio/mpeg", enclosure.MediaType);
        Assert.AreEqual(12345678L, enclosure.Length);
    }

    [TestMethod]
    public void FeedItem_GetPrimaryLink_ShouldFindAlternateLink()
    {
        // Arrange
        var item = new FeedItem
        {
            Title = "文章",
            Links =
            [
                new FeedLink(new Uri("https://example.com/audio.mp3"), FeedLinkType.Enclosure),
                new FeedLink(new Uri("https://example.com/article"), FeedLinkType.Alternate),
            ],
        };

        // Act
        var primaryLink = item.GetPrimaryLink();

        // Assert
        Assert.IsNotNull(primaryLink);
        Assert.AreEqual("https://example.com/article", primaryLink.ToString());
    }

    [TestMethod]
    public void FeedItem_WithDateTimes_ShouldStoreDates()
    {
        // Arrange
        var published = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var updated = new DateTimeOffset(2024, 1, 16, 12, 0, 0, TimeSpan.Zero);

        // Act
        var item = new FeedItem
        {
            Title = "文章",
            PublishedAt = published,
            UpdatedAt = updated,
        };

        // Assert
        Assert.AreEqual(published, item.PublishedAt);
        Assert.AreEqual(updated, item.UpdatedAt);
    }
}

