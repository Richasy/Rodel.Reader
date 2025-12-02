// Copyright (c) Richasy. All rights reserved.

using RichardSzalay.MockHttp;

namespace Richasy.RodelReader.Services.OpdsService.Test.Operators;

/// <summary>
/// CatalogNavigator 导航测试.
/// </summary>
[TestClass]
public class CatalogNavigatorTests
{
    private readonly ILogger<OpdsClient> _logger = NullLogger<OpdsClient>.Instance;
    private MockHttpMessageHandler _mockHttp = null!;
    private CatalogNavigator _navigator = null!;
    private IOpdsDispatcher _dispatcher = null!;
    private IOpdsParser _parser = null!;
    private OpdsClientOptions _options = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        _options = new OpdsClientOptions
        {
            RootUri = new Uri("https://opds.example.com/opds/v1.2/catalog"),
            UserAgent = "Test/1.0",
        };
        _dispatcher = new OpdsDispatcher(httpClient, _logger);
        _parser = new OpdsV1Parser(_logger);
        _navigator = new CatalogNavigator(_dispatcher, _parser, _options, _logger);
    }

    [TestMethod]
    public async Task GetRootAsync_ReturnsRootFeed()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/catalog")
            .Respond("application/atom+xml", TestDataFactory.RootFeed);

        // Act
        var feed = await _navigator.GetRootAsync();

        // Assert
        Assert.IsNotNull(feed);
        Assert.AreEqual("My Library", feed.Title);
        Assert.AreEqual(2, feed.Entries.Count);
    }

    [TestMethod]
    public async Task GetFeedAsync_WithCustomUri_ReturnsFeed()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/all")
            .Respond("application/atom+xml", TestDataFactory.BooksFeed);

        // Act
        var feed = await _navigator.GetFeedAsync(new Uri("https://opds.example.com/opds/v1.2/all"));

        // Assert
        Assert.IsNotNull(feed);
        Assert.AreEqual("All Books", feed.Title);
    }

    [TestMethod]
    public async Task GetNextPageAsync_ReturnsNull_WhenNoNextPage()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/catalog")
            .Respond("application/atom+xml", TestDataFactory.RootFeed);

        var feed = await _navigator.GetRootAsync();

        // Act
        var nextFeed = await _navigator.GetNextPageAsync(feed);

        // Assert
        Assert.IsNull(nextFeed);
    }

    [TestMethod]
    public async Task GetNextPageAsync_ReturnsNextPage_WhenAvailable()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/all?page=2")
            .Respond("application/atom+xml", TestDataFactory.PagedFeed);
        _mockHttp.When("https://opds.example.com/opds/v1.2/all?page=3")
            .Respond("application/atom+xml", TestDataFactory.RootFeed);

        var feed = await _navigator.GetFeedAsync(new Uri("https://opds.example.com/opds/v1.2/all?page=2"));

        // Act
        var nextFeed = await _navigator.GetNextPageAsync(feed);

        // Assert
        Assert.IsNotNull(nextFeed);
        Assert.AreEqual("My Library", nextFeed.Title);
    }

    [TestMethod]
    public async Task GetPreviousPageAsync_ReturnsPreviousPage_WhenAvailable()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/all?page=2")
            .Respond("application/atom+xml", TestDataFactory.PagedFeed);
        _mockHttp.When("https://opds.example.com/opds/v1.2/all?page=1")
            .Respond("application/atom+xml", TestDataFactory.BooksFeed);

        var feed = await _navigator.GetFeedAsync(new Uri("https://opds.example.com/opds/v1.2/all?page=2"));

        // Act
        var prevFeed = await _navigator.GetPreviousPageAsync(feed);

        // Assert
        Assert.IsNotNull(prevFeed);
        Assert.AreEqual("All Books", prevFeed.Title);
    }

    [TestMethod]
    public async Task NavigateToEntryAsync_ReturnsEntryFeed_ForNavigationEntry()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/catalog")
            .Respond("application/atom+xml", TestDataFactory.RootFeed);
        _mockHttp.When("https://opds.example.com/opds/v1.2/new")
            .Respond("application/atom+xml", TestDataFactory.BooksFeed);

        var rootFeed = await _navigator.GetRootAsync();
        var newBooksEntry = rootFeed.Entries.First(e => e.Title == "New Books");

        // Act
        var entryFeed = await _navigator.NavigateToEntryAsync(newBooksEntry);

        // Assert
        Assert.IsNotNull(entryFeed);
        Assert.AreEqual("All Books", entryFeed.Title);
    }

    [TestMethod]
    public async Task NavigateToEntryAsync_ReturnsNull_ForBookEntry()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/all")
            .Respond("application/atom+xml", TestDataFactory.BooksFeed);

        var feed = await _navigator.GetFeedAsync(new Uri("https://opds.example.com/opds/v1.2/all"));
        var bookEntry = feed.Entries.First(e => e.Title == "The Great Adventure");

        // Act
        var entryFeed = await _navigator.NavigateToEntryAsync(bookEntry);

        // Assert
        Assert.IsNull(entryFeed);
    }
}
