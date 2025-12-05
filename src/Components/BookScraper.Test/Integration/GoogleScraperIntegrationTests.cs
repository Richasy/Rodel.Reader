// Copyright (c) Richasy. All rights reserved.

namespace BookScraper.Test.Integration;

[TestClass]
[TestCategory("Integration")]
public class GoogleScraperIntegrationTests : ScraperIntegrationTestBase
{
    private GoogleBookScraper _scraper = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<GoogleBookScraper>();
    }

    [TestInitialize]
    public override void TestSetup()
    {
        base.TestSetup();
        _scraper = GetService<GoogleBookScraper>();
    }

    [TestMethod]
    public void Type_ReturnsGoogle()
    {
        Assert.AreEqual(ScraperType.Google, _scraper.Type);
    }

    [TestMethod]
    public async Task SearchBooksAsync_WithValidKeyword_ReturnsResults()
    {
        // Arrange
        var keyword = "Harry Potter";

        // Act
        var results = await _scraper.SearchBooksAsync(keyword);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreNotEqual(0, results.Count, "应该返回搜索结果");
    }

    [TestMethod]
    public async Task GetBookDetailAsync_WithValidBook_ReturnsDetailedInfo()
    {
        // Arrange
        var book = new ScrapedBook
        {
            Id = "abYKXvCwEToC",
            Title = "Harry Potter",
            Source = ScraperType.Google,
        };

        // Act
        var result = await _scraper.GetBookDetailAsync(book);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(book.Id, result.Id);
    }
}
