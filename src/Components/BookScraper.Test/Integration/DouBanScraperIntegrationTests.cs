// Copyright (c) Richasy. All rights reserved.

namespace BookScraper.Test.Integration;

[TestClass]
[TestCategory("Integration")]
public class DouBanScraperIntegrationTests : ScraperIntegrationTestBase
{
    private DouBanBookScraper _scraper = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<DouBanBookScraper>();
    }

    [TestInitialize]
    public override void TestSetup()
    {
        base.TestSetup();
        _scraper = GetService<DouBanBookScraper>();
    }

    [TestMethod]
    public void Type_ReturnsDouBan()
    {
        Assert.AreEqual(ScraperType.DouBan, _scraper.Type);
    }

    [TestMethod]
    public async Task SearchBooksAsync_WithValidKeyword_ReturnsResults()
    {
        // Arrange
        var keyword = "三体";

        // Act
        var results = await _scraper.SearchBooksAsync(keyword);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreNotEqual(0, results.Count, "应该返回搜索结果");
        CollectionAssert.AllItemsAreNotNull(results.ToList());
    }

    [TestMethod]
    public async Task GetBookDetailAsync_WithValidBook_ReturnsDetailedInfo()
    {
        // Arrange
        var book = new ScrapedBook
        {
            Id = "2567698",
            Title = "三体",
            Source = ScraperType.DouBan,
        };

        // Act
        var result = await _scraper.GetBookDetailAsync(book);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(book.Id, result.Id);
        Assert.IsFalse(string.IsNullOrEmpty(result.Author));
        Assert.IsFalse(string.IsNullOrEmpty(result.Description));
    }
}
