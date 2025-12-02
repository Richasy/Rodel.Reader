// Copyright (c) Richasy. All rights reserved.

using System.Net;

namespace Richasy.RodelReader.Services.OpdsService.Test.Integration;

/// <summary>
/// Komga OPDS 集成测试.
/// 使用 Komga Demo 服务器: https://demo.komga.org
/// 账号: demo@komga.org
/// 密码: komga-demo
/// </summary>
[TestClass]
[TestCategory("Integration")]
public class KomgaIntegrationTests
{
    private const string KomgaBaseUrl = "https://demo.komga.org";
    private const string KomgaOpdsPath = "/opds/v1.2/catalog";
    private const string KomgaUsername = "demo@komga.org";
    private const string KomgaPassword = "komga-demo";

    private OpdsClient? _client;

    [TestInitialize]
    public void Setup()
    {
        var options = new OpdsClientOptions
        {
            RootUri = new Uri($"{KomgaBaseUrl}{KomgaOpdsPath}"),
            UserAgent = "RodelReader.OpdsService.Test/1.0",
            Timeout = TimeSpan.FromSeconds(30),
            Credentials = new NetworkCredential(KomgaUsername, KomgaPassword),
        };

        _client = new OpdsClient(options);
    }

    [TestMethod]
    public async Task GetRootCatalog_ReturnsValidFeed()
    {
        // Act
        var feed = await _client!.Catalog.GetRootAsync();

        // Assert
        Assert.IsNotNull(feed);
        Assert.IsFalse(string.IsNullOrEmpty(feed.Title));
        Assert.IsTrue(feed.Entries.Count > 0, "Root catalog should have navigation entries");

        Console.WriteLine($"Catalog Title: {feed.Title}");
        Console.WriteLine($"Entries Count: {feed.Entries.Count}");
        foreach (var entry in feed.Entries)
        {
            Console.WriteLine($"  - {entry.Title} (Navigation: {entry.IsNavigationEntry})");
        }
    }

    [TestMethod]
    public async Task NavigateToFirstEntry_ReturnsSubFeed()
    {
        // Arrange
        var rootFeed = await _client!.Catalog.GetRootAsync();

        var firstNavEntry = rootFeed.GetNavigationEntries().FirstOrDefault();
        if (firstNavEntry is null)
        {
            Assert.Inconclusive("No navigation entries found in root catalog");
            return;
        }

        // Act
        var subFeed = await _client.Catalog.NavigateToEntryAsync(firstNavEntry);

        // Assert
        Assert.IsNotNull(subFeed);
        Console.WriteLine($"Sub-feed Title: {subFeed.Title}");
        Console.WriteLine($"Sub-feed Entries: {subFeed.Entries.Count}");
    }

    [TestMethod]
    public async Task Search_WorksWithKomga()
    {
        // Arrange
        var rootFeed = await _client!.Catalog.GetRootAsync();

        // Check if search is supported
        var searchUri = _client.Search.GetSearchDescriptionUri(rootFeed);
        if (searchUri is null)
        {
            Assert.Inconclusive("Search is not available on this server");
            return;
        }

        Console.WriteLine($"Search URI: {searchUri}");

        // Get search template
        var searchTemplate = await _client.Search.GetSearchTemplateAsync(rootFeed);
        if (searchTemplate is null)
        {
            Assert.Inconclusive("Could not get search template");
            return;
        }

        Console.WriteLine($"Search Template: {searchTemplate}");

        // Act - search for a generic term
        var results = await _client.Search.SearchAsync(searchTemplate, "book");

        // Assert
        Assert.IsNotNull(results);
        Console.WriteLine($"Search Results Title: {results.Title}");
        Console.WriteLine($"Found {results.Entries.Count} results");
        foreach (var entry in results.Entries.Take(5))
        {
            Console.WriteLine($"  - {entry.Title}");
        }
    }

