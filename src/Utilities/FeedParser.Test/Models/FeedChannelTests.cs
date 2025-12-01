// Copyright (c) Reader Copilot. All rights reserved.

namespace FeedParser.Test.Models;

/// <summary>
/// FeedChannel 模型测试.
/// </summary>
[TestClass]
public sealed class FeedChannelTests
{
    [TestMethod]
    public void FeedChannel_RequiredProperties_ShouldBeSet()
    {
        // Arrange & Act
        var channel = new FeedChannel
        {
            Title = "测试频道",
            Description = "频道描述",
        };

        // Assert
        Assert.AreEqual("测试频道", channel.Title);
        Assert.AreEqual("频道描述", channel.Description);
    }

    [TestMethod]
    public void FeedChannel_OptionalProperties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var channel = new FeedChannel { Title = "频道" };

        // Assert
        Assert.AreEqual("频道", channel.Title);
        Assert.IsNull(channel.Description);
        Assert.IsNull(channel.Id);
        Assert.IsNull(channel.Language);
        Assert.IsNull(channel.Copyright);
        Assert.IsNull(channel.Generator);
        Assert.IsNull(channel.LastBuildDate);
        Assert.IsNull(channel.PublishedAt);
        Assert.IsNotNull(channel.Links);
        Assert.AreEqual(0, channel.Links.Count);
        Assert.IsNotNull(channel.Categories);
        Assert.AreEqual(0, channel.Categories.Count);
        Assert.IsNotNull(channel.Contributors);
        Assert.AreEqual(0, channel.Contributors.Count);
        Assert.IsNotNull(channel.Images);
        Assert.AreEqual(0, channel.Images.Count);
    }

    [TestMethod]
    public void FeedChannel_WithCollections_ShouldStoreValues()
    {
        // Arrange & Act
        var channel = new FeedChannel
        {
            Title = "频道",
            Links =
            [
                new FeedLink(new Uri("https://example.com"), FeedLinkType.Alternate),
            ],
            Categories = [new FeedCategory("技术")],
            Contributors = [new FeedPerson("作者", FeedPersonType.Author)],
            Images = [new FeedImage(new Uri("https://example.com/logo.png"), FeedImageType.Logo)],
        };

        // Assert
        Assert.AreEqual(1, channel.Links.Count);
        Assert.AreEqual(1, channel.Categories.Count);
        Assert.AreEqual(1, channel.Contributors.Count);
        Assert.AreEqual(1, channel.Images.Count);
    }

    [TestMethod]
    public void FeedChannel_GetPrimaryLink_ShouldFindAlternateLink()
    {
        // Arrange
        var channel = new FeedChannel
        {
            Title = "频道",
            Links =
            [
                new FeedLink(new Uri("https://example.com/feed.xml"), FeedLinkType.Self),
                new FeedLink(new Uri("https://example.com"), FeedLinkType.Alternate),
            ],
        };

        // Act
        var primaryLink = channel.GetPrimaryLink();

        // Assert
        Assert.IsNotNull(primaryLink);
        Assert.AreEqual("https://example.com/", primaryLink.ToString());
    }

    [TestMethod]
    public void FeedChannel_GetPrimaryLink_NoAlternate_ShouldFallbackToFirstLink()
    {
        // Arrange
        var channel = new FeedChannel
        {
            Title = "频道",
            Links = [new FeedLink(new Uri("https://example.com/feed.xml"), FeedLinkType.Self)],
        };

        // Act - 没有Alternate链接时，应fallback到第一个链接
        var primaryLink = channel.GetPrimaryLink();

        // Assert
        Assert.IsNotNull(primaryLink);
        Assert.AreEqual("https://example.com/feed.xml", primaryLink.ToString());
    }

    [TestMethod]
    public void FeedChannel_GetPrimaryLink_NoLinks_ShouldReturnNull()
    {
        // Arrange
        var channel = new FeedChannel
        {
            Title = "频道",
            Links = [],
        };

        // Act
        var primaryLink = channel.GetPrimaryLink();

        // Assert
        Assert.IsNull(primaryLink);
    }
}

