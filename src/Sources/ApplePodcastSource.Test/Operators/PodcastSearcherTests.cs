// Copyright (c) Richasy. All rights reserved.

using RichardSzalay.MockHttp;

namespace Richasy.RodelReader.Sources.ApplePodcast.Test.Operators;

[TestClass]
public sealed class PodcastSearcherTests
{
    private readonly ILogger<ApplePodcastClient> _logger = NullLogger<ApplePodcastClient>.Instance;

    [TestMethod]
    public async Task SearchAsync_ReturnsResults()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://itunes.apple.com/search?term=test&media=podcast&entity=podcast&limit=100")
            .Respond("application/json", TestDataFactory.SearchResponse);

        var httpClient = mockHandler.ToHttpClient();
        var options = new ApplePodcastClientOptions();

        using var client = new ApplePodcastClient(options, httpClient, _logger);

        // Act
        var podcasts = await client.Search.SearchAsync("test");

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.AreEqual(2, podcasts.Count);
        Assert.AreEqual("111111", podcasts[0].Id);
        Assert.AreEqual("Search Result 1", podcasts[0].Name);
        Assert.AreEqual("Artist One", podcasts[0].Artist);
        Assert.AreEqual("https://example.com/feed1.xml", podcasts[0].FeedUrl);
    }

    [TestMethod]
    public async Task SearchAsync_WithLimit_UsesCorrectUrl()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://itunes.apple.com/search?term=technology&media=podcast&entity=podcast&limit=25")
            .Respond("application/json", TestDataFactory.SearchResponse);

        var httpClient = mockHandler.ToHttpClient();
        var options = new ApplePodcastClientOptions { DefaultLimit = 100 };

        using var client = new ApplePodcastClient(options, httpClient, _logger);

        // Act
        var podcasts = await client.Search.SearchAsync("technology", 25);

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.AreEqual(2, podcasts.Count);
    }

    [TestMethod]
    public async Task SearchAsync_EmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://itunes.apple.com/search*")
            .Respond("application/json", TestDataFactory.EmptySearchResponse);

        var httpClient = mockHandler.ToHttpClient();
        var options = new ApplePodcastClientOptions();

        using var client = new ApplePodcastClient(options, httpClient, _logger);

        // Act
        var podcasts = await client.Search.SearchAsync("nonexistent");

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.AreEqual(0, podcasts.Count);
    }

    [TestMethod]
    public async Task SearchAsync_EncodesSpecialCharacters()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://itunes.apple.com/search?term=hello%20world&media=podcast&entity=podcast&limit=100")
            .Respond("application/json", TestDataFactory.SearchResponse);

        var httpClient = mockHandler.ToHttpClient();
        var options = new ApplePodcastClientOptions();

        using var client = new ApplePodcastClient(options, httpClient, _logger);

        // Act
        var podcasts = await client.Search.SearchAsync("hello world");

        // Assert
        Assert.IsNotNull(podcasts);
    }
}
