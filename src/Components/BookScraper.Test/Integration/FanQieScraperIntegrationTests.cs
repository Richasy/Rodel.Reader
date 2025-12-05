// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Sources.FanQie;
using Richasy.RodelReader.Sources.FanQie.Abstractions;

namespace BookScraper.Test.Integration;

[TestClass]
[TestCategory("Integration")]
public class FanQieScraperIntegrationTests : ScraperIntegrationTestBase
{
    private FanQieBookScraper _scraper = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFanQieClient>(sp =>
        {
            var logger = sp.GetService<ILogger<FanQieClient>>();
            return new FanQieClient(logger: logger);
        });
        services.AddSingleton<FanQieBookScraper>();
    }

    [TestInitialize]
    public override void TestSetup()
    {
        base.TestSetup();
        _scraper = GetService<FanQieBookScraper>();
    }

    [TestMethod]
    public void FeatureId_ReturnsFanQie()
    {
        Assert.AreEqual(FanQieBookScraper.Id, _scraper.FeatureId);
    }

    [TestMethod]
    public async Task SearchBooksAsync_WithValidKeyword_ReturnsResults()
    {
        // Arrange
        var keyword = "斗破苍穹";

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
            Id = "7143038691944959011",
            Title = "十日终焉",
            ScraperId = FanQieBookScraper.Id,
        };

        // Act
        var result = await _scraper.GetBookDetailAsync(book);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(book.Id, result.Id);
        Assert.AreEqual("番茄小说", result.Publisher);
    }
}
