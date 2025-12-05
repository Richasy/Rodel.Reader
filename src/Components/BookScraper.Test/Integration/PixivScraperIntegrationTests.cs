// Copyright (c) Richasy. All rights reserved.

namespace BookScraper.Test.Integration;

[TestClass]
[TestCategory("Integration")]
public class PixivScraperIntegrationTests : ScraperIntegrationTestBase
{
    private PixivBookScraper _scraper = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PixivBookScraper>();
    }

    [TestInitialize]
    public override void TestSetup()
    {
        base.TestSetup();
        _scraper = GetService<PixivBookScraper>();
    }

    [TestMethod]
    public void Type_ReturnsPixiv()
    {
        Assert.AreEqual(ScraperType.Pixiv, _scraper.Type);
    }

    [TestMethod]
    [Ignore("需要网络访问和 Pixiv 登录，仅手动运行")]
    public async Task SearchBooksAsync_WithValidKeyword_ReturnsResults()
    {
        // Arrange
        var keyword = "小说";

        // Act
        var results = await _scraper.SearchBooksAsync(keyword);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreNotEqual(0, results.Count, "应该返回搜索结果");
    }

    [TestMethod]
    public async Task GetBookDetailAsync_ReturnsOriginalBook()
    {
        // Arrange
        var book = new ScrapedBook
        {
            Id = "26631466",
            Title = "慈愛の女神",
            Source = ScraperType.Pixiv,
        };

        // Act
        var result = await _scraper.GetBookDetailAsync(book);

        // Assert
        Assert.AreSame(book, result);
    }
}
