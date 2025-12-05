// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Components.BookScraper.Internal;
using Richasy.RodelReader.Components.BookScraper.Internal.Responses;
using Richasy.RodelReader.Components.BookScraper.Internal.Responses.Pixiv;

namespace Richasy.RodelReader.Components.BookScraper.Services;

/// <summary>
/// Pixiv 小说刮削器.
/// </summary>
public sealed class PixivBookScraper : IBookScraper
{
    private const string SearchUrl = "https://www.pixiv.net/ajax/search/novels/{0}?word={0}&order=date_d&mode=all&p=1&s_mode=s_tag&gs=1&lang=zh";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PixivBookScraper>? _logger;

    /// <summary>
    /// 初始化 <see cref="PixivBookScraper"/> 类的新实例.
    /// </summary>
    /// <param name="httpClientFactory">HTTP 客户端工厂.</param>
    /// <param name="logger">日志记录器.</param>
    public PixivBookScraper(
        IHttpClientFactory httpClientFactory,
        ILogger<PixivBookScraper>? logger = null)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public ScraperType Type => ScraperType.Pixiv;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScrapedBook>> SearchBooksAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Pixiv 小说搜索: {Keyword}", keyword);

        var encodedKeyword = Uri.EscapeDataString(keyword);
        var url = string.Format(CultureInfo.InvariantCulture, SearchUrl, encodedKeyword);
        var client = _httpClientFactory.CreateClient(HttpClientNames.Scraper);

        try
        {
            var json = await client.GetStringAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
            var response = JsonSerializer.Deserialize(json, JsonContext.Default.PixivSearchResponse);

            if (response is null || response.error)
            {
                _logger?.LogWarning("Pixiv 搜索失败或返回错误");
                return Array.Empty<ScrapedBook>();
            }

            var novelList = response.body?.novel?.data;
            if (novelList is null || novelList.Length == 0)
            {
                _logger?.LogInformation("Pixiv 搜索无结果: {Keyword}", keyword);
                return Array.Empty<ScrapedBook>();
            }

            var result = novelList.Select(ParseNovelItem).ToList();
            _logger?.LogInformation("Pixiv 搜索完成，找到 {Count} 本书籍", result.Count);
            return result.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Pixiv 搜索失败: {Keyword}", keyword);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<ScrapedBook> GetBookDetailAsync(
        ScrapedBook book,
        CancellationToken cancellationToken = default)
    {
        // Pixiv 搜索结果已包含完整信息，无需额外请求
        _logger?.LogDebug("Pixiv 书籍详情（使用搜索结果）: {BookId}", book.Id);
        return Task.FromResult(book);
    }

    private static ScrapedBook ParseNovelItem(PixivSearchNovelItem item)
    {
        var rating = item.bookmarkCount switch
        {
            < 100 => 1,
            < 500 => 2,
            < 1000 => 3,
            < 5000 => 4,
            _ => 5,
        };

        var subtitle = item.tags?.FirstOrDefault();
        var publishDate = item.latestPublishDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return new ScrapedBook
        {
            Id = item.id,
            Title = item.title,
            Author = item.userName,
            Cover = item.cover?.urls?.original,
            Description = item.caption,
            Subtitle = subtitle,
            Rating = rating,
            Publisher = "Pixiv",
            PublishDate = publishDate,
            WebLink = $"https://www.pixiv.net/novel/series/{item.id}",
            Source = ScraperType.Pixiv,
        };
    }
}
