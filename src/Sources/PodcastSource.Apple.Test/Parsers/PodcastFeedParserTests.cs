// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.Podcast.Apple.Internal;

namespace Richasy.RodelReader.Sources.Podcast.Apple.Test.Parsers;

[TestClass]
public sealed class PodcastFeedParserTests
{
    private readonly ILogger _logger = NullLogger.Instance;

    [TestMethod]
    public async Task ParseAsync_ValidFeed_ReturnsPodcastDetail()
    {
        // Arrange
        var parser = new PodcastFeedParser(_logger);

        // Act
        var result = await parser.ParseAsync(TestDataFactory.SimpleFeed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Podcast", result.Name);
        Assert.AreEqual("A test podcast for unit testing", result.Description);
        Assert.AreEqual("Test Author", result.Author);
        Assert.AreEqual("https://example.com/cover.jpg", result.Cover);
        Assert.AreEqual("https://example.com", result.Website);
    }

    [TestMethod]
    public async Task ParseAsync_ValidFeed_ParsesEpisodes()
    {
        // Arrange
        var parser = new PodcastFeedParser(_logger);

        // Act
        var result = await parser.ParseAsync(TestDataFactory.SimpleFeed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Episodes.Count);

        var episode1 = result.Episodes[0];
        Assert.AreEqual("ep-001", episode1.Id);
        Assert.AreEqual("Episode 1", episode1.Title);
        Assert.AreEqual("First episode description", episode1.Description);
        Assert.AreEqual("https://example.com/ep1.mp3", episode1.AudioUrl);
        Assert.AreEqual("audio/mpeg", episode1.AudioMimeType);
        Assert.AreEqual(12345678, episode1.FileSizeInBytes);
        Assert.AreEqual(5445, episode1.DurationInSeconds); // 1:30:45 = 5445 seconds
        Assert.AreEqual(1, episode1.Season);
        Assert.AreEqual(1, episode1.Episode);
        Assert.AreEqual("full", episode1.EpisodeType);
    }

    [TestMethod]
    public async Task ParseAsync_FeedWithCategories_ParsesCategories()
    {
        // Arrange
        var parser = new PodcastFeedParser(_logger);

        // Act
        var result = await parser.ParseAsync(TestDataFactory.SimpleFeed);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.CategoryIds.Count > 0);
        Assert.IsTrue(result.CategoryIds.Contains("Technology"));
    }

    [TestMethod]
    public async Task ParseAsync_DurationInSeconds_ParsesCorrectly()
    {
        // Arrange
        var parser = new PodcastFeedParser(_logger);
        var feedWithSecondsDuration = TestDataFactory.SimpleFeed.Replace("1:30:45", "3600", StringComparison.Ordinal);

        // Act
        var result = await parser.ParseAsync(feedWithSecondsDuration);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3600, result.Episodes[0].DurationInSeconds);
    }

    [TestMethod]
    public async Task ParseAsync_DurationInMinutesSeconds_ParsesCorrectly()
    {
        // Arrange
        var parser = new PodcastFeedParser(_logger);

        // Act
        var result = await parser.ParseAsync(TestDataFactory.SimpleFeed);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2730, result.Episodes[1].DurationInSeconds); // 45:30 = 2730 seconds
    }

    [TestMethod]
    public async Task ParseAsync_Stream_Works()
    {
        // Arrange
        var parser = new PodcastFeedParser(_logger);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(TestDataFactory.SimpleFeed));

        // Act
        var result = await parser.ParseAsync(stream);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Podcast", result.Name);
    }

    [TestMethod]
    public async Task ParseAsync_NoChannel_ReturnsNull()
    {
        // Arrange
        var parser = new PodcastFeedParser(_logger);
        var noChannelFeed = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"></rss>";

        // Act
        var result = await parser.ParseAsync(noChannelFeed);

        // Assert
        Assert.IsNull(result);
    }
}
