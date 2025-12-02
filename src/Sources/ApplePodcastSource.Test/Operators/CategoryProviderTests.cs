// Copyright (c) Richasy. All rights reserved.

using RichardSzalay.MockHttp;
using Richasy.RodelReader.Sources.ApplePodcast.Internal;

namespace Richasy.RodelReader.Sources.ApplePodcast.Test.Operators;

[TestClass]
public sealed class CategoryProviderTests
{
    private readonly ILogger<ApplePodcastClient> _logger = NullLogger<ApplePodcastClient>.Instance;

    [TestMethod]
    public void GetCategories_ReturnsAllPredefinedCategories()
    {
        // Arrange
        var options = new ApplePodcastClientOptions();
        var mockDispatcher = new Mock<IPodcastDispatcher>(MockBehavior.Strict);
        var provider = new CategoryProvider(mockDispatcher.Object, options, _logger);

        // Act
        var categories = provider.GetCategories();

        // Assert
        Assert.IsNotNull(categories);
        Assert.IsTrue(categories.Count > 0);
        Assert.IsTrue(categories.Any(c => c.Id == "0")); // All
        Assert.IsTrue(categories.Any(c => c.Id == "1318")); // Technology
    }

    [TestMethod]
    public async Task GetTopPodcastsAsync_ReturnsResults()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://itunes.apple.com/us/rss/toppodcasts/limit=100/genre=0/json")
            .Respond("application/json", TestDataFactory.TopPodcastsResponse);

        var httpClient = mockHandler.ToHttpClient();
        var options = new ApplePodcastClientOptions();

        using var client = new ApplePodcastClient(options, httpClient, _logger);

        // Act
        var podcasts = await client.Categories.GetTopPodcastsAsync("0");

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.AreEqual(2, podcasts.Count);
        Assert.AreEqual("12345", podcasts[0].Id);
        Assert.AreEqual("Test Podcast 1", podcasts[0].Name);
        Assert.AreEqual("Test Artist", podcasts[0].Artist);
    }

    [TestMethod]
    public async Task GetTopPodcastsAsync_WithRegion_UsesCorrectUrl()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://itunes.apple.com/cn/rss/toppodcasts/limit=50/genre=1318/json")
            .Respond("application/json", TestDataFactory.TopPodcastsResponse);

        var httpClient = mockHandler.ToHttpClient();
        var options = new ApplePodcastClientOptions { DefaultRegion = "us", DefaultLimit = 100 };

        using var client = new ApplePodcastClient(options, httpClient, _logger);

        // Act
        var podcasts = await client.Categories.GetTopPodcastsAsync("1318", "cn", 50);

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.AreEqual(2, podcasts.Count);
    }

    [TestMethod]
    public async Task GetTopPodcastsAsync_EmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://itunes.apple.com/us/rss/toppodcasts/limit=100/genre=0/json")
            .Respond("application/json", TestDataFactory.EmptyTopPodcastsResponse);

        var httpClient = mockHandler.ToHttpClient();
        var options = new ApplePodcastClientOptions();

        using var client = new ApplePodcastClient(options, httpClient, _logger);

        // Act
        var podcasts = await client.Categories.GetTopPodcastsAsync("0");

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.AreEqual(0, podcasts.Count);
    }

    [TestMethod]
    public void GetCategories_IncludesKnownCategoryIds()
    {
        // Arrange
        var options = new ApplePodcastClientOptions();
        var mockDispatcher = new Mock<IPodcastDispatcher>(MockBehavior.Strict);
        var provider = new CategoryProvider(mockDispatcher.Object, options, _logger);

        // Act
        var categories = provider.GetCategories();

        // Assert - 验证一些已知的分类 ID
        var knownIds = new[] { "0", "1309", "1305", "1512", "1304", "1533", "1318", "1321", "1301", "1303" };
        foreach (var id in knownIds)
        {
            Assert.IsTrue(categories.Any(c => c.Id == id), $"Missing category ID: {id}");
        }
    }
}
