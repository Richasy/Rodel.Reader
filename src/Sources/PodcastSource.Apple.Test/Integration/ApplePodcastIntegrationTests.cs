// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Podcast.Apple.Test.Integration;

/// <summary>
/// Apple Podcast 集成测试.
/// 直接访问 iTunes API 验证功能.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public class ApplePodcastIntegrationTests
{
    private ApplePodcastClient? _client;

    [TestInitialize]
    public void Setup()
    {
        var options = new ApplePodcastClientOptions
        {
            DefaultRegion = "us",
            DefaultLimit = 25,
            Timeout = TimeSpan.FromSeconds(30),
        };

        _client = new ApplePodcastClient(options);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
    }

    [TestMethod]
    public void GetCategories_ReturnsAllCategories()
    {
        // Act
        var categories = _client!.Categories.GetCategories();

        // Assert
        Assert.IsNotNull(categories);
        Assert.IsTrue(categories.Count > 0, "Should return at least one category");

        Console.WriteLine($"Total Categories: {categories.Count}");
        foreach (var category in categories)
        {
            Console.WriteLine($"  - {category.Id}: {category.DefaultName}");
        }
    }

    [TestMethod]
    public async Task GetTopPodcasts_US_ReturnsResults()
    {
        // Act
        var podcasts = await _client!.Categories.GetTopPodcastsAsync(
            PodcastCategory.All.Id,
            "us",
            25);

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.IsTrue(podcasts.Count > 0, "Should return at least one podcast");

        Console.WriteLine($"Top Podcasts (US - All): {podcasts.Count}");
        for (var i = 0; i < Math.Min(10, podcasts.Count); i++)
        {
            var podcast = podcasts[i];
            Console.WriteLine($"  - [{podcast.Id}] {podcast.Name}");
            Console.WriteLine($"    Artist: {podcast.Artist}");
            Console.WriteLine($"    Cover: {podcast.Cover?.Substring(0, Math.Min(50, podcast.Cover?.Length ?? 0))}...");
        }
    }

    [TestMethod]
    public async Task GetTopPodcasts_CN_ReturnsResults()
    {
        // Act
        var podcasts = await _client!.Categories.GetTopPodcastsAsync(
            PodcastCategory.All.Id,
            "cn",
            25);

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.IsTrue(podcasts.Count > 0, "Should return at least one podcast for China region");

        Console.WriteLine($"Top Podcasts (CN - All): {podcasts.Count}");
        for (var i = 0; i < Math.Min(10, podcasts.Count); i++)
        {
            Console.WriteLine($"  - [{podcasts[i].Id}] {podcasts[i].Name}");
        }
    }

    [TestMethod]
    public async Task GetTopPodcasts_TechnologyCategory_ReturnsResults()
    {
        // Act
        var podcasts = await _client!.Categories.GetTopPodcastsAsync(
            PodcastCategory.Technology.Id,
            "us",
            25);

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.IsTrue(podcasts.Count > 0, "Should return technology podcasts");

        Console.WriteLine($"Top Technology Podcasts (US): {podcasts.Count}");
        for (var i = 0; i < Math.Min(10, podcasts.Count); i++)
        {
            Console.WriteLine($"  - [{podcasts[i].Id}] {podcasts[i].Name}");
        }
    }

    [TestMethod]
    public async Task Search_ReturnsResults()
    {
        // Arrange
        const string keyword = "technology";

        // Act
        var podcasts = await _client!.Search.SearchAsync(keyword, 25);

        // Assert
        Assert.IsNotNull(podcasts);
        Assert.IsTrue(podcasts.Count > 0, $"Should find podcasts for keyword '{keyword}'");

        Console.WriteLine($"Search Results for '{keyword}': {podcasts.Count}");
        for (var i = 0; i < Math.Min(10, podcasts.Count); i++)
        {
            var podcast = podcasts[i];
            Console.WriteLine($"  - [{podcast.Id}] {podcast.Name}");
            Console.WriteLine($"    Artist: {podcast.Artist}");
            Console.WriteLine($"    Feed URL: {podcast.FeedUrl}");
        }
    }

    [TestMethod]
    public async Task Search_ChineseKeyword_ReturnsResults()
    {
        // Arrange
        const string keyword = "科技";

        // Act
        var podcasts = await _client!.Search.SearchAsync(keyword, 25);

        // Assert
        Assert.IsNotNull(podcasts);
        // Chinese keyword might return fewer results
        Console.WriteLine($"Search Results for '{keyword}': {podcasts.Count}");
        for (var i = 0; i < Math.Min(10, podcasts.Count); i++)
        {
            Console.WriteLine($"  - [{podcasts[i].Id}] {podcasts[i].Name}");
        }
    }

    [TestMethod]
    public async Task GetPodcastDetailById_ReturnsDetailWithEpisodes()
    {
        // Arrange - First search for a podcast to get a valid ID
        var searchResults = await _client!.Search.SearchAsync("Serial", 5);

        if (searchResults.Count == 0)
        {
            Assert.Inconclusive("Could not find a podcast to test detail retrieval");
            return;
        }

        var firstPodcast = searchResults[0];
        Console.WriteLine($"Testing with podcast: {firstPodcast.Name} (ID: {firstPodcast.Id})");

        // Act
        var detail = await _client.Details.GetDetailByIdAsync(firstPodcast.Id);

        // Assert
        Assert.IsNotNull(detail, "Should return podcast detail");
        Assert.IsFalse(string.IsNullOrEmpty(detail.Name), "Podcast should have a name");

        Console.WriteLine($"Podcast Detail:");
        Console.WriteLine($"  Name: {detail.Name}");
        Console.WriteLine($"  Author: {detail.Author}");
        Console.WriteLine($"  Description: {detail.Description?.Substring(0, Math.Min(100, detail.Description?.Length ?? 0))}...");
        Console.WriteLine($"  Website: {detail.Website}");
        Console.WriteLine($"  Feed URL: {detail.FeedUrl}");
        Console.WriteLine($"  Cover: {detail.Cover}");
        Console.WriteLine($"  Episodes: {detail.Episodes.Count}");

        if (detail.Episodes.Count > 0)
        {
            Console.WriteLine($"\nFirst 5 Episodes:");
            for (var i = 0; i < Math.Min(5, detail.Episodes.Count); i++)
            {
                var episode = detail.Episodes[i];
                Console.WriteLine($"  - {episode.Title}");
                Console.WriteLine($"    Duration: {episode.DurationInSeconds}s");
                Console.WriteLine($"    Published: {episode.PublishedDate}");
                Console.WriteLine($"    Audio: {episode.AudioUrl}");
            }
        }
    }

    [TestMethod]
    public async Task GetPodcastDetailByFeedUrl_ReturnsDetailWithEpisodes()
    {
        // Arrange - Use a well-known podcast feed URL
        // The Vergecast - a popular tech podcast
        const string feedUrl = "https://feeds.megaphone.fm/vergecast";

        // Act
        var detail = await _client!.Details.GetDetailByFeedUrlAsync(feedUrl);

        // Assert
        Assert.IsNotNull(detail, "Should return podcast detail from feed URL");
        Assert.IsFalse(string.IsNullOrEmpty(detail.Name), "Podcast should have a name");
        Assert.IsTrue(detail.Episodes.Count > 0, "Podcast should have episodes");

        Console.WriteLine($"Podcast from Feed URL:");
        Console.WriteLine($"  Name: {detail.Name}");
        Console.WriteLine($"  Author: {detail.Author}");
        Console.WriteLine($"  Episodes: {detail.Episodes.Count}");
        Console.WriteLine($"  Categories: {string.Join(", ", detail.CategoryIds)}");
    }

    [TestMethod]
    public async Task EndToEnd_BrowseAndGetDetail()
    {
        // This test simulates a user flow:
        // 1. Get categories
        // 2. Get top podcasts in a category
        // 3. Get details for the first podcast

        // Step 1: Get categories
        var categories = _client!.Categories.GetCategories();
        Assert.IsTrue(categories.Count > 0);
        Console.WriteLine($"Step 1: Found {categories.Count} categories");

        // Step 2: Get top podcasts in Technology category
        var podcasts = await _client.Categories.GetTopPodcastsAsync(
            PodcastCategory.Technology.Id,
            "us",
            10);

        Assert.IsTrue(podcasts.Count > 0);
        Console.WriteLine($"Step 2: Found {podcasts.Count} podcasts in Technology");

        var firstPodcast = podcasts[0];
        Console.WriteLine($"  Selected: {firstPodcast.Name} (ID: {firstPodcast.Id})");

        // Step 3: Get podcast details
        var detail = await _client.Details.GetDetailByIdAsync(firstPodcast.Id);

        Assert.IsNotNull(detail);
        Console.WriteLine($"Step 3: Got detail for '{detail.Name}'");
        Console.WriteLine($"  Episodes: {detail.Episodes.Count}");

        if (detail.Episodes.Count > 0)
        {
            var latestEpisode = detail.Episodes[0];
            Console.WriteLine($"  Latest Episode: {latestEpisode.Title}");
            Console.WriteLine($"  Audio URL: {latestEpisode.AudioUrl}");
        }
    }

    [TestMethod]
    public async Task MultipleCategoriesTopPodcasts_AllReturnResults()
    {
        // Test multiple categories to ensure they all work
        var categoriesToTest = new[]
        {
            PodcastCategory.Technology,
            PodcastCategory.News,
            PodcastCategory.Comedy,
            PodcastCategory.Science,
            PodcastCategory.Education,
        };

        foreach (var category in categoriesToTest)
        {
            var podcasts = await _client!.Categories.GetTopPodcastsAsync(
                category.Id,
                "us",
                10);

            Assert.IsNotNull(podcasts, $"Category {category.Id} should return results");
            Console.WriteLine($"Category {category.Id}: {podcasts.Count} podcasts");

            if (podcasts.Count > 0)
            {
                Console.WriteLine($"  First: {podcasts[0].Name}");
            }
        }
    }
}
