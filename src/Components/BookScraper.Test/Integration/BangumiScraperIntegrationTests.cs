// Copyright (c) Richasy. All rights reserved.

namespace BookScraper.Test.Integration;

[TestClass]
[TestCategory("Integration")]
public class BangumiScraperIntegrationTests : ScraperIntegrationTestBase
{
    private BangumiBookScraper _scraper = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<BangumiBookScraper>();
    }

    [TestInitialize]
    public override void TestSetup()
    {
        base.TestSetup();
        _scraper = GetService<BangumiBookScraper>();
    }

    [TestMethod]
    public void Type_ReturnsBangumi()
    {
        Assert.AreEqual(ScraperType.Bangumi, _scraper.Type);
    }

    [TestMethod]
    public async Task SearchBooksAsync_WithValidKeyword_ReturnsResults()
    {
        // Arrange
        var keyword = "魔法禁书目录";

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
            Id = "3559",
            Title = "魔法禁书目录",
            Source = ScraperType.Bangumi,
            WebLink = "https://bangumi.tv/subject/3559",
        };

        // Act
        var result = await _scraper.GetBookDetailAsync(book);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(book.Id, result.Id);
        Assert.IsFalse(string.IsNullOrEmpty(result.Description));
    }
}
