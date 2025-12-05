// Copyright (c) Richasy. All rights reserved.

namespace BookScraper.Test.Integration;

[TestClass]
[TestCategory("Integration")]
public class JinJiangScraperIntegrationTests : ScraperIntegrationTestBase
{
    private JinJiangBookScraper _scraper = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<JinJiangBookScraper>();
    }

    [TestInitialize]
    public override void TestSetup()
    {
        base.TestSetup();
        _scraper = GetService<JinJiangBookScraper>();
    }

    [TestMethod]
    public void Type_ReturnsJinJiang()
    {
        Assert.AreEqual(ScraperType.JinJiang, _scraper.Type);
    }

    [TestMethod]
    public async Task SearchBooksAsync_WithValidKeyword_ReturnsResults()
    {
        // Arrange
        var keyword = "穿越";

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
            Id = "9235710",
            Title = "侯门",
            Source = ScraperType.JinJiang,
        };

        // Act
        var result = await _scraper.GetBookDetailAsync(book);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(book.Id, result.Id);
        Assert.AreEqual("晋江文学城", result.Publisher);
    }
}