    [TestMethod]
    public async Task GetBookEntries_ContainsAcquisitionLinks()
    {
        // Arrange
        var rootFeed = await _client!.Catalog.GetRootAsync();

        // Navigate to find books
        var allBooksEntry = rootFeed.Entries
            .FirstOrDefault(e => e.Title?.Contains("All", StringComparison.OrdinalIgnoreCase) == true
                || e.Title?.Contains("Latest", StringComparison.OrdinalIgnoreCase) == true
                || e.Title?.Contains("series", StringComparison.OrdinalIgnoreCase) == true);

        if (allBooksEntry is null)
        {
            allBooksEntry = rootFeed.GetNavigationEntries().FirstOrDefault();
        }

        if (allBooksEntry is null)
        {
            Assert.Inconclusive("No navigation entries found");
            return;
        }

        var booksFeed = await _client.Catalog.NavigateToEntryAsync(allBooksEntry);
        if (booksFeed is null)
        {
            Assert.Inconclusive("Could not navigate to books feed");
            return;
        }

        // Keep navigating until we find actual books
        var maxDepth = 3;
        var currentFeed = booksFeed;
        while (maxDepth > 0 && !currentFeed.GetBookEntries().Any())
        {
            var navEntry = currentFeed.GetNavigationEntries().FirstOrDefault();
            if (navEntry is null)
            {
                break;
            }

            var newFeed = await _client.Catalog.NavigateToEntryAsync(navEntry);
            if (newFeed is null)
            {
                break;
            }

            currentFeed = newFeed;
            maxDepth--;
        }

        // Assert
        var books = currentFeed.GetBookEntries().ToList();
        Console.WriteLine($"Found {books.Count} books");

        foreach (var book in books.Take(3))
        {
            Console.WriteLine($"\nBook: {book.Title}");
            Console.WriteLine($"  Authors: {string.Join(", ", book.Authors.Select(a => a.Name))}");
            Console.WriteLine($"  Acquisitions: {book.Acquisitions.Count}");

            foreach (var acq in book.Acquisitions)
            {
                Console.WriteLine($"    - Type: {acq.Type}, MediaType: {acq.MediaType}");
                Console.WriteLine($"      URL: {acq.Href}");
            }

            var cover = book.GetCoverImage();
            if (cover is not null)
            {
                Console.WriteLine($"  Cover: {cover.Href}");
            }
        }

        if (books.Count > 0)
        {
            var firstBook = books[0];
            Assert.IsTrue(firstBook.Acquisitions.Count > 0, "Book entries should have acquisition links");
        }
    }

    [TestMethod]
    public async Task Pagination_WorksCorrectly()
    {
        // Arrange
        var rootFeed = await _client!.Catalog.GetRootAsync();

        // Find a feed with many entries that might be paginated
        var largeEntry = rootFeed.GetNavigationEntries()
            .FirstOrDefault(e => e.Title?.Contains("All", StringComparison.OrdinalIgnoreCase) == true
                || e.Title?.Contains("series", StringComparison.OrdinalIgnoreCase) == true);

        if (largeEntry is null)
        {
            largeEntry = rootFeed.GetNavigationEntries().FirstOrDefault();
        }

        if (largeEntry is null)
        {
            Assert.Inconclusive("No navigation entries found");
            return;
        }

        var feed = await _client.Catalog.NavigateToEntryAsync(largeEntry);
        if (feed is null)
        {
            Assert.Inconclusive("Could not navigate to feed");
            return;
        }

        Console.WriteLine($"Feed: {feed.Title}");
        Console.WriteLine($"Has Next Page: {feed.HasNextPage}");
        Console.WriteLine($"Has Previous Page: {feed.HasPreviousPage}");
        Console.WriteLine($"Entries: {feed.Entries.Count}");

        // Act - try to get next page if available
        if (feed.HasNextPage)
        {
            var nextFeed = await _client.Catalog.GetNextPageAsync(feed);

            // Assert
            Assert.IsNotNull(nextFeed);
            Console.WriteLine($"\nNext Page: {nextFeed.Title}");
            Console.WriteLine($"Next Page Entries: {nextFeed.Entries.Count}");
        }
    }

    [TestMethod]
    public async Task Facets_AreParsedCorrectly()
    {
        // Arrange
        var rootFeed = await _client!.Catalog.GetRootAsync();

        // Navigate through feeds looking for facets
        var feedsToCheck = new Queue<OpdsFeed>();
        feedsToCheck.Enqueue(rootFeed);

        OpdsFeed? feedWithFacets = null;
        var checkedCount = 0;

        while (feedsToCheck.Count > 0 && checkedCount < 5)
        {
            var currentFeed = feedsToCheck.Dequeue();
            checkedCount++;

            if (currentFeed.FacetGroups.Count > 0)
            {
                feedWithFacets = currentFeed;
                break;
            }

            // Navigate to first navigation entry
            var navEntry = currentFeed.GetNavigationEntries().FirstOrDefault();
            if (navEntry is not null)
            {
                var subFeed = await _client.Catalog.NavigateToEntryAsync(navEntry);
                if (subFeed is not null)
                {
                    feedsToCheck.Enqueue(subFeed);
                }
            }
        }

        // Assert
        if (feedWithFacets is null)
        {
            Console.WriteLine("No facets found in the first 5 feeds checked");
            Assert.Inconclusive("No facets found - Komga might not have facet support enabled");
            return;
        }

        Console.WriteLine($"Feed with Facets: {feedWithFacets.Title}");
        Console.WriteLine($"Facet Groups: {feedWithFacets.FacetGroups.Count}");

        foreach (var group in feedWithFacets.FacetGroups)
        {
            Console.WriteLine($"\n  Group: {group.Title}");
            foreach (var facet in group.Facets.Take(3))
            {
                Console.WriteLine($"    - {facet.Title} (Count: {facet.Count}, Active: {facet.IsActive})");
            }
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
    }
}
