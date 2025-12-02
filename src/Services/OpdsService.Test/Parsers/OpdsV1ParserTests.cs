// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace Richasy.RodelReader.Services.OpdsService.Test.Parsers;

/// <summary>
/// OpdsV1Parser 解析器测试.
/// </summary>
[TestClass]
public class OpdsV1ParserTests
{
    private readonly ILogger<OpdsClient> _logger = NullLogger<OpdsClient>.Instance;
    private OpdsV1Parser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new OpdsV1Parser(_logger);
    }

    [TestMethod]
    public void ParseFeed_RootFeed_ReturnsCorrectFeed()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.RootFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/catalog");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        Assert.AreEqual("urn:uuid:root-catalog", feed.Id);
        Assert.AreEqual("My Library", feed.Title);
        Assert.AreEqual("A test OPDS catalog", feed.Subtitle);
        Assert.IsNotNull(feed.UpdatedAt);
        Assert.IsNotNull(feed.Icon);
        Assert.AreEqual(2, feed.Entries.Count);
        Assert.IsTrue(feed.Links.Count >= 3);
    }

    [TestMethod]
    public void ParseFeed_RootFeed_ParsesLinksCorrectly()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.RootFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/catalog");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var selfLink = feed.GetSelfLink();
        Assert.IsNotNull(selfLink);
        Assert.AreEqual(OpdsLinkRelation.Self, selfLink.Relation);

        var startLink = feed.GetStartLink();
        Assert.IsNotNull(startLink);
        Assert.AreEqual(OpdsLinkRelation.Start, startLink.Relation);

        var searchLink = feed.GetSearchLink();
        Assert.IsNotNull(searchLink);
        Assert.AreEqual(OpdsLinkRelation.Search, searchLink.Relation);
    }

    [TestMethod]
    public void ParseFeed_RootFeed_ParsesNavigationEntries()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.RootFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/catalog");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var navEntries = feed.GetNavigationEntries().ToList();
        Assert.AreEqual(2, navEntries.Count);

        var newBooksEntry = navEntries.First(e => e.Title == "New Books");
        Assert.IsTrue(newBooksEntry.IsNavigationEntry);
        Assert.IsFalse(newBooksEntry.IsBookEntry);

        var navLink = newBooksEntry.GetNavigationLink();
        Assert.IsNotNull(navLink);
        Assert.IsTrue(navLink.Href.ToString().Contains("/new", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ParseFeed_BooksFeed_ParsesBookEntries()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var bookEntries = feed.GetBookEntries().ToList();
        Assert.AreEqual(2, bookEntries.Count);

        var book1 = bookEntries.First(e => e.Title == "The Great Adventure");
        Assert.IsTrue(book1.IsBookEntry);
        Assert.IsFalse(book1.IsNavigationEntry);
    }

    [TestMethod]
    public void ParseFeed_BooksFeed_ParsesAuthors()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var book1 = feed.Entries.First(e => e.Title == "The Great Adventure");
        Assert.AreEqual(2, book1.Authors.Count);

        var johnDoe = book1.Authors.First(a => a.Name == "John Doe");
        Assert.IsNotNull(johnDoe.Uri);
        Assert.AreEqual("https://example.com/authors/john", johnDoe.Uri.ToString());

        var janeSmith = book1.Authors.First(a => a.Name == "Jane Smith");
        Assert.IsNull(janeSmith.Uri);
    }

    [TestMethod]
    public void ParseFeed_BooksFeed_ParsesCategories()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var book1 = feed.Entries.First(e => e.Title == "The Great Adventure");
        Assert.AreEqual(2, book1.Categories.Count);

        var fictionCategory = book1.Categories.First(c => c.Term == "fiction");
        Assert.AreEqual("Fiction", fictionCategory.Label);
    }

    [TestMethod]
    public void ParseFeed_BooksFeed_ParsesDublinCoreMetadata()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var book1 = feed.Entries.First(e => e.Title == "The Great Adventure");
        Assert.AreEqual("en", book1.Language);
        Assert.AreEqual("Example Publisher", book1.Publisher);
        Assert.AreEqual("isbn:978-0-123456-78-9", book1.Identifier);
    }

    [TestMethod]
    public void ParseFeed_BooksFeed_ParsesImages()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var book1 = feed.Entries.First(e => e.Title == "The Great Adventure");
        Assert.AreEqual(2, book1.Images.Count);

        var cover = book1.GetCoverImage();
        Assert.IsNotNull(cover);
        Assert.AreEqual(OpdsLinkRelation.Image, cover.Relation);
        Assert.IsTrue(cover.Href.ToString().Contains("book-1.jpg", StringComparison.Ordinal));

        var thumbnail = book1.GetThumbnail();
        Assert.IsNotNull(thumbnail);
        Assert.AreEqual(OpdsLinkRelation.Thumbnail, thumbnail.Relation);
    }

    [TestMethod]
    public void ParseFeed_BooksFeed_ParsesAcquisitionLinks()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var book1 = feed.Entries.First(e => e.Title == "The Great Adventure");
        Assert.AreEqual(2, book1.Acquisitions.Count);

        var epubAcq = book1.GetAcquisitionByMediaType("application/epub+zip");
        Assert.IsNotNull(epubAcq);
        Assert.AreEqual(AcquisitionType.OpenAccess, epubAcq.Type);

        var pdfAcq = book1.GetAcquisitionByMediaType("application/pdf");
        Assert.IsNotNull(pdfAcq);
    }

    [TestMethod]
    public void ParseFeed_BooksFeed_ParsesFacets()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        Assert.AreEqual(2, feed.FacetGroups.Count);

        var authorsGroup = feed.FacetGroups.First(g => g.Title == "Authors");
        Assert.AreEqual(1, authorsGroup.Facets.Count);
        Assert.AreEqual("John Doe", authorsGroup.Facets[0].Title);
        Assert.AreEqual(5, authorsGroup.Facets[0].Count);
        Assert.IsFalse(authorsGroup.Facets[0].IsActive);

        var genresGroup = feed.FacetGroups.First(g => g.Title == "Genres");
        Assert.AreEqual(1, genresGroup.Facets.Count);
        Assert.IsTrue(genresGroup.Facets[0].IsActive);
        Assert.AreEqual(10, genresGroup.Facets[0].Count);
    }

    [TestMethod]
    public void ParseFeed_PagedFeed_ParsesPaginationLinks()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.PagedFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all?page=2");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        Assert.IsTrue(feed.HasNextPage);
        Assert.IsTrue(feed.HasPreviousPage);

        var nextLink = feed.GetNextLink();
        Assert.IsNotNull(nextLink);
        Assert.IsTrue(nextLink.Href.ToString().Contains("page=3", StringComparison.Ordinal));

        var prevLink = feed.GetPreviousLink();
        Assert.IsNotNull(prevLink);
    }

    [TestMethod]
    public void ParseFeed_PaidBooksFeed_ParsesPriceAndIndirectAcquisition()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.PaidBooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/paid");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        var paidBook = feed.Entries[0];
        Assert.AreEqual(3, paidBook.Acquisitions.Count);

        var buyAcq = paidBook.Acquisitions.First(a => a.Type == AcquisitionType.Buy);
        Assert.IsNotNull(buyAcq.Price);
        Assert.AreEqual(9.99m, buyAcq.Price.Value);
        Assert.AreEqual("USD", buyAcq.Price.CurrencyCode);
        Assert.IsTrue(buyAcq.IndirectMediaTypes.Count > 0);

        var sampleAcq = paidBook.Acquisitions.FirstOrDefault(a => a.Type == AcquisitionType.Sample);
        Assert.IsNotNull(sampleAcq);

        var borrowAcq = paidBook.Acquisitions.FirstOrDefault(a => a.Type == AcquisitionType.Borrow);
        Assert.IsNotNull(borrowAcq);
    }

    [TestMethod]
    public void ParseFeed_EmptyFeed_ReturnsEmptyEntries()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.EmptyFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/empty");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        Assert.AreEqual("Empty Catalog", feed.Title);
        Assert.AreEqual(0, feed.Entries.Count);
        Assert.IsFalse(feed.HasNextPage);
        Assert.IsFalse(feed.SupportsSearch);
    }

    [TestMethod]
    public void ParseFeed_MinimalFeed_HandlesMinimalData()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.MinimalFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/minimal");

        // Act
        var feed = _parser.ParseFeed(stream, baseUri);

        // Assert
        Assert.AreEqual("Minimal", feed.Title);
        Assert.IsNull(feed.Id);
        Assert.IsNull(feed.UpdatedAt);
    }

    [TestMethod]
    public async Task ParseFeedAsync_Works()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.RootFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/catalog");

        // Act
        var feed = await _parser.ParseFeedAsync(stream, baseUri);

        // Assert
        Assert.AreEqual("My Library", feed.Title);
        Assert.AreEqual(2, feed.Entries.Count);
    }

    [TestMethod]
    public void ParseOpenSearchDescription_ReturnsSearchTemplate()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.OpenSearchDescription));

        // Act
        var template = _parser.ParseOpenSearchDescription(stream);

        // Assert
        Assert.IsNotNull(template);
        Assert.IsTrue(template.Contains("{searchTerms}", StringComparison.Ordinal));
        Assert.IsTrue(template.Contains("application/atom+xml", StringComparison.Ordinal) || template.Contains("opds.example.com", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ParseOpenSearchDescriptionAsync_Works()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.OpenSearchDescription));

        // Act
        var template = await _parser.ParseOpenSearchDescriptionAsync(stream);

        // Assert
        Assert.IsNotNull(template);
        Assert.IsTrue(template.Contains("{searchTerms}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void GetOpenAccessAcquisition_ReturnsFirstOpenAccess()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");
        var feed = _parser.ParseFeed(stream, baseUri);

        // Act
        var book = feed.Entries.First(e => e.Title == "The Great Adventure");
        var acquisition = book.GetOpenAccessAcquisition();

        // Assert
        Assert.IsNotNull(acquisition);
        Assert.AreEqual(AcquisitionType.OpenAccess, acquisition.Type);
    }

    [TestMethod]
    public void GetDownloadableAcquisitions_ReturnsOpenAccessFirst()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestDataFactory.BooksFeed));
        var baseUri = new Uri("https://opds.example.com/opds/v1.2/all");
        var feed = _parser.ParseFeed(stream, baseUri);

        // Act
        var book = feed.Entries.First(e => e.Title == "The Great Adventure");
        var downloads = book.GetDownloadableAcquisitions().ToList();

        // Assert
        Assert.AreEqual(2, downloads.Count);
        Assert.AreEqual(AcquisitionType.OpenAccess, downloads[0].Type);
    }
}
