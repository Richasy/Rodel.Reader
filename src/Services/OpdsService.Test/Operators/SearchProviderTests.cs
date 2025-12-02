// Copyright (c) Richasy. All rights reserved.

using System.Text;
using RichardSzalay.MockHttp;

namespace Richasy.RodelReader.Services.OpdsService.Test.Operators;

/// <summary>
/// SearchProvider 搜索测试.
/// </summary>
[TestClass]
public class SearchProviderTests
{
    private readonly ILogger<OpdsClient> _logger = NullLogger<OpdsClient>.Instance;
    private MockHttpMessageHandler _mockHttp = null!;
    private SearchProvider _searchProvider = null!;
    private IOpdsDispatcher _dispatcher = null!;
    private OpdsV1Parser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        _dispatcher = new OpdsDispatcher(httpClient, _logger);
        _parser = new OpdsV1Parser(_logger);
        _searchProvider = new SearchProvider(_dispatcher, _parser, _logger);
    }

    [TestMethod]
    public async Task GetSearchDescriptionUri_ReturnUri_WhenSearchLinkExists()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.RootFeed));
        var feed = await _parser.ParseFeedAsync(stream, new Uri("https://opds.example.com/opds/v1.2/catalog"));

        // Act
        var uri = _searchProvider.GetSearchDescriptionUri(feed);

        // Assert
        Assert.IsNotNull(uri);
        Assert.IsTrue(uri.ToString().Contains("opensearch", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task GetSearchDescriptionUri_ReturnsNull_WhenNoSearchLink()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.EmptyFeed));
        var feed = await _parser.ParseFeedAsync(stream, new Uri("https://opds.example.com/opds/v1.2/empty"));

        // Act
        var uri = _searchProvider.GetSearchDescriptionUri(feed);

        // Assert
        Assert.IsNull(uri);
    }

    [TestMethod]
    public async Task GetSearchTemplateAsync_ReturnsTemplate()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/opensearch")
            .Respond("application/opensearchdescription+xml", TestDataFactory.OpenSearchDescription);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.RootFeed));
        var feed = await _parser.ParseFeedAsync(stream, new Uri("https://opds.example.com/opds/v1.2/catalog"));

        // Act
        var template = await _searchProvider.GetSearchTemplateAsync(feed);

        // Assert
        Assert.IsNotNull(template);
        Assert.IsTrue(template.Contains("{searchTerms}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildSearchUri_CreatesCorrectUri()
    {
        // Arrange
        var template = "https://opds.example.com/search?q={searchTerms}";

        // Act
        var uri = _searchProvider.BuildSearchUri(template, "test query");

        // Assert
        Assert.IsNotNull(uri);
        Console.WriteLine($"Generated URI ToString: {uri}");
        Console.WriteLine($"Generated URI AbsoluteUri: {uri.AbsoluteUri}");
        Console.WriteLine($"Generated URI Query: {uri.Query}");
        // Uri.AbsoluteUri preserves encoding, ToString may decode
        Assert.IsTrue(uri.AbsoluteUri.Contains("test%20query", StringComparison.Ordinal), $"AbsoluteUri was: {uri.AbsoluteUri}");
    }

    [TestMethod]
    public async Task SearchAsync_WithTemplateAndQuery_ReturnsResults()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/search?q=test")
            .Respond("application/atom+xml", TestDataFactory.SearchResultsFeed);

        var template = "https://opds.example.com/opds/v1.2/search?q={searchTerms}";

        // Act
        var results = await _searchProvider.SearchAsync(template, "test");

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual("Search Results", results.Title);
        Assert.AreEqual(1, results.Entries.Count);
    }

    [TestMethod]
    public async Task SearchAsync_WithFeed_ReturnsResults()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/opensearch")
            .Respond("application/opensearchdescription+xml", TestDataFactory.OpenSearchDescription);
        _mockHttp.When("https://opds.example.com/opds/v1.2/search?q=book")
            .Respond("application/atom+xml", TestDataFactory.SearchResultsFeed);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.RootFeed));
        var feed = await _parser.ParseFeedAsync(stream, new Uri("https://opds.example.com/opds/v1.2/catalog"));

        // Act
        var results = await _searchProvider.SearchAsync(feed, "book");

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual("Search Results", results.Title);
    }

    [TestMethod]
    public async Task SearchAsync_WithFeed_ThrowsOpdsException_WhenNoSearchSupport()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.EmptyFeed));
        var feed = await _parser.ParseFeedAsync(stream, new Uri("https://opds.example.com/opds/v1.2/empty"));

        // Act & Assert
        var ex = await Assert.ThrowsExactlyAsync<OpdsException>(
            async () => await _searchProvider.SearchAsync(feed, "test"));
        Assert.IsNotNull(ex);
    }

    [TestMethod]
    public async Task SearchAsync_EncodesQueryProperly()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/search?q=hello%20world")
            .Respond("application/atom+xml", TestDataFactory.SearchResultsFeed);

        var template = "https://opds.example.com/opds/v1.2/search?q={searchTerms}";

        // Act
        var results = await _searchProvider.SearchAsync(template, "hello world");

        // Assert
        Assert.IsNotNull(results);
    }

    [TestMethod]
    public async Task SearchAsync_HandlesEmptyResults()
    {
        // Arrange
        _mockHttp.When("https://opds.example.com/opds/v1.2/search?q=notfound")
            .Respond("application/atom+xml", TestDataFactory.EmptyFeed);

        var template = "https://opds.example.com/opds/v1.2/search?q={searchTerms}";

        // Act
        var results = await _searchProvider.SearchAsync(template, "notfound");

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Entries.Count);
    }
}
